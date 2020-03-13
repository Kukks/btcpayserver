using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Data;
using BTCPayServer.Events;
using BTCPayServer.Payments.Bitcoin;
using BTCPayServer.Services.Invoices;
using BTCPayServer.Services.Stores;
using Microsoft.Extensions.Hosting;
using NBitcoin.RPC;
using NBXplorer;

namespace BTCPayServer.Payments.PayJoin
{
    public class PayJoinTransactionBroadcaster : IHostedService
    {
        // The spec mentioned to give a few mins(1-2), but i don't think it took under consideration the time taken to re-sign inputs with interactive methods( multisig, Hardware wallets, etc). I think 5 mins might be ok.
        private static readonly TimeSpan BroadcastAfter = TimeSpan.FromMinutes(5);

        private readonly EventAggregator _eventAggregator;
        private readonly ExplorerClientProvider _explorerClientProvider;
        private readonly PayJoinStateProvider _payJoinStateProvider;

        private CompositeDisposable leases = new CompositeDisposable();

        public PayJoinTransactionBroadcaster(
            EventAggregator eventAggregator,
            ExplorerClientProvider explorerClientProvider,
            PayJoinStateProvider payJoinStateProvider)
        {
            _eventAggregator = eventAggregator;
            _explorerClientProvider = explorerClientProvider;
            _payJoinStateProvider = payJoinStateProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var loadCoins = _payJoinStateProvider.LoadCoins();
            //if the wallet was updated, we need to remove the state as the utxos no longer fit
            leases.Add(_eventAggregator.Subscribe<WalletChangedEvent>(evt =>
                _payJoinStateProvider.RemoveState(evt.WalletId)));

            leases.Add(_eventAggregator.Subscribe<NewOnChainTransactionEvent>(txEvent =>
            {
                if (!txEvent.NewTransactionEvent.Outputs.Any() ||
                    (txEvent.NewTransactionEvent.TransactionData.Transaction.RBF &&
                     txEvent.NewTransactionEvent.TransactionData.Confirmations == 0))
                {
                    return;
                }

                var relevantStates =
                    _payJoinStateProvider.Get(txEvent.CryptoCode, txEvent.NewTransactionEvent.DerivationStrategy);

                foreach (var relevantState in relevantStates)
                {
                    //if any of the exposed inputs were spent, remove them from our state
                    relevantState.PruneRecordsOfUsedInputs(txEvent.NewTransactionEvent.TransactionData.Transaction
                        .Inputs);
                }
            }));
            _ = BroadcastTransactionsPeriodically(cancellationToken);
            await loadCoins;
        }

        private async Task BroadcastTransactionsPeriodically(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await BroadcastStaleTransactions(BroadcastAfter, cancellationToken);
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }

        public async Task BroadcastStaleTransactions(TimeSpan broadcastAfter, CancellationToken cancellationToken)
        {
            List<Task> tasks = new List<Task>();
            foreach (var state in _payJoinStateProvider.GetAll())
            {
                var explorerClient = _explorerClientProvider.GetExplorerClient(state.Key.CryptoCode);
                //broadcast any transaction sent to us that we have proposed a payjoin tx for but has not been broadcasted after x amount of time.
                //This is imperative to preventing users from attempting to get as many utxos exposed from the merchant as possible.
                var staleTxs = state.Value.GetStaleRecords(broadcastAfter)
                    .Where(item => !item.TxSeen || item.Transaction.RBF);

                tasks.AddRange(staleTxs.Select(async staleTx =>
                {
                    //if the transaction signals RBF and was broadcasted, check if it was rbfed out
                    if (staleTx.TxSeen && staleTx.Transaction.RBF)
                    {
                        var proposedTransaction = await explorerClient.GetTransactionAsync(staleTx.ProposedTransactionHash, cancellationToken);
                        var result = await explorerClient.BroadcastAsync(proposedTransaction.Transaction, cancellationToken);
                        var accounted = result.Success ||
                                        result.RPCCode == RPCErrorCode.RPC_TRANSACTION_ALREADY_IN_CHAIN ||
                                        !(
                                            // Happen if a blocks mined a replacement
                                            // Or if the tx is a double spend of something already in the mempool without rbf
                                            result.RPCCode == RPCErrorCode.RPC_TRANSACTION_ERROR ||
                                            // Happen if RBF is on and fee insufficient
                                            result.RPCCode == RPCErrorCode.RPC_TRANSACTION_REJECTED);

                        if (accounted)
                        {
                            //if it wasn't replaced just yet, do not attempt to move the exposed coins to the priority list
                            return;
                        }
                    }
                    else
                    {
                        await explorerClient
                            .BroadcastAsync(staleTx.Transaction, cancellationToken);
                    }

                    
                    state.Value.RemoveRecord(staleTx, true);
                }));
            }

            await Task.WhenAll(tasks);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _payJoinStateProvider.SaveCoins();
            leases.Dispose();
        }
    }
}
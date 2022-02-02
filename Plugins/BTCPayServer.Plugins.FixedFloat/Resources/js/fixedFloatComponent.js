Vue.component("FixedFloat", {
    props: ["toCurrency", "toCurrencyDue", "toCurrencyAddress"], methods: {
    },
    data: function () {
        return {
            shown: false
        }
    }
    , computed: {
        url: function () {
            let settleMethodId = "";
            let amount = !this.$parent.srvModel.isUnsetTopUp ? this.toCurrencyDue : undefined;
            if (this.toCurrency.endsWith('LightningLike')) {
                settleMethodId = "BTCLN";
            } else {
                settleMethodId = this.toCurrency.toUpperCase();
            }
            const topup = this.$parent.srvModel.isUnsetTopUp;
            return "https://widget.fixedfloat.com/?" +
                "to=" +settleMethodId + 
                "&lockReceive=true&ref=fkbyt39c" +
                "&address="+this.toCurrencyAddress +
                (topup? "" : "&lockType=true&hideType=true&lockAmount=true&toAmount=" + this.toCurrencyDue  );

        }
    }
});

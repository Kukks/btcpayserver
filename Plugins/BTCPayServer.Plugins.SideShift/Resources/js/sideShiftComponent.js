Vue.component("SideShift", {
    props: ["toCurrency", "toCurrencyDue", "toCurrencyAddress"], methods: {
        openDialog: function (e) {
            if (e && e.preventDefault) {
                e.preventDefault();
            }
            let settleMethodId = "";
            let amount = !this.$parent.srvModel.isUnsetTopUp ? this.toCurrencyDue : undefined;
            if (this.toCurrency.toLowerCase() === "lbtc") {
                settleMethodId = "liquid";
            } else if (this.toCurrency.toLowerCase() === "usdt") {
                settleMethodId = "usdtla";
            } else if (this.toCurrency.endsWith('LightningLike')) {
                settleMethodId = "ln";
            } else {
                settleMethodId = this.toCurrency.toLowerCase();
            }
            window.__SIDESHIFT__ = {
                parentAffiliateId: "qg0OrfHJV",
                defaultSettleMethodId: settleMethodId,
                settleAddress: this.toCurrencyAddress,
                settleAmount: amount,
                type: !this.$parent.srvModel.isUnsetTopUp ? "fixed" : undefined
            };

            window.sideshift.show();
        }
    }
});

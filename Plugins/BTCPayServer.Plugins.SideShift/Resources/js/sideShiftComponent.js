Vue.component("SideShift",
    {
        props: ["toCurrency", "toCurrencyDue", "toCurrencyAddress"],
        methods: {
            openDialog: function (e) {
                if (e && e.preventDefault) {
                    e.preventDefault();
                }
                window.__SIDESHIFT__ = {
                    parentAffiliateId: "qg0OrfHJV",
                    defaultSettleMethodId: this.toCurrency,
                    settleAddress: this.toCurrencyAddress,
                    settleAmount:!this.$parent.srvModel.isUnsetTopUp?  this.toCurrencyDue: undefined,
                    type: !this.$parent.srvModel.isUnsetTopUp? "fixed": undefined
                };
                window.sideshift.show();
            }
        }
    });

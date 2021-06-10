Vue.component("SideShift" , 
    {
        props: ["toCurrency", "toCurrencyDue", "toCurrencyAddress"],
        methods: {
            openDialog: function (e) {
                if (e && e.preventDefault) {
                    e.preventDefault();
                }
                window.__SIDESHIFT__ = {
                    parentAffiliateId: YourAffiliateId,
                    defaultSettleMethodId: this.toCurrency,
                    settleAddress: this.toCurrencyAddress,
                    settleAmount: this.toCurrencyDue,
                    type: "fixed"
                };
            }
        }
    });

Vue.component("LNURLWithdrawContactless", {
    data: function () {
        return {
            supported: 'NDEFReader' in window,
            scanning: false,
            submitting: false
        }
    },
    methods: {
        startScan: async function () {
            try {
                if (this.scanning || this.submitting) {
                    return;
                }
                const self = this;
                self.submitting = false;
                self.scanning = true;
                if (!this.supported) {
                    const result = prompt("enter lnurl withdraw");
                    if (result) {
                        self.sendData.bind(self)(result);

                        self.scanning = false;
                        return;
                    }
                }
                ndef = new NDEFReader()
                await ndef.scan()

                ndef.addEventListener('readingerror', () => {
                    self.scanning = false;
                })

                ndef.addEventListener('reading', ({message, serialNumber}) => {
                    //Decode NDEF data from tag
                    const record = message.records[0]
                    const textDecoder = new TextDecoder('utf-8')
                    const lnurl = textDecoder.decode(record.data)

                    //User feedback, show loader icon
                    self.scanning = false;
                    self.sendData.bind(self)(lnurl);

                })
            } catch(e) {
                self.scanning = false;
                self.submitting = false;
            }
        },
        sendData: function (lnurl) {

            this.submitting = true;
            //Post LNURLW data to server
            var xhr = new XMLHttpRequest()
            xhr.open('POST', window.lnurlWithdrawSubmitUrl, true)
            xhr.setRequestHeader('Content-Type', 'application/json')
            xhr.send(JSON.stringify({lnurl, destination: this.$parent.srvModel.btcAddress}))
            const self = this;
            //User feedback, reset on failure
            xhr.onload = function () {
                if (xhr.readyState === xhr.DONE) {

                    console.log(xhr.response);
                    console.log(xhr.responseText);
                    self.scanning = false;
                    self.submitting = false;
                }
            }
        }
    }
});

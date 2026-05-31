(function () {
    class CookieConsent {
        constructor() {
            this.cookieConsentName = "CookieConsent";
            this.cookieHint = document.getElementById("cookie-hint");
            if (this.cookieHint) {
                if (!this.isCookieSet()) {
                    this.showCookieHint();
                }
            }
        }

        showCookieHint() {
            if (this.cookieHint) {
                this.cookieHint.style.display = 'block';
                const okButton = this.cookieHint.querySelector('.btn');
                if (okButton) {
                    okButton.style.display = 'inline-block';
                    okButton.addEventListener('click', e => this.setCookieConsent(e));
                }
            }
        }

        hideCookieHint() {
            if (this.cookieHint) {
                const okButton = this.cookieHint.querySelector('.btn');
                if (okButton) {
                    okButton.removeEventListener('click', e => this.setCookieConsent(e));
                }
                this.cookieHint.style.display = 'none';
            }
        }

        isCookieSet() {
            let rVal = document.cookie
                .split("; ")
                .find((row) => row.startsWith(this.cookieConsentName));
            return rVal;
        }

        setCookieConsent(event) {
            console.debug("cookie consent ok", event);

            event.preventDefault();

            if (!this.isCookieSet()) {
                const today = new Date(Date.now());
                const addDays = 365;
                const expires = new Date(today.getTime() + addDays * 24 * 60 * 60 * 1000).toGMTString();
                document.cookie =
                    this.cookieConsentName + "=true; path=/; SameSite=Lax; expires=" + expires + "; Secure";

                this.hideCookieHint();
            }
        }
    }
    const cookieConsent = new CookieConsent();
})();
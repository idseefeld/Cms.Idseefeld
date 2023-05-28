"use strict";

var _createClass = (function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; })();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

(function () {
    var CookieConsent = (function () {
        function CookieConsent() {
            _classCallCheck(this, CookieConsent);

            this.cookieConsentName = "CookieConsent";
            this.cookieHint = document.getElementById("cookie-hint");
            if (this.cookieHint) {
                if (!this.isCookieSet()) {
                    this.showCookieHint();
                }
            }
        }

        _createClass(CookieConsent, [{
            key: "showCookieHint",
            value: function showCookieHint() {
                var _this = this;

                if (this.cookieHint) {
                    this.cookieHint.style.display = 'block';
                    var okButton = this.cookieHint.querySelector('.btn');
                    if (okButton) {
                        okButton.style.display = 'inline-block';
                        okButton.addEventListener('click', function (e) {
                            return _this.setCookieConsent(e);
                        });
                    }
                }
            }
        }, {
            key: "hideCookieHint",
            value: function hideCookieHint() {
                var _this2 = this;

                if (this.cookieHint) {
                    var okButton = this.cookieHint.querySelector('.btn');
                    if (okButton) {
                        okButton.removeEventListener('click', function (e) {
                            return _this2.setCookieConsent(e);
                        });
                    }
                    this.cookieHint.style.display = 'none';
                }
            }
        }, {
            key: "isCookieSet",
            value: function isCookieSet() {
                var _this3 = this;

                var rVal = document.cookie.split("; ").find(function (row) {
                    return row.startsWith(_this3.cookieConsentName);
                });
                return rVal;
            }
        }, {
            key: "setCookieConsent",
            value: function setCookieConsent(event) {
                console.debug("cookie consent ok", event);

                event.preventDefault();

                if (!this.isCookieSet()) {
                    var today = new Date(Date.now());
                    var addDays = 365;
                    var expires = new Date(today.getTime() + addDays * 24 * 60 * 60 * 1000).toGMTString();
                    document.cookie = this.cookieConsentName + "=true; path=/; SameSite=Lax; expires=" + expires + "; Secure";

                    this.hideCookieHint();
                }
            }
        }]);

        return CookieConsent;
    })();

    var cookieConsent = new CookieConsent();
})();


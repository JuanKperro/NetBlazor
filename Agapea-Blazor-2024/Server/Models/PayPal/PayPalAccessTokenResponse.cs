namespace Agapea_netcore_mvc_23_24.Models.PayPal
{
    //clase q va a  mapear respuesta de PAYPAL cuando objeto TOKEN de servicio para poder hacer pet.a su REST API
    //el formato es: https://developer.paypal.com/api/rest/#link-getaccesstoken
    /*
        {
          "scope": "https://uri.paypal.com/services/invoicing https://uri.paypal.com/services/disputes/read-buyer https://uri.paypal.com/services/payments/realtimepayment https://uri.paypal.com/services/disputes/update-seller https://uri.paypal.com/services/payments/payment/authcapture openid https://uri.paypal.com/services/disputes/read-seller https://uri.paypal.com/services/payments/refund https://api-m.paypal.com/v1/vault/credit-card https://api-m.paypal.com/v1/payments/.* https://uri.paypal.com/payments/payouts https://api-m.paypal.com/v1/vault/credit-card/.* https://uri.paypal.com/services/subscriptions https://uri.paypal.com/services/applications/webhooks",
          "access_token": "A21AAFEpH4PsADK7qSS7pSRsgzfENtu-Q1ysgEDVDESseMHBYXVJYE8ovjj68elIDy8nF26AwPhfXTIeWAZHSLIsQkSYz9ifg",
          "token_type": "Bearer",
          "app_id": "APP-80W284485P519543T",
          "expires_in": 31668,
          "nonce": "2020-04-03T15:35:36ZaYZlGvEkV4yVSz8g6bAKFoGSEzuy3CQcz3ljhibkOHg"
        }     
     */
    public class PayPalAccessTokenResponse
    {
        #region ...propiedades de la clase ...
        public String scope { get; set; }
        public String access_token { get; set; }
        public String token_type { get; set; }
        public String app_id { get; set; }
        public Int32 expires_in { get; set; }
        public String nonce { get; set; }

        #endregion
    }
}

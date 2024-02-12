namespace Agapea_netcore_mvc_23_24.Models.PayPal
{
    //clase q va a mapear objeto JSON de respuesta de PAYPAL  cuando creo una orden de pago para un pedido
    /*
        {
            "id": "5O190127TN364715T",
            "status": "PAYER_ACTION_REQUIRED",
            "payment_source": {
            "paypal": { }
            },
            "links": [
                        {
                            "href": "https://api-m.paypal.com/v2/checkout/orders/5O190127TN364715T",
                            "rel": "self",
                            "method": "GET"
                        },
                        {
                            "href": "https://www.paypal.com/checkoutnow?token=5O190127TN364715T",
                            "rel": "payer-action",
                            "method": "GET"
                        }
            ]
}     
     */
    public class PayPalCreateOrderResponse
    {
        #region ....propiedades de clase....
        public string id { get; set; }
        public List<PayPalOrderLinks> links { get; set; }
        #endregion
    }

    public class PayPalOrderLinks {

        #region ...propiedades de clase...
        public string href { get; set; }
        public string rel { get; set; }
        public string method { get; set; }

        #endregion
    }
}

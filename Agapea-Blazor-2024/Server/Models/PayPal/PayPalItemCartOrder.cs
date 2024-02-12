namespace Agapea_netcore_mvc_23_24.Models.PayPal
{
    //clase q mapea objeto item q necesita un Order de paypal en array purchase_items.items
    //https://developer.paypal.com/docs/api/orders/v2/#orders_create!path=purchase_units/items&t=request
    public class PayPalItemCartOrder
    {
        #region ...propieades de clase....
        public string name { get; set; }
        public string quantity { get; set; }
        public UnitAmount unit_amount { get; set; } = new UnitAmount();

        #endregion
    }


    public class UnitAmount {
        public String  currency_code { get; set; }
        public String value { get; set; }
    }
}

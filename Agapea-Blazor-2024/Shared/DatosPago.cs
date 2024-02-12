using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgapeaBlazor2024.Shared
{
    public class DatosPago
    {
        #region ...propiedades clase ....
        //datos de envio
        public Direccion DireccionPrincipal { get; set; } = new Direccion();

        public bool esPrincipal_envio = false;
        public Direccion DireccionEnvio { get; set; } = new Direccion();

        public bool esEnvio_fac = false;
        public Direccion ?DireccionFacturacion { get; set; } = new Direccion();

        public bool esEnvio_otroDestinatario = false;
        public String NombreDestinatario { get; set; } = "";
        public String ApellidosDestinatario { get; set; } = "";
        public String TelefonoDestinatario { get; set; } = "";
        public String EmailDestinatario { get; set; } = "";

        //datos facturacion
        public String?NombreFactura { get; set; } = "";
        public String? DocFiscalFactura { get; set; } = "";

        //datos pago
        public String metodoPago { get; set; } = "pagotarjeta";
        public String NumeroTarjeta { get; set; } = "";
        public String NombreBanco { get; set; } = "";
        public int MesCaducidad { get; set; } = 0;
        public int AnioCaducidad { get; set; } = 0;
        public int CVV { get; set; } = 0;

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgapeaBlazor2024.Shared
{
    public class RestMessage
    {
        //clase q mapea respuesta de nuestros servicios RESTFULL creados en el server asp.net core
        #region ....propiedades clase RestMessage...
        public int Codigo { get; set; }
        public String Mensaje { get; set; } = "";
        public String Error { get; set; } = "";
        public String? TokenSesion { get; set; } = "";
        public Cliente? DatosCliente { get; set; }
        public Object? OtrosDatos {  get; set; }

        #endregion
    }
}

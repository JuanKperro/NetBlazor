namespace AgapeaBlazor2024.Server.Models
{
    public interface IClienteCorreo
    {
        #region ...propiedades de la interface que deben implementar servicios para mandar emails...
        //como propiedades: Usuario con el que te conectas al servicio proveedor externo de correo
        //                  Password con la que te autentificas ante el servicio externo para poder mandar correos
        public String UserId { get; set; }
        public String Key { get; set; }

        #endregion

        #region ...metodos de la interface que deben implementar servicios para mandar emails...
        public bool EnviarEmail(String emailcliente, String subject, String cuerpoMensaje, String? ficheroAdjunto);

        public Task EnviarEmailMailChimp(string emailcliente, string subject, string cuerpoMensaje);
        #endregion


    }
}

using AgapeaBlazor2024.Shared;

namespace AgapeaBlazor2024.Client.Models.Services.Interfaces
{
    public interface IRestService
    {
        //interface que sirve como patron para definir servicios a inyectar en Program.cs para hacer
        //pet. ajax a servicios REST externos...
        #region ....metodos/props para zona cliente....
        Task<RestMessage> RegistrarCliente(Cliente nuevoCliente);
        Task<RestMessage> LoginCliente(Cuenta credenciales);
        Task<RestMessage> LoginCliente(string idcliente);

        Task<RestMessage> ActualizarDatosCliente(Cliente cliente);

        Task<RestMessage> subirImagen(string imagen);

        Task<RestMessage> AplicarCambioContraseña(string idCliente, string token, string pass);

        Task<RestMessage> EnviarOpinion(Opinion opinionAguardar);

        Task<List<Opinion>> RecuperarOpiniones(string isbn13);

        Task<RestMessage> OperarOpinion(Dictionary<string, string> datosOperacion);

        #endregion


        #region ....metodos/props para zona Tienda.....
        Task<List<Libro>> RecuperarLibros(String idcat);
        Task<Libro> RecuperarUnLibro(String isbn13);
        Task<List<Categoria>> RecuperarCategorias(String idcat);
        Task<List<Provincia>> RecuperarProvincias();
        Task<List<Municipio>> RecuperarMunicipios(String codpro);

        Task<RestMessage> FinalizarPedido(DatosPago pago, Pedido nuevoPedido);

        #endregion
    }
}

using AgapeaBlazor2024.Shared;

namespace AgapeaBlazor2024.Client.Models.Services.Interfaces
{
    public interface IStorageService
    {

        //prop.publica q solo usa servicio indexedDB para op.asincronas y notificar recepcion de datos....
        public event EventHandler<Cliente> ClienteRecupIndexedDBEvent;




        #region ..... metodos SINCRONOS almacenamiento valores en servicios storage... (OBSERVABLES)
        void AlmacenamientoDatosCliente(Cliente datoscliente);
        void AlmacenamientoJWT(String jwt);
        Cliente RecuperarDatosCliente();
        String RecuperarJWT();

        List<ItemPedido> RecuperarItemsPedido();
        void OperarItemsPedido(Libro libro, int cantidad, String operacion);

        #endregion


        #region ... metodos ASINCRONOS almacenamiento valores en servicios storage....(LOCALSTORAGE, INDEXEDDB,...)
        Task AlmacenamientoDatosClienteAsync(Cliente datoscliente);
        Task AlmacenamientoJWTAsync(String jwt);

        Task<Cliente> RecuperarDatosClienteAsync();
        Task<String> RecuperarJWTAsync();

        Task<List<ItemPedido>> RecuperarItemsPedidoAsync();
        Task OperarItemsPedidoAsync (Libro libro, int cantidad, String operacion);


        #endregion
    }
}

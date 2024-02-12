using AgapeaBlazor2024.Client.Models.Services.Interfaces;
using AgapeaBlazor2024.Shared;
using Microsoft.JSInterop;

namespace AgapeaBlazor2024.Client.Models.Services
{
    public class IndexedDBService : IStorageService
    {

        #region .... propiedades clase servicio almacenamiento datos en INDEXED-DB...
        private IJSRuntime _jsService;
        private DotNetObjectReference<IndexedDBService> _refIndexedService;

        //prop.de tipo EVENTO q vamos a usar para notificar a aquellos componentes q usen el servicio
        //q se han recuperado desde JS (indexedDB) los datos del cliente asincronos...y ya estan disponibles
        public event EventHandler<Cliente> ClienteRecupIndexedDBEvent;
        #endregion

        public IndexedDBService(IJSRuntime jsServiceDI)
        {
                this._jsService = jsServiceDI;
                this._refIndexedService = DotNetObjectReference.Create(this);
        }


        #region ....metodos clase servicio almacenamiento datos en INDEXED-DB....

        public async Task AlmacenamientoDatosClienteAsync(Cliente datoscliente)
        {
            await this._jsService.InvokeVoidAsync("adminIndexedDB.almacenarDatosCliente", datoscliente);
        }


        public async Task AlmacenamientoJWTAsync(string jwt)
        {
            await this._jsService.InvokeVoidAsync("adminIndexedDB.almacenarToken", jwt);
        }


        public async Task<Cliente> RecuperarDatosClienteAsync()
        {
            //a la funcion de javascript recuperarDatosCliente de indexedDB le paso la ref.del servicio para que
            //cuando acabe su ejecucion (es asincrono, no se lo q va a tardar...) llame a metodo de este servicio...
            return await this._jsService.InvokeAsync<Cliente>(
                                        "adminIndexedDB.recuperarDatosCliente", 
                                        this._refIndexedService
                                        //, ¿¿ y el email?? <------ falta 2º parametro para recuperar desde indexedDB datoscliente
                                        );
        }


        public async Task<string> RecuperarJWTAsync()
        {
            return await this._jsService.InvokeAsync<string>(
                                        "adminIndexedDB.recuperarTokenCliente",
                                        this._refIndexedService
                                        //, ¿¿ y el email?? <------ falta 2º parametro para recuperar desde indexedDB JWT de ese cliente
                                        );
        }

        //metodo invocable desde codigo JS (nuestro fichero: ManageIndexedBD.js)
        [JSInvokable("CallbackServIndexedDBblazor")]
        public void CallFromJS(Cliente clienteIndexedDB)
        {
            //cuando recibo datos del cliente desde JS, disparo evento (notifico a quien este escuchando)...
            this.ClienteRecupIndexedDBEvent.Invoke(this, clienteIndexedDB);
        }

        [JSInvokable("CallbackServIndexedDBblazorJWT")]
        public void CallFromJS2(String jwt)
        {

        }



        #region //// metodos sincronos no utitlizables /////
        public void AlmacenamientoDatosCliente(Cliente datoscliente)
        {
            throw new NotImplementedException();
        }
        public void AlmacenamientoJWT(string jwt)
        {
            throw new NotImplementedException();
        }
        public Cliente RecuperarDatosCliente()
        {
            throw new NotImplementedException();
        }
        public string RecuperarJWT()
        {
            throw new NotImplementedException();
        }

        public List<ItemPedido> RecuperarItemsPedido()
        {
            throw new NotImplementedException();
        }

        public void OperarItemsPedido(Libro libro, int cantidad, string operacion)
        {
            throw new NotImplementedException();
        }

        public Task<List<ItemPedido>> RecuperarItemsPedidoAsync()
        {
            throw new NotImplementedException();
        }

        public Task OperarItemsPedidoAsync(Libro libro, int cantidad, string operacion)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

    }
}

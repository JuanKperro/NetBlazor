using AgapeaBlazor2024.Client.Models.Services.Interfaces;
using AgapeaBlazor2024.Shared;
using Microsoft.JSInterop;

namespace AgapeaBlazor2024.Client.Models.Services
{
    public class LocalStorageService : IStorageService
    {
        #region ...propiedades servicio LocalStorage....
        private IJSRuntime _jsService;

        public event EventHandler<Cliente> ClienteRecupIndexedDBEvent;
        #endregion

        public LocalStorageService(IJSRuntime jsServiceDI)
        {
                this._jsService = jsServiceDI;
        }




        #region ...metodos servicio LocalStorage....

        #region ///metodos sincronos (sin uso en este caso) ////
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

        #endregion

        public async Task AlmacenamientoDatosClienteAsync(Cliente datoscliente)
        {
            //tendria q ejecutar esto desde javascript: localStorage.setItem("datoscliente", JSON.stringify(datoscliente))
            await this._jsService.InvokeVoidAsync("adminLocalStorage.almacenarValor", "datoscliente", datoscliente);
        }


        public async Task AlmacenamientoJWTAsync(string jwt)
        {
            await this._jsService.InvokeVoidAsync("adminLocalStorage.almacenarValor", "JWT", jwt);
        }


        public async Task<Cliente> RecuperarDatosClienteAsync()
        {
            return await this._jsService.InvokeAsync<Cliente>("adminLocalStorage.recuperarValor", "datoscliente");
        }


        public async Task<string> RecuperarJWTAsync()
        {
            return await this._jsService.InvokeAsync<string>("adminLocalStorage.recuperarValor", "JWT");
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
    }
}

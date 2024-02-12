using AgapeaBlazor2024.Client.Models.Services.Interfaces;
using AgapeaBlazor2024.Shared;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;



namespace AgapeaBlazor2024.Client.Models.Services
{
    public class MiRestService : IRestService
    {
        #region ....propiedades del servicio....
        private HttpClient _httpClient;
        private IStorageService _storageSvc;
        #endregion

        public MiRestService(HttpClient httpClientDI, IStorageService storageSvc)
        {
            this._httpClient = httpClientDI;
            this._storageSvc = storageSvc;

        }




        #region ...metodos del servicio ....

        #region /////////// llamada endpoints zona Cliente ////////////////
        async Task<RestMessage> IRestService.LoginCliente(Cuenta credenciales)
        {
            HttpResponseMessage _resp = await this._httpClient.PostAsJsonAsync<Cuenta>("/api/RESTCliente/LoginCliente", credenciales);
            RestMessage _bodyresp = await _resp.Content.ReadFromJsonAsync<RestMessage>();
            return _bodyresp;
        }

        async Task<RestMessage> IRestService.LoginCliente(string idcliente)
        {
            HttpResponseMessage _resp = await this._httpClient.GetAsync($"/api/RESTCliente/LoginCliente?idcliente={idcliente}");
            RestMessage _bodyresp = await _resp.Content.ReadFromJsonAsync<RestMessage>();
            return _bodyresp;
        }

        async Task<RestMessage> IRestService.RegistrarCliente(Cliente nuevoCliente)
        {
            HttpResponseMessage _resp = await this._httpClient.PostAsJsonAsync<Cliente>("/api/RESTCliente/RegistrarCliente", nuevoCliente);
            RestMessage _bodyresp = await _resp.Content.ReadFromJsonAsync<RestMessage>();
            return _bodyresp;
        }

        async Task<RestMessage> IRestService.ActualizarDatosCliente(Cliente clienteupdate)
        {
            String _jwt = this._storageSvc.RecuperarJWT();
            this._httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwt);

            string jsoncli = JsonSerializer.Serialize<Cliente>(clienteupdate);
            HttpResponseMessage _resp = await this._httpClient.PostAsJsonAsync<string>("/api/RESTCliente/ActualizarCliente", jsoncli);
            RestMessage _bodyresp = await _resp.Content.ReadFromJsonAsync<RestMessage>();
            return _bodyresp;
        }

        public async Task<RestMessage> subirImagen(string imagen)
        {
            String _jwt = this._storageSvc.RecuperarJWT();
            this._httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwt);

            HttpResponseMessage httresp = await this._httpClient.PostAsJsonAsync<string>("/api/RESTCliente/subirImagen", imagen);

            return await httresp.Content.ReadFromJsonAsync<RestMessage>();
        }

        public async Task<RestMessage> AplicarCambioContraseña(string idCliente, string token, string pass)
        {

            Dictionary<string, string> datos = new Dictionary<string, string>()
            {
                { "idCliente", idCliente },
                { "token", token },
                { "pass", pass }
            };

            HttpResponseMessage resp = await this._httpClient.PostAsJsonAsync<Dictionary<string, string>>("/api/RESTCliente/AplicarCambioContraseña", datos);

            return await resp.Content.ReadFromJsonAsync<RestMessage>();
        }

        public async Task<RestMessage> EnviarOpinion(Opinion opinionAguardar)
        {
            string token = this._storageSvc.RecuperarJWT();
            this._httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Baerer", $"{token}");
            HttpResponseMessage resp = await this._httpClient.PostAsJsonAsync<Opinion>("api/RESTCliente/GuardarOpinion", opinionAguardar);
            return await resp.Content.ReadFromJsonAsync<RestMessage>();

        }

        public async Task<List<Opinion>> RecuperarOpiniones(string isbn13)
        {
            HttpResponseMessage resp = await this._httpClient.GetAsync($"api/RESTTienda/RecuperarOpiniones?isbn={isbn13}");
            return await resp.Content.ReadFromJsonAsync<List<Opinion>>();

        }


        public async Task<RestMessage> OperarOpinion(Dictionary<string, string> datosOperacion)
        {
            HttpResponseMessage resp = await this._httpClient.PostAsJsonAsync<Dictionary<string, string>>("api/RESTCliente/OperarOpinion", datosOperacion);
            return await resp.Content.ReadFromJsonAsync<RestMessage>();
        }
        #endregion


        #region /////////// llamada endpoints zona Tienda ////////////////
        public async Task<List<Categoria>> RecuperarCategorias(string idcat)
        {
            if (String.IsNullOrEmpty(idcat)) idcat = "raices";
            return await this._httpClient
                            .GetFromJsonAsync<List<Categoria>>($"/api/RESTTienda/RecuperarCategorias?idcat={idcat}") ?? new List<Categoria>();
        }

        public async Task<List<Libro>> RecuperarLibros(string idcat)
        {
            return await this._httpClient
                            .GetFromJsonAsync<List<Libro>>($"/api/RESTTienda/RecuperarLibros?idcat={idcat}") ?? new List<Libro>();
        }

        public async Task<Libro> RecuperarUnLibro(string isbn13)
        {
            return await this._httpClient
                            .GetFromJsonAsync<Libro>($"/api/RESTTienda/RecuperarUnLibro?isbn13={isbn13}") ?? new Libro();
        }

        public async Task<List<Provincia>> RecuperarProvincias()
        {
            return await this._httpClient.GetFromJsonAsync<List<Provincia>>("/api/RESTTienda/RecuperarProvincias") ?? new List<Provincia>();

        }

        public async Task<List<Municipio>> RecuperarMunicipios(string codpro)
        {
            return await this._httpClient.GetFromJsonAsync<List<Municipio>>($"/api/RESTTienda/RecuperarMunicipios?codpro={codpro}") ?? new List<Municipio>();

        }

        public async Task<RestMessage> FinalizarPedido(DatosPago datosPago, Pedido nuevoPedido)
        {

            Dictionary<string, string> datos = new Dictionary<string, string>() {
                { "DatosPago", JsonSerializer.Serialize<DatosPago>(datosPago) },
                { "Pedido", JsonSerializer.Serialize<Pedido>(nuevoPedido) }
            };

            this._httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", "Bearer:" + _storageSvc.RecuperarJWTAsync);

            HttpResponseMessage resp = await this._httpClient.PostAsJsonAsync<Dictionary<string, string>>
                ("/api/RESTTienda/FinalizarPedido", datos);

            return await resp.Content.ReadFromJsonAsync<RestMessage>();

        }


        #endregion

        #endregion
    }
}

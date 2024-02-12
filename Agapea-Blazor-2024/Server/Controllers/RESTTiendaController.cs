using Agapea_netcore_mvc_23_24.Models.PayPal;
using AgapeaBlazor2024.Server.Models;
using AgapeaBlazor2024.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


using AgapeaBlazor2024.Server.Models.PayPal;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Routing;


namespace AgapeaBlazor2024.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RESTTiendaController : ControllerBase
    {
        #region ...propiedades servicio RESTFULL control endpoints Tienda....
        private AplicacionDBContext _dbContext; //<---variable encapsula obj. AplicationDbContext definido en Program.cs como objeto inyectable...
        private IConfiguration _accesoAppSettings;
        private HttpClient _clienteHttp;
        private UserManager<MiClienteIdentity> _userManagerService;
        private SignInManager<MiClienteIdentity> _signInManagerService;
        #endregion

        public RESTTiendaController(UserManager<MiClienteIdentity> userManagerDI,
                                     SignInManager<MiClienteIdentity> signinManagerDI,
                                     AplicacionDBContext dbContext, IConfiguration accesoAppSettingsDI)
        {
            this._dbContext = dbContext;
            this._accesoAppSettings = accesoAppSettingsDI;
            this._clienteHttp = new HttpClient();
            this._userManagerService = userManagerDI;
            this._signInManagerService = signinManagerDI;
        }


        #region ...metodos servicio RESTFULL control endpoints Tienda...
        [HttpGet]
        public List<Categoria> RecuperarCategorias([FromQuery] String idcat)
        {
            try
            {
                //si en idcat esta vacio, quiero recuperar categorias "raiz" IdCategoria="un digito"
                //si no, quiero recuperar subcategorias de una categoria q pasan:  IdCategoria=idcat-"digito"
                Regex _patronBusqueda;
                if (String.IsNullOrEmpty(idcat) || idcat == "raices")
                {
                    _patronBusqueda = new Regex("^[0-9]{1,}$"); //<<---- "^\d+$"
                }
                else
                {
                    _patronBusqueda = new Regex("^" + idcat + "-[0-9]{1,}$");
                }
                //si usas patrones directamente en la consulta LINQ al intentar traducir esta consulta a lenguaje SQL
                //en SqlServer, como no hay operadores de tipo expresion Regular no puede convertirlo y zzzzzzzzasssss
                //excepcion....¿solucion?

                //return this._dbContext
                //            .Categorias
                //            .Where(
                //                    (Categoria unacat) => _patronBusqueda.IsMatch(unacat.IdCategoria)
                //                )
                //            .ToList<Categoria>();



                //dos opciones:
                // - el operador LIKE si existe en sqlserver <---- se mapea contra metodo .Contains() de LINQ
                // - te descargas tooooooooooooda la tabla en memoria y luego lo filtras con op.linq
                //    para hacer esto usas el metodo .AsEnumerable() tras el nombre del DbSet
                return this._dbContext
                            .Categorias
                            .AsEnumerable<Categoria>() //<--- SELECT * FROM dbo.Categorias y por cada fila construye objeto Categoria
                            .Where(
                                    (Categoria unacat) => _patronBusqueda.IsMatch(unacat.IdCategoria)
                                )
                            .ToList<Categoria>();

            }
            catch (Exception ex)
            {
                return new List<Categoria>();
            }
        }

        [HttpGet]
        public Libro RecuperarUnLibro([FromQuery] String isbn13)
        {
            try
            {
                return this._dbContext
                            .Libros
                            .Where(
                                    (Libro unlibro) => unlibro.ISBN13 == isbn13
                                    )
                            .Single<Libro>();
            }
            catch (Exception ex)
            {

                return new Libro();
            }
        }

        [HttpGet]
        public List<Libro> RecuperarLibros([FromQuery] String idcat)
        {
            try
            {
                //si idcat esta vacio, recuperas libros en oferta, libros mas vendidos del mes, libros de una festividad...
                //yo por defecto, libros de categoria '2-10'
                if (String.IsNullOrEmpty(idcat)) idcat = "2-10";

                //para hacer la query SELECT sobre tabla Libros, necesito el  DbSet<Libro> Libros de AplicacionDbContext
                //y tienes q usar LINQ para hacer la query
                return this._dbContext
                             .Libros
                             .Where((Libro unlibro) => unlibro.IdCategoria.StartsWith(idcat))
                             .ToList<Libro>();

            }
            catch (Exception ex)
            {

                return new List<Libro>();
            }
        }

        [HttpGet]
        public List<Provincia> RecuperarProvincias()
        {
            try
            {
                return this._dbContext.Provincias.AsEnumerable<Provincia>().OrderBy((Provincia p) => p.PRO).ToList<Provincia>();
            }
            catch (Exception ex)
            {

                return new List<Provincia>();
            }
        }

        [HttpGet]
        public List<Municipio> RecuperarMunicipios([FromQuery] String codpro)
        {
            try
            {
                return this._dbContext.Municipios.Where((Municipio muni) => muni.CPRO == codpro).ToList<Municipio>();
            }
            catch (Exception ex)
            {

                return new List<Municipio>();
            }
        }

        [HttpPost]
        public async Task<RestMessage> FinalizarPedido([FromBody] Dictionary<string, string> datos, [FromHeader(Name = "Authorization")] string jwt)
        {
            try
            {
                Console.WriteLine("JWT: " + jwt);

                DatosPago datosPago = JsonSerializer.Deserialize<DatosPago>(datos["DatosPago"]);
                Pedido nuevoPedido = JsonSerializer.Deserialize<Pedido>(datos["Pedido"]);

                bool _pagoOK = false;
                string url_encaminar = "https://localhost:7262/Cliente/PanelCliente";
                //0º hacer el pago, a "Mis Compras"
                if (datosPago.metodoPago == "pagotarjeta")
                {
                    //pago stripe
                    if (!await PagoStripe(datosPago, nuevoPedido))
                    {
                        throw new Exception("Error en el pago con Stripe...");
                    }

                }
                else
                {
                    // pago paypal
                    url_encaminar = await PagoPaypal(datosPago, nuevoPedido);
                    if (String.IsNullOrEmpty(url_encaminar))
                    {
                        throw new Exception("Error en el pago con PayPal...");
                    }
                    else
                    {
                        _pagoOK = true;
                    }
                }
                

                 this._dbContext.Pedidos.Add(nuevoPedido);
                 this._dbContext.SaveChanges();
                
                //1º almacenar en BD si hay direcciones nuevas de envio y/o facturacion
                if (!datosPago.esPrincipal_envio)
                {
                    Direccion direccionEnvioaGuardar = datosPago.DireccionEnvio;
                    direccionEnvioaGuardar.IdCliente = nuevoPedido.IdCliente;
                    this._dbContext.Direcciones.Add(direccionEnvioaGuardar);
                }
                if (!String.IsNullOrEmpty(datosPago.DireccionFacturacion.Calle) && !datosPago.esEnvio_fac)
                {
                    Direccion direccionFacturacionaGuardar = datosPago.DireccionFacturacion;
                    direccionFacturacionaGuardar.IdCliente = nuevoPedido.IdCliente;
                    this._dbContext.Direcciones.Add(direccionFacturacionaGuardar);
                }
                this._dbContext.SaveChanges();
                //2º generacion factura en pdf y envio de email al cliente con factura y almacenar pedido en BD <---- lo hago en metodo de accion FinalizarPedidoOK q se lanza si el pago ha ido ok


                //3º redirigir al cliente segun el metodo de pago 
                //y devolver un mensaje de exito o fracaso...
                return new RestMessage
                {
                    Codigo = 0,
                    Mensaje = url_encaminar,

                };
            }
            catch (Exception ex)
            {
                return new RestMessage
                {
                    Codigo = 1,
                    Mensaje = "Pedido  con errores",
                    Error = ex.Message,
                    OtrosDatos = null
                };
            }
        }

        [HttpGet]
        public async Task<ActionResult> PayPalCallBack(
                                       [FromQuery] string idcliente,
                                       [FromQuery] string idpedido,
                                       [FromQuery] Boolean? cancel,
                                       [FromQuery] string token,
                                       [FromQuery] string PayerID
                                       )
        {
            try
            {
                //1.paso volver a obtener un token de accesi a la api de paypal
                string _accessToken = await this.GetAccessTokenPAYPAL(
                            this._accesoAppSettings["PayPalAPIKEYS:ClientId"],
                            this._accesoAppSettings["PayPalAPIKEYS:ClientSecret"]
                        );
                //2.Completar la orden en paypal...
                PedidoPayPal _pedidoPayPal = this._dbContext.PedidosPayPal.Where((PedidoPayPal ped) => ped.idCliente == idcliente && ped.idPedido == idpedido).Single<PedidoPayPal>();

                HttpRequestMessage _reqOrderCapture = new HttpRequestMessage(HttpMethod.Post, $"https://api.sandbox.paypal.com/v2/checkout/orders/{_pedidoPayPal.idPedidoPaypal}/capture");
                _reqOrderCapture.Headers.Add("Authorization", $"Bearer {_accessToken}");
                HttpResponseMessage _respOrderCapture = await this._clienteHttp.SendAsync(_reqOrderCapture);
                if (_respOrderCapture.IsSuccessStatusCode)
                {
                    //3.almacenar en BD el pedido como pagado
                    Pedido _pedido = this._dbContext.Pedidos.Where((Pedido ped) => ped.IdPedido == idpedido).Single<Pedido>();
                    _pedido.EstadoPedido = "pagado";

                    this._dbContext.SaveChanges();
                    String _respJSONSerializada = await _respOrderCapture.Content.ReadAsStringAsync();
                    JsonNode jsonNode = JsonNode.Parse(_respJSONSerializada);
                    //4. redirigir al cliente a la vista de "Mis Compras"
                    jsonNode["status"].ToString();
                    return Redirect($"https://localhost:7262/Tienda/FinalizarPedidoOk?idcliente={idcliente}&?idpedido={idpedido}");
                }
                else
                {
                    throw new Exception("Error en la captura de la orden en PayPal");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Redirect($"https://localhost:7262/Tienda/FinalizarPedidoOk?idpedido={idpedido}");
            }



            /*
             * 
             formato url de vuelta de paypal:
                https://localhost:7262/api/RESTTienda/PaypalCallBack ? idcliente=da40e3ea-a602-4f9c-a544-2e590e7bf508 & 
                                                                       idpedido=883f2fbf-0afc-4211-8df2-aed2696bffac & 
                                                                       token=31V84989DC5331204 & 
                                                                       PayerID=W59CS27V2S8US
             */


        }

        [HttpGet]
        public async Task<List<Opinion>> RecuperarOpiniones([FromQuery] string isbn)
        {
            List<Opinion> opinions = new List<Opinion>();
            opinions = this._dbContext.Opiniones.Where((Opinion opi) => opi.IdLibro == isbn & opi.Aprobada).ToList<Opinion>();
            return opinions;
        }

        #endregion

        #region ...METODOS PRIVADOS DE TIENDA CONTROLLER...
        private async Task<bool> PagoStripe(DatosPago datosPago, Pedido pedidoActual)
        {
            try
            {

                Direccion dirEnvio = datosPago.DireccionEnvio;

                string _claveSTRIPE = this._accesoAppSettings["StripeAPIKeys:ClaveAPI"];

                Dictionary<string, string> customerStripeValues = new Dictionary<string, string> {
                { "name", datosPago.NombreDestinatario + " " + datosPago.ApellidosDestinatario },
                { "email", datosPago.EmailDestinatario },
                { "phone", datosPago.TelefonoDestinatario },
                { "address[city]", dirEnvio.MunicipioDirec.DMUN50 },
                { "address[state]", dirEnvio.ProvinciaDirec.PRO},
                { "address[country]", dirEnvio.Pais },
                { "address[postal_code]", dirEnvio.CP.ToString() },
                { "address[line1]", dirEnvio.Calle }
            };
                HttpRequestMessage _request = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/customers");
                _request.Headers.Add("Authorization", $"Bearer {_claveSTRIPE}");
                _request.Content = new FormUrlEncodedContent(customerStripeValues);

                HttpResponseMessage _respuesta = await this._clienteHttp.SendAsync(_request);

                if (_respuesta.IsSuccessStatusCode)
                {
                    JsonNode _respJSON = JsonNode.Parse(await _respuesta.Content.ReadAsStringAsync());

                    //recupero prop."id" del objeto json devuelto por STRIPE sin deserializar a una clase propia
                    //https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/deserialization?pivots=dotnet-8-0#deserialize-without-a-net-class
                    //o usas JsonDocument o JSON-DOM <---mas rapido, permite JPath

                    string _customerId = _respJSON["id"].ToString();

                    //--------------------------------------------------------------------
                    //2º paso) crear Card asociada al customer-id q me devuelve stripe...
                    //https://stripe.com/docs/api/cards/create?lang=curl
                    //---------------------------------------------------------------------
                    Dictionary<string, string> _cardOptions = new Dictionary<string, string> { { "source", "tok_visa" } };

                    HttpRequestMessage _request2 = new HttpRequestMessage(HttpMethod.Post, $"https://api.stripe.com/v1/customers/{_customerId}/sources");
                    _request2.Headers.Add("Authorization", $"Bearer {_claveSTRIPE}");
                    _request2.Content = new FormUrlEncodedContent(_cardOptions);

                    HttpResponseMessage _respuesta2 = await _clienteHttp.SendAsync(_request2);
                    if (_respuesta2.IsSuccessStatusCode)
                    {
                        JsonNode _cardJSON = JsonNode.Parse(await _respuesta2.Content.ReadAsStringAsync());
                        string _cardId = _cardJSON["id"].ToString();

                        //------------------------------------------------------------------------
                        //3º paso) crear un charge con el customer-id(1ºpaso) y el card-id(2ºpaso)
                        //https://stripe.com/docs/api/charges/create?lang=curl
                        //------------------------------------------------------------------------
                        Dictionary<string, string> _chargeValues = new Dictionary<string, string> {
                        { "customer", _customerId },
                        { "source", _cardId },
                        { "currency", "eur" },
                        { "amount", (System.Convert.ToInt64(pedidoActual.Total) * 100).ToString() },
                        { "description", pedidoActual.IdPedido }
                    };

                        HttpRequestMessage _request3 = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/charges");
                        _request3.Headers.Add("Authorization", $"Bearer {_claveSTRIPE}");
                        _request3.Content = new FormUrlEncodedContent(_chargeValues);

                        HttpResponseMessage _respuesta3 = await this._clienteHttp.SendAsync(_request3);
                        if (_respuesta3.IsSuccessStatusCode)
                        {
                            //tengo q leer propiedad "status" del json q hay en la respuesta, si es "succeeded" el cobro ha ido ok
                            JsonNode _chargeJSON = JsonNode.Parse(await _respuesta3.Content.ReadAsStringAsync());
                            string _status = _chargeJSON["status"].ToString();

                            if (_status == "succeeded")
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        private async Task<string> PagoPaypal(DatosPago datosPago, Pedido pedidoActual)
        {
            try
            {
                string _accessToken = await this.GetAccessTokenPAYPAL(
                        this._accesoAppSettings["PayPalAPIKEYS:ClientId"],
                        this._accesoAppSettings["PayPalAPIKEYS:ClientSecret"]
                    );

                List<PayPalItemCartOrder> _itemsPedidoPayPalOrder = pedidoActual.ElementosPedido
                                                                                .Select((ItemPedido item) => new PayPalItemCartOrder
                                                                                {
                                                                                    name = item.LibroItem.Titulo,
                                                                                    quantity = item.CantidadItem.ToString(),
                                                                                    unit_amount = new UnitAmount
                                                                                    {
                                                                                        currency_code = "EUR",
                                                                                        value = item.LibroItem.Precio.ToString().Replace(",", ".")
                                                                                    }
                                                                                }
                                                                                )
                                                                                .ToList<PayPalItemCartOrder>();
                var order = new
                {
                    intent = "CAPTURE",
                    purchase_units = new[] {
                        new {
                                items=_itemsPedidoPayPalOrder,
                                amount=new { 
                                    //en amount pay-pal solo marca como requeridos estas dos props: currency_code y value
                                    //OJO!!! pero si incluyes en purchase_units el array items, si o si, tienes q meter la prop. breakdown
                                    // pq comprueba si valor total del pedido puesto en el value coincide con subtotal de los items...
                                    // necesario meter los gastos de envio...q se mete en prop. breakdown
                                    currency_code="EUR",
                                    value = pedidoActual.Total.ToString().Replace(",","."),
                                    breakdown =  new {
                                        item_total = new { currency_code="EUR", value=pedidoActual.SubTotal.ToString().Replace(",",".") },
                                        shipping = new { currency_code="EUR", value=pedidoActual.GastosEnvio.ToString().Replace(",",".") }
                                    }
                                }
                        }
                    },
                    //OJO!! la api pone q esta DEPRECATED pooner las urls asi...hay q meterlo en: 
                    //payment_source.paypal.experience_context
                    application_context = new
                    {
                        return_url = $"https://localhost:7262/api/RESTTienda/PayPalCallback?idcliente={pedidoActual.IdCliente}&idpedido={pedidoActual.IdPedido}",
                        cancel_url = $"https://localhost:7262/api/RESTTienda/PayPalCallback?idcliente={pedidoActual.IdCliente}&idpedido={pedidoActual.IdPedido}&Cancel=true"
                    }

                };


                HttpRequestMessage _request = new HttpRequestMessage(HttpMethod.Post, "https://api.sandbox.paypal.com/v2/checkout/orders");
                _request.Headers.Add("Authorization", $"Bearer {_accessToken}");
                _request.Content = new StringContent(JsonSerializer.Serialize(order), System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage _response = await this._clienteHttp.SendAsync(_request);

                if (_response.IsSuccessStatusCode)
                {
                    string _contSerializadoResp = await _response.Content.ReadAsStringAsync();
                    PayPalCreateOrderResponse _contenidoresp = JsonSerializer.Deserialize<PayPalCreateOrderResponse>(_contSerializadoResp);

                    //para finalizar el pagp de paypal necesito el order-Id q te crea paypal cuando aprueba como correcto este objeto Order
                    //pero no puedo pasarlo en el RETURN_URL como parametro pq aun no se ha mandado la pet. a paypal
                    // ¿solucion?
                    // - en una variable de sesion para el server de paypal q id a cada cliente claro, pq esa url la ejecuta el server de paypal, no el cliente con su navegador, y el server de paypal  si tiene estado de sesion aqui la tendria de 8340583058353 clientes, hay q tener cuidado pq puede petar la ram
                    // - almacenarlo en la bd  en una tabla asociandolo con el idCliente y luego recuperarlo en el metodo q se ejecuta en el return_url

                    //HttpContext.Session.SetString(pedidoActual.IdCliente, _contenidoresp.id);
                    // guardado en BBDD la sesion: idCliente, idPedido, idPayPalOrder
                    PedidoPayPal _pedidoPayPal = new PedidoPayPal
                    {
                        idCliente = pedidoActual.IdCliente,
                        idPedido = pedidoActual.IdPedido,
                        idPedidoPaypal = _contenidoresp.id
                    };
                    this._dbContext.PedidosPayPal.Add(_pedidoPayPal);



                    PayPalOrderLinks _urlsalto = _contenidoresp
                                                    .links
                                                    .Where((PayPalOrderLinks enlace) => enlace.rel == "approve")
                                                    .Single<PayPalOrderLinks>();


                    return _urlsalto.href;
                }
                else
                {
                    throw new Exception("Error en la creacion de la orden de pago en PayPal");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }


        }

        private async Task<string> GetAccessTokenPAYPAL(string clientId, string clientSecret)
        {
            HttpRequestMessage _request = new HttpRequestMessage(HttpMethod.Post, "https://api.sandbox.paypal.com/v1/oauth2/token");

            //tengo q codificar en base64 la combinacion clientId:clientSecret
            string _base64Auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            _request.Headers.Add("Authorization", $"Basic {_base64Auth}");
            _request.Content = new StringContent(
                                    "grant_type=client_credentials",
                                     System.Text.Encoding.UTF8,
                                    "application/x-www-form-urlencoded"
                                );

            HttpResponseMessage _response = await this._clienteHttp.SendAsync(_request);
            //en la respuesta:  Headers + Content .... yo quiero el content, su propiedad access_token
            if (_response.IsSuccessStatusCode)
            {
                String _respJSONSerializado = await _response.Content.ReadAsStringAsync();
                PayPalAccessTokenResponse _respDesSerializado = JsonSerializer.Deserialize<PayPalAccessTokenResponse>(_respJSONSerializado);
                return _respDesSerializado.access_token;
            }
            else
            {
                return null;
            }
        }


        private String __GeneraJWT(String nombre, String apellidos, String email, String idcliente)
        {
            SecurityKey _clavefirma = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(this._accesoAppSettings["JWT:firmaJWT"]));

            JwtSecurityToken _jwt = new JwtSecurityToken(
                    issuer: this._accesoAppSettings["JWT:issuer"],
                    audience: null,
                    claims: new List<Claim> {
                                new Claim("nombre",nombre),
                                new Claim("apellidos", apellidos),
                                new Claim("email", email),
                                new Claim("clienteId", idcliente)
                    },
                    notBefore: null,
                    expires: DateTime.Now.AddHours(2),
                    signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(_clavefirma, SecurityAlgorithms.HmacSha256)
                );

            string _tokenjwt = new JwtSecurityTokenHandler().WriteToken(_jwt);
            return _tokenjwt;
        }
        #endregion

    }
}

using AgapeaBlazor2024.Server.Models;
using AgapeaBlazor2024.Shared;
using MailChimp.Net.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Policy;
using System.Text.Json;
using System.Web;


namespace AgapeaBlazor2024.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RESTClienteController : ControllerBase
    {


        #region ....propiedades clase servicio REST: RESTClienteController....
        private UserManager<MiClienteIdentity> _userManagerService;
        private SignInManager<MiClienteIdentity> _signInManagerService;
        private IClienteCorreo _clienteEmailService;
        private IConfiguration _accesoAppSettings;
        private IUrlHelper _urlHelper;
        private AplicacionDBContext _dbContext;
        #endregion


        public RESTClienteController(UserManager<MiClienteIdentity> userManagerDI,
                                     SignInManager<MiClienteIdentity> signinManagerDI,
                                     IClienteCorreo clienteEmailServiceDI,
                                     IConfiguration accesoAppSettingsDI, AplicacionDBContext dbContext)
        {
            this._userManagerService = userManagerDI;
            this._signInManagerService = signinManagerDI;
            this._clienteEmailService = clienteEmailServiceDI;
            this._accesoAppSettings = accesoAppSettingsDI;
            this._dbContext = dbContext;
        }



        #region ....metodos clase servicio REST: RESTClienteController....
        [HttpPost]
        public async Task<RestMessage> RegistrarCliente([FromBody] Cliente nuevocliente)
        {
            try
            {
                //1º usando el servicio UserManager de Identity crear nueva cuenta
                MiClienteIdentity _clienteACrear = new MiClienteIdentity
                {
                    Nombre = nuevocliente.Nombre,
                    Apellidos = nuevocliente.Apellidos,
                    FechaNacimiento = DateTime.Now,
                    Email = nuevocliente.Credenciales.Email,
                    Descripcion = nuevocliente.Descripcion ?? "",
                    Genero = nuevocliente.Genero ?? "",
                    ImagenAvatarBASE64 = nuevocliente.Credenciales.ImagenCuentaBASE64 ?? "",
                    UserName = nuevocliente.Credenciales.Login,
                    PhoneNumber = nuevocliente.Telefono
                };
                IdentityResult _resultRegistro = await this._userManagerService.CreateAsync(_clienteACrear, nuevocliente.Credenciales.Password);

                if (_resultRegistro.Succeeded)
                {
                    //2º usando el servicio UserManager de Identity crear un token de activacion de la cuenta creada y mandarla
                    //por email; la url en la q se envia este token de un solo uso generado por Identity, tiene q ser una url
                    //q invoque a un metodo de este servicio REST <--- en el mismo, debo comprobar si el token es correcto o no
                    String _tokenActivacionEmail = await this._userManagerService.GenerateEmailConfirmationTokenAsync(_clienteACrear);

                    //OJO!!!! con meter el token a pelo en la url, pq tiene caracteres "raros" q no se interpretan despues bien, hay q codificarla
                    String tokenUrl = HttpUtility.UrlEncode(_tokenActivacionEmail);
                    String idclienteUrl = HttpUtility.UrlEncode(_clienteACrear.Id);
                    String _urlMail = $"https://localhost:7262/api/RESTCliente/ActivarCuenta?token={tokenUrl}&idcliente={idclienteUrl}";
                    //string _urlMail = Url.RouteUrl("ActivarCuenta", new { token = _tokenActivacionEmail, idcliente = _clienteACrear.Id } "https", "localhost");
                    String _mensajeEmail = $@"
                        <h3>
                            <strong>Te has Registrado correctamente en Agapea.com</strong>
                        </h3>
                        <p>Pulsa el siguiente enlace <a href={_urlMail}> ACTIVAR TU CUENTA </a> de usuario en Agapea.</p>
                 ";
                    this._clienteEmailService.EnviarEmail(
                                                            nuevocliente.Credenciales.Email,
                                                            "Bienvenido al portal de Agapea.com, activa tu cuenta!!!",
                                                            _mensajeEmail,
                                                            ""
                                                            );
                    return new RestMessage
                    {
                        Codigo = 0,
                        Mensaje = "Registro OK, se ha mandado email para activar cuenta",
                        Error = "",
                        TokenSesion = null,
                        DatosCliente = null,
                        OtrosDatos = null
                    };
                }
                else
                {
                    throw new Exception(_resultRegistro.Errors.Take(1).Select((IdentityError err) => err.Description).Single<String>());
                }

            }
            catch (Exception ex)
            {

                return new RestMessage
                {
                    Codigo = 1,
                    Mensaje = "Error en el registro de la cuenta del usuario",
                    Error = ex.Message,
                    TokenSesion = null,
                    DatosCliente = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpGet]
        public async Task ActivarCuenta([FromQuery] String token, [FromQuery] String idcliente)
        {
            //3º usando el servicio UserManager de Identiy confirmar el token de activacion y activar cuenta si el token
            //es correcto, para eso tengo q recuperar los datos del cliente Identity asociados a ese idcliente:
            MiClienteIdentity _cliente = await this._userManagerService.FindByIdAsync(idcliente);

            IdentityResult _resultCompToken = await this._userManagerService.ConfirmEmailAsync(_cliente, token);
            if (!_resultCompToken.Succeeded)
            {
                String _error = _resultCompToken.Errors.Take(1).Select((IdentityError err) => err.Description).Single<String>();
                throw new Exception("token invalido, no se ha activado cuenta..." + _error);
            }


        }


        [HttpPost]
        public async Task<RestMessage> LoginCliente([FromBody] Cuenta credenciales)
        {
            try
            {
                //1º paso: usando servicio SignInManager de Identity comprobar credenciales recibidas desde blazor
                //usando metodo .PasswordSignInAsync; primero recuperamos cliente de Identity usando email:
                MiClienteIdentity _cliente = await this._userManagerService.FindByEmailAsync(credenciales.Email);

                Cliente _clienteAdevolver = crearClienteDevolver(_cliente);

                Microsoft.AspNetCore.Identity.SignInResult _resultLogin = await this._signInManagerService.PasswordSignInAsync(_cliente, credenciales.Password, true, false);
                if (_resultLogin.Succeeded)
                {
                    //2º paso: generar JWT como token de sesion para el cliente blazor
                    String _jwt = this.__GeneraJWT(_cliente.Nombre, _cliente.Apellidos, credenciales.Email, _cliente.Id);

                    return new RestMessage
                    {
                        Codigo = 0,
                        Mensaje = "Login OK, se ha mandado jwt de sesion",
                        Error = "",
                        TokenSesion = _jwt,
                        DatosCliente = _clienteAdevolver,
                        OtrosDatos = null
                    };
                }
                else
                {
                    throw new Exception("Login fallido, email o password incorrectos");
                }
            }
            catch (Exception ex)
            {

                return new RestMessage
                {
                    Codigo = 1,
                    Mensaje = "Email o contraseña incorrectos, fallo en el LOGIN",
                    Error = ex.Message,
                    TokenSesion = null,
                    DatosCliente = null,
                    OtrosDatos = null
                };
            }
        }


        [HttpGet]
        public async Task<RestMessage> LoginCliente([FromQuery] string idcliente)
        {
            try
            {
                //1º paso: usando servicio SignInManager de Identity comprobar credenciales recibidas desde blazor
                //usando metodo .PasswordSignInAsync; primero recuperamos cliente de Identity usando email:
                MiClienteIdentity _cliente = await this._userManagerService.FindByIdAsync(idcliente);

                Cliente _clienteAdevolver = new Cliente
                {
                    IdCliente = _cliente.Id,
                    Nombre = _cliente.Nombre,
                    Apellidos = _cliente.Apellidos,
                    Telefono = _cliente.PhoneNumber,
                    Genero = _cliente.Genero,
                    Descripcion = _cliente.Descripcion,
                    FechaNacimiento = _cliente.FechaNacimiento,
                    Credenciales = new Cuenta
                    {
                        Email = _cliente.Email,
                        Login = _cliente.UserName,
                        Password = "",
                        ImagenCuentaBASE64 = _cliente.ImagenAvatarBASE64
                    },
                    DireccionesCliente = this._dbContext.Direcciones.Where((Direccion dir) => dir.IdCliente == idcliente)
                    .ToList<Direccion>(),
                    PedidosCliente = this._dbContext.Pedidos.Where((Pedido ped) => ped.IdCliente == idcliente)
                    .ToList<Pedido>()

                };
                // no lo guardo en variable porq no quiero nada
                await this._signInManagerService.SignInAsync(_cliente, true);
                String _jwt = this.__GeneraJWT(_cliente.Nombre, _cliente.Apellidos, _cliente.Email, _cliente.Id);

                return new RestMessage
                {
                    Codigo = 0,
                    Mensaje = "Login OK, se ha mandado jwt de sesion",
                    Error = "",
                    TokenSesion = _jwt,
                    DatosCliente = _clienteAdevolver,
                    OtrosDatos = null
                };

            }
            catch (Exception ex)
            {

                return new RestMessage
                {
                    Codigo = 1,
                    Mensaje = "Email o contraseña incorrectos, fallo en el LOGIN",
                    Error = ex.Message,
                    TokenSesion = null,
                    DatosCliente = null,
                    OtrosDatos = null
                };
            }
        }

        [HttpPost]
        public async Task<RestMessage> AplicarCambioContraseña([FromBody] Dictionary<string, string> datos)
        {
            try
            {
                string idcliente = datos["idCliente"];
                string token = datos["token"];
                string pass = datos["pass"];

                MiClienteIdentity clienteAactualizar = this._userManagerService.FindByIdAsync(idcliente).Result;
                IdentityResult resultado = await this._userManagerService.ResetPasswordAsync(clienteAactualizar, token, pass);
                Cliente clienteADevolver = crearClienteDevolver(clienteAactualizar);
                if (!resultado.Succeeded)
                {
                    throw new Exception("Error al aplicar cambios en la contraseña");
                }

                return new RestMessage
                {
                    Codigo = 0,
                    Mensaje = "Cambio de contraseña OK",
                    Error = "",
                    TokenSesion = null,
                    DatosCliente = clienteADevolver,

                };

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ///Aqui redireccionariamos si habria algun tipo de fallo
                return new RestMessage
                {
                    Codigo = 1,
                    Mensaje = "Cambio de contraseña incorrecto",
                    Error = "",
                    TokenSesion = null,
                    DatosCliente = null

                };

            }

        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<RestMessage> ActualizarCliente([FromBody] string jsoncliente)

        {

            try
            {
                Console.WriteLine("Ha entrado al metodo del Server.ActualizarCliente...");

                Cliente clienteupdate = JsonSerializer.Deserialize<Cliente>(jsoncliente);

                MiClienteIdentity clienteIdentity = await this._userManagerService.FindByIdAsync(clienteupdate.IdCliente);
                //string jwt = HttpContext.Request.Headers["Authorization"][0].Split(" ")[1].ToString();

                clienteIdentity.Nombre = clienteupdate.Nombre;
                clienteIdentity.Apellidos = clienteupdate.Apellidos;
                clienteIdentity.PhoneNumber = clienteupdate.Telefono;
                clienteIdentity.Genero = clienteupdate.Genero;
                clienteIdentity.Descripcion = clienteupdate.Descripcion;
                clienteIdentity.ImagenAvatarBASE64 = clienteupdate.Credenciales.ImagenCuentaBASE64;
                clienteIdentity.UserName = clienteupdate.Credenciales.Login;
                clienteIdentity.Email = clienteupdate.Credenciales.Email;
                //CONFIRMANDO CAMBIOS
                IdentityResult _result = await this._userManagerService.UpdateAsync(clienteIdentity);
                if (_result.Succeeded == false)
                {
                    throw new Exception("Error al actualizar el clienteIdentity");
                }

                // Actualizar las direcciones del cliente

                List<Direccion> direcciones = this._dbContext.Direcciones.Where((Direccion dir) => dir.IdCliente == clienteIdentity.Id).ToList<Direccion>();
                //Verificar si hay direcciones que borrar
                foreach (Direccion dir in direcciones)
                {
                    if (dir.IdDireccion != clienteupdate.DireccionesCliente.First((Direccion d) => d.IdDireccion == dir.IdDireccion).IdDireccion)
                    {
                        this._dbContext.Direcciones.Remove(dir);
                    }
                }
                //Actualizar o añadir las direcciones del cliente
                foreach (Direccion direccionActual in clienteupdate.DireccionesCliente)
                {
                    //Comprobar si existe en la BBDD esta direccion
                    Direccion direccionExistente = this._dbContext.Direcciones.First((Direccion dir) => dir.IdDireccion == direccionActual.IdDireccion);
                    bool existe = direccionExistente != null;
                    if (existe)
                    {
                        direccionExistente.Calle = direccionActual.Calle;
                        direccionExistente.CP = direccionActual.CP;
                        direccionExistente.ProvinciaDirec = direccionActual.ProvinciaDirec;
                        direccionExistente.MunicipioDirec = direccionActual.MunicipioDirec;
                        direccionExistente.Pais = direccionActual.Pais;
                        this._dbContext.Direcciones.Update(direccionExistente);

                    }
                    else
                    {
                        this._dbContext.Direcciones.Add(direccionActual);
                    }
                }
                this._dbContext.SaveChanges();
                // Si la password es diferente (quiere cambiarla), envio email de verificacion
                #region PRUEBAS CAMBIO CONTRASEÑA MAILCHIMP.....
                bool cambiopass = !(await this._userManagerService.CheckPasswordAsync(clienteIdentity, clienteupdate.Credenciales.Password));
                if (cambiopass)
                {
                    string tokenrestePass = await this._userManagerService.GeneratePasswordResetTokenAsync(clienteIdentity);
                    String tokenUrl = HttpUtility.UrlEncode(tokenrestePass);
                    String idclienteUrl = HttpUtility.UrlEncode(clienteIdentity.Id);


                    String _urlMail = $"https://localhost:7262/Cliente/CambioContraseñaOk?token={tokenUrl}&idcliente={idclienteUrl}&pass={clienteupdate.Credenciales.Password}";
                    String _mensajeEmail = $@"
                        <h3>
                            <strong>Has cambiado tu contraseña en Agapea.com</strong>
                        </h3>
                        <p>Pulsa el siguiente enlace <a href={_urlMail}>EFECTUAR CAMBIO DE TU CONTRAÑA </a> de usuario en Agapea.</p>
                 ";

                    this._clienteEmailService.EnviarEmail(clienteupdate.Credenciales.Email, "CambioContraseña", _mensajeEmail, "");

                    return new RestMessage
                    {
                        Codigo = 2,
                        Mensaje = "Actualizacion de datos del cliente OK",
                        Error = "",
                        TokenSesion = null,
                        DatosCliente = clienteupdate,
                        OtrosDatos = null
                    };
                    //IdentityResult resultChgPass = await this._userManagerService.ResetPasswordAsync(clienteIdentity, tokenrestePass, clienteupdate.Credenciales.Password);
                    /*
                     
                    if (!resultChgPass.Succeeded)
                    {
                        throw new Exception("Error al cambiar la contraseña del clienteIdentity");
                    }*/

                }

                #endregion

                return new RestMessage
                {
                    Codigo = 0,
                    Mensaje = "Actualizacion de datos del cliente OK",
                    Error = "",
                    TokenSesion = null,
                    DatosCliente = clienteupdate,
                    OtrosDatos = null
                };


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new RestMessage
                {
                    Codigo = 1,
                    Mensaje = "Error en la actualizacion de los datos del cliente",
                    Error = ex.Message,
                    TokenSesion = null,
                    DatosCliente = null,
                    OtrosDatos = null
                };
            }

        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        public async Task<RestMessage> subirImagen([FromBody] string imagen)
        {
            try
            {
                string _jwt = this.Request.Headers["Authorization"][0].Split(" ")[1].ToString();
                string idcliente = new JwtSecurityTokenHandler().ReadJwtToken(_jwt).Claims.First((Claim cl) => cl.Type == "clienteId").Value;
                MiClienteIdentity cliente = await this._userManagerService.FindByIdAsync(idcliente);

                cliente.ImagenAvatarBASE64 = imagen;

                IdentityResult _result = await this._userManagerService.UpdateAsync(cliente);
                if (!_result.Succeeded)
                {
                    throw new Exception("Error al hacer update en BBD");
                }
                Cliente clienteDevolver = crearClienteDevolver(cliente);

                return new RestMessage
                {
                    Codigo = 0,
                    Mensaje = "Subida de la imagen OK",
                    Error = "",
                    TokenSesion = null,
                    DatosCliente = clienteDevolver,
                    OtrosDatos = null
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new RestMessage
                {
                    Codigo = 1,
                    Mensaje = "Error en la subida de la imagen",
                    Error = ex.Message,
                    TokenSesion = null,
                    DatosCliente = null,
                    OtrosDatos = null
                };


            }

        }

        [HttpPost]
        public async Task<RestMessage> GuardarOpinion([FromBody] Opinion opinion)
        {
            try
            {
                MiClienteIdentity clienteIdentity = await this._userManagerService.FindByIdAsync(opinion.IdCliente);
                this._dbContext.Opiniones.Add(opinion);
                this._dbContext.SaveChanges();
                Cliente clienteAdevolver = crearClienteDevolver(clienteIdentity);
                return new RestMessage
                {
                    Codigo = 0,
                    Mensaje = "Opinion guardada correctamente",
                    Error = "",
                    TokenSesion = null,
                    DatosCliente = clienteAdevolver,
                    OtrosDatos = null
                };



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new RestMessage
                {
                    Codigo = 1,
                    Mensaje = "Error al guardar la opinion",
                    Error = ex.Message,
                    DatosCliente = null
                };

            }


        }

        [HttpPost]

        public async Task<RestMessage> OperarOpinion([FromBody] Dictionary<string, string> datosOperacion)
        {
            try
            {
                string operacion = datosOperacion["operacion"];
                string idopinion = datosOperacion["idOpinion"];
                string texto = datosOperacion["textoOpinion"];

                Opinion opinion = this._dbContext.Opiniones.First((Opinion ope) => ope.IdOpinion == idopinion);
                if (operacion == "eliminar")
                {
                    this._dbContext.Opiniones.Remove(opinion);

                }
                else
                {
                    opinion.TextoOpinion = texto;
                }

                this._dbContext.SaveChanges();
                Cliente clienteAdevolver = this.crearClienteDevolver(await this._userManagerService.FindByIdAsync(opinion.IdCliente));
                return new RestMessage
                {
                    Codigo = 0,
                    Mensaje = "Opinion operada correctamente",
                    Error = "",
                    DatosCliente = clienteAdevolver
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new RestMessage
                {
                    Codigo = 1,
                    Mensaje = "Error al operar la opinion",
                    Error = ex.Message,
                    DatosCliente = null
                };
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

        private Cliente crearClienteDevolver(MiClienteIdentity _cliente)
        {
            Cliente _clienteAdevolver = new Cliente
            {
                IdCliente = _cliente.Id,
                Nombre = _cliente.Nombre,
                Apellidos = _cliente.Apellidos,
                Telefono = _cliente.PhoneNumber,
                Genero = _cliente.Genero,
                Descripcion = _cliente.Descripcion,
                FechaNacimiento = _cliente.FechaNacimiento,
                Credenciales = new Cuenta
                {
                    Email = _cliente.Email,
                    Login = _cliente.UserName,
                    Password = "",
                    ImagenCuentaBASE64 = _cliente.ImagenAvatarBASE64
                },
                DireccionesCliente = this._dbContext.Direcciones.Where((Direccion dir) => dir.IdCliente == _cliente.Id)
                       .ToList<Direccion>(),
                PedidosCliente = this._dbContext.Pedidos.Where((Pedido ped) => ped.IdCliente == _cliente.Id)
                       .ToList<Pedido>(),
                OpinionesCliente = this._dbContext.Opiniones.Where((Opinion op) => op.IdCliente == _cliente.Id).ToList<Opinion>()
            };

            return _clienteAdevolver;

        }
        #endregion

    }
}

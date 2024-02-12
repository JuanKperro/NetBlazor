using Agapea_netcore_mvc_23_24.Models.Servicios;
using AgapeaBlazor2024.Server.Models;
using AgapeaBlazor2024.Server.Models.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.SymbolStore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

/* ------------------- configuracion de identity con EF (Entity-Framework) -------------------------
   el conjunto de tablas sql-server q se crean atraves de EF y q usa Identity se mapean contra objetos DbSet 
    todos los DbSets forman un DBContext; este DbContext es el q define las props de cada DbSet (mapea props de cada clase con
    columnas de un DbSet, establece claves, indices, relaciones, etc...)
            
 */
String _cadenaConexionBD = builder.Configuration.GetConnectionString("BlazorSqlServerConnectionString");
String _nombreEnsamblado = Assembly.GetExecutingAssembly().GetName().Name;

//1º paso: configurar cadena de conexion q va a usar el DbContext para volcar cambios en migraciones y recup.datos
builder.Services.AddDbContext<AplicacionDBContext>((DbContextOptionsBuilder opciones) =>
{
    opciones.UseSqlServer(_cadenaConexionBD, (SqlServerDbContextOptionsBuilder opc) => opc.MigrationsAssembly(_nombreEnsamblado));
});

//2º paso: configuro servicios de Identity: UserManager y SignInManager
builder.Services.AddIdentity<MiClienteIdentity, IdentityRole>(
                        (IdentityOptions opciones) =>
                        {

                            //opciones configuracion UserManager...
                            opciones.Password = new PasswordOptions
                            {
                                RequireDigit = true,
                                RequireUppercase = true,
                                RequireLowercase = true,
                                RequireNonAlphanumeric = true,
                                RequiredLength = 6
                            };
                            opciones.User = new UserOptions { RequireUniqueEmail = true };


                            //opciones configuracion SignInManager...
                            opciones.SignIn = new SignInOptions { RequireConfirmedEmail = true };
                            opciones.Lockout = new LockoutOptions
                            {
                                AllowedForNewUsers = false,
                                MaxFailedAccessAttempts = 3,
                                DefaultLockoutTimeSpan = TimeSpan.FromHours(3)
                            };
                        }
                 )
                .AddEntityFrameworkStores<AplicacionDBContext>()
                .AddDefaultTokenProviders(); //<---- saltara excepcion a la hora de validar emails cuando Identity genera token de activacion pq no
                                             // existe un proveedor de tokens configurado por defecto: .AddTokenProvider<>()

//--------------------------------------------------------------------------------------------------------




//----------- configuracion servicio generacion de JWT una vez q Idenity de OK ----------------------------
byte[] _bytesFirma = System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:firmaJWT"]);


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) //<---- cambia el esquema de autentificacion a JWT frente a las cookies q se usan por defecto
                .AddJwtBearer(
                    (JwtBearerOptions opciones) =>
                    {
                        opciones.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                        {
                            ValidateIssuer = true, //<---- validar si el jwt ha sido generado por mi servidor (claim "issuer")
                            ValidateLifetime = true, //<---validar la fecha de expiracion del jwt (claim "exp")
                            ValidateIssuerSigningKey = true,//<----validar si el jwt ha sido firmado por el servidor
                            ValidateAudience = false, //<---- validar subdominios para los q es valido el jwt (claim "audience")
                            ValidIssuer = builder.Configuration["JWT:issuer"],
                            IssuerSigningKey = new SymmetricSecurityKey(_bytesFirma)
                        };
                    }
                ); //<------ configuramos la comprobacion de los claims de los JWT recibidos desde los clientes blazor

//----------------------------------------------------------------------------------------------------------


// ------------ inyeccion servicio envio email a traves de mailjet ------------
builder.Services.AddScoped<IClienteCorreo, MailJetServicecs>();
///*builder.Services.AddScoped<IClienteCorreo, MailChimpService>();*/


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

//--------------------- meto en la pipeline los middleware para la autentificacion y autorizacion usando identity --------------
app.UseAuthentication();
app.UseAuthorization();
//------------------------------------------------------------------------------------------------------------------------------


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

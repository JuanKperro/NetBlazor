using AgapeaBlazor2024.Server.Models.PayPal;
using AgapeaBlazor2024.Shared;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AgapeaBlazor2024.Server.Models
{
    public class AplicacionDBContext : IdentityDbContext
    {

        /*
          esta clase va a servir para q Identity genere atraves de EF el DBContext para generar tablas a partir de clases modelo
          usando DbSets....como estamos creando un DBContext Personalizado pq vamos a añadir tablas propias ademas de las de Identity
          EF te obliga a sobrecargar el constructor (sino lo pones te salta un error indicandote que sobrecargues el constructor)
         */

        public AplicacionDBContext(DbContextOptions<AplicacionDBContext> options) : base(options) { }


        #region ....propiedades de la clase AplicacionDBContext ......

        //nos definimos un DbSet por cada clase modelo a mapear en el DbContext como propiedad....
        public DbSet<Direccion> Direcciones { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<Provincia> Provincias { get; set; }

        public DbSet<Municipio> Municipios { get; set; }

        public DbSet<PedidoPayPal> PedidosPayPal { get; set; }

        public DbSet<Opinion> Opiniones { get; set; }

        #endregion


        #region ....metodos clase AplicacionDBContext .....
        //metodo que se ejecuta para crar las tablas a partir de las clases en el momento que se lanza migracion....
        //para lanzar migracion y q se vuelquen cambios sobre la BD fisica:
        //  - 1º abres consola de administracion de paquetes powershell de Nuget: Herramientas ---> administracion paquetes NuGet ----> consola adminstracion....
        //              Add-Migration nº_nombre_migracion 
        //
        // OJO!!!!  la migracion no se genera si el proyecto no compila (hay errores...)

        //  - 2º paso, si todo esta ok para ejecutar la migracion:  
        //          Update-Database
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region //// modificacion tabla IdentityUser ///
            builder.Entity<MiClienteIdentity>();
            //builder.Entity<MiClienteIdentity>().HasMany<Direccion>(p => p.).WithOne().HasForeignKey<Direccion>(s => s.IdDireccion);
            #endregion

            #region ////// creacion tabla Direcciones a partir de clase modelo Direccion /////
            builder.Entity<Direccion>().ToTable("Direcciones");
            builder.Entity<Direccion>().HasKey((Direccion dir) => dir.IdDireccion);

            builder.Entity<Direccion>().Property((Direccion dir) => dir.Calle).IsRequired().HasMaxLength(250);
            builder.Entity<Direccion>().Property((Direccion dir) => dir.CP).IsRequired().HasMaxLength(5);

            //cuando la clase modelo tiene como prop. un objeto de otra clase, EF no puede mapearlo contra un tipo de dato de SqlServer
            //¿solucion? o no almacenas esa prop como columna de tabla o serializas esa prop a un string usando metodo:
            // .HasConversion( 1ºparam_lambda_serializacion, 2ºparam_lambda_deserializacion)
            builder.Entity<Direccion>().Property((Direccion dir) => dir.ProvinciaDirec)
                                       .HasConversion(
                                            prov => JsonSerializer.Serialize<Provincia>(prov, (JsonSerializerOptions)null),
                                            prov => JsonSerializer.Deserialize<Provincia>(prov, (JsonSerializerOptions)null)
                                        ).HasColumnName("Provincia");

            builder.Entity<Direccion>().Property((Direccion dir) => dir.MunicipioDirec)
                           .HasConversion(
                                muni => JsonSerializer.Serialize<Municipio>(muni, (JsonSerializerOptions)null),
                                muni => JsonSerializer.Deserialize<Municipio>(muni, (JsonSerializerOptions)null)
                            ).HasColumnName("Municipio");



            #endregion

            #region ----------- creacion tabla libros a partir del DbSet ---------------------------
            builder.Entity<Libro>().ToTable("Libros");
            builder.Entity<Libro>().HasKey("ISBN13");
            builder.Entity<Libro>().Property((Libro lib) => lib.Precio).HasColumnType("DECIMAL(5,2)");

            #endregion

            #region -------- creacion tabla categorias a partir del DbSet -----------------            
            builder.Entity<Categoria>().ToTable("Categorias");
            builder.Entity<Categoria>().HasNoKey();
            #endregion


            #region ---------- creacion tabla pedidosPayPal a partir del DbSet ------------------
            builder.Entity<PedidoPayPal>().ToTable("PedidosPayPal");
            builder.Entity<PedidoPayPal>().HasKey("idPedidoPaypal");
            #endregion

            builder.Entity<Provincia>().HasNoKey();
            builder.Entity<Municipio>().HasNoKey();

            #region ---------- creacion tabla pedidos a partir del DbSet ------------------
            builder.Entity<Pedido>().ToTable("Pedidos");
            builder.Entity<Pedido>().HasKey("IdPedido");
            builder.Entity<Pedido>().Property((Pedido ped) => ped.GastosEnvio).HasColumnType("DECIMAL(5,2)");
            builder.Entity<Pedido>().Property((Pedido ped) => ped.SubTotal).HasColumnType("DECIMAL(5,2)");
            builder.Entity<Pedido>().Property((Pedido ped) => ped.Total).HasColumnType("DECIMAL(5,2)");
            builder.Entity<Pedido>().Property((Pedido ped) => ped.ElementosPedido)
                                    .HasConversion(
                                        lista => JsonSerializer.Serialize(lista, (JsonSerializerOptions)null),
                                        lista => JsonSerializer.Deserialize<List<ItemPedido>>(lista, (JsonSerializerOptions)null)
                                    ).HasColumnName("listaItems");

            //builder.Entity<Pedido>().HasOne<Direccion>(p => p.DireccionEnvio).WithOne().HasForeignKey<Direccion>(s => s.IdDireccion);

            #endregion

            #region ---------- creacion tabla opiniones a partir del DbSet ------------------
            builder.Entity<Opinion>().ToTable("Opiniones");
            builder.Entity<Opinion>().HasKey("IdOpinion");
            builder.Entity<Opinion>().Property((Opinion o) => o.Libro).HasConversion(
                libro => JsonSerializer.Serialize<Libro>(libro, (JsonSerializerOptions)null),
                libro => JsonSerializer.Deserialize<Libro>(libro, (JsonSerializerOptions)null)
                );

            #endregion
        }
        #endregion



    }
}

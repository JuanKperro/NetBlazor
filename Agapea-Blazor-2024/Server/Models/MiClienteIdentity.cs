using AgapeaBlazor2024.Shared;
using Microsoft.AspNetCore.Identity;


namespace AgapeaBlazor2024.Server.Models
{
    public class MiClienteIdentity : IdentityUser
    {
        //clase personalizada para añadir sobre las props. de IdentityUser datos propios que me interesan y q Identity no refleja
        #region .... propiedades nuevas q añadimos a clase IdentityUser .....
        public String Nombre { get; set; }
        public String Apellidos { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public String Genero { get; set; }
        public String Descripcion { get; set; }
        public String ImagenAvatarBASE64 { get; set; }


        #endregion
    }
}

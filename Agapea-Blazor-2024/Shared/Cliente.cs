using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AgapeaBlazor2024.Shared
{
    public class Cliente : IValidatableObject
    {
        #region .... propiedades de clase ....

        public String IdCliente { get; set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = "* el Nombre es obligatorio")]
        [MaxLength(50, ErrorMessage = "* no se admiten mas de 50 caracteres en el nombre")]
        public String Nombre { get; set; } = "";




        [Required(ErrorMessage = "* los Apellidos son obligatorios")]
        [MaxLength(200, ErrorMessage = "* no se admiten mas de 200 caracteres en los apellidos")]
        public String Apellidos { get; set; } = "";


        [Required(ErrorMessage = "* el Telefono de contacto es obligatorio")]
        [RegularExpression(@"^\d{3}(\s?\d{2}){3}$", ErrorMessage = "* formato de Telefono invalido: 666 11 22 33")]
        public String Telefono { get; set; } = "";

        public String Genero { get; set; } = "";
        public String Descripcion { get; set; } = ""; //intereses, etc...
        public DateTime FechaNacimiento { get; set; }

        //credenciales o cuenta del usuario...
        public Cuenta Credenciales { get; set; } = new Cuenta();

        public List<Direccion> DireccionesCliente { get; set; } = new List<Direccion>();

        public List<Pedido> PedidosCliente { get; set; } = new List<Pedido>();

        public List<Opinion> OpinionesCliente { get; set; } = new List<Opinion>();




        #endregion

        #region ....metodos de clase....
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> listaErrores = new List<ValidationResult>();
            
              bool valida = Validator.TryValidateObject(this.Credenciales, new ValidationContext(this.Credenciales), listaErrores, true);
           
            return listaErrores;
            
        }

        #endregion
    }
}

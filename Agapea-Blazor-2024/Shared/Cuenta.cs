using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgapeaBlazor2024.Shared
{
    public class Cuenta
{
    #region ....propiedades de clase MODELO CUENTA...

    [Required(ErrorMessage = "* el Email es obligatorio")]
    [EmailAddress(ErrorMessage = "* formato de Email invalido")]
    public String Email { get; set; } = "";


    [Required(ErrorMessage = "* la Contraseña es obligatoria")]
    [MinLength(8, ErrorMessage = "* la Contraseña debe tener al menos 8 caracteres")]
    [MaxLength(200, ErrorMessage = "* no se admite una contraseña tan larga")]
    [RegularExpression(@"^(?=.*\d)(?=.*[\u0021-\u002b\u003c-\u0040])(?=.*[A-Z])(?=.*[a-z])\S{8,}$", ErrorMessage = "* la contraseña debe contener al menos MAYS,MINS, digito y caracter raro.")]
    public String Password { get; set; } = "";


    public String Login { get; set; } = "";


    public Boolean CuentaActiva { get; set; } = false;
    public String ImagenCuentaBASE64 { get; set; } = "";

    #endregion

    #region ...metodos de clase MODELO CUENTA...

    #endregion


}
}

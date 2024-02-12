using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgapeaBlazor2024.Shared
{
    public class Libro
{
    #region .....propiedades clase libro....
    public String IdCategoria { get; set; } = "";
    public String Titulo { get; set; } = "";
    public String Editorial { get; set; } = "";
    public String Autores { get; set; } = "";
    public String ImagenLibroBASE64 { get; set; } = "";
    public String Edicion { get; set; } = "";
    public String Dimensiones { get; set; } = "";
    public String Idioma { get; set; } = "Español";
    public String ISBN13 { get; set; } = "";
    public String ISBN10 { get; set; } = "";
    public String Resumen { get; set; } = "";
    public int NumeroPaginas { get; set; } = 0;
    public Decimal Precio { get; set; } = 0;

    #endregion


    #region ....metodos clase libro ....

    #endregion
}
}

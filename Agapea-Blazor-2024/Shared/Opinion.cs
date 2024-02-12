using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgapeaBlazor2024.Shared
{
    public class Opinion
    {
        public String IdOpinion { get; set; } = Guid.NewGuid().ToString();
        public String IdCliente { get; set; } = "";

        public String LoginCliente { get; set; } = "";

        public String IdLibro { get; set; } = "";
        public Libro Libro { get; set; }

        public String TextoOpinion { get; set; } = "";

        public int Puntuacion { get; set; } = 0;

        public DateTime FechaOpinion { get; set; } = DateTime.Now;

        public bool Aprobada { get; set; } = false;

    }
}

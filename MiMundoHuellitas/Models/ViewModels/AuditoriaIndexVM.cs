using System.Collections.Generic;

namespace MiMundoHuellitas.Models.ViewModels
{
    public class AuditoriaIndexVM
    {
        public AuditoriaFiltroVM Filtro { get; set; }
        public List<AuditoriaVM> Registros { get; set; }

        public AuditoriaIndexVM()
        {
            Filtro = new AuditoriaFiltroVM();
            Registros = new List<AuditoriaVM>();
        }
    }
}
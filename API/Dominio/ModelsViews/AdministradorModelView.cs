using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.DTOs.Enuns;

namespace minimal_api.Dominio.ModelsViews
{
    public class AdministradorModelView
    {
        public int Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Perfil { get; set; } = default!;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace minimal_api.Dominio.ModelsViews
{
    public struct ErroValidacao
    {
        public List<string> Mensagem { get; set; }
    }
}
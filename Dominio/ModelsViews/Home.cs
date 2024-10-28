using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace minimal_api.Dominio.ModelsViews
{
    public struct Home
    {
        public string Mensagem { get => "Bem vindo a API de veículos - MinimalAPI"; }
        public string Doc { get => "/swagger"; }
    }
}
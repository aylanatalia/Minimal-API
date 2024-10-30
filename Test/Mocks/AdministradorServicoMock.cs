using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Interfaces;
using MinimalApi.DTOs;

namespace Test.Mocks
{
    public class AdministradorServicoMock : iAdministradorServicos
    {
        private static List<Administrador> administradores = new List<Administrador>(){
            new Administrador{
                Id = 1, 
                Email = "ayla@teste.com",
                Senha = "ladygaga",
                Perfil = "Adm"
            },
            new Administrador{
                Id = 2,
                Email = "natalia@teste.com",
                Senha = "disease",
                Perfil = "Editor"
            }
        };

        public Administrador? BuscaPorId(int id)
        {
            return administradores.Find(a => a.Id == id);
        }

        public Administrador Incluir(Administrador administrador)
        {
            administrador.Id = administradores.Count() + 1;
            administradores.Add(administrador);

            return administrador;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            return administradores.Find(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha);
        }

        public List<Administrador> Todos(int? pagina)
        {
            return administradores;
        }
    }
}
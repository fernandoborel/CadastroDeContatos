using Contatos.Models;
using System.Collections.Generic;

namespace Contatos.Repositorio
{
    public interface IUsuarioRepositorio
    {
        UsuarioModel BuscaPorLogin(string login);
        UsuarioModel BuscarPorEmailELogin(string email ,string login);
        UsuarioModel ListarPorId(int id);
        List<UsuarioModel> BuscarTodos();
        UsuarioModel Adicionar(UsuarioModel contato);
        UsuarioModel Atualizar(UsuarioModel contato);
        bool Apagar(int id);
    }
}

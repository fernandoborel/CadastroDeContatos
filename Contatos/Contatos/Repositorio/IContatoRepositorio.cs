using Contatos.Models;
using System.Collections.Generic;

namespace Contatos.Repositorio
{
    public interface IContatoRepositorio
    {
        List<ContatoModel> BuscarTodos();

        ContatoModel Adicionar(ContatoModel contato);
    }
}

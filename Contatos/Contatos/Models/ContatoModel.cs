using System.ComponentModel.DataAnnotations;

namespace Contatos.Models
{
    public class ContatoModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Nome obrigatório")]
        public string Nome { get; set; }
        [Required(ErrorMessage = "E-mail obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido!")]
        public string Email { get; set;}
        [Required(ErrorMessage = "Celular obrigatório")]
        [Phone(ErrorMessage = "Número inválido!")]
        public string Celular { get; set;}

        public int? UsuarioId { get; set; }

        public UsuarioModel Usuario { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Contatos.Models
{
    public class AlterarSenhaModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Digite a senha atual do usuário")]
        public string SenhaAtual { get; set; }

        [Required(ErrorMessage = "Digite a nova senha do usuário")]
        public string NovaSenha { get; set; }

        [Required(ErrorMessage = "Confirme a nova senha do usuário")]
        [Compare("NovaSenha", ErrorMessage = "As senhas são diferentes")]
        public string ConfirmarNovaSenha { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Contatos.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Digite seu nome ou e-mail")]
        public string Login { get; set; }
        [Required(ErrorMessage = "Digite sua senha")]
        public string Senha { get; set; }
    }
}

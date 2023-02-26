using System.ComponentModel.DataAnnotations;

namespace Contatos.Models
{
    public class RedefinirSenhaModel
    {
        [Required(ErrorMessage = "Digite seu login")]
        public string Login { get; set; }
        [Required(ErrorMessage = "Digite seu e-mail")]
        public string Email { get; set; }
    }
}

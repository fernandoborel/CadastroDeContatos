using Contatos.Enums;
using Contatos.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Contatos.Models
{
    public class UsuarioModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Nome obrigatório")]
        public string Nome { get; set; }
        [Required(ErrorMessage = "Login obrigatório")]
        public string Login { get; set; }
        [Required(ErrorMessage = "E-mail obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido!")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Perfil é obrigatório")]
        public PerfilEnum? Perfil { get; set; }
        [Required(ErrorMessage = "Senha obrigatória")]
        public string Senha { get; set; }
        public DateTime DataCadastro { get; set; }
        public DateTime? DataAtualizacao { get; set;}

        public virtual List<ContatoModel> Contatos { get; set; }

        public bool SenhaValida(string senha)
        {
            return Senha == senha.GerarHash();
        }

        public void SetSenhaHash()
        {
            Senha = Senha.GerarHash();
        }

        public void SetNovaSenha(string novaSenha)
        {
            Senha = novaSenha.GerarHash();
        }

        public string GerarNovaSenha()
        {
            string novaSenha = Guid.NewGuid().ToString().Substring(0,8);
            Senha = novaSenha.GerarHash();
            return novaSenha;
        }
    }
}

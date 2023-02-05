﻿using Contatos.Enums;
using System;
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
        public bool SenhaValida(string senha)
        {
            return Senha == senha;
        }
    }
}

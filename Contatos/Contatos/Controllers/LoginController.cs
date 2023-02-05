using Contatos.Helper;
using Contatos.Models;
using Contatos.Repositorio;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Contatos.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly ISessao _sessao;
        public LoginController(IUsuarioRepositorio usuarioRepositorio,
                               ISessao sessao)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _sessao = sessao;
        }

        public IActionResult Index()
        {
            if (_sessao.BuscarSessaoDoUsuario() != null) return RedirectToAction("Index", "Home");
            
            return View();
        }

        public IActionResult Sair()
        {
            _sessao.RemoverSessaoDoUsuario();
            return RedirectToAction("Index", "Login");
        }


        [HttpPost]
        public IActionResult Entrar(LoginModel loginModel)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    UsuarioModel usuarioModel = _usuarioRepositorio.BuscaPorLogin(loginModel.Login);
                    
                    if(usuarioModel != null)
                    {
                        if (usuarioModel.SenhaValida(loginModel.Senha))
                        {
                            _sessao.CriarSessaoDoUsuario(usuarioModel);
                            return RedirectToAction("Index", "Home");
                        }
                        
                        TempData["MensagemErro"] = $"Senha inválida!";
                    }

                    TempData["MensagemErro"] = $"Usuário ou senha inválidos!";
                }

                return View("Index");
            }
            catch(Exception erro)
            {
                TempData["MensagemErro"] = $"Ops, não foi possível realizar o login! Erro: {erro.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}

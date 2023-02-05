using Contatos.Models;
using Contatos.Repositorio;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Contatos.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public LoginController(IUsuarioRepositorio usuarioRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
        }

        public IActionResult Index()
        {
            return View();
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

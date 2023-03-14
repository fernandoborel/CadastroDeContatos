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
        private readonly IEmail _email;

        public LoginController(IUsuarioRepositorio usuarioRepositorio,
                               ISessao sessao,
                               IEmail email)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _sessao = sessao;
            _email = email;
        }

        public IActionResult Index()
        {
            if (_sessao.BuscarSessaoDoUsuario() != null) return RedirectToAction("Index", "Home");
            
            return View();
        }

        public IActionResult RedefinirSenha()
        {
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

        [HttpPost]
        public IActionResult EnviarLinkParaRedefinirSenha(RedefinirSenhaModel redefinirSenhaModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    UsuarioModel usuarioModel = _usuarioRepositorio.BuscarPorEmailELogin(redefinirSenhaModel.Email, redefinirSenhaModel.Login);

                    if (usuarioModel != null)
                    {
                        string novaSenha = usuarioModel.GerarNovaSenha();
                        string mensagem = $"Sua nova senha é: {novaSenha}";

                        bool emailEnviado = _email.Enviar(usuarioModel.Email, "Sistema de Contatos - Nova Senha", mensagem);

                        if (emailEnviado)
                        {
                            _usuarioRepositorio.Atualizar(usuarioModel);
                            TempData["MensagemSucesso"] = $"Enviamos para seu e-mail uma nova senha.";
                        }
                        else
                        {
                            TempData["MensagemErro"] = $"E-mail inválido!";

                        }

                        return RedirectToAction("Index","Login");
                    }

                    TempData["MensagemErro"] = $"Usuário ou e-mail inválidos!";
                }

                return View("Index");
            }
            catch (Exception erro)
            {
                TempData["MensagemErro"] = $"Ops, não conseguimos redefinir sua senha! Erro: {erro.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}

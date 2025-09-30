using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MiMundoHuellitas.Models;

namespace MiMundoHuellitas.Controllers
{
    public class AccountController : Controller
    {
        // ====== LOGIN ======
        [HttpGet, AllowAnonymous]
        public ActionResult Login() => View();

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Login(string Email, string Password, bool RememberMe = false, string returnUrl = null)
        {
            // 1) validar contra usuarios registrados en memoria
            var valido = Usuarios.Validar(Email, Password);

            // 2)usuario demo SOLO 
            if (!valido)
                valido = ((Email == "admin@demo.com" || Email == "admin") && Password == "123456");

            if (!valido)
            {
                TempData["LoginError"] = "Credenciales inválidas.";
                return View();
            }

            FormsAuthentication.SetAuthCookie(Email, RememberMe);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // ====== REGISTRO ======
        [HttpGet, AllowAnonymous]
        public ActionResult Registro() => View();

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Registro(string Email, string Password, string ConfirmPassword, bool AutoLogin = true)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                TempData["RegisterError"] = "Completa todos los campos.";
                return View();
            }
            if (Password != ConfirmPassword)
            {
                TempData["RegisterError"] = "La confirmación no coincide.";
                return View();
            }
            if (Usuarios.Existe(Email))
            {
                TempData["RegisterError"] = "Ya existe un usuario con ese correo/usuario.";
                return View();
            }

            Usuarios.Agregar(Email, Password);
            TempData["RegisterOk"] = "¡Cuenta creada correctamente!";

            if (AutoLogin)
            {
                FormsAuthentication.SetAuthCookie(Email, false);
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Login", "Account");
        }

        // ====== LOGOUT ======
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}


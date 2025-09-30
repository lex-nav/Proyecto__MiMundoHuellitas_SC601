using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MiMundoHuellitas.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [HttpGet, AllowAnonymous]
        public ActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Login(string Email, string Password, bool RememberMe = false, string returnUrl = null)
        {
            // DEMO (sin BD)
            bool esValido = ((Email == "admin@demo.com" || Email == "admin") && Password == "123456");
            if (!esValido)
            {
                TempData["LoginError"] = "Credenciales inválidas.";
                return View();
            }

            FormsAuthentication.SetAuthCookie(Email, RememberMe);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        // Luego añadiremos Registro() y RecuperarAcceso() aquí mismo
    }
}


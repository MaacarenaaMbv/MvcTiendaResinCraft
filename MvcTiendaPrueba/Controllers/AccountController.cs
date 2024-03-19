using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MvcTiendaPrueba.Data;
using MvcTiendaPrueba.Extensions;
using MvcTiendaPrueba.Models;
using MvcTiendaPrueba.Repositories;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MvcTiendaPrueba.Controllers
{
    public class AccountController : Controller
    {
        private RepositoryTienda repo;
        private TiendaContext context;

        public AccountController(RepositoryTienda repo)
        {
            this.repo = repo;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> PanelAdmin()
        {
            var categorias = await repo.GetCategoriasAsync();
            ViewData["CATEGORIAS"] = categorias;
            return View();
        }
        public async Task<IActionResult> NewCategoria()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> NewCategoria(string nombre)
        {
            await this.repo.InsertCategoriaAsync(nombre);
            return View();
        }

        public async Task<IActionResult> NewProducto()
        {
            var categorias = await repo.GetCategoriasAsync();
            ViewData["CATEGORIAS"] = categorias;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> NewProducto(string nombre, string descripcion, int precio, int idcategoria)
        {
            var categorias = await repo.GetCategoriasAsync();
            ViewData["CATEGORIAS"] = categorias;
            await this.repo.InsertProductoAsync(nombre, descripcion, precio, idcategoria);
            return View();
        }


        public IActionResult Register()
        {
            var provincias = repo.GetProvincias();
            ViewData["PROVINCIAS"] = provincias;
            return View();
        }

        public IActionResult MiPerfil()
        {
            string correo = HttpContext.Session.GetString("USUARIO");

            Usuario usuario = JsonConvert.DeserializeObject<Usuario>(correo);

            return View(usuario);
        }
        public IActionResult EditarPerfil()
        {
            string correo = HttpContext.Session.GetString("USUARIO");
            Usuario usuario = JsonConvert.DeserializeObject<Usuario>(correo);

            return View(usuario);
        }
        [HttpPost]
        public async Task<IActionResult> EditarPerfil(Usuario usuarioModificado)
        {
            string correo = HttpContext.Session.GetString("USUARIO");
            Usuario usuario = JsonConvert.DeserializeObject<Usuario>(correo);

            usuarioModificado.Salt = usuario.Salt;
            usuarioModificado.PassEncript = usuario.PassEncript;

            await this.repo.ModificarUsuario(usuarioModificado);
 
            HttpContext.Session.SetObject("USUARIO", usuarioModificado);

            return RedirectToAction("MiPerfil", "Account");
        }


        [HttpPost]
        public async Task<IActionResult> Register
            (string nombreUsuario, string nombre, string apellido, string correo, string direccion, int idprovincia, string passEncript, string telefono)
        {
            await this.repo.RegisterUserAsync(nombreUsuario, nombre,apellido,correo, direccion, idprovincia, passEncript, telefono);
            ViewData["MENSAJE"] = "Usuario registrado correctamente";
            return RedirectToAction("Login");
        }


        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string correo, string contrasenia)
        {
            Usuario user = await this.repo.LogInUserAsync(correo, contrasenia);
            if (user == null)
            {
                ViewData["MENSAJE"] = "Credenciales incorrectas";
                return View();
            }
            else
            {
                HttpContext.Session.SetObject("USUARIO", user);

                bool esAdmin = await EsAdmin(user.Correo);

                HttpContext.Session.SetObject("EsAdmin", esAdmin);

                return View(user);
            }
        }

        private async Task<bool> EsAdmin(string userEmail)
        {
            return userEmail == "admin@gmail.com";
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("USUARIO");
            return RedirectToAction("Login");
        }


    }
}

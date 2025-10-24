
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMCS.Data;
using CMCS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace CMCS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        public AccountController(AppDbContext db) { _db = db; }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var vm = new RegisterViewModel();
            vm.RolesList = await _db.Roles
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name })
                .ToListAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            model.RolesList = await _db.Roles
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name })
                .ToListAsync();

            if (!ModelState.IsValid) return View(model);

            // Simple username uniqueness check
            if (await _db.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already exists.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                PasswordHash = HashPassword(model.Password),
                RoleId = model.RoleId
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Registration successful. You can now log in.";
            return RedirectToAction("Login"); // Redirect to login
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Message = TempData["Success"];
            return View();
        }

        private string HashPassword(string password)
        {
            // Simple SHA256 hash for demo only. Use a proper password hasher in production.
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}

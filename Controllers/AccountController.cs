using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CookieAuth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace CookieAuth.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationContext _db;

        public AccountController(ApplicationContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            var user = _db.Users.FirstOrDefault
                (u => u.Email == model.Email && u.Password == model.Password);

            if (user != null)
            {
                if (user.Status == "Blocked")
                {
                    ModelState.AddModelError("", "Ваш аккаунт заблокирован");
                    
                    return View(model);
                }
                
                user.LastLoginDate = DateTime.Now;

                await _db.SaveChangesAsync();
                
                await Authenticate(model.Email);

                return RedirectToAction("Index", "Home");
            }
                
            ModelState.AddModelError("", "Неправильный адрес электронной почтый или пароль");

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            var user = _db.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                _db.Users.Add(
                    new User
                    {
                        Email = model.Email, 
                        Name = model.Name,
                        Password = model.Password,
                        RegistrationDate = DateTime.Now,
                        LastLoginDate = DateTime.Now,
                        Status = "User",
                        IsSelected = false
                    });

                await _db.SaveChangesAsync();
 
                await Authenticate(model.Email);
 
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Неправильный адрес электронной почтый или пароль");
            
            return View(model);
        }

        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultNameClaimType, userName)
            };

            var claimsId = new ClaimsIdentity(
                claims,
                "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType
            );

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsId));
        }

        [HttpPost]
        public async Task<IActionResult> Block(string[] selectedUsersId)
        {
            foreach (var id in selectedUsersId)
            {
                var success = int.TryParse(id, out var idNum);

                if (!success) continue;
                
                var userToBlock = _db.Users.First(u => u.Id == idNum);
                userToBlock.Status = "Blocked";
                await _db.SaveChangesAsync();

                if (User.Identity != null && userToBlock.Email == User.Identity.Name)
                {
                    await Logout();
                }
            }
            
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(string[] selectedUsersId)
        {
            foreach (var id in selectedUsersId)
            {
                var success = int.TryParse(id, out var idNum);

                if (!success) continue;

                var userToUnblock = _db.Users.First(u => u.Id == idNum);
                userToUnblock.Status = "User";
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string[] selectedUsersId)
        {
            foreach (var id in selectedUsersId)
            {
                var success = int.TryParse(id, out var idNum);
                
                if (!success) continue;

                var userToDelete = _db.Users.First(u => u.Id == idNum);
                _db.Users.Remove(userToDelete);
                await _db.SaveChangesAsync();
                
                if (User.Identity != null && userToDelete.Email == User.Identity.Name)
                {
                    await Logout();
                }
            }
            
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }
    }
}
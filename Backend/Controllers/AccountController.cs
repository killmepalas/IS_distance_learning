using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IS_distance_learning.Models;
using IS_distance_learning.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace IS_distance_learning.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDBContext _context;
        public AccountController(AppDBContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Register()
        {
            ViewBag.Groups = await _context.Groups.ToListAsync();
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                Account account = await _context.Accounts.FirstOrDefaultAsync(u => u.Login == model.Login);
                if (account == null)
                {
                    account = new Account { Login = model.Login, Password = model.Password, Name = model.Name, LastName = model.LastName, MiddleName = model.MiddleName, RoleId = model.RoleId, GroupId = model.GroupId};

                    var role = await _context.Roles.FindAsync(model.RoleId);
                    account.Role = role;

                    _context.Accounts.Add(account);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Home");
                }
                else
                    ModelState.AddModelError("", "Некорректные данные или аккаунт с таким именем уже существует.");
            }
            return View(model);
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
            if (ModelState.IsValid)
            {
                Account account = await _context.Accounts
                    .Include(r => r.Role).FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == model.Password);
                if (account != null)
                {
                    await Authenticate(account); // аутентификация

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }
        private async Task Authenticate(Account account)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, account.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, account.Role.Name),
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString())
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var account = await _context.Accounts.Include(g => g.Group).Include(r => r.Role).FirstOrDefaultAsync(x => x.Id == accountId);
            return View(account);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index(int? RoleId, int? GroupId)
        {
            ViewBag.Groups = await _context.Groups.ToListAsync();
            ViewBag.Roles = await _context.Roles.ToListAsync();
            if (RoleId == 0)
            {
                var SelectedAccounts = new AccountModel { SelectedAccounts = await _context.Accounts.Include(g => g.Group).ToListAsync() };
                return View(SelectedAccounts);
            }
            else if (RoleId != 0 && GroupId == 0)
            {
                var SelectedAccounts = new AccountModel { SelectedAccounts = await _context.Accounts.Include(g => g.Group).Where(acc => acc.RoleId == RoleId).ToListAsync() };
                return View(SelectedAccounts);
            }
            else
            {
                var SelectedAccounts = new AccountModel { SelectedAccounts = await _context.Accounts.Include(g => g.Group).Where(acc => acc.RoleId == RoleId && acc.GroupId == GroupId).ToListAsync() };
                return View(SelectedAccounts);
            }
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id)
        {
            ViewBag.Groups = await _context.Groups.ToListAsync();
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, Account model)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            account.Login = model.Login;
            account.Password = model.Password;
            account.Name = model.Name;
            account.LastName = model.LastName;
            account.MiddleName = model.MiddleName;
            account.RoleId = model.RoleId;
            account.GroupId = model.GroupId;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Account", new { RoleId = 0, GroupId = 0 });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Account");
            }
            else
            {
                return NotFound();
            }
        }
    }
}

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
                List<Account> accounts = new List<Account>();
                List<Account> teachers = await _context.Teachers.ToListAsync();
                List<Account> admins = await _context.Admins.ToListAsync();
                List<Account> students = await _context.Students.ToListAsync();
                accounts.AddRange(teachers);
                accounts.AddRange(admins);
                accounts.AddRange(students);
                Account account = accounts.FirstOrDefault(a => a.Login == model.Login);
                if (account == null)
                {
                    var role = await _context.Roles.FindAsync(model.RoleId);
                    account.Role = role;
                    if(model.RoleId == 1)
                    {
                        account = new Admin { Login = model.Login, Password = model.Password, Name = model.Name, LastName = model.LastName, MiddleName = model.MiddleName, RoleId = model.RoleId };
                        await _context.Admins.AddAsync(account);
                    } else if (model.RoleId == 2)
                    {
                        account = new Teacher { Login = model.Login, Password = model.Password, Name = model.Name, LastName = model.LastName, MiddleName = model.MiddleName, RoleId = model.RoleId };
                        await _context.Teachers.AddAsync(account);
                    } else
                    {
                        account = new Student { Login = model.Login, Password = model.Password, Name = model.Name, LastName = model.LastName, MiddleName = model.MiddleName, RoleId = model.RoleId, GroupId = model.GroupId };
                        await _context.Students.AddAsync(account);
                    }
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Account");
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
                List<Account> accounts = new List<Account>();
                List<Account> teachers = await _context.Teachers.Include(a => a.Role).ToListAsync();
                List<Account> admins = await _context.Admins.Include(a => a.Role).ToListAsync();
                List<Account> students = await _context.Students.Include(a => a.Role).ToListAsync();
                accounts.AddRange(teachers);
                accounts.AddRange(admins);
                accounts.AddRange(students);
                var account = accounts.FirstOrDefault(u => u.Login == model.Login && u.Password == model.Password);
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
            Account account;
            var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (User.IsInRole("admin"))
            {
                account = await _context.Admins.Include(a => a.Role).FirstOrDefaultAsync(a => a.Id == accountId);
                return View(account);
            } else if(User.IsInRole("teacher"))
            {
                account = await _context.Teachers.Include(a => a.Role).FirstOrDefaultAsync(a => a.Id == accountId);
                return View(account);
            } else
            {
                account = await _context.Students.Include(a => a.Groups).Include(a => a.Role).FirstOrDefaultAsync(a => a.Id == accountId);
                return View(account);
            }
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index(int roleId = 0, int groupId = 0)
        {
            ViewBag.Groups = await _context.Groups.ToListAsync();
            ViewBag.Roles = await _context.Roles.ToListAsync();
            List<Account> accounts = new List<Account>();
            if (roleId == 0)
            {
                List<Account> teachers = await _context.Teachers.Include(a => a.Role).ToListAsync();
                List<Account> admins = await _context.Admins.Include(a => a.Role).ToListAsync();
                List<Account> students = await _context.Students.Include(a => a.Groups).Include(a => a.Role).ToListAsync();
                accounts.AddRange(teachers);
                accounts.AddRange(admins);
                accounts.AddRange(students);
                var selectedAccounts = new AccountModel { SelectedAccounts = accounts };
                return View(selectedAccounts);
            }
            else if (groupId == 0)
            {
                if (roleId == 1)
                {
                    List<Account> admins = await _context.Admins.Include(a => a.Role).ToListAsync();
                    var selectedAccounts = new AccountModel { SelectedAccounts = admins };
                    return View(selectedAccounts);
                } else if (roleId == 2)
                {
                    List<Account> teachers = await _context.Teachers.Include(a => a.Role).ToListAsync();
                    var selectedAccounts = new AccountModel { SelectedAccounts = teachers };
                    return View(selectedAccounts);
                } else
                {
                    List<Account> students = await _context.Students.Include(a => a.Groups).Include(a => a.Role).ToListAsync();
                    var selectedAccounts = new AccountModel { SelectedAccounts = students };
                    return View(selectedAccounts);
                }
            }
            else
            {
                List<Account> students = await _context.Students.Include(a => a.Groups).Where(a => a.GroupId == groupId).ToListAsync();
                var selectedAccounts = new AccountModel { SelectedAccounts = students };
                return View(selectedAccounts);
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
        public async Task<IActionResult> Update(int id, int roleId, Account model)
        {
            Account account;
            if (roleId == 1)
            {
                account = await _context.Admins.FindAsync(id);
                await _context.Admins.Remove(account);
            } else if (roleId == 2)
            {
                account = await _context.Teachers.FindAsync(id);
                 await _context.Teachers.Remove(account);
            } else
            {
                account = await _context.Students.FindAsync(id);
                await _context.Students.Remove(account);
            }

            if(model.RoleId == 1)
            {
                account.Login = model.Login;
                account.Password = model.Password;
                account.Name = model.Name;
                account.LastName = model.LastName;
                account.MiddleName = model.MiddleName;
                account.RoleId = model.RoleId;
                await _context.Admins.AddAsync(account);
            } else if(model.RoleId == 2)
            {
                account.Login = model.Login;
                account.Password = model.Password;
                account.Name = model.Name;
                account.LastName = model.LastName;
                account.MiddleName = model.MiddleName;
                account.RoleId = model.RoleId;
                await _context.Teachers.AddAsync(account);
            } else
            {
                account.Login = model.Login;
                account.Password = model.Password;
                account.Name = model.Name;
                account.LastName = model.LastName;
                account.MiddleName = model.MiddleName;
                account.RoleId = model.RoleId;
                account.GroupId = model.GroupId;
                await _context.Students.AddAsync(account);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Account", new { RoleId = 0, GroupId = 0 });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id, int roleId)
        {
            Account account;
            if (roleId == 1)
            {
                account = await _context.Admins.FindAsync(id);
                await _context.Admins.Remove(account);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Account");
            }
            else if (roleId == 2)
            {
                account = await _context.Teachers.FindAsync(id);
                await _context.Teachers.Remove(account);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Account");
            }
            else
            {
                account = await _context.Students.FindAsync(id);
                await _context.Students.Remove(account);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Account");
            }
        }
    }
}

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
using System;

namespace IS_distance_learning.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        public AccountController(AppDbContext context)
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
                    if (model.RoleId == 1)
                    {
                        account = new Account 
                        { 
                            Login = model.Login, 
                            Password = model.Password,
                            Name = model.Name,
                            LastName = model.LastName,
                            MiddleName = model.MiddleName,
                            RoleId = model.RoleId 
                        };                        
                        await _context.Accounts.AddAsync(account);
                        await _context.Admins.AddAsync(new Admin { Account = account });
                        await _context.SaveChangesAsync();
                    }
                    else if (model.RoleId == 2)
                    {
                        account = new Account 
                        { 
                            Login = model.Login,
                            Password = model.Password,
                            Name = model.Name,
                            LastName = model.LastName,
                            MiddleName = model.MiddleName, 
                            RoleId = model.RoleId 
                        };
                        await _context.Accounts.AddAsync(account);
                        await _context.Teachers.AddAsync(new Teacher { Account = account });
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        account = new Account 
                        {
                            Login = model.Login,
                            Password = model.Password,
                            Name = model.Name, 
                            LastName = model.LastName,
                            MiddleName = model.MiddleName,
                            RoleId = model.RoleId 
                        };
                        await _context.Accounts.AddAsync(account);
                        await _context.Students.AddAsync(new Student { GroupId = model.GroupId, Account = account });
                        await _context.SaveChangesAsync();
                    }

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
                Account account = await _context.Accounts
                    .Include(a => a.Role)
                    .FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == model.Password);
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
            ClaimsIdentity id = new(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
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
            Account account;
            if (User.IsInRole("student"))
            {
                account = await _context.Accounts
                    .Include(a => a.Student.Group)
                    .Include(a => a.Role)
                    .FirstOrDefaultAsync(a => a.Id == accountId);
                return View(account);
                
            }
            else
            {
                account = await _context.Accounts
                    .Include(a => a.Role)
                    .FirstOrDefaultAsync(a => a.Id == accountId);
                return View(account);
            }
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index(int roleId = 0, int groupId = 0)
        {
            ViewBag.Groups = await _context.Groups.ToListAsync();
            ViewBag.Roles = await _context.Roles.ToListAsync();
            List<Account> accounts = new ();
            if (roleId == 0)
            {
                accounts = await _context.Accounts
                    .Include(a => a.Teacher)
                    .Include(a => a.Admin)
                    .Include(a => a.Role)
                    .Include(a => a.Student)
                    .ThenInclude(s => s.Group)
                    .ToListAsync();
                var selectedAccounts = accounts;
                return View(selectedAccounts);
            }
            else if (groupId == 0)
            {
                accounts = await _context.Accounts
                    .Where(a => a.RoleId == roleId)
                    .Include(a => a.Admin)
                    .Include(a => a.Role)
                    .Include(a => a.Teacher)
                    .Include(a => a.Student)
                    .ThenInclude(s => s.Group)
                    .ToListAsync();
                var selectedAccounts = accounts;
                return View(selectedAccounts);
            }
            else
            {
                accounts = await _context.Accounts
                    .Include(a => a.Student)
                    .Where(a => a.Student.GroupId == groupId)
                    .ToListAsync();
                var selectedAccounts = accounts;
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
        public async Task<IActionResult> Update(int id, int? GroupId, Account model)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

             _context.Accounts.Remove(account);

            if (model.RoleId == 1)
            {
                account = new Account
                {
                    Login = model.Login,
                    Password = model.Password,
                    Name = model.Name,
                    LastName = model.LastName,
                    MiddleName = model.MiddleName,
                    RoleId = model.RoleId
                };
                await _context.Accounts.AddAsync(account);
                await _context.Admins.AddAsync(new Admin { Account = account });
                await _context.SaveChangesAsync();
            }
            else if (model.RoleId == 2)
            {
                account = new Account
                {
                    Login = model.Login,
                    Password = model.Password,
                    Name = model.Name,
                    LastName = model.LastName,
                    MiddleName = model.MiddleName,
                    RoleId = model.RoleId
                };
                await _context.Accounts.AddAsync(account);
                await _context.Teachers.AddAsync(new Teacher { Account = account });
                await _context.SaveChangesAsync();
            }
            else
            {
                account = new Account
                {
                    Login = model.Login,
                    Password = model.Password,
                    Name = model.Name,
                    LastName = model.LastName,
                    MiddleName = model.MiddleName,
                    RoleId = model.RoleId
                };
                await _context.Accounts.AddAsync(account);
                await _context.Students.AddAsync(new Student { GroupId = GroupId, Account = account });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Account", new { RoleId = 0, GroupId = 0 });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            Account account = await _context.Accounts
                .Include(a => a.Student).ThenInclude(s => s.CourseGrades)
                .Include(a => a.Student).ThenInclude(s => s.TestsGrades)
                .FirstOrDefaultAsync(a => a.Id == id);
            _context.Remove(account);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Account");
        }
    }
}

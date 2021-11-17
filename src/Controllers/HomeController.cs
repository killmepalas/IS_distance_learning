using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IS_distance_learning.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IS_distance_learning.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if(User.Identity.IsAuthenticated)
            {
                Account account = await _context.Accounts
                .Include(a => a.Teacher)
                .Include(a => a.Student)
                .ThenInclude(s => s.Group)
                .ThenInclude(g => g.Courses)
                .ThenInclude(c => c.Teacher)
                .FirstOrDefaultAsync(a => a.Id == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
                List<Course> courses = new();
                if (User.IsInRole("admin"))
                {
                    courses = await _context.Courses
                        .Include(c => c.Teacher)
                        .ThenInclude(t => t.Account)
                        .ToListAsync();
                }
                else if (User.IsInRole("teacher"))
                {

                    courses = await _context.Courses
                        .Include(c => c.Teacher)
                        .ThenInclude(t => t.Account)
                        .Where(c => c.TeacherId == account.Teacher.Id)
                        .ToListAsync();
                }
                else
                {
                    courses = await _context.Courses
                        .Include(c => c.Teacher)
                        .ThenInclude(t => t.Account)
                        .Include(c => c.Groups)
                        .Where(c => c.Groups.Contains(account.Student.Group))
                        .ToListAsync();
                }
                return View(courses);
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}
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
        private readonly AppDBContext _context;

        public HomeController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Course> courses = null;
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("admin"))
                {
                    courses = await _context.Courses.Include(c => c.Account).ToListAsync();
                }
                else if (User.IsInRole("teacher"))
                {

                    courses = await _context.Courses.Include(c => c.Account).Where(c => c.AccountId == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier))).ToListAsync();
                }
                else
                {
                    var student = await _context.Accounts
                        .Include(acc => acc.Group)
                        .ThenInclude(gr => gr.Courses)
                        .ThenInclude(c => c.Account)
                        .FirstOrDefaultAsync(a => a.Id == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));

                    courses = student.Group.Courses;
                }
            }

            return View(courses);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}
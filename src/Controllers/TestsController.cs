using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IS_distance_learning.Models;
using Microsoft.AspNetCore.Authorization;

namespace IS_distance_learning.Controllers
{
    public class TestsController : Controller
    {
        private readonly AppDbContext _context;

        public TestsController(AppDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Tests.Include(t => t.Course);
            return View(await appDbContext.ToListAsync());
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var test = await _context.Tests
                .Include(t => t.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public IActionResult Create()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name");
            return View();
        }
        
        [HttpPost]
        [Authorize(Roles = "teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Date,ExpirationDate,CourseId")] Test test)
        {
            if (ModelState.IsValid)
            {
                var course = await _context.Courses.FindAsync(test.CourseId);
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x =>
                    x.AccountId.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (course == null || course.TeacherId != teacher.Id || test.ExpirationDate.CompareTo(test.Date) <= 0)
                {
                    return BadRequest(new {error = "Data is invalid."});
                }
                await _context.AddAsync(test);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", test.CourseId);
            return View(test);
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> Edit(int id)
        {
            var test = await _context.Tests.FindAsync(id);
            if (test == null)
            {
                return NotFound();
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", test.CourseId);
            return View(test);
        }
        
        [HttpPost]
        [Authorize(Roles = "teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Description,Date,ExpirationDate,CourseId")] Test dto)
        {
            if (ModelState.IsValid)
            {
                var test = await _context.Tests.FindAsync(id);
                if (test == null)
                {
                    return NotFound();
                }
                var course = await _context.Courses.FindAsync(dto.CourseId);
                var teacher = await _context.Teachers.FirstOrDefaultAsync(x =>
                    x.AccountId.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (course == null || course.TeacherId != teacher.Id || dto.ExpirationDate.CompareTo(dto.Date) <= 0)
                {
                    return BadRequest(new {error = "Data is invalid."});
                }

                test.Name = dto.Name;
                test.Description = dto.Description;
                test.Date = dto.Date;
                test.ExpirationDate = dto.ExpirationDate;
                test.CourseId = dto.CourseId;
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name", dto.CourseId);
            return View(dto);
        }
        
        [HttpGet]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> Delete(int id)
        {
            var test = await _context.Tests
                .Include(t => t.Course)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (test == null)
            {
                return NotFound();
            }
            var course = await _context.Courses.FindAsync(test.CourseId);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(x =>
                x.AccountId.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (course.TeacherId != teacher.Id)
            {
                return Forbid();
            }

            return View(test);
        }
        
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var test = await _context.Tests.FindAsync(id);
            if (test == null)
            {
                return NotFound();
            }
            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IS_distance_learning.Models;
using IS_distance_learning.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace IS_distance_learning.Controllers
{
    public class CourseController : Controller
    {
        private readonly AppDbContext _context;

        public CourseController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "teacher,admin")]
        public async Task<IActionResult> Index()
        {
            List<Course> courses;
            Account account = await _context.Accounts.Include(a => a.Teacher).FirstOrDefaultAsync(a => a.Id == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            if (User.IsInRole("admin"))
            {
                courses = await _context.Courses.Include(c => c.Teacher).ThenInclude(t => t.Account).ToListAsync();
            }
            else
            {
                courses = await _context.Courses.Include(c => c.Teacher).ThenInclude(t => t.Account).Where(c => c.TeacherId == account.Teacher.Id).ToListAsync();
            }

            return View(courses);
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> Create(Course course, int AccountId)
        {
            if (ModelState.IsValid)
            {
                var account = await _context.Accounts.Include(a => a.Teacher).FirstOrDefaultAsync(a => a.Id == AccountId);
                course.TeacherId = account.Teacher.Id;
                await _context.AddAsync(course);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Course");
            }

            return View(course);
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> Update(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> Update(int id, Course course, int AccountId)
        {
            if (id != course.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var account = await _context.Accounts.Include(a => a.Teacher).FirstOrDefaultAsync(a => a.Id == AccountId);
                    course.TeacherId = account.Teacher.Id;
                    _context.Courses.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CourseExistsAsync(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction("Index", "Course");
            }

            return View(course);
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> AddGroups(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var groups = await _context.Groups.Where(gr => !gr.Courses.Contains(course)).ToListAsync();
            List<SelectListItem> itemList = new ();
            foreach (var g in groups)
            {
                SelectListItem selListItem = new () { Value = g.Id.ToString(), Text = g.Name + "  " + g.Code };
                itemList.Add(selListItem);
            }

            ViewBag.Groups = itemList;
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> AddGroups(int id, [Bind("Id,Name,AccountId")] Course model, int[] selectedGroups)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var course = await _context.Courses.FindAsync(model.Id);
            if (course == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (selectedGroups != null)
                    {
                        foreach (var g in _context.Groups.Where(gr => selectedGroups.Contains(gr.Id)))
                        {
                            course.Groups.Add(g);
                        }
                    }

                    _context.Courses.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CourseExistsAsync(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction("Index", "Course");
            }

            return View(course);
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> DeleteGroups(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var groups = await _context.Groups.Where(gr => gr.Courses.Contains(course)).ToListAsync();
            List<SelectListItem> itemList = new ();
            foreach (var g in groups)
            {
                SelectListItem selListItem = new () { Value = g.Id.ToString(), Text = g.Name + "  " + g.Code };
                itemList.Add(selListItem);
            }

            ViewBag.Groups = itemList;
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> DeleteGroups(int id, [Bind("Id,Name,AccountId")] Course model,
            int[] selectedGroups)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var course = await _context.Courses.Include(g => g.Groups).FirstOrDefaultAsync(c => c.Id == model.Id);
            if (course == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (selectedGroups != null)
                    {
                        foreach (var g in _context.Groups.Where(gr => selectedGroups.Contains(gr.Id)))
                        {
                            course.Groups.Remove(g);
                        }

                        _context.Courses.Update(course);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CourseExistsAsync(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction("Index", "Course");
            }

            return View(course);
        }

        [HttpPost]
        [Authorize(Roles = "teacher, admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Course");
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.Courses.Include(x => x.Tests).Include(c => c.Groups).Include(c => c.Teacher).ThenInclude(t => t.Account).FirstOrDefaultAsync(c => c.Id == id);
            if (User.IsInRole("admin"))
            {
                var details = new CourseDetailsModel { Name = course.Name, Description = course.Description, Teacher = course.Teacher, Groups = course.Groups, Tests = course.Tests };
                return View(details);
            }
            else if (User.IsInRole("teacher"))
            {
                var details = new CourseDetailsModel { CourseId = course.Id, Name = course.Name, Description = course.Description, Teacher = course.Teacher, Groups = course.Groups, Tests = course.Tests };
                return View(details);
            }
            else
            {
                var student = await _context.Accounts.FirstOrDefaultAsync(x => x.Login == User.Identity.Name);
                var attempts = await _context.Attempts.Include(x => x.Test).Where(x => x.StudentId == student.Student.Id && x.Test.CourseId == course.Id).ToListAsync();
                
                var tests = course.Tests.Where(x => !attempts.Any() || attempts.All(y => y.TestId != x.Id)).ToList();

                var details = new CourseDetailsModel { Name = course.Name, Description = course.Description, Teacher = course.Teacher, Tests = tests, Attempts = attempts};
                return View(details);
            }
        }

        private async Task<bool> CourseExistsAsync(int id)
        {
            return await _context.Courses.AnyAsync(e => e.Id == id);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IS_distance_learning.Models;
using Microsoft.AspNetCore.Authorization;

namespace IS_distance_learning.Controllers
{
    public class CourseController : Controller
    {
        private readonly AppDBContext _context;

        public CourseController(AppDBContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses.Include(c => c.Account).ToListAsync();
            return View(courses);
        }

        public IActionResult Create()
        {
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "LastName");
            return View();
        }

        [Authorize(Roles = "teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,AccountId")] Course course)
        {
            if (ModelState.IsValid)
            {
                await _context.AddAsync(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "LastName", course.AccountId);
            return View(course);
        }

        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "LastName", course.AccountId);
            return View(course);
        }

        [Authorize(Roles = "teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [Bind("Id,Name,AccountId")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
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
                ;
            }

            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "LastName", course.AccountId);
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
            List<SelectListItem> itemList = new List<SelectListItem> { };
            foreach (var g in groups)
            {
                SelectListItem selListItem = new SelectListItem()
                    {Value = g.Id.ToString(), Text = g.Name + "  " + g.Code};
                itemList.Add(selListItem);
            }

            ViewBag.Groups = itemList;
            return View(course);
        }

        [Authorize(Roles = "teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGroups(int id, [Bind("Id,Name,AccountId")] Course model,
            int[] selectedGroups)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(model.Id);
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

                    _context.Update(course);
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
                ;
            }

            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "LastName", course.AccountId);
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
            List<SelectListItem> itemList = new List<SelectListItem> { };
            foreach (var g in groups)
            {
                SelectListItem selListItem = new SelectListItem()
                    {Value = g.Id.ToString(), Text = g.Name + "  " + g.Code};
                itemList.Add(selListItem);
            }

            ViewBag.Groups = itemList;
            return View(course);
        }

        [Authorize(Roles = "teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroups(int id, [Bind("Id,Name,AccountId")] Course model,
            int[] selectedGroups)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var course = await _context.Courses.Include(g => g.Groups).FirstOrDefaultAsync(c => c.Id == model.Id);
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
                ;
            }

            return View(course);
        }

        [HttpPost]
        [Authorize(Roles = "teacher")]
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

        private async Task<bool> CourseExistsAsync(int id)
        {
            return await _context.Courses.AnyAsync(e => e.Id == id);
        }
    }
}
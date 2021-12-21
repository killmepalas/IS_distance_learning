using IS_distance_learning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IS_distance_learning.Controllers
{
    public class CourseGradeController : Controller
    {
        private readonly AppDbContext _context;
        public CourseGradeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int grade)
        {
            if (ModelState.IsValid)
            {
                var courseGrade = await _context.CoursesGrades.FindAsync(id);
                if (courseGrade == null)
                {
                    return NotFound();
                }

                courseGrade.Grade = grade;

                _context.CoursesGrades.Update(courseGrade);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Course", new { id = courseGrade.CourseId });
            }

            return View(grade);
        }
    }
}

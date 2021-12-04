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
        public async Task<IActionResult> Details(int id)
        {
            var test = await _context.Tests.Include(t => t.Questions).FirstOrDefaultAsync(x => x.Id == id);
            if (test == null)
            {
                return NotFound();
            } 
            return View(test);
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public IActionResult Create(int CourseId)
        {
            ViewBag.CourseId = CourseId;
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
                return RedirectToAction("Details", "Course", new {id = course.Id});
            }
            return View(test);
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> Update(int id)
        {
            var test = await _context.Tests.FindAsync(id);
            if (test == null)
            {
                return NotFound();
            }
            return View(test);
        }
        
        [HttpPost]
        [Authorize(Roles = "teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [Bind("Name,Description,Date,ExpirationDate,CourseId")] Test dto)
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
                return RedirectToAction("Details", "Course", new {id = course.Id});
            }
            return View(dto);
        }

        [HttpPost]
        [Authorize(Roles = "teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var test = await _context.Tests.FindAsync(id);
            if (test == null)
            {
                return NotFound();
            }
            var course = await _context.Courses.FindAsync(test.CourseId);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(x =>
                x.AccountId.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (course.TeacherId != teacher.Id)
            {
                Forbid();
            }
            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Course", new {id = course.Id});
        }

        [HttpGet]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> Pass(int testId, int questionId = 0)
        {
            var test = await _context.Tests.Include(x => x.Questions).ThenInclude(x => x.Answers)
                .FirstOrDefaultAsync(x => x.Id == testId);
            if (questionId == 0)
            {
                return View(test.Questions.First());
            }

            var question = test.Questions.FirstOrDefault(x => x.Id == questionId);
            if (question == null)
            {
                return RedirectToAction("Details", "Tests", new {id = test.Id});
            }

            return View(question);
        }

        [HttpPost]
        [Authorize(Roles = "student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pass(Answer dto)
        {
            if (ModelState.IsValid)
            {
                
                var answer = await _context.Answers.FindAsync(dto.Id);
                var question = await _context.Questions.FindAsync(answer.QuestionId);
                var test = await _context.Tests.FindAsync(question.TestId);
                var student = await _context.Accounts.FirstOrDefaultAsync(x => x.Login == User.Identity.Name);
                var attempt = await _context.Attempts.FirstOrDefaultAsync(x => x.StudentId == student.Id && x.TestId == test.Id);
                if (attempt == null)
                {
                    attempt = new Attempt
                    {
                        StudentId = student.Id,
                        TestId = test.Id,
                        Mark = (answer.IsRight ? 1 : 0)
                    };
                    
                    await _context.Attempts.AddAsync(attempt);
                }
                else
                {
                    attempt.Mark += (answer.IsRight ? 1 : 0);
                    _context.Attempts.Update(attempt);
                }
                
                await _context.SaveChangesAsync();
                var next = await _context.Questions.FirstOrDefaultAsync(x => x.Id > question.Id && x.TestId == test.Id);
                if (next == null)
                {
                    return RedirectToAction("Details", "Course", new {id = test.CourseId});
                }
                return RedirectToAction("Pass", "Tests", new {testId = test.Id, questionId = next.Id});
            }
            ModelState.AddModelError("", "Ответ на вопрос не выбран!");

            return View();
        }
    }
}

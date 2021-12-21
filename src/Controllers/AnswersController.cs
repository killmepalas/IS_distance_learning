using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IS_distance_learning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IS_distance_learning.Controllers
{
    public class AnswersController : Controller
    {
        private readonly AppDbContext _context;

        public AnswersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public IActionResult Create(int QuestionId)
        {
            ViewBag.QuestionId = QuestionId;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Text,IsRight,QuestionId")] Answer dto)
        {
            if (ModelState.IsValid)
            {
                await _context.Answers.AddAsync(dto);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Questions", new {id = dto.QuestionId});
            }

            return View(dto);
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> Update(int id)
        {
            var answer = await _context.Answers.FindAsync(id);
            if (answer == null)
            {
                return NotFound();
            }

            return View(answer);
        }

        [HttpPost]
        [Authorize(Roles = "teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [Bind("Text,IsRight")] Answer dto)
        {
            if (ModelState.IsValid)
            {
                var answer = await _context.Answers.FindAsync(id);
                if (answer == null)
                {
                    return NotFound();
                }

                answer.Text = dto.Text;
                answer.IsRight = dto.IsRight;

                _context.Answers.Update(answer);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Questions", new {id = answer.QuestionId});
            }

            return View(dto);
        }

        [HttpPost]
        [Authorize(Roles = "teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var answer = await _context.Answers.FindAsync(id);
            if (answer == null)
            {
                return NotFound();
            }

            _context.Answers.Remove(answer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Questions", new {id = answer.QuestionId});
        }
        
        [HttpGet]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> Index(int courseId, int testId = 0, int groupId = 0)
        {
            ViewBag.CourseId = courseId;
            ViewBag.Groups = await _context.Groups.ToListAsync();
            ViewBag.Tests = await _context.Tests.Where(t => t.CourseId == courseId).ToListAsync();
            List<TestGrade> testsGrades = new ();
            if (testId != 0 && groupId != 0)
            {
                testsGrades = await _context.TestsGrades
                    .Include(tg => tg.Test)
                    .ThenInclude(t => t.Attempts)
                    .Include(tg => tg.Student)
                    .ThenInclude(s => s.Account)
                    .Include(tg => tg.Student)
                    .ThenInclude(s => s.Group)
                    .Where(tg => tg.Student.GroupId == groupId && tg.TestId == testId && tg.Test.CourseId == courseId)
                    .ToListAsync();
                return View(testsGrades);
            }
            else if (testId == 0 && groupId == 0)
            {
                testsGrades = await _context.TestsGrades
                    .Include(tg => tg.Test)
                    .ThenInclude(t => t.Attempts)
                    .Where(tg => tg.Test.CourseId == courseId)
                    .Include(tg => tg.Student)
                    .ThenInclude(s => s.Account)
                    .Include(tg => tg.Student)
                    .ThenInclude(s => s.Group)
                    .ToListAsync();
                return View(testsGrades);
            }
            else if (testId == 0)
            {
                testsGrades = await _context.TestsGrades
                    .Include(tg => tg.Test)
                    .ThenInclude(t => t.Attempts)
                    .Include(tg => tg.Student)
                    .ThenInclude(s => s.Account)
                    .Include(tg => tg.Student)
                    .ThenInclude(s => s.Group)
                    .Where(tg => tg.Student.GroupId == groupId && tg.Test.CourseId == courseId)
                    .ToListAsync();
                return View(testsGrades);
            }
            else
            {
                testsGrades = await _context.TestsGrades
                    .Include(tg => tg.Test)
                    .ThenInclude(t => t.Attempts)
                    .Include(tg => tg.Student)
                    .ThenInclude(s => s.Account)
                    .Include(tg => tg.Student)
                    .ThenInclude(s => s.Group)
                    .Where(tg => tg.TestId == testId && tg.Test.CourseId == courseId)
                    .ToListAsync();
                return View(testsGrades);
            }
        }
    }
}
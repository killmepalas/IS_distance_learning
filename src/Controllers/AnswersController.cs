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
        [Authorize]
        public async Task<IActionResult> Details(int questionId, int id)
        {
            var answer = await _context.Answers.FirstOrDefaultAsync(x => x.QuestionId == questionId && x.Id == id);
            if (answer == null)
            {
                return NotFound();
            }

            return View(answer);
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
            ViewBag.Tests = await _context.Tests.Where(x => x.CourseId == courseId).ToListAsync();
            List<Attempt> attempts = new ();
            if (testId != 0 && groupId != 0)
            {
                attempts = await _context.Attempts.Include(x => x.Test).ThenInclude(t => t.Questions).Where(x => x.Test.CourseId == courseId)
                    .Include(x => x.Test)
                    .Include(a => a.Student)
                    .ThenInclude(s => s.Account)
                    .Where(a => a.Student.GroupId == groupId && a.TestId == testId)
                    .ToListAsync();
                var selectedAttempts = attempts;
                return View(selectedAttempts);
            }
            else if (testId == 0 && groupId == 0)
            {
                attempts = await _context.Attempts.Include(x => x.Test).Where(x => x.Test.CourseId == courseId)
                    .Include(a => a.Test)
                    .ThenInclude(t => t.Questions)
                    .Include(a => a.Student)
                    .ThenInclude(s => s.Account)
                    .ToListAsync();
                var selectedAttempts = attempts;
                return View(selectedAttempts);
            }
            else if (testId == 0)
            {
                attempts = await _context.Attempts.Include(x => x.Test).Where(x => x.Test.CourseId == courseId)
                   .Include(a => a.Test)
                   .ThenInclude(t => t.Questions)
                   .Include(a => a.Student)
                   .ThenInclude(s => s.Account)
                   .Where(a => a.Student.GroupId == groupId)
                   .ToListAsync();
                var selectedAttempts = attempts;
                return View(selectedAttempts);
            }
            else
            {
                attempts = await _context.Attempts.Include(x => x.Test).ThenInclude(t => t.Questions).Where(x => x.Test.CourseId == courseId)
                    .Include(x => x.Test)
                    .Include(a => a.Student)
                    .ThenInclude(s => s.Account)
                    .Where(a => a.TestId == testId)
                    .ToListAsync();
                var selectedAttempts = attempts;
                return View(selectedAttempts);
            }
        }
    }
}
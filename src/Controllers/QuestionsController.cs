using System.Threading.Tasks;
using IS_distance_learning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IS_distance_learning.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly AppDbContext _context;

        public QuestionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var question = await _context.Questions.Include(q => q.Answers).FirstOrDefaultAsync(x => x.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public IActionResult Create(int TestId)
        {
            ViewBag.TestId = TestId;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Text,TestId")] Question dto)
        {
            if (ModelState.IsValid)
            {
                await _context.Questions.AddAsync(dto);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Tests", new {id = dto.TestId});
            }

            return View(dto);
        }

        [HttpGet]
        [Authorize(Roles = "teacher")]
        public async Task<IActionResult> Update(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        [HttpPost]
        [Authorize(Roles = "teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [Bind("Text")] Question dto)
        {
            if (ModelState.IsValid)
            {
                var question = await _context.Questions.FindAsync(id);
                if (question == null)
                {
                    return NotFound();
                }

                question.Text = dto.Text;

                _context.Questions.Update(question);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Tests", new {id = question.TestId});
            }

            return View(dto);
        }

        [HttpPost]
        [Authorize(Roles = "teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Tests", new {id = question.TestId});
        }
    }
}
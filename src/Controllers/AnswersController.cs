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
    }
}
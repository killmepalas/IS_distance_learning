using System.Threading.Tasks;
using IS_distance_learning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IS_distance_learning.Controllers
{
    public class GroupController : Controller
    {
        private readonly AppDbContext _context;

        public GroupController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var groups = await _context.Groups.ToListAsync();
            return View(groups);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create(Group model)
        {
            if (ModelState.IsValid)
            {
                var group = await _context.Groups.FirstOrDefaultAsync(g => g.Name == model.Name && g.Code == model.Code);
                if (group == null)
                {
                    group = new Group() {Name = model.Name, Code = model.Code};
                    await _context.Groups.AddAsync(group);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Group");
                }
                else
                {
                    ModelState.AddModelError("", "Некорректные данные или такая группа уже существует.");
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, Group model)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            group.Name = model.Name;
            group.Code = model.Code;
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Group");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group != null)
            {
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Group");
            }
            else
            {
                return NotFound();
            }
        }
    }
}
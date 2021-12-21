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
            var test = await _context.Tests.Include(t => t.Course).Include(t => t.Questions).FirstOrDefaultAsync(x => x.Id == id);
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
        public async Task<IActionResult> Create([Bind("Name,Description,Date,ExpirationDate,AttemptCount,CourseId")] Test test)
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
        public async Task<IActionResult> Update(int id, [Bind("Name,Description,Date,ExpirationDate,AttemptCount,CourseId")] Test dto)
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
                test.AttemptCount = dto.AttemptCount;
                test.CourseId = dto.CourseId;
                _context.Tests.Update(test);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Course", new {id = course.Id});
            }
            return View(dto);
        }

        [HttpPost]
        [Authorize(Roles = "teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var test = await _context.Tests
                .Include(t => t.Attempts)
                .Include(t => t.TestsGrades)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (test == null)
            {
                return NotFound();
            }
            var course = await _context.Courses.FindAsync(test.CourseId);
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.AccountId.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier));
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
        public async Task<IActionResult> Pass(int testId, int questionId = 0, int attemptId = 0, int newAttempt = 0)
        {
            var test = await _context.Tests.Include(x => x.Course).Include(x => x.Questions).ThenInclude(x => x.Answers)
                .FirstOrDefaultAsync(x => x.Id == testId);
            var account = await _context.Accounts.Include(x => x.Student).ThenInclude(s => s.Attempts).FirstOrDefaultAsync(x => x.Login == User.Identity.Name);
            var attempt = await _context.Attempts.FirstOrDefaultAsync(a => a.Id == attemptId && a.TestId == testId && a.StudentId == account.Student.Id);

            if (attempt == null || newAttempt == 1)
            {
                attempt = new Attempt
                {
                    StudentId = account.Student.Id,
                    TestId = test.Id,
                    Mark = 0,
                    Grade = 0
                };
                await _context.Attempts.AddAsync(attempt);
                await _context.SaveChangesAsync();
                ViewBag.attemptId = attempt.Id;
            }
            ViewBag.attemptId = attempt.Id;
            if (questionId == 0)
            {
                return View(test.Questions.First());
            }

            var question = test.Questions.FirstOrDefault(x => x.Id == questionId);
            if (question == null)
            {
                return RedirectToAction("Details", "Course", new {id = test.Course.Id});
            }

            return View(question);
        }

        [HttpPost]
        [Authorize(Roles = "student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pass(int id, int attemptId)
        {
            if (ModelState.IsValid)
            {
                
                var answer = await _context.Answers.FindAsync(id);
                if (answer == null)
                {
                    return NotFound();
                }
                var question = await _context.Questions.FindAsync(answer.QuestionId);
                var test = await _context.Tests.Include(t => t.Questions).FirstOrDefaultAsync(t => t.Id == question.TestId);
                var account = await _context.Accounts.Include(a => a.Student).ThenInclude(s => s.Attempts).FirstOrDefaultAsync(a => a.Login == User.Identity.Name);
                var studentAttempts = await _context.Attempts.Where(a => a.StudentId == account.Student.Id && a.TestId == test.Id).ToListAsync();
                var attempt = await _context.Attempts.FirstOrDefaultAsync(a => a.StudentId == account.Student.Id && a.TestId == test.Id && a.Id == attemptId);

                attempt.Mark += (answer.IsRight ? 1 : 0);
                attempt.Grade = GetAttemptGrade(attempt.Mark, test.Questions.Count);

                _context.Attempts.Update(attempt);
                await _context.SaveChangesAsync();

                var next = await _context.Questions.FirstOrDefaultAsync(q => q.Id > question.Id && q.TestId == test.Id);
                if (next == null)
                {
                    var testGrade = await _context.TestsGrades.FirstOrDefaultAsync(tg => tg.TestId == test.Id && tg.StudentId == account.Student.Id);

                    if (testGrade == null)
                    {
                        testGrade = new TestGrade
                        {
                            StudentId = account.Student.Id,
                            TestId = test.Id,
                            Grade = GetTestGrade(studentAttempts)
                        };
                        await _context.TestsGrades.AddAsync(testGrade);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        testGrade.Grade = GetTestGrade(studentAttempts);
                        _context.TestsGrades.Update(testGrade);
                        await _context.SaveChangesAsync();
                    }

                    var testsGrades = await _context.TestsGrades.Include(tg => tg.Test).Where(tg => tg.StudentId == account.Student.Id && tg.Test.CourseId == test.CourseId).ToListAsync();
                    var courseGrade = await _context.CoursesGrades.FirstOrDefaultAsync(cg => cg.StudentId == account.Student.Id && cg.CourseId == test.CourseId);
                    if (courseGrade == null)
                    {
                        courseGrade = new CourseGrade
                        {
                            CourseId = test.CourseId,
                            StudentId = account.Student.Id,
                            Grade = GetCourseGrade(testsGrades)
                        };
                        await _context.CoursesGrades.AddAsync(courseGrade);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        courseGrade.Grade = GetCourseGrade(testsGrades);
                        _context.CoursesGrades.Update(courseGrade);
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction("Details", "Course", new {id = test.CourseId});
                }
                return RedirectToAction("Pass", "Tests", new {testId = test.Id, questionId = next.Id, attemptId = attempt.Id});
            }
            ModelState.AddModelError("", "Ответ на вопрос не выбран!");

            return View();
        }

        private static int GetAttemptGrade(int CorrectAnswersCount, int QuestionsCount)
        {
            int percent = (Int32)(CorrectAnswersCount / (QuestionsCount / 100M));
            if (percent >= 90 )
            {
                return 5;
            } 
            else if (percent >= 80)
            {
                return 4;
            }
            else if (percent >= 70)
            {
                return 3;
            }
            else if (percent >= 60)
            {
                return 2;
            }
            else if (percent >= 50)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private static int GetTestGrade(List<Attempt> attempts)
        {
            int attemptsCount = attempts.Count;
            int sum = 0;
            int grade;

            foreach(Attempt a in attempts)
            {
                sum += a.Grade;
            }

            double tmp = sum / attemptsCount;

            if (attemptsCount <= 2)
            {
                grade = (Int32)Math.Round(tmp, MidpointRounding.AwayFromZero);
                if (grade < 0)
                {
                    return 0;
                }
                return grade;
            }
            tmp = sum / attemptsCount - ((attemptsCount - 2) * 0.5);
            grade = (Int32)Math.Round(tmp, MidpointRounding.AwayFromZero);
            if (grade < 0)
            {
                return 0;
            }
            return grade;
        }

        private static int GetCourseGrade(List<TestGrade> testGrades)
        {
            int sum = 0;

            foreach (TestGrade tg in testGrades)
            {
                sum += tg.Grade;
            }

            double tmp = sum / testGrades.Count;

            return (Int32)Math.Round(tmp, MidpointRounding.AwayFromZero);
        }

    }
}


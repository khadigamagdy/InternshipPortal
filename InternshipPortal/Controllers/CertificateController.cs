using InternshipPortal.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(Roles = "Student")]
    public class CertificateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CertificateController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int evaluationId)
        {
            var userId = _userManager.GetUserId(User);

            var evaluation = await _context.Evaluations
                .Include(e => e.InternshipApplication)
                    .ThenInclude(a => a.Student)
                .Include(e => e.InternshipApplication)
                    .ThenInclude(a => a.Internship)
                        .ThenInclude(i => i.Company)
                .FirstOrDefaultAsync(e =>
                    e.Id == evaluationId &&
                    e.InternshipApplication.Student.UserId == userId);

            if (evaluation == null)
            {
                return NotFound();
            }

            return View(evaluation);
        }
    }
}
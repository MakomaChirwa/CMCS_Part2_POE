using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CMCS.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace CMCS.Controllers
{
    [Authorize(Roles = "ProgrammeCoordinator")]
    public class CoordinatorController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CoordinatorController(ApplicationDbContext db) { _db = db; }

        public async Task<IActionResult> PreApprove()
        {
            var pending = await _db.Claims.Where(c => c.Status == CMCS.Models.ClaimStatus.Pending).ToListAsync();
            return View(pending);
        }

        [HttpPost]
        public async Task<IActionResult> PreApprove(int id, bool approve)
        {
            var c = await _db.Claims.FindAsync(id);
            if(c == null) return NotFound();
            c.Status = approve ? CMCS.Models.ClaimStatus.PreApproved : CMCS.Models.ClaimStatus.Rejected;
            c.ProcessedAt = System.DateTime.UtcNow;
            c.ProcessedBy = User.Identity.Name;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(PreApprove));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CMCS.Data;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Controllers
{
    [Authorize(Roles = "AcademicManager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ManagerController(ApplicationDbContext db) { _db = db; }

        public async Task<IActionResult> Approve()
        {
            var preapproved = await _db.Claims.Where(c => c.Status == CMCS.Models.ClaimStatus.PreApproved).ToListAsync();
            return View(preapproved);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id, bool approve)
        {
            var c = await _db.Claims.FindAsync(id);
            if(c == null) return NotFound();
            c.Status = approve ? CMCS.Models.ClaimStatus.Approved : CMCS.Models.ClaimStatus.Rejected;
            c.ProcessedAt = System.DateTime.UtcNow;
            c.ProcessedBy = User.Identity.Name;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Approve));
        }
    }
}

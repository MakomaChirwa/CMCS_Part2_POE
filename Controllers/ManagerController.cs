using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CMCS.Data;
using CMCS.Models; 
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Controllers
{
    [Authorize(Roles = "AcademicManager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ManagerController(ApplicationDbContext db) { _db = db; }


        // ---------------------------------------------------
        // 1. FETCH CLAIMS THAT ARE PRE-APPROVED BY COORDINATOR
        // ---------------------------------------------------
        public async Task<IActionResult> Approve()
        {
            var preapproved = await _db.Claims
                .Where(c => c.Status == ClaimStatus.PreApproved)
                .ToListAsync();

            // RUN AUTOMATION ON EACH CLAIM
            foreach (var claim in preapproved)
            {
                claim.AutomationMessages = RunAutomationChecks(claim);
            }

            return View(preapproved);
        }


        // ---------------------------------------------------
        // 2. ACADEMIC MANAGER APPROVAL ACTION
        // ---------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Approve(int id, bool approve)
        {
            var c = await _db.Claims.FindAsync(id);
            if (c == null) return NotFound();

            // Run automation before final approval
            var messages = RunAutomationChecks(c);

            // Block approval if automation finds critical errors
            if (messages.Any(m => m.Contains("ERROR")))
            {
                TempData["AutomationError"] =
                    "Approval blocked: Automation found critical issues. Fix errors first.";
                return RedirectToAction(nameof(Approve));
            }

            // Final decision
            c.Status = approve ? ClaimStatus.Approved : ClaimStatus.Rejected;
            c.ProcessedAt = System.DateTime.UtcNow;
            c.ProcessedBy = User.Identity.Name;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Approve));
        }


        // ---------------------------------------------------
        // 3. AUTOMATION ENGINE - Academic Manager Automation
        // ---------------------------------------------------
        private List<string> RunAutomationChecks(Claim claim)
        {
            var messages = new List<string>();

            // 1. Required Fields
            if (claim.HoursWorked <= 0)
                messages.Add("ERROR: Hours worked must be greater than 0.");

            if (claim.HourlyRate <= 0)
                messages.Add("ERROR: Hourly rate must be greater than 0.");

            // 2. Policy: Maximum 160 hours a month
            if (claim.HoursWorked > 160)
                messages.Add("ERROR: Hours exceed maximum limit of 160 per month.");

            // 3. Policy: Hourly rate range
            if (claim.HourlyRate < 150)
                messages.Add("WARNING: Hourly rate is below minimum HR policy rate (R150).");

            if (claim.HourlyRate > 450)
                messages.Add("WARNING: Hourly rate exceeds maximum HR policy rate (R450).");

            // 4. Auto-calculate amount
            var expectedAmount = claim.HoursWorked * claim.HourlyRate;

            if (claim.TotalAmount != expectedAmount)
            {
                messages.Add(
                    $"ERROR: Total amount mismatch. Expected: R{expectedAmount}, Found: R{claim.TotalAmount}"
                );
            }

            // 5. Compliance
            if (!messages.Any())
                messages.Add("OK: Claim fully complies with all policy rules.");

            return messages;
        }
    }
}

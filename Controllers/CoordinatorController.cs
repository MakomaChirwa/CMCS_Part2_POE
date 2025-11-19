using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CMCS.Data;
using CMCS.Models; 
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;


namespace CMCS.Controllers
{
    [Authorize(Roles = "ProgrammeCoordinator")]
    public class CoordinatorController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CoordinatorController(ApplicationDbContext db) { _db = db; }


        // ------------------------------------------------------
        // 1. PULL ALL PENDING CLAIMS (Before Pre-Approval)
        // ------------------------------------------------------
        public async Task<IActionResult> PreApprove()
        {
            var pending = await _db.Claims
                .Where(c => c.Status == ClaimStatus.Pending)
                .ToListAsync();

            // Add automation results to each claim so the coordinator can see issues
            foreach (var claim in pending)
            {
                claim.AutomationMessages = RunAutomationChecks(claim);
            }

            return View(pending);
        }


        // ------------------------------------------------------
        // 2. PRE-APPROVAL ACTION (With Automation Enforcement)
        // ------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> PreApprove(int id, bool approve)
        {
            var c = await _db.Claims.FindAsync(id);
            if (c == null) return NotFound();

            // Automation check BEFORE pre-approval
            var messages = RunAutomationChecks(c);

            // Critical errors WILL block pre-approval
            if (messages.Any(m => m.Contains("ERROR")))
            {
                TempData["AutomationError"] =
                    "Pre-approval blocked: Automation found critical issues. Fix errors first.";
                return RedirectToAction(nameof(PreApprove));
            }

            // Safe to pre-approve
            c.Status = approve ? ClaimStatus.PreApproved : ClaimStatus.Rejected;
            c.ProcessedAt = System.DateTime.UtcNow;
            c.ProcessedBy = User.Identity.Name;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(PreApprove));
        }


        // ------------------------------------------------------
        // 3. AUTOMATION ENGINE (Same logic as Academic Manager)
        // ------------------------------------------------------
        private List<string> RunAutomationChecks(Claim claim)
        {
            var messages = new List<string>();

            // 1. Required field checks
            if (claim.HoursWorked <= 0)
                messages.Add("ERROR: Hours worked must be greater than 0.");

            if (claim.HourlyRate <= 0)
                messages.Add("ERROR: Hourly rate must be greater than 0.");

            // 2. HR policy: Max 160 hours per month
            if (claim.HoursWorked > 160)
                messages.Add("ERROR: Hours exceed maximum limit of 160 per month.");

            // 3. HR policy: Hourly rate must be between R150–R450
            if (claim.HourlyRate < 150)
                messages.Add("WARNING: Hourly rate is below minimum HR policy rate (R150).");

            if (claim.HourlyRate > 450)
                messages.Add("WARNING: Hourly rate exceeds maximum HR policy rate (R450).");

            // 4. Auto total calculation
            var expectedAmount = claim.HoursWorked * claim.HourlyRate;

            if (claim.TotalAmount != expectedAmount)
            {
                messages.Add($"ERROR: Total amount mismatch. Expected: R{expectedAmount}, Found: R{claim.TotalAmount}");
            }

            // 5. If no issues, compliance OK
            if (!messages.Any())
                messages.Add("OK: Claim complies with all policy rules.");

            return messages;
        }
    }
}

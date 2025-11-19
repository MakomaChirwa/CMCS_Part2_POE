using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CMCS.Data;
using CMCS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CMCS.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HRController(ApplicationDbContext db) { _db = db; }

        // VIEW: Shows all approved claims
        public async Task<IActionResult> ProcessClaims()
        {
            var approvedClaims = await _db.Claims
                .Where(c => c.Status == ClaimStatus.Approved)
                .ToListAsync();

            return View(approvedClaims);
        }

        // ACTION: Marks claim as processed and generates invoice number
        [HttpPost]
        public async Task<IActionResult> MarkProcessed(int id)
        {
            var claim = await _db.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.IsPaid = true;
            claim.InvoiceNumber = "INV-" + DateTime.Now.Ticks; // Auto invoice
            claim.ProcessedAt = DateTime.UtcNow;
            claim.ProcessedBy = User.Identity.Name;

            await _db.SaveChangesAsync();

            TempData["HRMessage"] = "Claim processed successfully and invoice generated.";

            return RedirectToAction(nameof(ProcessClaims));
        }
    }
}

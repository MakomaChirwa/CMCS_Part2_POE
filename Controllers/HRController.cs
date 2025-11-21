using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<IdentityUser> _userManager;

        public HRController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // --------------------------------------------------------------------
        // SECTION A — CLAIM PROCESSING
        // --------------------------------------------------------------------
        public async Task<IActionResult> ProcessClaims()
        {
            var approvedClaims = await _db.Claims
                .Where(c => c.Status == ClaimStatus.Approved)
                .ToListAsync();

            return View(approvedClaims);
        }

        [HttpPost]
        public async Task<IActionResult> MarkProcessed(int id)
        {
            var claim = await _db.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.IsPaid = true;
            claim.InvoiceNumber = "INV-" + DateTime.Now.Ticks;
            claim.ProcessedAt = DateTime.UtcNow;
            claim.ProcessedBy = User.Identity.Name;

            await _db.SaveChangesAsync();

            TempData["HRMessage"] = "Claim processed successfully.";
            return RedirectToAction(nameof(ProcessClaims));
        }

        // --------------------------------------------------------------------
        // SECTION B — LECTURER MANAGEMENT
        // --------------------------------------------------------------------

        // VIEW ALL LECTURERS
        public async Task<IActionResult> Lecturers()
        {
            var lecturers = await _userManager.GetUsersInRoleAsync("Lecturer");
            return View(lecturers);
        }

        // EDIT LECTURER (GET)
        public async Task<IActionResult> EditLecturer(string id)
        {
            if (id == null) return NotFound();

            var lecturer = await _userManager.FindByIdAsync(id);
            if (lecturer == null) return NotFound();

            return View(lecturer);
        }

        // EDIT LECTURER (POST)
        [HttpPost]
        public async Task<IActionResult> EditLecturer(IdentityUser model)
        {
            var lecturer = await _userManager.FindByIdAsync(model.Id);
            if (lecturer == null) return NotFound();

            lecturer.Email = model.Email;
            lecturer.UserName = model.UserName;
            lecturer.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(lecturer);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Could not update lecturer profile.");
                return View(model);
            }

            TempData["HRMessage"] = "Lecturer information updated successfully.";
            return RedirectToAction(nameof(Lecturers));
        }

        // RESET LECTURER PASSWORD
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var lecturer = await _userManager.FindByIdAsync(id);
            if (lecturer == null) return NotFound();

            string newPassword = "Password123!"; // ❗ You can change this default

            var token = await _userManager.GeneratePasswordResetTokenAsync(lecturer);
            var result = await _userManager.ResetPasswordAsync(lecturer, token, newPassword);

            if (!result.Succeeded)
            {
                TempData["HRMessage"] = "Failed to reset password.";
                return RedirectToAction(nameof(Lecturers));
            }

            TempData["HRMessage"] = $"Password reset successfully. New Password: {newPassword}";
            return RedirectToAction(nameof(Lecturers));
        }

        // DEACTIVATE LECTURER
        [HttpPost]
        public async Task<IActionResult> Deactivate(string id)
        {
            var lecturer = await _userManager.FindByIdAsync(id);
            if (lecturer == null) return NotFound();

            lecturer.LockoutEnd = DateTime.UtcNow.AddYears(99);

            await _userManager.UpdateAsync(lecturer);

            TempData["HRMessage"] = $"Lecturer {lecturer.UserName} has been deactivated.";
            return RedirectToAction(nameof(Lecturers));
        }

        // REACTIVATE LECTURER
        [HttpPost]
        public async Task<IActionResult> Reactivate(string id)
        {
            var lecturer = await _userManager.FindByIdAsync(id);
            if (lecturer == null) return NotFound();

            lecturer.LockoutEnd = null;

            await _userManager.UpdateAsync(lecturer);

            TempData["HRMessage"] = $"Lecturer {lecturer.UserName} has been reactivated.";
            return RedirectToAction(nameof(Lecturers));
        }
    }
}

using CMCS.Data;
using CMCS.Models;
using CMCS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO; // REQUIRED for Path
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Claim = CMCS.Models.Claim;

namespace CMCS.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _files;

        public LecturerController(ApplicationDbContext db, IFileService files)
        {
            _db = db;
            _files = files;
        }

        // -------------------------------
        // SUBMIT CLAIM (GET)
        // -------------------------------
        public IActionResult Submit() => View();


        // -------------------------------
        // SUBMIT CLAIM (POST)
        // -------------------------------
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit([FromForm] Claim model, IFormFile upload)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Validate hours + rate
            if (model.HoursWorked < 0 || model.HourlyRate < 0)
            {
                ModelState.AddModelError("", "Hours worked and hourly rate must be non-negative.");
                return View(model);
            }

            // Auto-calc payment
            model.TotalPayment = (double)Math.Round(model.HoursWorked * model.HourlyRate, 2);

            // Identify logged-in lecturer
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                ModelState.AddModelError("", "User not authenticated.");
                return View(model);
            }

            model.LecturerId = userId;

            // Set initial claim workflow values
            model.Status = ClaimStatus.Pending;
            model.SubmittedAt = DateTime.UtcNow;

            // Optional file upload
            if (upload != null && upload.Length > 0)
            {
                var ext = Path.GetExtension(upload.FileName).ToLower();
                var allowed = new[] { ".pdf", ".docx", ".xlsx" };

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Only PDF, DOCX and XLSX files are allowed.");
                    return View(model);
                }

                if (upload.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "File must be under 5MB.");
                    return View(model);
                }

                using var stream = upload.OpenReadStream();
                model.UploadedFilePath = await _files.SaveClaimFileAsync(
                    userId, upload.FileName, stream
                );
            }

            // Save to DB
            _db.Claims.Add(model);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Claim submitted successfully.";
            return RedirectToAction(nameof(Track));
        }


        // -------------------------------
        // TRACK CLAIMS
        // -------------------------------
        public async Task<IActionResult> Track()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var claims = await _db.Claims
                .Where(c => c.LecturerId == userId)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return View(claims);
        }
    }
}

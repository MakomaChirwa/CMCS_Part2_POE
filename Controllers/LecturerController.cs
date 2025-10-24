using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CMCS.Data;
using CMCS.Models;
using CMCS.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Claim = CMCS.Models.Claim;

namespace CMCS.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _files;
        public LecturerController(ApplicationDbContext db, IFileService files){ _db = db; _files = files; }

        public async Task<IActionResult> Submit() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit([FromForm]Claim model, IFormFile upload)
        {
            if(!ModelState.IsValid) return View(model);
            model.LecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity.Name;
            if(upload != null && upload.Length > 0){
                var ext = System.IO.Path.GetExtension(upload.FileName).ToLower();
                var ok = new[]{".pdf",".docx",".xlsx"};
                if(upload.Length > 5*1024*1024 || !ok.Contains(ext)){ ModelState.AddModelError("","Invalid file"); return View(model); }
                using var s = upload.OpenReadStream();
                model.UploadedFilePath = await _files.SaveClaimFileAsync(model.LecturerId, upload.FileName, s);
            }
            _db.Claims.Add(model);
            await _db.SaveChangesAsync();
            TempData["Message"] = "Claim submitted";
            return RedirectToAction(nameof(Track));
        }

        public async Task<IActionResult> Track()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity.Name;
            var list = await _db.Claims.Where(c => c.LecturerId == userId).OrderByDescending(c=>c.SubmittedAt).ToListAsync();
            return View(list);
        }
    }
}

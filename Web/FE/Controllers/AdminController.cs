using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web.Services;
using Web.Data;

namespace Web.Controllers;

[Authorize(Policy = "CanUpload")]
public class AdminController(IBlobService blobs, ApplicationDbContext db) : Controller
{
    public IActionResult Index() => View();

    [HttpPost]
    [RequestSizeLimit(1024L * 1024L * 200L)]
    public async Task<IActionResult> Upload(IFormFile file, string? year, string? summary)
    {
        if (file == null || file.Length == 0) return BadRequest("File mancante");
        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Sono ammessi solo PDF");
        using var s = file.OpenReadStream();
        await blobs.UploadAsync(s, file.FileName, year, summary);

        var identityId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var appUserId = await db.UsersProfile.Where(x => x.IdentityUserId == identityId).Select(x => x.Id).FirstAsync();
        db.Documents.Add(new Document
        {
            Id = Guid.NewGuid(),
            FileName = file.FileName,
            BlobName = file.FileName,
            Year = year,
            Summary = summary,
            Size = file.Length,
            UploadedAt = DateTimeOffset.UtcNow,
            UploadedByUserId = appUserId
        });
        await db.SaveChangesAsync();

        TempData["ok"] = "Caricato!";
        return RedirectToAction("Index");
    }
}
using Api.Services;
using Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController(IBlobService blobs, ApplicationDbContext db, IConfiguration cfg) : ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> List([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? minutes)
    {
        // policy a livello BE: anche se il FE filtra, il BE decide cosa serve
        var canDownload = User.HasClaim(c => c.Type == "func" && c.Value == "download") || User.HasClaim(c => c.Type == "func" && c.Value == "admin");
        if (!canDownload) return Forbid();
        int sas = minutes ?? cfg.GetSection("AzureStorage").GetValue<int>("DefaultSasMinutes", 30);
        var list = await blobs.ListAsync(from, to, sas);
        // opzionale: unire con tabella Documents se vuoi metadati extra
        return Ok(list);
    }

    [Authorize]
    [HttpPost]
    [RequestSizeLimit(1024L*1024L*200L)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string? year, [FromForm] string? summary)
    {
        var canUpload = User.HasClaim(c => c.Type == "func" && c.Value == "upload") || User.HasClaim(c => c.Type == "func" && c.Value == "admin");
        if (!canUpload) return Forbid();
        if (file == null || file.Length == 0) return BadRequest("File mancante");
        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) return BadRequest("Solo PDF");
        using var s = file.OpenReadStream();
        await blobs.UploadAsync(s, file.FileName, year, summary);

        var identityId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var appUserId = await db.UsersProfile.Where(x => x.IdentityUserId == identityId).Select(x => x.Id).FirstAsync();
        await db.SaveChangesAsync();
        return Ok();
    }
}
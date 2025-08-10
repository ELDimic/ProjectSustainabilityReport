using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Controllers;

[Authorize(Roles = Roles.Admin + "," + Roles.Consulter)]
public class ReportsController(IBlobService blobs, IConfiguration cfg) : Controller
{
    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> List(DateTime? from, DateTime? to, int? minutes)
    {
        int sas = minutes ?? cfg.GetSection("AzureStorage").GetValue<int>("DefaultSasMinutes", 30);
        var items = await blobs.ListAsync(from, to, sas);
        return PartialView("_List", items);
    }
}

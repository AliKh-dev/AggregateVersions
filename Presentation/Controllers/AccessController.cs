using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AggregateVersions.Presentation.Controllers
{
    public class AccessController(IAccessesService accessesService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            await accessesService.SetParent();

            List<Access> accesses = await accessesService.GetAll();

            return View(accesses);
        }
    }
}

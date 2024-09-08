using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AggregateVersions.Presentation.Controllers
{
    [Route("[controller]")]
    public class DataBasesController(IDataBasesService dataBasesService) : Controller
    {
        [HttpGet]
        [Route("[action]")]
        public IActionResult Add(Guid projectID)
        {
            DataBase dataBase = new() { ProjectID = projectID };

            return View(dataBase);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Add(DataBase dataBase)
        {
            Guid dataBaseID = await dataBasesService.Add(dataBase);

            return RedirectToAction(actionName: nameof(ProjectsController.Details), controllerName: "Projects", routeValues: new { projectID = dataBase.ProjectID });
        }


        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Edit(Guid dataBaseID)
        {
            DataBase? dataBase = await dataBasesService.GetByID(dataBaseID);

            return View(dataBase);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Edit(DataBase dataBase)
        {
            await dataBasesService.Edit(dataBase.ID, dataBase.Name);

            return RedirectToAction(actionName: nameof(ProjectsController.Details), controllerName: "Projects", routeValues: new { projectID = dataBase.ProjectID });
        }


        [Route("[action]")]
        public async Task<IActionResult> Delete(Guid dataBaseID)
        {
            DataBase? dataBase = await dataBasesService.GetByID(dataBaseID);
            await dataBasesService.Delete(dataBaseID);

            return RedirectToAction(actionName: nameof(ProjectsController.Details), controllerName: "Projects", routeValues: new { projectID = dataBase?.ProjectID });
        }
    }
}

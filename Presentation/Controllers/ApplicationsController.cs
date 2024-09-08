using AggregateVersions.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AggregateVersions.Presentation.Controllers
{
    [Route("[controller]")]
    public class ApplicationsController(IApplicationsService applicationsService) : Controller
    {
        [HttpGet]
        [Route("[action]")]
        public IActionResult Add(Guid projectID)
        {
            Domain.Entities.Application application = new() { ProjectID = projectID };

            return View(application);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Add(Domain.Entities.Application application)
        {
            Guid applicationID = await applicationsService.Add(application);

            return RedirectToAction(actionName: nameof(ProjectsController.Details), controllerName: "Projects", routeValues: new { projectID = application.ProjectID });
        }


        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Edit(Guid applicationID)
        {
            Domain.Entities.Application? application = await applicationsService.GetByID(applicationID);

            return View(application);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Edit(Domain.Entities.Application application)
        {
            await applicationsService.Edit(application.ID, application.Name);

            return RedirectToAction(actionName: nameof(ProjectsController.Details), controllerName: "Projects", routeValues: new { projectID = application.ProjectID });
        }


        [Route("[action]")]
        public async Task<IActionResult> Delete(Guid applicationID)
        {
            Domain.Entities.Application? application = await applicationsService.GetByID(applicationID);

            await applicationsService.Delete(applicationID);

            return RedirectToAction(actionName: nameof(ProjectsController.Details), controllerName: "Projects", routeValues: new { projectID = application?.ProjectID });
        }
    }
}

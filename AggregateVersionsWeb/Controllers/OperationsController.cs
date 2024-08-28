using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AggregateVersions.Presentation.Controllers
{
    [Route("[controller]")]
    public class OperationsController(IOperationsService operationsService) : Controller
    {
        [HttpGet]
        [Route("[action]")]
        public IActionResult Add(Guid projectID)
        {
            Operation operation = new() { ProjectID = projectID };
            return View(operation);
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Add(Operation operation)
        {
            Guid operationID = await operationsService.Add(operation);

            return RedirectToAction(actionName: nameof(ProjectsController.Details), controllerName: "Projects", routeValues: new { projectID = operation.ProjectID });
        }


        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Edit(Guid operationID)
        {
            Operation? operation = await operationsService.GetByID(operationID);

            return View(operation);
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Edit(Operation operation)
        {
            await operationsService.Edit(operation.ID, operation.Name);

            return RedirectToAction(actionName: nameof(ProjectsController.Details), controllerName: "Projects", routeValues: new { projectID = operation.ProjectID });
        }


        [Route("[action]")]
        public async Task<IActionResult> Delete(Guid operationID)
        {
            Operation? operation = await operationsService.GetByID(operationID);
            await operationsService.Delete(operationID);

            return RedirectToAction(actionName: nameof(ProjectsController.Details), controllerName: "Projects", routeValues: new { projectID = operation?.ProjectID });
        }
    }
}

using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AggregateVersions.Presentation.Controllers
{
    [Route("[controller]")]
    public class ProjectsController(IProjectsService projectsService,
                                    IOperationsService operationsService,
                                    IApplicationsService applicationsService,
                                    IDataBasesService dataBasesService) : Controller
    {
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Index()
        {
            List<Project> projects = await projectsService.GetAll();
            return View(projects);
        }


        [HttpGet]
        [Route("[action]")]
        public IActionResult Add()
        {
            Project project = new();
            return View(project);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Add(Project project)
        {
            await projectsService.Add(project);

            return RedirectToAction(actionName: nameof(Index));
        }


        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Edit(Guid projectID)
        {
            Project? project = await projectsService.GetByID(projectID);

            return View(project);
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Edit(Project project)
        {
            await projectsService.Edit(project.ID, project.Name);

            return RedirectToAction(actionName: nameof(Index));
        }


        [Route("[action]")]
        public async Task<IActionResult> Delete(Guid projectID)
        {
            await projectsService.Delete(projectID);

            return RedirectToAction(actionName: nameof(Index));
        }


        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Details(Guid projectID)
        {
            Project? project = await projectsService.GetByID(projectID);

            if (project != null)
            {
                project.Operations = await operationsService.GetByProjectID(projectID);

                project.Applications = await applicationsService.GetByProjectID(projectID);

                project.DataBases = await dataBasesService.GetByProjectID(projectID);
            }

            return View(project);
        }
    }
}

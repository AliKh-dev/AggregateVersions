using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;

namespace AggregateVersions.Application.Services
{
    public class ProjectsService(IProjectsRepository repository) : IProjectsService
    {
        public async Task<List<Project>> GetAll()
        {
            return await repository.GetAll();
        }

        public async Task<Project?> GetByName(string projectName)
        {
            return await repository.GetByName(projectName);
        }

        public async Task<Project?> GetByID(Guid projectID)
        {
            return await repository.GetByID(projectID);
        }

        public async Task<Guid> Add(Project project)
        {
            Guid projectID = Guid.NewGuid();

            project.ID = projectID;

            await repository.Insert(project);

            await repository.Save();

            return projectID;
        }

        public async Task<bool> Edit(Guid projectID, string projectName)
        {
            Project? project = await repository.GetByID(projectID);

            if (project is null)
                return false;

            project.Name = projectName;

            repository.Update(project);

            await repository.Save();

            return true;
        }

        public async Task<bool> Delete(Guid projectID)
        {
            Project? project = await repository.GetByID(projectID);

            if (project is null)
                return false;

            repository.Delete(project);

            await repository.Save();

            return true;
        }
    }
}

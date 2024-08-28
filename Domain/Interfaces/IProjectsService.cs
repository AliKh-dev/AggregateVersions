using AggregateVersions.Domain.Entities;

namespace AggregateVersions.Domain.Interfaces
{
    public interface IProjectsService
    {
        Task<List<Project>> GetAll();

        Task<Project?> GetByName(string projectName);

        Task<Project?> GetByID(Guid projectID);

        Task<Guid> Add(Project project);

        Task<bool> Edit(Guid projectID, string projectName);

        Task<bool> Delete(Guid projectID);
    }
}

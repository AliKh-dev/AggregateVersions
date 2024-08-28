using AggregateVersions.Domain.Entities;

namespace AggregateVersions.Domain.Interfaces
{
    public interface IProjectsRepository : IDisposable
    {
        Task<List<Project>> GetAll();
        Task<Project?> GetByID(Guid projectID);
        Task<Project?> GetByName(string projectName);
        Task Insert(Project project);
        void Update(Project project);
        void Delete(Project project);
        Task Save();
    }
}

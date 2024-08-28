using AggregateVersions.Domain.Entities;

namespace AggregateVersions.Domain.Interfaces
{
    public interface IApplicationsRepository : IDisposable
    {
        Task<List<Application>> GetAll();
        Task<Application?> GetByID(Guid applicationID);
        Task<Application?> GetByName(string applicationName);
        Task<List<Application>?> GetByProjectID(Guid projectID);
        Task Insert(Application application);
        void Update(Application application);
        void Delete(Application application);
        Task Save();
    }
}

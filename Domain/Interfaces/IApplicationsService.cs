using AggregateVersions.Domain.Entities;

namespace AggregateVersions.Domain.Interfaces
{
    public interface IApplicationsService
    {
        Task<List<Application>> GetAll();

        Task<Application?> GetByName(string applicationName);

        Task<Application?> GetByID(Guid applicationID);

        Task<List<Application>?> GetByProjectID(Guid projectID);

        Task<Guid> Add(Application application);

        Task<bool> Edit(Guid applicationID, string applicationName);

        Task<bool> Delete(Guid applicationID);
    }
}

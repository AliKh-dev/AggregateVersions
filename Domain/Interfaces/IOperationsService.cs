using AggregateVersions.Domain.Entities;

namespace AggregateVersions.Domain.Interfaces
{
    public interface IOperationsService
    {
        Task<List<Operation>> GetAll();

        Task<Operation?> GetByName(string operationName);

        Task<Operation?> GetByID(Guid operationID);

        Task<List<Operation>?> GetByProjectID(Guid projectID);

        Task<Guid> Add(Operation operation);

        Task<bool> Edit(Guid operationID, string operationName);

        Task<bool> Delete(Guid operationID);
    }
}

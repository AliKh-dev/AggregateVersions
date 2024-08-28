using AggregateVersions.Domain.Entities;

namespace AggregateVersions.Domain.Interfaces
{
    public interface IOperationsRepository : IDisposable
    {
        Task<List<Operation>> GetAll();
        Task<Operation?> GetByID(Guid operationID);
        Task<Operation?> GetByName(string operationName);
        Task<List<Operation>?> GetByProjectID(Guid projectID);
        Task Insert(Operation operation);
        void Update(Operation operation);
        void Delete(Operation operation);
        Task Save();
    }
}

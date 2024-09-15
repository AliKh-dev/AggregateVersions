using AggregateVersions.Domain.Entities;

namespace AggregateVersions.Domain.Interfaces
{
    public interface IAccessesService
    {
        Task<List<Access>> GetAll();
        Task<List<Access>> GetSorted();
        Task SetParent();
        Task<Access?> GetByTitle(string accessTitle);
        Task<Access?> GetByGuid(Guid accessGuid);
        Task<Guid> Add(Access access);
        Task<bool> Edit(Guid accessGuid, string accessTitle);
        Task<bool> Delete(Guid accessGuid);
    }
}

using AggregateVersions.Domain.DTO;

namespace AggregateVersions.Domain.Interfaces
{
    public interface IAccessesService
    {
        Task<List<AccessResponse>> GetAll();
        Task<List<AccessResponse>> GetSorted();
        Task<AccessResponse?> GetByTitle(string accessTitle);
        Task<AccessResponse?> GetByID(long accessID);
        Task<List<AccessResponse>?> GetParents(AccessRequest? access);
        Task SetParent();
        Task<Guid> Add(AccessRequest access);
        Task<bool> Edit(long accessID, string accessTitle);
        Task<bool> Delete(long accessID);
    }
}

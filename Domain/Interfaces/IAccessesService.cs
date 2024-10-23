using AggregateVersions.Domain.DTO;
using AggregateVersions.Domain.Entities;

namespace AggregateVersions.Domain.Interfaces
{
    public interface IAccessesService
    {
        Task<List<AccessResponse>> GetAll();
        Task<List<AccessResponse>> GetSorted();
        Task<AccessResponse?> GetByTitle(string accessTitle);
        Task<AccessResponse?> GetByID(long? accessID);
        Task<List<AccessResponse>?> GetParents(AccessRequest? access);
        Task SetParent();
        Task<bool> HaveBaseKey(string key);
        List<Access> GetNonExistentAccesses(List<Access> accesses);
        Task Add(List<Access> accesses);
        Task<bool> Edit(long accessID, string accessTitle);
        Task<bool> Delete(long accessID);
    }
}

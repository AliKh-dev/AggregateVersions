using AggregateVersions.Domain.Entities;

namespace AggregateVersions.Domain.Interfaces
{
    public interface IAccessesRepository
    {
        Task<List<Access>> GetAll();
        Task<List<Access>> GetSorted();
        Task<List<Access>> GetParents(Access? access);
        Task SetParent();
        Task<Access?> GetByID(long accessID);
        Task<Access?> GetByTitle(string accessTitle);
        Task<bool> HaveBaseKey(string key);
        Task Insert(List<Access> accesses);
        void Update(Access access);
        void Delete(Access access);
        Task Save();
    }
}

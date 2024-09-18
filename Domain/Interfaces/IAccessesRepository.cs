using AggregateVersions.Domain.Entities;

namespace AggregateVersions.Domain.Interfaces
{
    public interface IAccessesRepository : IDisposable
    {
        Task<List<Access>> GetAll();
        Task<List<Access>> GetSorted();
        Task<List<Access>> GetParents(Access? access);
        Task SetParent();
        Task<Access?> GetByID(long accessID);
        Task<Access?> GetByTitle(string accessTitle);
        Task Insert(Access access);
        void Update(Access access);
        void Delete(Access access);
        Task Save();
    }
}

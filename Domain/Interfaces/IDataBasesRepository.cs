using AggregateVersions.Domain.Entities;

namespace AggregateVersions.Domain.Interfaces
{
    public interface IDataBasesRepository : IDisposable
    {
        Task<List<DataBase>> GetAll();
        Task<DataBase?> GetByID(Guid dataBaseID);
        Task<DataBase?> GetByName(string dataBaseName);
        Task<List<DataBase>?> GetByProjectID(Guid projectID);
        Task Insert(DataBase dataBase);
        void Update(DataBase dataBase);
        void Delete(DataBase dataBase);
        Task Save();
    }
}

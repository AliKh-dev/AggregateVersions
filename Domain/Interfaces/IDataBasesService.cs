using AggregateVersions.Domain.Entities;

namespace AggregateVersions.Domain.Interfaces
{
    public interface IDataBasesService
    {
        Task<List<DataBase>> GetAll();

        Task<DataBase?> GetByName(string dataBaseName);

        Task<DataBase?> GetByID(Guid dataBaseID);

        Task<List<DataBase>?> GetByProjectID(Guid projectID);

        Task<Guid> Add(DataBase dataBase);

        Task<bool> Edit(Guid dataBaseID, string dataBaseName);

        Task<bool> Delete(Guid dataBaseID);
    }
}

using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;

namespace AggregateVersions.Application.Services
{
    public class DataBasesService(IDataBasesRepository repository) : IDataBasesService
    {
        public async Task<List<DataBase>> GetAll()
        {
            return await repository.GetAll();
        }

        public async Task<DataBase?> GetByName(string dataBaseName)
        {
            return await repository.GetByName(dataBaseName);
        }

        public async Task<DataBase?> GetByID(Guid dataBaseID)
        {
            return await repository.GetByID(dataBaseID);
        }

        public async Task<List<DataBase>?> GetByProjectID(Guid projectID)
        {
            return await repository.GetByProjectID(projectID);
        }

        public async Task<Guid> Add(DataBase dataBase)
        {
            Guid dataBaseID = Guid.NewGuid();

            dataBase.ID = dataBaseID;

            await repository.Insert(dataBase);

            await repository.Save();

            return dataBaseID;
        }

        public async Task<bool> Edit(Guid dataBaseID, string dataBaseName)
        {
            DataBase? dataBase = await repository.GetByID(dataBaseID);

            if (dataBase is null)
                return false;

            dataBase.Name = dataBaseName;

            repository.Update(dataBase);

            await repository.Save();

            return true;
        }

        public async Task<bool> Delete(Guid dataBaseID)
        {
            DataBase? dataBase = await repository.GetByID(dataBaseID);

            if (dataBase is null)
                return false;

            repository.Delete(dataBase);

            await repository.Save();

            return true;
        }
    }
}

using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;

namespace AggregateVersions.Application.Services
{
    public class OperationsServices(IOperationsRepository repository) : IOperationsService
    {
        public async Task<List<Operation>> GetAll()
        {
            return await repository.GetAll();
        }

        public async Task<Operation?> GetByName(string operationName)
        {
            return await repository.GetByName(operationName);
        }

        public async Task<Operation?> GetByID(Guid operationID)
        {
            return await repository.GetByID(operationID);
        }

        public async Task<List<Operation>?> GetByProjectID(Guid projectID)
        {
            return await repository.GetByProjectID(projectID);
        }

        public async Task<Guid> Add(Operation operation)
        {
            Guid operationID = Guid.NewGuid();
            operation.ID = operationID;

            await repository.Insert(operation);

            await repository.Save();

            return operationID;
        }

        public async Task<bool> Edit(Guid operationID, string operationName)
        {
            Operation? operation = await repository.GetByID(operationID);

            if (operation is null)
                return false;

            operation.Name = operationName;

            repository.Update(operation);

            await repository.Save();

            return true;
        }

        public async Task<bool> Delete(Guid operationID)
        {
            Operation? operation = await repository.GetByID(operationID);

            if (operation is null)
                return false;

            repository.Delete(operation);

            await repository.Save();

            return true;
        }
    }
}

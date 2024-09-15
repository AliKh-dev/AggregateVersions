using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;

namespace AggregateVersions.Application.Services
{
    public class AccessesService(IAccessesRepository repository) : IAccessesService
    {
        public async Task<List<Access>> GetAll()
        {
            return await repository.GetAll();
        }

        public async Task<List<Access>> GetSorted()
        {
            return await repository.GetSorted();
        }

        public async Task<Access?> GetByTitle(string accessTitle)
        {
            return await repository.GetByTitle(accessTitle);
        }

        public async Task<Access?> GetByGuid(Guid accessGuid)
        {
            return await repository.GetByID(accessGuid);
        }

        public async Task SetParent()
        {
            await repository.SetParent();
        }

        public async Task<Guid> Add(Access access)
        {
            Guid accessGuid = Guid.NewGuid();

            access.Guid = accessGuid;

            await repository.Insert(access);

            await repository.Save();

            return accessGuid;
        }

        public async Task<bool> Edit(Guid accessGuid, string accessTitle)
        {
            Access? access = await repository.GetByID(accessGuid);

            if (access is null)
                return false;

            access.Title = accessTitle;

            repository.Update(access);

            await repository.Save();

            return true;
        }

        public async Task<bool> Delete(Guid accessGuid)
        {
            Access? access = await repository.GetByID(accessGuid);

            if (access is null)
                return false;

            repository.Delete(access);

            await repository.Save();

            return true;
        }
    }
}

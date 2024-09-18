using AggregateVersions.Domain.DTO;
using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;

namespace AggregateVersions.Application.Services
{
    public class AccessesService(IAccessesRepository repository) : IAccessesService
    {
        public async Task<List<AccessResponse>> GetAll()
        {
            List<Access> accesses = await repository.GetAll();

            List<AccessResponse> accessResponses = [];

            accesses.ForEach(ac => accessResponses.Add(ac.ToAccessExportResponse()));

            return accessResponses;
        }

        public async Task<List<AccessResponse>> GetSorted()
        {

            List<Access> sortedAccesses = await repository.GetSorted();

            List<AccessResponse> accessResponses = [];

            sortedAccesses.ForEach(ac => accessResponses.Add(ac.ToAccessExportResponse()));

            return accessResponses;
        }

        public async Task<AccessResponse?> GetByID(long accessID)
        {
            Access? access = await repository.GetByID(accessID);

            if (access == null)
                return null;

            return access.ToAccessExportResponse();
        }

        public async Task<AccessResponse?> GetByTitle(string accessTitle)
        {
            Access? access = await repository.GetByTitle(accessTitle);

            if (access == null)
                return null;

            return access.ToAccessExportResponse();
        }

        public async Task<List<AccessResponse>?> GetParents(AccessRequest? access)
        {
            if (access == null)
                return null;

            Access? matchingAccess = await repository.GetByID(access.ID);

            List<Access> accesses = await repository.GetParents(matchingAccess);

            List<AccessResponse> parents = [];

            accesses.ForEach(ac => parents.Add(ac.ToAccessExportResponse()));

            return parents;
        }

        public async Task SetParent()
        {
            await repository.SetParent();
        }

        public async Task<Guid> Add(AccessRequest access)
        {
            Guid accessGuid = Guid.NewGuid();

            Access accessEntity = new() { Guid = accessGuid, ID = access.ID, Title = access.Title };

            await repository.Insert(accessEntity);

            await repository.Save();

            return accessGuid;
        }

        public async Task<bool> Edit(long accessID, string accessTitle)
        {
            Access? access = await repository.GetByID(accessID);

            if (access is null)
                return false;

            access.Title = accessTitle;

            repository.Update(access);

            await repository.Save();

            return true;
        }

        public async Task<bool> Delete(long accessID)
        {
            Access? access = await repository.GetByID(accessID);

            if (access is null)
                return false;

            repository.Delete(access);

            await repository.Save();

            return true;
        }

    }
}

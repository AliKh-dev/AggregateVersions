using AggregateVersions.Domain.Interfaces;

namespace AggregateVersions.Application.Services
{
    public class ApplicationsService(IApplicationsRepository repository) : IApplicationsService
    {
        public async Task<List<Domain.Entities.Application>> GetAll()
        {
            return await repository.GetAll();
        }

        public async Task<Domain.Entities.Application?> GetByName(string applicationName)
        {
            return await repository.GetByName(applicationName);
        }

        public async Task<Domain.Entities.Application?> GetByID(Guid applicationID)
        {
            return await repository.GetByID(applicationID);
        }

        public async Task<List<Domain.Entities.Application>?> GetByProjectID(Guid projectID)
        {
            return await repository.GetByProjectID(projectID);
        }

        public async Task<Guid> Add(Domain.Entities.Application application)
        {
            Guid applicationID = Guid.NewGuid();

            application.ID = applicationID;

            await repository.Insert(application);

            await repository.Save();

            return applicationID;
        }

        public async Task<bool> Edit(Guid applicationID, string applicationName)
        {
            Domain.Entities.Application? application = await repository.GetByID(applicationID);

            if (application is null)
                return false;

            application.Name = applicationName;

            repository.Update(application);

            await repository.Save();

            return true;
        }

        public async Task<bool> Delete(Guid applicationID)
        {
            Domain.Entities.Application? application = await repository.GetByID(applicationID);

            if (application is null)
                return false;

            repository.Delete(application);

            await repository.Save();

            return true;
        }
    }
}

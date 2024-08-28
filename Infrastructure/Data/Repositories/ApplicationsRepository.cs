using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AggregateVersions.Infrastructure.Data.Repositories
{
    public class ApplicationsRepository(OperationContext context) : IApplicationsRepository
    {
        private bool _disposed = false;

        public async Task<List<Application>> GetAll()
        {
            return await context.Applications.AsNoTracking().ToListAsync();
        }

        public async Task<Application?> GetByID(Guid applicationID)
        {
            return await context.Applications.AsNoTracking().FirstOrDefaultAsync(app => app.ID == applicationID);
        }

        public async Task<Application?> GetByName(string applicationName)
        {
            return await context.Applications.AsNoTracking().FirstOrDefaultAsync(app => app.Name == applicationName);
        }

        public async Task<List<Application>?> GetByProjectID(Guid projectID)
        {
            return await context.Applications.AsNoTracking().Where(app => app.ProjectID == projectID).ToListAsync();

        }

        public async Task Insert(Application application)
        {
            await context.Applications.AddAsync(application);
        }

        public void Update(Application application)
        {
            context.Applications.Entry(application).State = EntityState.Modified;
        }

        public void Delete(Application application)
        {
            context.Applications.Remove(application);
        }

        public async Task Save()
        {
            await context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                    context.Dispose();
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

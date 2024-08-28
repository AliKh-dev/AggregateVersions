using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AggregateVersions.Infrastructure.Data.Repositories
{
    public class ProjectsRepository(OperationContext context) : IProjectsRepository
    {
        private bool _disposed = false;

        public async Task<List<Project>> GetAll()
        {
            return await context.Projects.AsNoTracking().ToListAsync();
        }

        public async Task<Project?> GetByID(Guid projectID)
        {
            return await context.Projects.AsNoTracking().FirstOrDefaultAsync(pro => pro.ID == projectID);
        }

        public async Task<Project?> GetByName(string projectName)
        {
            return await context.Projects.AsNoTracking().FirstOrDefaultAsync(pro => pro.Name == projectName);
        }

        public async Task Insert(Project project)
        {
            await context.Projects.AddAsync(project);
        }

        public void Update(Project project)
        {
            context.Projects.Entry(project).State = EntityState.Modified;
        }

        public void Delete(Project project)
        {
            context.Projects.Remove(project);
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

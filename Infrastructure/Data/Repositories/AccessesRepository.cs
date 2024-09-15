using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AggregateVersions.Infrastructure.Data.Repositories
{
    public class AccessesRepository(OperationContext context) : IAccessesRepository
    {
        private bool _disposed = false;

        public async Task<List<Access>> GetAll()
        {
            return await context.Accesses.ToListAsync();
        }

        public async Task SetParent()
        {
            List<Access> accesses = await context.Accesses.ToListAsync();

            foreach (Access access in accesses)
                access.Parent = accesses.FirstOrDefault(ac => ac.ID == access.ParentId);

            await Save();
        }

        public async Task<List<Access>> GetSorted()
        {
            return await context.Accesses.AsNoTracking().OrderBy(ac => ac.ParentId).ToListAsync();
        }

        public async Task<Access?> GetByID(Guid accessGuid)
        {
            return await context.Accesses.AsNoTracking().FirstOrDefaultAsync(ac => ac.Guid == accessGuid);
        }

        public async Task<Access?> GetByTitle(string accessTitle)
        {
            return await context.Accesses.AsNoTracking().FirstOrDefaultAsync(ac => ac.Title == accessTitle);
        }

        public async Task Insert(Access access)
        {
            await context.Accesses.AddAsync(access);
        }

        public void Update(Access access)
        {
            context.Accesses.Entry(access).State = EntityState.Modified;
        }

        public void Delete(Access access)
        {
            context.Accesses.Remove(access);
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

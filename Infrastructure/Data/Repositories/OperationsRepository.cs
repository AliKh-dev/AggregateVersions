using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AggregateVersions.Infrastructure.Data.Repositories
{
    public class OperationsRepository(OperationContext context) : IOperationsRepository
    {
        private bool _disposed = false;

        public async Task<List<Operation>> GetAll()
        {
            return await context.Operations.AsNoTracking().ToListAsync();
        }

        public async Task<Operation?> GetByID(Guid operationID)
        {
            return await context.Operations.AsNoTracking().FirstOrDefaultAsync(op => op.ID == operationID);
        }

        public async Task<Operation?> GetByName(string operationName)
        {
            return await context.Operations.AsNoTracking().FirstOrDefaultAsync(op => op.Name == operationName);
        }

        public async Task<List<Operation>?> GetByProjectID(Guid projectID)
        {
            return await context.Operations.AsNoTracking().Where(op => op.ProjectID == projectID).ToListAsync();
        }

        public async Task Insert(Operation operation)
        {
            await context.Operations.AddAsync(operation);
        }

        public void Update(Operation operation)
        {
            context.Operations.Entry(operation).State = EntityState.Modified;
        }

        public void Delete(Operation operation)
        {
            context.Operations.Remove(operation);
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

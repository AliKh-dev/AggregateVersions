using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AggregateVersions.Infrastructure.Data.Repositories
{
    public class DataBasesRepository(OperationContext context) : IDataBasesRepository
    {
        private bool _disposed = false;

        public async Task<List<DataBase>> GetAll()
        {
            return await context.DataBases.AsNoTracking().ToListAsync();
        }

        public async Task<DataBase?> GetByID(Guid dataBaseID)
        {
            return await context.DataBases.AsNoTracking().FirstOrDefaultAsync(db => db.ID == dataBaseID);
        }

        public async Task<DataBase?> GetByName(string dataBaseName)
        {
            return await context.DataBases.AsNoTracking().FirstOrDefaultAsync(db => db.Name == dataBaseName);
        }

        public async Task<List<DataBase>?> GetByProjectID(Guid projectID)
        {
            return await context.DataBases.AsNoTracking().Where(db => db.ProjectID == projectID).ToListAsync();
        }

        public async Task Insert(DataBase dataBase)
        {
            await context.DataBases.AddAsync(dataBase);
        }

        public void Update(DataBase dataBase)
        {
            context.DataBases.Entry(dataBase).State = EntityState.Modified;
        }

        public void Delete(DataBase dataBase)
        {
            context.DataBases.Remove(dataBase);
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

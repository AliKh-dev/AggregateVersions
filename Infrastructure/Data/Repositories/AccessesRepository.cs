﻿using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AggregateVersions.Infrastructure.Data.Repositories
{
    public class AccessesRepository(OperationContext context) : IAccessesRepository
    {
        public async Task<List<Access>> GetAll()
        {
            return await context.Accesses.ToListAsync();
        }

        public async Task<List<Access>> GetParents(Access? access)
        {
            List<Access> accesses = await GetAll();
            List<Access> parents = [];

            while (access != null && access.ParentId != null && access.ParentId != 0)
            {
                Access? parent = accesses.FirstOrDefault(ac => ac.ID == access.ParentId);

                if (parent != null)
                    parents.Add(parent);

                access = parent;
            }

            return parents;
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

        public async Task<Access?> GetByID(long accessID)
        {
            return await context.Accesses.AsNoTracking().FirstOrDefaultAsync(ac => ac.ID == accessID);
        }

        public async Task<Access?> GetByTitle(string accessTitle)
        {
            return await context.Accesses.AsNoTracking().FirstOrDefaultAsync(ac => ac.Title == accessTitle);
        }

        public async Task Insert(List<Access> accesses)
        {
            using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
            try
            {
                context.AddRange(accesses);
                await context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                Console.WriteLine("RollBack!");
            }
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

        public async Task<bool> HaveBaseKey(string key)
        {
            if ((await context.Accesses.FirstOrDefaultAsync(ac => ac.Key == key)) == null)
                return false;
            return true;
        }
        public List<Access> GetNonExistentAccesses(List<Access> accesses)
        {
            HashSet<string> existingKeys = new(context.Accesses.Select(temp => temp.Key ?? ""));

            List<Access> nonExistentAccesses = accesses.Where(access => !existingKeys.Contains(access.Key ?? ""))
                                                       .ToList();

            return nonExistentAccesses;
        }

    }
}

using AggregateVersions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AggregateVersions.Infrastructure.Data
{
    public class OperationContext(DbContextOptions<OperationContext> options) : DbContext(options)
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<DataBase> DataBases { get; set; }
        public DbSet<Access> Accesses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure primary key
            modelBuilder.Entity<Project>()
                        .HasKey(project => project.ID);


            modelBuilder.Entity<DataBase>()
                        .HasKey(database => database.ID);


            // Configure one-to-many relationship
            modelBuilder.Entity<Project>()
                        .HasMany(project => project.DataBases)
                        .WithOne(dataBase => dataBase.Project)
                        .HasForeignKey(dataBase => dataBase.ProjectID);

            modelBuilder.Entity<Access>().Ignore(x => x.Parent).ToTable("COM_ACC_Access");
            modelBuilder.Entity<Access>().HasKey(ac => ac.ID);
        }
    }
}

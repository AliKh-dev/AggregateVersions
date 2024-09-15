using AggregateVersions.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AggregateVersions.Infrastructure.Data
{
    public class OperationContext(DbContextOptions<OperationContext> options) : DbContext(options)
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<DataBase> DataBases { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Access> Accesses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure primary key
            modelBuilder.Entity<Project>()
                        .HasKey(project => project.ID);

            modelBuilder.Entity<Operation>()
                        .HasKey(operation => operation.ID);

            modelBuilder.Entity<DataBase>()
                        .HasKey(database => database.ID);

            modelBuilder.Entity<Application>()
                        .HasKey(application => application.ID);

            // Configure one-to-many relationship
            modelBuilder.Entity<Project>()
                        .HasMany(project => project.Operations)
                        .WithOne(operation => operation.Project)
                        .HasForeignKey(operation => operation.ProjectID);

            modelBuilder.Entity<Project>()
                        .HasMany(project => project.DataBases)
                        .WithOne(dataBase => dataBase.Project)
                        .HasForeignKey(dataBase => dataBase.ProjectID);

            modelBuilder.Entity<Project>()
                        .HasMany(project => project.Applications)
                        .WithOne(application => application.Project)
                        .HasForeignKey(application => application.ProjectID);
            modelBuilder.Entity<Access>().Ignore(x => x.Parent);

            //string projectsJson = File.ReadAllText("C:\\Users\\Ali\\source\\repos\\VersionsAggregationWeb\\VersionsAggregationWeb\\projects.json");

            //string operationsJson = File.ReadAllText("C:\\Users\\Ali\\source\\repos\\VersionsAggregationWeb\\VersionsAggregationWeb\\operations.json");

            //string applicationsJson = File.ReadAllText("C:\\Users\\Ali\\source\\repos\\VersionsAggregationWeb\\VersionsAggregationWeb\\applications.json");

            //string dataBasesJson = File.ReadAllText("C:\\Users\\Ali\\source\\repos\\VersionsAggregationWeb\\VersionsAggregationWeb\\databases.json");

            //List<Project>? projects = JsonSerializer.Deserialize<List<Project>>(projectsJson);
            //List<Operation>? operations = JsonSerializer.Deserialize<List<Operation>>(operationsJson);
            //List<Application>? applications = JsonSerializer.Deserialize<List<Application>>(applicationsJson);
            //List<DataBase>? dataBases = JsonSerializer.Deserialize<List<DataBase>>(dataBasesJson);

            //if (projects != null)
            //    modelBuilder.Entity<Project>().HasData(projects);

            //if (operations != null)
            //    modelBuilder.Entity<Operation>().HasData(operations);

            //if (applications != null)
            //    modelBuilder.Entity<Application>().HasData(applications);

            //if (dataBases != null)
            //    modelBuilder.Entity<DataBase>().HasData(dataBases);
        }
    }
}

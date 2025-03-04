using Marelli.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Marelli.Infra.Context
{
    public class DemurrageContext : DbContext
    {
        public DemurrageContext(DbContextOptions<DemurrageContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var configurations = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
                .ToList();

            foreach (var configuration in configurations)
            {
                dynamic configurationInstance = Activator.CreateInstance(configuration);
                modelBuilder.ApplyConfiguration(configurationInstance);
            }

            modelBuilder.Entity<UserProject>()
                .HasKey(up => new { up.UserId, up.ProjectId });

            modelBuilder.Entity<UserGroup>()
                .HasKey(up => new { up.UserId, up.GroupId });

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BuildTableRow>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.Deleted = true;
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }


        public DbSet<User> User { get; set; }
        public DbSet<BuildTableRow> BuildTableRow { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<BuildingState> BuildingState { get; set; }
        public DbSet<FileVerify> FileVerify { get; set; }
        public DbSet<LogAndArtifact> LogAndArtifact { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<UserProject> UserProject { get; set; }
        public DbSet<UserGroup> UserGroup { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<UserToken> UserToken { get; set; }
        public DbSet<BuildLog> BuildLog { get; set; }
        public DbSet<Baseline> Baseline { get; set; }


    }
}

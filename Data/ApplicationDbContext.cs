

using API_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Backend.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        // Constructor accepting DbContextOptions

        // DbSets for your entities
        public DbSet<User> Users { get; init; }
        public DbSet<ExperimentRequest> ExperimentRequests { get; init; }
        public DbSet<ClusterParameters> ClusterParameters { get; init; }
        public DbSet<Algorithm> Algorithms { get; init; }
        public DbSet<AlgorithmParameter> AlgorithmParameters { get; init; }
        public DbSet<UploadSession> UploadSessions { get; init; }
        public DbSet<ExperimentAlgorithmParameterValue> ExperimentAlgorithmParameterValues { get; init; }
        public DbSet<ExperimentResult> ExperimentResults { get; init; }
        public DbSet<DataVisualizationModel> DataVisualizations { get; init; }
        public DbSet<VisualizationExperiment> VisualizationExperiments { get; init; }
        public DbSet<StoredDataSet> StoredDataSets { get; init; }

        // Override OnModelCreating to configure relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserID);
                entity.Property(e => e.UserID)
                      .IsRequired()
                      .HasMaxLength(36); // For GUIDs
            });

            // Configure StoredDataSet
            modelBuilder.Entity<StoredDataSet>(entity =>
            {
                entity.HasKey(e => e.DataSetID);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.StoredDataSets)
                      .HasForeignKey(e => e.UserID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure UploadSession
            modelBuilder.Entity<UploadSession>(entity =>
            {
                entity.HasKey(e => e.UploadId);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure other entities as needed...
            modelBuilder.Entity<ExperimentRequest>()
                .Property(x => x.Status)
                .HasConversion(
                    y => y.ToString(),
                    y => (ExperimentStatus)Enum.Parse(typeof(ExperimentStatus), y)
                );

            modelBuilder.Entity<ExperimentAlgorithmParameterValue>(entity =>
            {
                entity.HasKey(x => new { x.ExperimentID, x.ParameterID });
            });

            modelBuilder.Entity<ExperimentAlgorithmParameterValue>()
                .HasKey(e => new { e.ExperimentID, e.ParameterID });
            modelBuilder.Entity<ExperimentAlgorithmParameterValue>()
                .HasOne(e => e.ExperimentRequest)
                .WithMany()
                .HasForeignKey(e => e.ExperimentID);

            modelBuilder.Entity<ExperimentAlgorithmParameterValue>()
                .HasOne(e => e.AlgorithmParameter)
                .WithMany()
                .HasForeignKey(e => e.ParameterID);


            base.OnModelCreating(modelBuilder);
        }
    }
}
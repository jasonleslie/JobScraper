using Microsoft.EntityFrameworkCore;

namespace JobScraper.Models
{
    public partial class DBContext : DbContext
    {
        public DBContext()
        {
        }

        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Jobdata> Jobdata { get; set; }
        public virtual DbSet<SeekConfig> SeekConfig { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Jobdata>(entity =>
            {
                entity.ToTable("jobdata");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Createdate)
                    .HasColumnName("createdate")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(e => e.Description)
                    .HasColumnName("description")
                    .HasColumnType("character varying");

                entity.Property(e => e.Link)
                    .IsRequired()
                    .HasColumnName("link")
                    .HasColumnType("character varying");

                entity.Property(e => e.Location)
                    .HasColumnName("location")
                    .HasColumnType("character varying")
                    .HasDefaultValueSql("'Unknown'::character varying");

                entity.Property(e => e.State)
                    .HasColumnName("state")
                    .HasColumnType("character varying")
                    .HasDefaultValueSql("'Unknown'::character varying");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasColumnType("character varying");
            });

            modelBuilder.Entity<SeekConfig>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("seek_config");

                entity.Property(e => e.DateRange).HasColumnName("date_range");

                entity.Property(e => e.ExcludeLocations).HasColumnName("exclude_locations");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.SearchStates)
                    .IsRequired()
                    .HasColumnName("search_states");
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

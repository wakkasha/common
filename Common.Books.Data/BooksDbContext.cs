using Common.Books.Data.Entities;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Common.Books.Data;

public class BooksDbContext : ApiAuthorizationDbContext<ApplicationUser>
{
    public BooksDbContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions)
        : base(options, operationalStoreOptions)
    {
    }

    public DbSet<BookRequest> BookRequests { get; set; } = null!;
    public DbSet<BookPreview> BookPreviews { get; set; } = null!;
    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Page> Pages { get; set; } = null!;
    public DbSet<Communication> Communications { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>()
            .HasMany(e => e.Pages)
            .WithOne(e => e.Book)
            .HasForeignKey(e => e.BookId)
            .IsRequired();

        modelBuilder.Entity<Book>()
            .HasOne(b => b.BookPreview)
            .WithMany()
            .HasForeignKey(b => b.BookPreviewId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Book>()
            .HasOne(b => b.BookRequest)
            .WithMany()
            .HasForeignKey(b => b.BookRequestId)
            .OnDelete(DeleteBehavior.NoAction); // Preventing cascading delete

        modelBuilder.Entity<BookPreview>(entity =>
        {
            // entity.Property(e => e.PromptResponse).HasColumnType("nvarchar(max)");
            entity.OwnsOne(x => x.PromptResponse, cb =>
            {
                cb.ToJson();
                cb.Property(x => x.Title);
                cb.OwnsMany(x=>x.Pages);
            });
        });

        base.OnModelCreating(modelBuilder);
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BooksDbContext>
    {
        public BooksDbContext CreateDbContext(string[] args)
        {
            // Note: Typically for Azure Functions, you'd use "local.settings.json"
            // but for simplicity, let's keep using "appsettings.json"
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // This should point to where your config file is.
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<BooksDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new BooksDbContext(optionsBuilder.Options, Options.Create(new OperationalStoreOptions()));
        }
    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SakiBee.Models;

namespace SakiBee.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Technology", Description = "Description 1"},
                new Category { Id = 2, Name = "Health", Description = "Description 2" },
                new Category { Id = 3, Name = "LifeStyle", Description = "Description 3" }
                );
            modelBuilder.Entity<Post>().HasData(
                new Post
                {
                    Id = 1,
                    Title = "Tech Post 1",
                    content = "Content of Tech Post 1",
                    Author = "John Doe",
                    PublishDate = new DateTime(2023, 1, 1), // Static date instead of DateTime.Now
                    CategoryId = 1,
                    FeatureImagePath = "tech_image.jpg", // Sample image path
                },
                new Post
                {
                    Id = 2,
                    Title = "Health Post 1",
                    content = "Content of Health Post 1",
                    Author = "Jane Doe",
                    PublishDate = new DateTime(2023, 1, 1),
                    CategoryId = 2,
                    FeatureImagePath = "health_image.jpg",
                },
                new Post
                {
                    Id = 3,
                    Title = "Lifestyle Post 1",
                    content = "Content of Lifestyle Post 1",
                    Author = "Alex Smith",
                    PublishDate = new DateTime(2023, 1, 1),
                    CategoryId = 3,
                    FeatureImagePath = "lifestyle_image.jpg",
                }
                );

        }
    }
}

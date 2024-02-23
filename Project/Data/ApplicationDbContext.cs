using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{

		}

		public DbSet<FuelHistory> FuelHistories { get; set; }

		protected void OnModelCreating(ModelBuilder modelBuilder)
		{

			modelBuilder.Entity<FuelHistory>().HasData(
				new FuelHistory { Id = 1, DeliveryDate = "2/3/2022", GallonsRequested = 2 }

			);
		}
	}
}
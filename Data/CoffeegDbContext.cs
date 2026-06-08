using Coffeeg.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TestFunctionApp.Data
{
    public class CoffeegDbContext : IdentityDbContext<User>
    {
        public DbSet<BeverageCustomisation> BeverageCustomisations { get; set; }
        public DbSet<BeverageType> BeverageTypes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<ComplexIngredient> ComplexIngredients { get; set; }
        public DbSet<IngredientAmount> IngredientAmounts { get; set; }
        public DbSet<ComplexIngredientAmount> ComplexIngredientAmounts { get; set; }

        public CoffeegDbContext(DbContextOptions<CoffeegDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .ToTable("User")
                .Property(e => e.UserName).HasColumnName("Username");

            modelBuilder.Entity<User>()
                .Property(e => e.FirstName).HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(e => e.LastName).HasMaxLength(100);


            modelBuilder.Entity<IdentityRole>()
                .ToTable("Role")
                .HasData(
                    new IdentityRole { Id = "member", Name = "Member", NormalizedName = "MEMBER" },
                    new IdentityRole { Id = "moderator", Name = "Moderator", NormalizedName = "MODERATOR" },
                    new IdentityRole { Id = "admin", Name = "Admin", NormalizedName = "ADMIN" }
                );

            modelBuilder.Entity<IdentityUserRole<string>>()
                .ToTable("UserRole");

            modelBuilder.Entity<IdentityUserLogin<string>>()
                .ToTable("UserLogin");

            modelBuilder.Entity<IdentityRoleClaim<string>>()
                .ToTable("RoleClaim");

            modelBuilder.Entity<IdentityUserClaim<string>>()
                .ToTable("UserClaim");

            modelBuilder.Entity<IdentityUserToken<string>>()
                .ToTable("UserToken");

            modelBuilder.Entity<IngredientAmount>(entity =>
            {
                //entity.HasKey(e => e.Id);
                entity.HasKey(e => new { e.BeverageCustomisationId, e.IngredientId });

                entity.ToTable("IngredientAmount");

                entity.HasIndex(e => new { e.BeverageCustomisationId, e.IngredientId }, "UIX_IngredAmount_Unqiue").IsUnique();

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                //entity.HasOne(d => d.BeverageCustomisationId).WithMany(p => p.IngredientAmounts)
                //    .HasForeignKey(d => d.BeverageCustId)
                //    .OnDelete(DeleteBehavior.ClientSetNull)
                //    .HasConstraintName("IngredAmount_BeverageCust");

                //entity.HasOne(d => d.IngredientId).WithMany(p => p.IngredientAmounts)
                //    .HasForeignKey(d => d.IngredientId)
                //    .OnDelete(DeleteBehavior.ClientSetNull)
                //    .HasConstraintName("IngredAmount_Ingredient");
            });

            modelBuilder.Entity<ComplexIngredientAmount>(entity =>
            {
                entity.HasKey(e => e.Id); ;

                entity.ToTable("ComplexIngredientAmount");

                entity.HasIndex(e => new { e.BeverageCustomisationId, e.ComplexIngredientId }).IsUnique();

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                //entity.HasOne(d => d.BeverageCust).WithMany(p => p.ComplexIngredientAmounts)
                //    .HasForeignKey(d => d.BeverageCustId)
                //    .OnDelete(DeleteBehavior.ClientSetNull)
                //    .HasConstraintName("CompIngredAmount_BeverageCust");

                //entity.HasOne(d => d.CompIngred).WithMany(p => p.ComplexIngredientAmounts)
                //    .HasForeignKey(d => d.CompIngredId)
                //    .OnDelete(DeleteBehavior.ClientSetNull)
                //    .HasConstraintName("CompIngredAmount_CumpIngred");
            });

            // rename to singular in db
            modelBuilder.Entity<ComplexIngredient>()
                .ToTable("ComplexIngredient");

            modelBuilder.Entity<Ingredient>()
                .ToTable("Ingredient");

            modelBuilder.Entity<BeverageCustomisation>()
                .ToTable("BeverageCustomisation");

            modelBuilder.Entity<BeverageType>()
                .ToTable("BeverageType");

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JwtAuthApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthApi.Data
{
    public class ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : IdentityDbContext<AppUser>(options)
    {
        // DbSet for RefreshToken table
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<FoodImage> FoodImages { get; set; }
        public DbSet<Logo> Logos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure RefreshToken-User relationship
            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure FoodItem
            builder.Entity<FoodItem>()
                .HasOne(f => f.Seller)
                .WithMany(u => u.FoodItems)
                .HasForeignKey(f => f.SellerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Order
            builder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.Seller)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure OrderItem
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.FoodItem)
                .WithMany(f => f.OrderItems)
                .HasForeignKey(oi => oi.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Review
            builder.Entity<Review>()
                .HasOne(r => r.Order)
                .WithOne(o => o.Review)
                .HasForeignKey<Review>(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.Seller)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Review>()
                .HasOne(r => r.FoodItem)
                .WithMany(f => f.Reviews)
                .HasForeignKey(r => r.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FoodImage>()
                .HasOne(i => i.FoodItem)
                .WithMany(f => f.ImageUrls)
                .HasForeignKey(i => i.FoodItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create index for faster queries
            builder.Entity<FoodImage>()
                .HasIndex(fi => fi.FoodItemId);

            builder.Entity<Logo>()
                .HasOne(l => l.Seller)
                .WithOne(S => S.Logo)
                .HasForeignKey<Logo>(l => l.SellerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Logo>()
                .HasIndex(l => l.Id)
                .IsUnique();

            // Seed roles using migration approach
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "2",
                    Name = "User",
                    NormalizedName = "USER"
                },
                  new IdentityRole
                  {
                      Id = "3",
                      Name = "Seller",
                      NormalizedName = "SELLER"
                  }
            );

            // Create indexes for better performance
            builder.Entity<FoodItem>()
                .HasIndex(f => f.SellerId);

            builder.Entity<FoodItem>()
                .HasIndex(f => f.Category);

            builder.Entity<Order>()
                .HasIndex(o => o.CustomerId);

            builder.Entity<Order>()
                .HasIndex(o => o.SellerId);

            builder.Entity<Order>()
                .HasIndex(o => o.Status);

            builder.Entity<Review>()
                .HasIndex(r => r.SellerId);
        }
    }
}
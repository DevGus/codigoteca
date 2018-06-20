using codigoteca.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace codigoteca
{
    public class CodigotecaDBContext : DbContext
    {
        public CodigotecaDBContext() : base("codigoteca")
        {

        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<UserGroups> UserGroups { get; set; }
    /*
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserGroups>()
                .HasKey(c => new { c.User_UserID, c.Group_GroupID });

            modelBuilder.Entity<Users>()
                .HasMany(c => c.UserGroups)
                .WithRequired()
                .HasForeignKey(c => c.User_UserID);

            modelBuilder.Entity<Groups>()
                .HasMany(c => c.UserGroups)
                .WithRequired()
                .HasForeignKey(c => c.Group_GroupID);
        }*/
    }
}
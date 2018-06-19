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
        public CodigotecaDBContext() : base("Codigoteca")
        {

        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Invitation> Invitations{ get; set; }
    }

    /*protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }*/

}
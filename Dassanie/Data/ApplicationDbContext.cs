using System;
using System.Collections.Generic;
using System.Text;
using Dassanie.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dassanie.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { 
        }
        public DbSet<UserDetail> UserDetails { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<TwitterFollow> TwitterFollows { get; set; }

    }
}

﻿using Authorization.API.Model;
using Microsoft.EntityFrameworkCore;

namespace Authorization.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
    }

}

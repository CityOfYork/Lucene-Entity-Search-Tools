using CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces;
using CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests.Helpers
{
    public class MockContext : DbContext, IDbContext
    {
        public virtual DbSet<User> Users { get; set; }
    }

    public class MockNonIndexableContext: DbContext, IDbContext
    {
        public virtual DbSet<NonIndexable> NonIndexables { get; set; }
    }
}

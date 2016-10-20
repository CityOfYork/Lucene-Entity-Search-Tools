using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces
{
    public interface IDbContext
    {
        int SaveChanges();

        Task<int> SaveChangesAsync();

        DbChangeTracker ChangeTracker { get; }
        DbSet Set(Type entityType);
    }
}

using CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces;
using CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests.Helpers
{
    public class TestDbContext: DbContext, IDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<City> Cities { get;set; }

        public TestDbContext() : base("LuceneEntitySearchToolsTestContext")
        {

            Database.SetInitializer<TestDbContext>(new DropCreateDatabaseAlways<TestDbContext>());

            if (!Users.Any())
            {
                TextReader reader = new StreamReader("Helpers\\TestData\\MOCK_USERS.csv");

                string data = reader.ReadLine();

                while ((data = reader.ReadLine()) != null)
                {
                    string[] line = data.Split(',');
                    Users.Add(new User()
                    {
                        FirstName = line[1],
                        Surname = line[2],
                        Email = line[3],
                        IndexId = new Guid(line[4]),
                        JobTitle = line[5]
                    });
                }
                reader.Close();
                SaveChanges();
            }

            if (!Cities.Any())
            {
                TextReader reader = new StreamReader("Helpers\\TestData\\MOCK_CITIES.csv");

                string data = reader.ReadLine();
                while ((data = reader.ReadLine()) != null)
                {
                    string[] line = data.Split(',');
                    Cities.Add(new City()
                    {
                        Id = int.Parse(line[0]),
                        Country = line[1],
                        Code = line[2],
                        Name = line[3],
                        IndexId = new Guid(line[4])
                    });
                }
                reader.Close();
                SaveChanges();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Ricettario.Core.Accessors;
using ServiceStack.Data;

namespace Ricettario.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDbConnectionFactory _dbFactory;

        public HomeController(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Schedule", "Main");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public void CreateUserDatabase()
        {
            // This is the query which will create a new table in our database file with three columns. An auto increment column called "ID", and two NVARCHAR type columns with the names "Key" and "Value"
            var createTableQueries = new[] {@"CREATE TABLE IF NOT EXISTS [AspNetRoles] (
    [Id]   NVARCHAR (128) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR (256) NOT NULL
);",
@"CREATE TABLE IF NOT EXISTS [AspNetUsers] (
    [Id]                   NVARCHAR (128) NOT NULL PRIMARY KEY,
    [Email]                NVARCHAR (256) NULL,
    [EmailConfirmed]       BIT            NOT NULL,
    [PasswordHash]         NVARCHAR (4000) NULL,
    [SecurityStamp]        NVARCHAR (4000) NULL,
    [PhoneNumber]          NVARCHAR (4000) NULL,
    [PhoneNumberConfirmed] BIT            NOT NULL,
    [TwoFactorEnabled]     BIT            NOT NULL,
    [LockoutEndDateUtc]    DATETIME       NULL,
    [LockoutEnabled]       BIT            NOT NULL,
    [AccessFailedCount]    INT            NOT NULL,
    [UserName]             NVARCHAR (256) NOT NULL
);",
@"CREATE TABLE IF NOT EXISTS [AspNetUserRoles] (
    [UserId] NVARCHAR (128) NOT NULL,
    [RoleId] NVARCHAR (128) NOT NULL,
    PRIMARY KEY ([UserId], [RoleId]),
    FOREIGN KEY(UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    FOREIGN KEY(RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);",
@"CREATE TABLE IF NOT EXISTS [AspNetUserLogins] (
    [LoginProvider] NVARCHAR (128) NOT NULL,
    [ProviderKey]   NVARCHAR (128) NOT NULL,
    [UserId]        NVARCHAR (128) NOT NULL,
    PRIMARY KEY ([LoginProvider], [ProviderKey], [UserId]),
    FOREIGN KEY(UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);",
@"CREATE TABLE IF NOT EXISTS [AspNetUserClaims] (
    [Id]    INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    [UserId]     NVARCHAR (128) NOT NULL,
    [ClaimType]  NVARCHAR (4000) NULL,
    [ClaimValue] NVARCHAR (4000) NULL,
    FOREIGN KEY(UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);",
@"CREATE TABLE IF NOT EXISTS [__MigrationHistory] (
    [MigrationId]    NVARCHAR (150)  NOT NULL,
    [ContextKey]     NVARCHAR (300)  NOT NULL,
    [Model]          VARBINARY (4000) NOT NULL,
    [ProductVersion] NVARCHAR (32)   NOT NULL,
    PRIMARY KEY ([MigrationId], [ContextKey])
);"};

            System.Data.SQLite.SQLiteConnection.CreateFile(@"d:\work\Ricettario\Ricettario\App_Data ricettario.db3");        // Create the file which will be hosting our database
            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection(@"data source=d:\work\Ricettario\Ricettario\App_Data\ricettario.db3"))
            {
                using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                {
                    con.Open();                             // Open the connection to the database

                    foreach (var createTableQuery in createTableQueries)
                    {
                        com.CommandText = createTableQuery;     // Set CommandText to our query that will create the table
                        com.ExecuteNonQuery();                  // Execute the query
                    }

                    //com.CommandText = "Select * FROM MyTable";      // Select all rows from our database table

                    //using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                    //{
                    //    while (reader.Read())
                    //    {
                    //        Console.WriteLine(reader["Key"] + " : " + reader["Value"]);     // Display the value of the key and value column for every row
                    //    }
                    //}
                    con.Close();        // Close the connection to the database
                }
            }
        } 

        public ActionResult Debug()
        {
            ViewBag.Message = System.Configuration.ConfigurationManager.ConnectionStrings["AccountConnection"].ConnectionString;

            ViewBag.TestDb = TestDb();

            ViewBag.RicettarioDb = RicettarioDb();

            ViewBag.OrmLite = OrmLite();

            return View();
        }

        public string OrmLite()
        {
            using (var db = new FluentQuery(_dbFactory.OpenDbConnection()))
            {
                var accessor = new WeekAccessor(db);
                var week = accessor.GetSingle(20140428);
                return week.Name + ' ' + week.Id;
            }
        }

        public string RicettarioDb()
        {
            var sb = new StringBuilder();
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["RicettarioConnection"].ConnectionString;
            using (var con = new System.Data.SQLite.SQLiteConnection(connectionString))
            using (var com = new System.Data.SQLite.SQLiteCommand(con))
            {
                con.Open();
                com.CommandText = "Select * FROM WeekSchedule";

                using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sb.AppendLine(reader["Id"] + " : " + reader["Name"]);
                    }
                }
                con.Close(); // Close the connection to the database
            }
            return sb.ToString().Substring(0, 300);
        }

        public string TestDb()
        {
            var sb = new StringBuilder();
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["AccountConnection"].ConnectionString;
            using (var con = new System.Data.SQLite.SQLiteConnection(connectionString))
            using (var com = new System.Data.SQLite.SQLiteCommand(con))
            {
                con.Open();
                com.CommandText = "Select * FROM AspNetRoles";

                using (System.Data.SQLite.SQLiteDataReader reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sb.AppendLine(reader["Id"] + " : " + reader["Name"]);
                    }
                }
                con.Close(); // Close the connection to the database
            }

            return sb.ToString();
        }
    }
}
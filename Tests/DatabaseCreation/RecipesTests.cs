using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ricettario.Core.Accessors;
using ServiceStack.OrmLite;

namespace Tests.DatabaseCreation
{
    public class RecipesTests
    {
        [Test]
        public void ReadRecipes()
        {
            var dbFile = @"d:\work\Ricettario\Ricettario\App_Data\db.sqlite";
            var factory = new OrmLiteConnectionFactory(dbFile, SqliteDialect.Provider);
            var accessor = new RecipeAccessor(factory);
            var recipe = accessor.GetById(1);
            Console.WriteLine(recipe.Name);
            Console.WriteLine(recipe.Ingredients);
        }


        [Test]
        public void CreateDatabase()
        {
            var createTableQueries = new[] {
@"CREATE TABLE IF NOT EXISTS [Recipes] (
    [Id]                   INTEGER PRIMARY KEY,
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
            var dbFile = @"d:\work\Ricettario\Ricettario\App_Data\ricettario_shared.db3";
            System.Data.SQLite.SQLiteConnection.CreateFile(dbFile);
            using (System.Data.SQLite.SQLiteConnection con = new System.Data.SQLite.SQLiteConnection(@"data source=" + dbFile))
            {
                con.Open();
                foreach (var createTableQuery in createTableQueries)
                {
                    using (System.Data.SQLite.SQLiteCommand com = new System.Data.SQLite.SQLiteCommand(con))
                    {
                        com.CommandText = createTableQuery;
                        com.ExecuteNonQuery();
                    }
                }
                con.Close();
            }
        }
    }
}

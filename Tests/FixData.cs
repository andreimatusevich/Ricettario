using System;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Ricettario;
using ServiceStack.OrmLite;
using ServiceStack.Text;

namespace Tests
{
    public class FixData
    {
        public static string SqliteFileDb = @"d:\work\Ricettario\Ricettario\App_Data\db.sqlite";

        [Test]
        [UseReporter(typeof(WinMergeReporter))]
        public void Load()
        {
            var weekNumber = 20140721;

            var factory = new OrmLiteConnectionFactory(SqliteFileDb, SqliteDialect.Provider);

            using (var db = factory.OpenDbConnection())
            {
                var list = db.Select<ShoppingList>();
                foreach (var shoppingList in list)
                {
                    Console.WriteLine(shoppingList.WeekNumber);
                }
            }


            //var week = LookupWeekSchedule(weekNumber);
        }

        private WeekSchedule LookupWeekSchedule(int weekNumber)
        {
            //return Db.Select<WeekSchedule>(q => q.Id == weekNumber).FirstOrDefault();
            return null;
        }
    }
}
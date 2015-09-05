using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Ricettario;
using ServiceStack.OrmLite;
using ServiceStack.Text;

namespace Tests
{
    public class WeekTest
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
                WeekSchedule week = db.Select<WeekSchedule>(q => q.Id == weekNumber).FirstOrDefault();
                Approvals.Verify(week.Dump());
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

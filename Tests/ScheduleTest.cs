using System;
using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Ricettario;

namespace Tests
{
    public class ScheduleTest
    {
        [Test]
        [UseReporter(typeof(WinMergeReporter))]
        public void Week()
        {
            var week = ScheduleFactory.CreateWeekScheduleFor(20140203);

            week.Days[0].Meals[0].Entries.Add(new EntryReference() { Index = 0, Name = "Mega havka", RecipeId = 77 });

            var json = JsonConvert.SerializeObject(week, Formatting.Indented);
            
            Approvals.Verify(json);

            //Console.WriteLine(week.Name);
            //foreach (var day in week.Days)
            //{
            //    Console.WriteLine(day.Name + " id:" + day.Index);
            //    foreach (var meal in day.Meals)
            //    {
            //        //Console.WriteLine(meal.Name + " id:" + meal.Index);
            //    }
            //}
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ricettario
{
    [Serializable]
    public class ScheduleEntityBase
    {
        public int Index { get; set; }
        public string Name { get; set; }
    }

    [Serializable]
    public class WeekSchedule : ScheduleEntityBase, IIdentifiable
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public List<DaySchedule> Days { get; set; }
        public List<NamedReference> Products { get; set; }

        public WeekSchedule()
        {
            Days = new List<DaySchedule>();
            Products = new List<NamedReference>();
        }
    }

    [Serializable]
    public class DaySchedule : ScheduleEntityBase
    {
        public List<MealPlan> Meals { get; set; }
        public DateTime Date { get; set; }

        public DaySchedule()
        {
            Meals = new List<MealPlan>();
        }
    }

    [Serializable]
    public class MealPlan : ScheduleEntityBase
    {
        public List<EntryReference> Entries { get; set; }

        public MealPlan()
        {
            Entries = new List<EntryReference>();
        }
    }

    [Serializable]
    public class GlobalEntryReference
    {
        public int WeekNumber { get; set; }
        public int DayIndex { get; set; }
        public int MealIndex { get; set; }
        public int RecipeId { get; set; }
    }

    [Serializable]
    public class EntryReference : ScheduleEntityBase
    {
        public int RecipeId { get; set; }
    }

    public class ScheduleFactory
    {
        public const string BreakfastName = "Breakfast";
        public const string LunchName = "Lunch";
        public const string DinnerName = "Dinner";

        public const int Breakfast = 0;
        public const int Lunch = 1;
        public const int Dinner = 2;

        public static WeekSchedule CreateWeekScheduleFor(int? day)
        {
            var weekStart = (day.HasValue ? day.Value.ToWeek() : DateTime.Now).GetNextWeekday(DayOfWeek.Monday);
            var weekNumber = weekStart.ToWeekNumber();
            var week = new WeekSchedule()
            {
                Id = weekNumber,
                Date = weekStart,
                Name = weekNumber.ToWeekName(),
                Index = 0
            };

            foreach (var n in Enumerable.Range(0, 8))
            {
                week.Days.Add(CreateDay(week.Date, n));
            }
            return week;
        }

        private static DaySchedule CreateDay(DateTime date, int n)
        {
            var dayDate = date.AddDays(n - 2);
            var day = new DaySchedule { Date = dayDate, Name = dayDate.ToString("dddd"), Index = n };
            day.Meals.Add(CreateMeal(BreakfastName, Breakfast));
            day.Meals.Add(CreateMeal(LunchName, Lunch));
            day.Meals.Add(CreateMeal(DinnerName, Dinner));
            return day;
        }

        private static MealPlan CreateMeal(string name, int n)
        {
            return new MealPlan() { Name = name, Index = n };
        }
    }
}
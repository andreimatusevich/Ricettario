namespace Ricettario
{
    public static class ScheduleEasyAccess
    {
        private const int start = 2;

        public static DaySchedule Sunday(this WeekSchedule week)
        {
            return week.Days[start-1];
        }

        public static DaySchedule Monday(this WeekSchedule week)
        {
            return week.Days[start];
        }

        public static DaySchedule Tuesday(this WeekSchedule week)
        {
            return week.Days[start + 1];
        }
        
        public static DaySchedule Wednesday(this WeekSchedule week)
        {
            return week.Days[start + 2];
        }

        public static DaySchedule Thursday(this WeekSchedule week)
        {
            return week.Days[start + 3];
        }
        
        public static DaySchedule Friday(this WeekSchedule week)
        {
            return week.Days[start + 4];
        }

        public static DaySchedule Saturday(this WeekSchedule week)
        {
            return week.Days[start + 5];
        }

        public static MealPlan Breakfast(this DaySchedule day)
        {
            return day.Meals[ScheduleFactory.Breakfast];
        }

        public static MealPlan Lunch(this DaySchedule day)
        {
            return day.Meals[ScheduleFactory.Lunch];
        }

        public static MealPlan Dinner(this DaySchedule day)
        {
            return day.Meals[ScheduleFactory.Dinner];
        }
        
        public static bool IsDinner(this int index)
        {
            return index == ScheduleFactory.Dinner;
        }

        public static bool IsNotEndOfWeek(this int index)
        {
            return index != 7;
        }
    }
}
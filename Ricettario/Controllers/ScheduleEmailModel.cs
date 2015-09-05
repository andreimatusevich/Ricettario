using System;
using System.Collections.Generic;
using System.Linq;
using Ricettario.Core.Accessors;
using ServiceStack.Data;

namespace Ricettario.Controllers
{
    public class ScheduleEmailModelFactory
    {
        private readonly IDbConnectionFactory _dbFactory;

        public ScheduleEmailModelFactory(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public ScheduleEmailModel Get(int weekNumber)
        {
            using (var db = new FluentQuery(_dbFactory.OpenDbConnection()))
            {
                var recipes = db.Select<Recipe>();
                var schedule = db.Single<WeekSchedule>(s => s.Id == weekNumber);
                var selectedRecipes = schedule.Days.SelectMany(d => d.Meals.SelectMany(m => m.Entries.Select(e => e.RecipeId)))
                    .Distinct().Join(recipes, i => i, recipe => recipe.Id, (i, recipe) => recipe).ToList();
                return CreateModel(schedule, selectedRecipes);
            }
        }

        private ScheduleEmailModel CreateModel(WeekSchedule schedule, List<Recipe> selectedRecipes)
        {
            var model = new ScheduleEmailModel() { Name = schedule.Name };
            foreach (var day in schedule.Days)
            {
                var dayModel = new ScheduleEmailModel.Day() { Name = day.Name };
                model.Days.Add(dayModel);
                foreach (var meal in day.Meals)
                {
                    var mealModel = new ScheduleEmailModel.Meal() { Name = meal.Name };
                    dayModel.Meals.Add(mealModel);
                    if (meal.Entries.Any())
                    {
                        foreach (var entry in meal.Entries)
                        {
                            var entryModel = new ScheduleEmailModel.Entry() { Name = entry.Name };
                            mealModel.Entries.Add(entryModel);

                            var recipe = selectedRecipes.First(r => r.Id == entry.RecipeId);
                            entryModel.Reference = entryModel.ReferenceTitle = recipe.Reference ?? "";
                            entryModel.IsUrl = entryModel.Reference.StartsWith("http");
                            if (entryModel.IsUrl)
                            {
                                if (String.IsNullOrEmpty(recipe.Description) || recipe.Description.Length > 35)
                                {
                                    entryModel.ReferenceTitle = new Uri(entryModel.Reference)
                                        .GetLeftPart(UriPartial.Authority)
                                        .Replace("http://", "")
                                        .Replace("https://", "")
                                        .Replace(".livejournal.com", "");
                                }
                                else
                                {
                                    entryModel.ReferenceTitle = recipe.Description;
                                }
                            }

                        }
                    }
                }
            }
            return model;
        }
    }

    [Serializable]
    public class ScheduleEmailModel
    {
        public ScheduleEmailModel()
        {
            Days = new List<Day>();
        }

        public string Name { get; set; }
        public List<Day> Days { get; set; }

        [Serializable]
        public class Day
        {
            public Day()
            {
                Meals = new List<Meal>();
            }

            public string Name { get; set; }
            public List<Meal> Meals { get; set; }
        }

        [Serializable]
        public class Meal
        {
            public Meal()
            {
                Entries = new List<Entry>();
            }

            public string Name { get; set; }
            public List<Entry> Entries { get; set; }
        }

        [Serializable]
        public class Entry
        {
            public bool IsUrl { get; set; }
            public string Name { get; set; }
            public string ReferenceTitle { get; set; }
            public string Reference { get; set; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ricettario.Core.Accessors
{
    public class WeekAccessor
    {
        private IEasyData _db;

        List<Recipe> _recipes;
        private IEnumerable<Recipe> Recipes
        {
            get { return _recipes ?? (_recipes = _db.Select<Recipe>()); }
        }

        private EntryReference LookupEntry(string name)
        {
            var recipe = Recipes.Single(r => r.Name == name);
            return new EntryReference { RecipeId = recipe.Id, Name = recipe.Name };
        }

        private EntryReference LookupEntry(int id)
        {
            var recipe = Recipes.Single(r => r.Id == id);
            return new EntryReference { RecipeId = recipe.Id, Name = recipe.Name };
        }

        public WeekAccessor(IEasyData db)
        {
            _db = db;
        }

        public WeekSchedule GetSingle(int weekNumber)
        {
            return _db.Single<WeekSchedule>(q => q.Id == weekNumber);
        }

        public WeekSchedule GetOrCreate(int weekNumber)
        {
            var week = _db.Single<WeekSchedule>(q => q.Id == weekNumber);
            if (week == null)
            {
                week = ScheduleFactory.CreateWeekScheduleFor(weekNumber);
                PrePopulate(week);
                _db.Insert(week);
            }
            return week;
        }

        private void PrePopulate(WeekSchedule week)
        {
            week.Sunday().Breakfast().Entries.Add(LookupEntry("Омлет"));
            //week.Sunday().Breakfast().Entries.Add(LookupEntry("Компот"));
            //week.Sunday().Breakfast().Entries.Add(LookupEntry("Мюсли"));

            week.Monday().Breakfast().Entries.Add(LookupEntry("Panna cotta with berries"));


            week.Tuesday().Breakfast().Entries.Add(LookupEntry("Каша рисовая"));
            week.Tuesday().Breakfast().Entries.Add(LookupEntry("Блинчики кабачковые"));

            //week.Wednesday().Breakfast().Entries.Add(LookupEntry("Морковный сок"));
            week.Wednesday().Breakfast().Entries.Add(LookupEntry("Творог"));
            week.Wednesday().Breakfast().Entries.Add(LookupEntry("Налистники с творогом"));
            week.Wednesday().Breakfast().Entries.Add(LookupEntry("Сырники"));
            //week.Wednesday().Breakfast().Entries.Add(LookupEntry("Мюсли"));

            week.Thursday().Breakfast().Entries.Add(LookupEntry("Каша овсянная"));

            //week.Friday().Breakfast().Entries.Add(LookupEntry("Блинчики пухлые"));
            //week.Friday().Breakfast().Entries.Add(LookupEntry("Блины с бананом"));
            
            //week.Saturday().Breakfast().Entries.Add(LookupEntry("Свекольный сок"));
            week.Saturday().Breakfast().Entries.Add(LookupEntry("Каша манная"));
            
        }

        public void Delete(int weekNumber)
        {
            _db.Delete<WeekSchedule>(q => q.Id == weekNumber);
        }

        public void DeleteMeal(GlobalEntryReference entry)
        {
            var week = GetSingle(entry.WeekNumber);
            var meal = week.Days[entry.DayIndex].Meals[entry.MealIndex];
            meal.Entries = meal.Entries.Where(e => e.RecipeId != entry.RecipeId).ToList();
            _db.Update(week);
        }

        public void AddMeal(GlobalEntryReference entry, bool leftover = false)
        {
            var week = GetSingle(entry.WeekNumber);
            var meal = week.Days[entry.DayIndex].Meals[entry.MealIndex];

            var recipe = _db.Single<Recipe>(r => r.Id == entry.RecipeId);
            var namedEntry = new EntryReference { RecipeId = recipe.Id, Name = recipe.Name };

            meal.Entries.Add(namedEntry);

            if (leftover && entry.MealIndex.IsDinner() && entry.DayIndex.IsNotEndOfWeek())
            {
                var nextDayLunch = week.Days[entry.DayIndex + 1].Lunch();
                nextDayLunch.Entries.Add(namedEntry);
            }
            _db.Update(week);
        }

        public void AddRecipes(int weekNumber, int dayId, int mealId, int[] recipes)
        {
            var week = GetSingle(weekNumber);
            var day = week.Days[dayId];
            var meal = day.Meals[mealId];
            foreach (var recipe in recipes)
            {
                meal.Entries.Add(LookupEntry(recipe));
            }
            _db.Update(week);
        }

        public bool HasShoppingList(int weekNumber)
        {
            return _db.Single<ShoppingList>(q => q.WeekNumber == weekNumber) != null;
        }

        public void AddProduct(int weekNumber, NamedReference product)
        {
            var week = GetSingle(weekNumber);
            week.Products.Add(product);
            _db.Update(week);
        }
    }
}
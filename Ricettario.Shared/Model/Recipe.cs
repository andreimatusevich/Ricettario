using System;
using System.Collections.Generic;

namespace Ricettario.Shared.Model
{
    public interface IVersioned
    {
        string GUID { get; }
        DateTime Version { get; }
        string SchemaVersion { get;  }
    }

    public interface IThing
    {
        string Name { get; set; }
        string Source { get; set; }
        string SourceDescription { get; set; }
    }

    public interface ICreativeWork
    {
        string Author { get; set; }
        List<string> Tags { get; set; }
    }
    
    public class Recipe : IVersioned, IThing, ICreativeWork
    {
        //IVersioned
        public string GUID { get; set; }
        public DateTime Version { get; set; }
        public string SchemaVersion { get; set; }

        //IThing
        public string Name { get; set; }
        public string Source { get; set; }
        public string SourceDescription { get; set; }

        //ICreativeWork
        public List<string> Tags { get; set; }
        public string Author { get; set; }

        //Recipe
        public List<string> RecipeIngredients { get; set; }
        public List<string> RecipeInstructions { get; set; }
        public string RecipeYield { get; set; }
        public string RecipeDescription { get; set; }
    }

    public class RecipeFactory : IFactory<Recipe>
    {
        public Recipe Create()
        {
            return new Recipe()
            {
                GUID = Guid.NewGuid().ToString("N"),
                Version = DateTime.Now,
                SchemaVersion = "v1.0"
            };
        }
    }

    public interface IFactory<T>
    {
        T Create();
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ServiceStack.DataAnnotations;

namespace Ricettario
{
    [Serializable]
    public class Recipe : IIdentifiable, IParent<Ingredient>
    {
        public Recipe()
        {
            Ingredients = new List<Ingredient> {new Ingredient()};
            Name = String.Empty;
            Tags = String.Empty;
            Reference = String.Empty;
            Description = String.Empty;
        }

        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public List<Ingredient> Ingredients { get; set; }
        
        [Ignore]
        [JsonIgnore]
        public List<Ingredient> Childs
        {
            get { return Ingredients; }
            set { Ingredients = value; }
        }
        public string Tags { get; set; } 
    }

    [Serializable]
    public class Ingredient : IIdentifiable
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [Ignore]
        [JsonIgnore]
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class Product : IIdentifiable
    {
        private Location _whereToBuy;

        public Product()
        {
            Name = String.Empty;
            WhereToBuy = new Location();
            Synonyms = String.Empty;
        }

        protected bool Equals(Product other)
        {
            return Id == other.Id && string.Equals(Name, other.Name) && Equals(WhereToBuy, other.WhereToBuy) && Buy.Equals(other.Buy);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Product) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Id;
                hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (WhereToBuy != null ? WhereToBuy.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Buy.GetHashCode();
                return hashCode;
            }
        }

        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Synonyms { get; set; }

        public void AddSynonym(string name)
        {
            if (String.IsNullOrWhiteSpace(Synonyms))
            {
                Synonyms = name;
            }
            else
            {
                Synonyms += "," + name;
            }
        }

        public Location WhereToBuy
        {
            get { return _whereToBuy; }
            set
            {
                if (value != null)
                {
                    _whereToBuy = value;
                }
            }
        }

        public bool Buy { get; set; }
        public bool Skip { get; set; }
    }

    public interface IIdentifiable
    {
        int Id { get; set; }
        string Name { get; set; }
    }

    [Serializable]
    public class NamedReference : IIdentifiable
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public interface IParent<T>
    {
        List<T> Childs { get; set; } 
    }

    public class Location
    {
        public static readonly Location Empty = new Location(); 

        protected bool Equals(Location other)
        {
            return StoreId == other.StoreId && DepartmentId == other.DepartmentId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Location) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (StoreId*397) ^ DepartmentId;
            }
        }

        public int StoreId { get; set; }
        public int DepartmentId { get; set; }
    }

    public class Store : IIdentifiable, IParent<Department>
    {
        public Store()
        {
            Departments = new List<Department>();
        }

        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
        public List<Department> Departments { get; set; }

        [Ignore]
        [JsonIgnore]
        public List<Department> Childs
        {
            get { return Departments; }
            set { Departments = value; }
        }
    }

    public class Department : IIdentifiable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OrderHelper { get; set; }
    }

    public class StoreDepartment
    {
        public int StoreId { get; set; }
        public string Store { get; set; }
        public int DepartmentId { get; set; }
        public string Department { get; set; }
    }
}

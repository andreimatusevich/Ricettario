using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using ServiceStack.DataAnnotations;

namespace Ricettario
{
    [Serializable]
    public class ShoppingList : IIdentifiable, IParent<ShoppingListItem>
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
        public int WeekNumber { get; set; }
        public List<ShoppingListItem> Items { get; set; }

        [Ignore]
        [JsonIgnore]
        [ScriptIgnore]
        public List<ShoppingListItem> Childs
        {
            get { return Items; }
            set { Items = value; }
        }
    }

    [Serializable]
    public class ShoppingListItem : IIdentifiable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Store { get; set; }
        public string Department { get; set; }
        public string Recipe { get; set; }
        public string Product { get; set; }
        public bool Buy { get; set; }
        public bool Bought { get; set; }
        public string OrderHelper { get; set; }
        //public RecordData Meta { get; set; }
    }

    [Serializable]
    public class RecordData
    {
        public bool? Deleted { get; set; }
    }
}
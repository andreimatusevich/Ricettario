using System;
using System.Collections.Generic;

namespace Ricettario.Models
{
    [Serializable]
    public class ShoppingListViewModel : IIdentifiable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int WeekNumber { get; set; }
        public List<Store> Stores { get; set; }

        public class Store
        {
            public string Name { get; set; }
            public List<ShoppingListItem> Items { get; set; }
        }
    }
}
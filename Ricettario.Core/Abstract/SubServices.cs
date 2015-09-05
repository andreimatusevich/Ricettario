using System.Collections.Generic;

namespace Ricettario.Core.Abstract
{
    //TODO refactor to specific entities 
    public class EntityUnifiedRequest
    {
        public string Type { get; set; }
        public string Action { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }

        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public int Store { get; set; }
        public int Department { get; set; }
        public int Product { get; set; }
        public bool Buy { get; set; }
        public string OrderHelper { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public string Synonyms { get; set; }
        public string Ingredients { get; set; }
    }

    //TODO refactor to specific entities 
    public class EntityUnifiedResponse
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public IEnumerable<GridColumn> Columns { get; set; }
        public string Url { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }
    }
    
    public class Select2CellWithGroupsModel
    {
        public object name { get; set; }
        public IEnumerable<IEnumerable<object>> values { get; set; }
    }

    public class GridColumn
    {
        public string Name { get; set; }
        public string Type { get; set; }

        //var countries2 = [["Afghanistan", 0], ["Afg", 1], ["Ant", 2], ["Belav", 3], ["Bafto", 4]];
        public IEnumerable<IEnumerable<object>> Select2Cell { get; set; }
        //var numbers = [{name: 10, values: [
        //  [1, 1], [2, 2], [3, 3], [4, 4], [5, 5],
        //  [6, 6], [7, 7], [8, 8], [9, 9], [10, 10]
        //]}];
        public IEnumerable<Select2CellWithGroupsModel> Select2CellWithGroups { get; set; }
    }

    public interface ISubService
    {
        object Post(EntityUnifiedRequest request);
        object Get(EntityUnifiedRequest request);
        bool CanHandle(string type);
    }
    
    public interface IHttpServiceBase
    {
        object Redirect(string url);
    }
}
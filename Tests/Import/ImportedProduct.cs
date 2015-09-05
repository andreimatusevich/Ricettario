using NUnit.Framework.Constraints;

namespace Tests.Import
{
    public class ImportedProduct
    {
        public static readonly ImportedProduct Empty = new ImportedProduct {Product = "", Store = "", Department = "", Buy = "", Skip = "", Translation = ""};

        public string Product { get; set; }
        public string Store { get; set; }
        public string Department { get; set; }	
        public string Buy { get; set; }	
        public string Skip { get; set; }
        public string Translation { get; set; }	
    }

    public class ImportedRecipe
    {
        public string Row { get; set; }
        public string Entry { get; set; }
        public string Pagano { get; set; }
        public string DamianBook { get; set; }
        public string Receptishi { get; set; }
        public string Dietishi { get; set; }
        public string Internet { get; set; }
        public string Ingredients { get; set; }
        public string Product { get; set; }
    }

    public class ImportedRecipeType
    {
        public string Type { get; set; }
        public string Product { get; set; }
    }
}
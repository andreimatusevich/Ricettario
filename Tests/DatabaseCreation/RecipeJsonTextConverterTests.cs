using System;
using System.IO;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using ASP;
using Newtonsoft.Json;
using NUnit.Framework;
using Ricettario.Shared;
using Ricettario.Shared.Model;

namespace Tests.DatabaseCreation
{
    public class RecipeJsonTextConverterTests
    {
        [Test]
        [UseReporter(typeof(WinMergeReporter))]
        public void Simple()
        {
            var fileName = @"..\..\DatabaseCreation\Data\simple_recepie.txt";
            TestParse(fileName);
        }

        [Test]
        [UseReporter(typeof(WinMergeReporter))]
        public void GoogleDocs()
        {
            var fileName = @"..\..\DatabaseCreation\Data\google_docs_recepie.txt";
            TestParse(fileName);
        }

        [Test]
        [UseReporter(typeof(WinMergeReporter))]
        public void WithDescription()
        {
            var fileName = @"..\..\DatabaseCreation\Data\classic_buttermilk_waffles.txt";
            TestParse(fileName);
        }

        private static void TestParse(string fileName)
        {
            var lines = File.ReadAllLines(fileName);

            var parser = new RecipeTextConverter(new TestRecipeFactory());
            var recepie = parser.FromText(lines);

            var recepieJson = JsonConvert.SerializeObject(recepie, Formatting.Indented,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});

            var recepieText = parser.ToText(recepie); 

            Approvals.Verify(recepieJson + Environment.NewLine + "-------------------" + Environment.NewLine + recepieText);
        }
    }


    public class TestRecipeFactory: IFactory<Recipe>
    {
        public Recipe Create()
        {
            return new Recipe()
            {
            };
        }
    }
}
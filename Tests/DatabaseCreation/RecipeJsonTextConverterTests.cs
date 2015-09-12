using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using MoreLinq;
using Newtonsoft.Json;
using NUnit.Framework;
using Ricettario.Shared.Model;

namespace Tests.DatabaseCreation
{
    public static class Extensions
    {
        public static string TrimEx(this string text)
        {
            return text?.Trim(' ', ';', ',', ':');
        }

        public static bool IsAlike(this string text, params string[] ethalons)
        {
            var normalized = text?.TrimEx().ToLower();
            return ethalons.Select(e => e?.TrimEx().ToLower()).Any(e => e == normalized);
        }
    }

    public class TextRecipeParser
    {
        private readonly IFactory<Recipe> _factory;

        private static Dictionary<string, Action<Recipe, IEnumerable<string>>> _rules = new Dictionary
            <string, Action<Recipe, IEnumerable<string>>>
        {
            {
                "Name", (recepie, lines) => recepie.Name = lines.First().Trim()
            },
            {
                "RecipeYield",
                (recepie, lines) => recepie.RecipeYield = String.Join(" ", lines.Skip(1).TakeWhile(l => !String.IsNullOrEmpty(l)))
            },
            {
                "Ingredients",
                (recepie, lines) =>
                    recepie.RecipeIngredients =
                        lines.SkipWhile(l => !l.IsAlike("Ingredients"))
                            .Skip(1)
                            .SkipWhile(l => String.IsNullOrEmpty(l))
                            .TakeWhile(l => !String.IsNullOrEmpty(l)).ToList()
            },
            {
                "Instructions",
                (recepie, lines) =>
                    recepie.RecipeInstructions =
                        lines.SkipWhile(l => !l.IsAlike("Method", "Directions"))
                            .Skip(1).Where(l => !String.IsNullOrEmpty(l)).ToList()
            }
        };

        public TextRecipeParser(IFactory<Recipe> factory)
        {
            _factory = factory;
        }

        public Recipe FromLines(IEnumerable<string> lines)
        {
            var normalizedLines = lines.SkipWhile(l => String.IsNullOrEmpty(l.TrimEx())).Select(l => l.Trim()).ToList();
            var recepie = _factory.Create();
            _rules.Values.ForEach(r => r(recepie, normalizedLines));
            return recepie;
        }
    }

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

        private static void TestParse(string fileName)
        {
            var lines = File.ReadAllLines(fileName);

            var parser = new TextRecipeParser(new TestRecipeFactory());
            var recepie = parser.FromLines(lines);

            var result = JsonConvert.SerializeObject(recepie, Formatting.Indented,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            Approvals.Verify(result);
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
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using ApprovalTests;
using ApprovalTests.Reporters;
using Newtonsoft.Json;
using NUnit.Framework;
using Ricettario;
using Ricettario.Core.Accessors;
using Ricettario.Core.SubServices;

namespace Tests
{
    public class SubServicesTest
    {
        [Test]
        public void Test()
        {
            var englishSmall = "qwertyuiop[]asdfghjkl;'zxcvbnm,./";
            var russianSmall = "йцукенгшщзхъфывапролджэячсмитьбю.";
            var englishCaps = "QWERTYUIOP[]ASDFGHJKL;'ZXCVBNM,./";
            var russianCaps = "ЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮ.";

            var english = englishCaps + englishSmall;
            var russian = russianCaps + russianSmall;

            var sb = new StringBuilder();
            for(var i=0; i<english.Length; i++)
            {
                sb.AppendFormat("dictionary['{0}'] = '{1}';" + Environment.NewLine, english[i], russian[i]);
            }
            for (var i = 0; i < english.Length; i++)
            {
                sb.AppendFormat("dictionary['{0}'] = '{1}';" + Environment.NewLine, russian[i], english[i]);
            }
            var result = sb.ToString().Replace("'''", "'\\''");
            Console.WriteLine(result);
        }

        [Test]
        [UseReporter(typeof(WinMergeReporter))]
        public void ParseIngridients()
        {
            var products = JsonConvert.DeserializeObject<List<Product>>(File.ReadAllText(CsvTest.GetPath("CsvTest.Products.approved.txt"))).Skip(1).ToList();
            //var chickenbreasts = products.Where(p => p.Name == "chicken breasts");
            //products = chickenbreasts.Concat(products).ToList();
            products.Single(p => p.Name == "tomato").Synonyms += ",Томаты".ToLower();
            
            var text = @"Ingredients
4 boneless, skinless chicken breasts
4 boneless, skinless chicken breast
½ cup Dijon mustard
¼ cup maple syrup
1 tablespoon red wine vinegar
Salt & pepper
Fresh rosemary

Томаты очищенные сливовидные в собственном соку — 660 г
Помидорки черри — 500 г
Паста сухая — 100 г
Сливочное масло — 150 г
Тимьян свежий — 1 веточка
Базилик зеленый — 1 пучок
Сахар — 40 г
Оливковое масло — 50 мл
Пеперончино
Морская соль
Чеснок — 2 зубчика
";

//            text = @"
//Томаты очищенные сливовидные в собственном соку — 660 г
//Помидорки черри — 500 г";

            var sb = new StringBuilder();
            var accessor = new RecipeAccessor((IDbConnection) null);
            var rs = accessor.ParseIngridients(text, products);
            foreach (var r in rs)
            {
                sb.AppendLine(r.Id + " " + r.Description + " " + r.ProductId + " " + r.Name);
            }

            Approvals.Verify(sb.ToString());
        }
    }
}
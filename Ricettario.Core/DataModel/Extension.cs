using System;
using System.Net;
using System.Text;
using System.Linq;

namespace Ricettario
{
    public static class Extension
    {
        public static int ToWeekNumber(this DateTime date)
        {
            return date.Year * 10000 + date.Month * 100 + date.Day;
        }

        public static int WeekNumber(this WeekSchedule week)
        {
            return week.Date.ToWeekNumber();
        }

        public static DateTime ToWeek(this int date)
        {
            int year = date/10000;
            int month = (date - year*10000)/100;
            int day = date - year*10000 - month*100;
            return new DateTime(year, month, day);
        }

        public static string ToWeekName(this int date)
        {
            return date == 0 ? "" : date.ToWeek().ToString("MMMM dd");
        }

        public static DateTime GetNextWeekday(this DateTime start, DayOfWeek day)
        {
            var date = start.Date;
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)date.DayOfWeek + 7) % 7;
            return date.AddDays(daysToAdd);
        }

        public static int GetNextWeekNumber(int? weekNumber)
        {
            var number = weekNumber == null || weekNumber == 0 ? DateTime.Now.GetNextWeekday(DayOfWeek.Monday).ToWeekNumber() : weekNumber.Value;
            return number;
        }
        
        public static int ToDepartmentId(int storeId, int departmentId)
        {
            return storeId * 1000 + departmentId;
        }

        public static int FromDepartmentId(int storeId, int departmentId)
        {
            int parsedStoreId = departmentId / 1000;
            var parsedDepartmentId = departmentId - 1000 * parsedStoreId;
            if (parsedDepartmentId == 0) return parsedDepartmentId;
            if (parsedStoreId != storeId)
            {
                throw new ArgumentException(String.Format("Incorrect store id: {0} for department Id: {1}", storeId, departmentId));
            }
            return parsedDepartmentId;
        }

        public static string TranslateText(this string input, string from, string to)
        {
            if (String.IsNullOrEmpty(input)) return String.Empty;

            var url = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={2}&langpair={0}|{1}", @from, to, Uri.EscapeDataString(input));
            var webClient = new WebClient();
            webClient.Encoding = Encoding.GetEncoding("KOI8-R");
            var result = webClient.DownloadString(url);
            result = result.Substring(GetIndex(result, "id=result_box"));
            result = result.Substring(result.IndexOf("'#fff'\">") + "'#fff'\">".Length);
            result = result.Substring(0, result.IndexOf("</span>"));

            return result.ToLower();
        }

        public static bool IsCyrillic(this string text)
        {
            var cyrillic = text.ToCharArray().Count(c =>
            {
                var code = (int)c;
                return (code >= 0x0400 && code <= 0x0451);
            });
            return cyrillic > 4 || cyrillic > text.Length / 3;
        }
        
        private static int GetIndex(string result, string text, int shift = 0)
        {
            var index = result.IndexOf(text) + shift;
            return index < result.Length ? index : result.Length;
        }
    }
}
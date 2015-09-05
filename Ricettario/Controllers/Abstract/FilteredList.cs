using System;
using System.Collections.Generic;
using System.Linq;

namespace Ricettario.Controllers.Abstract
{
    public class FilteredList<T>
    {
        IEnumerable<T> _filtered;

        public FilteredList(IEnumerable<T> list)
        {
            _filtered = list;
        }

        public FilteredList<T> Where<TValue>(TValue value, Func<T, bool> where)
        {
            if (typeof(TValue) == typeof(string) && String.IsNullOrWhiteSpace(value as string))
            {
                return this;
            }
            if (!EqualsDefaultValue(value))
            {
                _filtered = _filtered.Where(where);
            }
            return this;
        }

        public bool EqualsDefaultValue<TValue>(TValue value)
        {
            return EqualityComparer<TValue>.Default.Equals(value, default(TValue));
        }

        public IEnumerable<T> ToEnumerable()
        {
            return _filtered;
        }
    }
}
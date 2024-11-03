using System.Linq.Expressions;

namespace Shop.Core.DataEF.Helpers
{
    public static class EfHelpers
    {
        public static Expression<Func<T, object>> SortHelper<T>(string sortBy)
        {
            var param = Expression.Parameter(typeof(T), "p");
            var property = Expression.Property(param, sortBy);
            var converted = Expression.Convert(property, typeof(object));

            return Expression.Lambda<Func<T, object>>(converted, param);
        }
    }
}

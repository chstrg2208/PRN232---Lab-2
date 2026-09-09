using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;

namespace PRN232.LMS.Services.Helpers
{
    public static class QueryHelper
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, string? sortString, string defaultSort = "Id")
        {
            if (string.IsNullOrWhiteSpace(sortString))
            {
                // check if defaultSort exists
                var prop = typeof(T).GetProperty(defaultSort, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    return query.OrderBy(prop.Name);
                }
                return query;
            }

            var orderParams = sortString.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var propertyInfos = typeof(T).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            var orderQueryBuilder = new StringBuilder();

            foreach (var param in orderParams)
            {
                var trimmed = param.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                bool isDescending = trimmed.StartsWith("-") || trimmed.EndsWith(" desc", StringComparison.OrdinalIgnoreCase);
                string cleanProperty;

                if (trimmed.StartsWith("-"))
                {
                    cleanProperty = trimmed[1..].Trim();
                }
                else if (trimmed.EndsWith(" desc", StringComparison.OrdinalIgnoreCase))
                {
                    cleanProperty = trimmed[..^5].Trim();
                }
                else if (trimmed.EndsWith(" asc", StringComparison.OrdinalIgnoreCase))
                {
                    cleanProperty = trimmed[..^4].Trim();
                }
                else
                {
                    cleanProperty = trimmed;
                }

                var objectProperty = propertyInfos.FirstOrDefault(pi => pi.Name.Equals(cleanProperty, StringComparison.OrdinalIgnoreCase));
                if (objectProperty == null) continue;

                var direction = isDescending ? "descending" : "ascending";
                orderQueryBuilder.Append($"{objectProperty.Name} {direction}, ");
            }

            var completeOrderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');
            if (string.IsNullOrWhiteSpace(completeOrderQuery))
            {
                return query;
            }

            return query.OrderBy(completeOrderQuery);
        }
    }
}

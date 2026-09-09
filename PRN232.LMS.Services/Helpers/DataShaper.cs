using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace PRN232.LMS.Services.Helpers
{
    public static class DataShaper
    {
        public static IEnumerable<object> ShapeCollection<T>(IEnumerable<T> entities, string? fields)
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                return entities.Cast<object>();
            }

            var propertyList = GetRequiredProperties<T>(fields);
            var shapedData = new List<object>();

            foreach (var entity in entities)
            {
                if (entity == null) continue;
                var shapedObject = FetchValues(entity, propertyList);
                shapedData.Add(shapedObject);
            }

            return shapedData;
        }

        public static object ShapeObject<T>(T entity, string? fields)
        {
            if (string.IsNullOrWhiteSpace(fields) || entity == null)
            {
                return entity!;
            }

            var propertyList = GetRequiredProperties<T>(fields);
            return FetchValues(entity, propertyList);
        }

        private static List<PropertyInfo> GetRequiredProperties<T>(string fields)
        {
            var propertyInfos = typeof(T).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            var requestedFields = fields.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(f => f.Trim().ToLowerInvariant())
                                        .ToHashSet();

            var selectedProperties = propertyInfos
                .Where(p => requestedFields.Contains(p.Name.ToLowerInvariant()))
                .ToList();

            return selectedProperties;
        }

        private static ExpandoObject FetchValues<T>(T entity, List<PropertyInfo> propertyList)
        {
            var expando = new ExpandoObject();
            var dict = (IDictionary<string, object?>)expando;

            foreach (var property in propertyList)
            {
                var propertyValue = property.GetValue(entity);
                dict[property.Name] = propertyValue;
            }

            return expando;
        }
    }
}

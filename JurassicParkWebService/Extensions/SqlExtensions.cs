using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JurassicParkWebService.Entities;

namespace JurassicParkWebService.Extensions; 

public static class SqlExtensions {
    public static void AddFromProperties<T>(this SqlParameterCollection parameters, T entity) {
        var properties = typeof(T).GetProperties().Where(x => x.Name != nameof(IdentifiableEntity.Id)).ToList();

        foreach (var property in properties) {
            var value = property.GetValue(entity);
            if (value != null && property.PropertyType.IsEnum) {
                value = value.ToString();
            }
            parameters.AddWithValue(property.Name, value);
        }
    }
}
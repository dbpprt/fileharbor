using System;
using System.Linq;
using System.Reflection;
using Dapper;

namespace Fileharbor.Common.Database
{
    /// <summary>
    ///     Uses the Name value of the ColumnAttribute specified, otherwise maps as usual.
    /// </summary>
    /// <typeparam name="T">The type of the object that this mapper applies to.</typeparam>
    public class ColumnNameAttributeTypeMapper<T> : FallbackTypeMapper
    {
        private static readonly string ColumnAttributeName = "ColumnNameAttribute";

        public ColumnNameAttributeTypeMapper()
            : base(new SqlMapper.ITypeMap[]
            {
                new CustomPropertyTypeMap(typeof(T), SelectProperty),
                new DefaultTypeMap(typeof(T))
            })
        {
        }

        private static PropertyInfo SelectProperty(Type type, string columnName)
        {
            return
                type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(
                    prop =>
                        prop.GetCustomAttributes(false)
                            // Search properties to find the one ColumnAttribute applied with Name property set as columnName to be Mapped 
                            .Any(attr => attr.GetType().Name == ColumnAttributeName
                                         &&
                                         attr.GetType().GetProperties(BindingFlags.Public |
                                                                      BindingFlags.NonPublic |
                                                                      BindingFlags.Instance)
                                             .Any(
                                                 f =>
                                                     f.Name == "Name" &&
                                                     f.GetValue(attr).ToString().ToLower() == columnName.ToLower()))
                        && // Also ensure the property is not read-only
                        (prop.DeclaringType == type
                            ? prop.GetSetMethod(true)
                            : prop.DeclaringType.GetProperty(prop.Name,
                                BindingFlags.Public | BindingFlags.NonPublic |
                                BindingFlags.Instance).GetSetMethod(true)) != null
                );
        }
    }
}
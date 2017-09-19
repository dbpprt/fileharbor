using System;

namespace Fileharbor.Common.Database
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ColumnNameAttribute : Attribute
    {
        public ColumnNameAttribute()
        {
        }

        public ColumnNameAttribute(string name)
        {
            Name = name;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public string Name { get; }
    }
}
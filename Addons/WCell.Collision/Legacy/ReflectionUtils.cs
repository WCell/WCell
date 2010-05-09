using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace WCell.Collision
{
    public static class ReflectionUtils
    {
        public static void EnumFields<T>(T obj, TextWriter writer, string indent)
        {
            Type objType = typeof(T);
            foreach (var item in objType.GetFields())
            {
                writer.WriteLine(indent + item.Name + ": {0}", item.GetValue(obj));
            }
        }
    }
}

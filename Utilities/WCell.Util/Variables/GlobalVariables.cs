using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WCell.Util.Variables;

namespace WCell.Util.Variables
{
	public static class GlobalVariables
	{
		/// <summary>
		/// Holds an array of static variable fields
		/// </summary>
		public static readonly Dictionary<string, TypeVariableDefinition> ByName =
			new Dictionary<string, TypeVariableDefinition>(StringComparer.InvariantCultureIgnoreCase);

		public static readonly Dictionary<string, TypeVariableDefinition> ByFullName =
			new Dictionary<string, TypeVariableDefinition>(StringComparer.InvariantCultureIgnoreCase);

		public static object Get(string name)
		{
			TypeVariableDefinition def;
			if (ByName.TryGetValue(name, out def))
			{
				return def.Value;
			}
			return null;
		}

		public static TypeVariableDefinition GetDefinition(string name)
		{
			TypeVariableDefinition def;
			ByName.TryGetValue(name, out def);
			return def;
		}

		public static bool Set(string name, object value)
		{
			TypeVariableDefinition def;
			if (ByName.TryGetValue(name, out def))
			{
				def.Value = value;
				return true;
			}
			return false;
		}

		public static bool Set(string name, string value)
		{
			TypeVariableDefinition def;
			if (ByName.TryGetValue(name, out def))
			{
				object valueObj = null;
				Utility.Parse(value, def.VariableType, ref valueObj);
				def.Value = valueObj;
				return true;
			}
			return false;
		}

		public static TypeVariableDefinition CreateDefinition(string name, MemberInfo member)
		{
			if (member is FieldInfo)
			{
				return new VariableField(name, (FieldInfo)member);
			}
			if (member is PropertyInfo)
			{
				return new VariableProperty(name, (PropertyInfo)member);
			}
			throw new ArgumentException("The given member \"" + member + "\" must be a FieldInfo or PropertyInfo.");
		}

		public static void AddVariablesOfAsm(Assembly asm)
		{
			var types = asm.GetTypes();
			foreach (var type in types)
			{
				foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Static))
				{
					MethodInfo setMethod;
					if (((member is FieldInfo && !((FieldInfo)member).IsInitOnly && !((FieldInfo)member).IsLiteral) ||
						(member is PropertyInfo && (setMethod = ((PropertyInfo)member).GetSetMethod()) != null && setMethod.IsPublic)) &&
						member.GetVariableType().GetInterface("ICollection") == null)
					{
						string name;
						var varAttrs = type.GetCustomAttributes<VariableAttribute>();
						if (varAttrs.Length > 0)
						{
							name = varAttrs[0].Name;
						}
						else
						{
							name = member.Name;
						}

						TypeVariableDefinition existingDef;
						if (ByName.TryGetValue(name, out existingDef))
						{
							throw new AmbiguousMatchException("Found global variable with name \"" + name + "\" twice (" + existingDef + "). " +
								"Either rename the variable or add a VariableAttribute to it to specify a different name for storage. " +
							"(public static variables that are not read-only, are automatically added to the global variable cache)");
						}

						var def = CreateDefinition(name, member);
						ByName.Add(member.Name, def);
						ByFullName.Add(def.FullName, def);
					}
				}
			}
		}
	}
}
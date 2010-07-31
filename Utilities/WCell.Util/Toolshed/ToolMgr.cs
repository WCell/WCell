using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WCell.Util.DynamicAccess;

namespace WCell.Util.Toolshed
{
	public class ToolMgr
	{
		public static void GetAllPublicStaticClassesOfAsm(Assembly asm, Dictionary<string, Type> map)
		{
			var types = asm.GetTypes();
			foreach (var type in types)
			{
				if (!type.IsStatic())
				{
					continue;
				}

				var classAttrs = type.GetCustomAttributes(true);
				var toolAttr = (ToolAttribute)classAttrs.Where((attr) => attr is ToolAttribute).First();
				if (toolAttr == null && classAttrs.Where((attr) => attr is NoToolAttribute).Count() > 0)
				{
					// NoTool - class
					continue;
				}

				var name = toolAttr != null ? toolAttr.Name : type.Name;
				if (map.ContainsKey(name))
				{
					throw new Exception(string.Format("Invalid Type name of static Tool class \"{0}\", used by {1} AND {2}",
					name, map[type.Name].FullName, type.FullName));
				}
				map.Add(name, type);
			}
		}

		public readonly Dictionary<string, IExecutable> Executables = new Dictionary<string, IExecutable>(StringComparer.InvariantCultureIgnoreCase);
		public readonly List<IExecutable> ExecutableList = new List<IExecutable>();

		public void AddStaticMethodsOfAsm(Assembly asm)
		{
			var types = asm.GetTypes();
			foreach (var type in types)
			{
				var names = new HashSet<string>();
				var classAttrs = type.GetCustomAttributes(true);
				var isToolClass = classAttrs.Where((attr) => attr is ToolAttribute).Count() > 0;
				if (!isToolClass && classAttrs.Where((attr) => attr is NoToolAttribute).Count() > 0)
				{
					// NoTool - class
					continue;
				}
				foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod))
				{
					var name = method.Name;
					if (name.StartsWith("get_") || name.StartsWith("set_"))
					{
						// ignore property setters and getters
						continue;
					}


					var excludeAttrs = method.GetCustomAttributes<NoToolAttribute>();
					if (excludeAttrs.Length > 0)
					{
						continue;
					}

					var toolAttr = method.GetCustomAttributes<ToolAttribute>().FirstOrDefault();
					if (toolAttr != null)
					{
						name = toolAttr.Name ?? name;
					}
					else if (!isToolClass)
					{
						// not a tool class and no ToolAttribute
						continue;
					}

					var success = true;
					var parms = method.GetParameters();
					for (var i = 0; i < parms.Length; i++)
					{
						var param = parms[i];
						var ptype = param.ParameterType;
						if (!ptype.IsSimpleType())
						{
							//// last value can be an array
							//if (i == parms.Length - 1 && ptype.IsArray && ptype.GetActualType().IsSimpleType())
							//{
							//    continue;
							//}

							success = false;
							break;
						}
					}

					if (success)
					{
						if (!names.Contains(name))
						{
							// Ignore overloads for the time being
							names.Add(name);
							Add(name, null, method);
						}
						else if (toolAttr != null)
						{
							throw new ToolException("Found multiple static methods with ToolAttribute, called: {0}." +
								"- Make sure that the names are unique.",
								method.GetMemberName());
						}
					}
					else if (toolAttr != null)
					{
						throw new ToolException("Static method {0} was marked with ToolAttribute" +
							" but had non-simple Parameters. " +
							"- Make sure to only give methods with simple parameters the ToolAttribute. You can exclude them with the NoToolAttribute.",
							method.GetMemberName());
					}
				}
			}
		}

		public void Add(IExecutable executable)
		{
			EnsureUniqueName(executable.Name);
			Executables.Add(executable.Name, executable);
			ExecutableList.Add(executable);
		}

		public void Add(string name, object targetObj, MethodInfo method)
		{
			Add(new MethodExcecutable(name, targetObj, method));
		}

		public IExecutable Get(string name)
		{
			IExecutable exec;
			Executables.TryGetValue(name, out exec);
			return exec;
		}

		public bool Execute(string name, params object[] args)
		{
			IExecutable executable;
			if (Executables.TryGetValue(name, out executable))
			{
				executable.Exec(args);
				return true;
			}
			return false;
		}

		private void EnsureUniqueName(string name)
		{
			if (Executables.ContainsKey(name))
			{
				throw new ToolException("Tried to add two Executables with same name (\"" + name + "\") to ToolMgr. - Make sure to use unique names.");
			}
		}
	}
}
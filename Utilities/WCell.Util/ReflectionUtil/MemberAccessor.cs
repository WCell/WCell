using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WCell.Util.ReflectionUtil
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="U">The type of the User objects</typeparam>
	public abstract class MemberAccessor<U>
	{
		#region Get Members
		/// <summary>
		/// Returns all members of an object with the given name (if character can use it)
		/// </summary>
		public MemberInfo[] GetMembers(U user, object obj, string accessName, ref object propHolder)
		{
			return GetMembers(user, obj, accessName, obj != null ? obj.GetType() : null, ref propHolder);
		}

		/// <summary>
		/// Returns all members of an object with the given name (if character can access all holders in the chain)
		/// </summary>
		public MemberInfo[] GetMembers(U user, object obj, string accessName, Type type, ref object memberHolder)
		{
			memberHolder = null;
			var propChain = accessName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			var current = obj;
			MemberInfo member = null;
			MemberInfo[] members = null;
			var isStatic = obj == null || accessName.StartsWith(".");
			var i = 0;
			if (isStatic)
			{
				if (type == null)
				{
					if (propChain.Length < 2)
					{
						return null;
					}
					// TODO: Look up type correctly
					type = GetType().Assembly.GetType("WCell.RealmServer.Global." + propChain[i++], false, true);
					if (type == null)
					{
						return null;
					}
				}
			}

			for (; i < propChain.Length; )
			{
				var propName = propChain[i];
				i++;
				var flags = BindingFlags.Public | BindingFlags.IgnoreCase |
							(current != null ? BindingFlags.Instance : BindingFlags.Static);


				members = type.GetMember(propName, MemberTypes.Field | MemberTypes.Property | MemberTypes.Method, flags);

				if (members == null || members.Length == 0 || (user != null && !CanRead(member, user)))
				{
					return null;
				}

				if (i < propChain.Length)
				{
					member = members[0];
					if (member is PropertyInfo)
					{
						current = ((PropertyInfo)member).GetValue(current, null);
					}
					else if (member is FieldInfo)
					{
						current = ((FieldInfo)member).GetValue(current);
					}
					else
					{
						current = ((MethodInfo)member).Invoke(current, null);
					}
					type = current.GetType();
				}
			}
			memberHolder = current;
			return members;
		}
		#endregion

		#region GetProp
		public MemberInfo GetProp(U user, object obj, string name, Type type, out Object propHolder)
		{
			propHolder = null;
			var members = GetMembers(user, obj, name, type, ref propHolder);
			if (members != null && members.Length > 0)
			{
				var prop = members[0];
				if (!prop.IsReadonly() &&
					(user == null || CanWrite(prop, user)))
				{
					return prop;
				}
			}
			return null;
		}
		#endregion

		#region Set & Mod PropValue
		/// <summary>
		/// Sets a property on the given object.
		/// </summary>
		public MemberInfo SetPropValue(U user, object obj, string name, string value)
		{
			return SetPropValue(user, obj, name, value, obj.GetType());
		}

		public MemberInfo SetPropValue(U user, object obj, string name, string value, Type type)
		{
			object propHolder;
			var prop = GetProp(user, obj, name, type, out propHolder);
			if (prop != null)
			{
				object val = null;
				if (Utility.Parse(value, prop.GetVariableType(), ref val))
				{
					prop.SetUnindexedValue(propHolder, val);
					return prop;
				}
			}
			return null;
		}

		public MemberInfo ModPropValue<T>(U user, object obj, string name, Type type, string delta, Utility.OperatorHandler<T> oper,
			ref T newValue)
		{
			object propHolder;
			var prop = GetProp(user, obj, name, type, out propHolder);
			if (prop != null)
			{
				object val = null;
				if (Utility.Parse(delta, prop.GetVariableType(), ref val))
				{
					var oldVal = prop.GetUnindexedValue(obj);
					prop.SetUnindexedValue(propHolder, newValue = oper((T)oldVal, (T)val));
					return prop;
				}
			}
			return null;
		}
		#endregion

		#region GetPropValue
		/// <summary>
		/// Returns the value of a property-chain if user == null or user may read the given prop.
		/// </summary>
		public bool GetPropValue(U user, object obj, ref string accessName, out object value)
		{
			return GetPropValue(user, obj, ref accessName, obj != null ? obj.GetType() : null, out value);
		}

		/// <summary>
		/// Returns the value of a property-chain if user == null or user may read the given prop.
		/// </summary>
		public bool GetPropValue(U user, object obj, ref string accessName, Type type, out object value)
		{
			object propHolder = null;
			var members = GetMembers(user, obj, accessName, type, ref propHolder);
			if (members != null && members.Length > 0)
			{
				var prop = members[0];
				if (user == null || CanRead(prop, user))
				{
					accessName = prop.Name;
					value = prop.GetUnindexedValue(propHolder);
					return true;
				}
			}

			value = null;
			return false;
		}
		#endregion

		#region Call
		public bool CallMethod(U user, object obj, ref string accessName, string[] args, out object result)
		{
			object methodHolder = null;
			var members = GetMembers(user, obj, accessName, ref methodHolder);
			if (members != null && members.Length > 0 && members[0] is MethodInfo)
			{
				foreach (var member in members)
				{
					if (!(member is MethodInfo))
					{
						continue;
					}

					var method = (MethodInfo)member;
					if ((user != null && !CanWrite(member, user)) || method.ContainsGenericParameters)
					{
						result = null;
						return false;
					}

					var paras = method.GetParameters();
					if (paras.Length == args.Length)
					{
						var argArr = new object[args.Length];
						var success = true;
						for (var i = 0; i < argArr.Length; i++)
						{
							object arg = null;
							// rule out non-simple arugments and try parsing
							var paramType = paras[i].ParameterType;
							if (!paramType.IsSimpleType() || !Utility.Parse(args[i], paramType, ref arg))
							{
								// try the next overload
								success = false;
								break;
							}
							argArr[i] = arg;
						}
						if (success)
						{
							accessName = method.Name;
							result = method.Invoke(methodHolder, argArr);
							return true;
						}
					}
				}
			}
			result = null;
			return false;
		}
		#endregion

		public abstract bool CanRead(MemberInfo member, U user);

		public abstract bool CanWrite(MemberInfo member, U user);
	}
}

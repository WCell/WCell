using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.DynamicAccess;
using System.Reflection;

namespace WCell.Util.DynamicAccess
{
	public class MethodExcecutable : IExecutable
	{
		private string m_name;
		private readonly object m_TargetObj;
		private readonly MethodInfo m_method;
		private readonly Type[] m_parameterTypes;

		public MethodExcecutable(string name, object targetObj, MethodInfo method)
		{
			if (method.IsStatic && targetObj != null)
			{
				throw new ArgumentException("Invalid Executable - Static method \"" + method.Name + "\" cannot have a targetObj (\"" + targetObj + "\")");
			}
			else if (!method.IsStatic && targetObj == null)
			{
				throw new ArgumentException("Invalid Executable - Instance method \"" + method.Name + "\" must have a targetObj (null).");
			}
			m_name = name;
			m_TargetObj = targetObj;
			m_method = method;
			m_parameterTypes = m_method.GetParameters().TransformArray(info => info.ParameterType);
		}

		public MethodInfo Method
		{
			get { return m_method; }
		}

		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		public Type[] ParameterTypes
		{
			get { return m_parameterTypes; }
		}

		public void Exec(params object[] args)
		{
			m_method.Invoke(m_TargetObj, args);
		}

		public override string ToString()
		{
			return string.Format("Method {0}.{1}({2})", m_method.DeclaringType.Name, m_name, Method.GetParameters().ToString(", "));
		}
	}
}
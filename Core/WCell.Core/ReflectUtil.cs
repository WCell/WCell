using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WCell.Intercommunication.DataTypes;
using WCell.Util.ReflectionUtil;

namespace WCell.Core
{
	public class ReflectUtil : MemberAccessor<IRoleGroup>
	{
		public static readonly ReflectUtil Instance = new ReflectUtil();

		public override bool CanRead(MemberInfo member, IRoleGroup user)
		{
			// TODO: Use Attributes to verify whether the user may use it
			return true;
		}

		public override bool CanWrite(MemberInfo member, IRoleGroup user)
		{
			// TODO: Use Attributes to verify whether the user may use it
			return true;
		}
	}
}

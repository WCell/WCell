/*************************************************************************
 *
 *   file		: StepPart.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-01-31 19:35:36 +0800 (Thu, 31 Jan 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 87 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace WCell.Core.Initialization
{
    public class InitializationStep
    {
    	public readonly InitializationPass Pass;
        public readonly string InitStepName = "";
        public readonly bool IsRequired;
        public object[] InitContext;
        public readonly MethodInfo InitMethod;

    	internal bool Executed;

		public InitializationStep(InitializationPass pass, string initStepName, bool isRequired, MethodInfo initMethod) : 
			this(pass, initStepName, isRequired, null, initMethod)
        {
        }

		public InitializationStep(InitializationPass pass, string initStepName, bool isRequired, object[] initContext, MethodInfo initMethod)
        {
			Pass = pass;
            InitStepName = initStepName;
            IsRequired = isRequired;
            InitContext = initContext;
			InitMethod = initMethod;
        }

    	public object[] GetArgs(InitMgr mgr)
    	{
			var paramInfo = InitMethod.GetParameters();
			object[] args = null;

			if (paramInfo.Length == 1)
			{
				if (paramInfo[0].ParameterType == typeof(InitMgr))
				{
					args = new object[] { mgr };
				}
			}

			return args;
    	}

		public override string ToString()
		{
			return !string.IsNullOrEmpty(InitStepName) ? InitStepName : InitMethod.DeclaringType.FullName + "." + InitMethod.Name;
		}
    }
}
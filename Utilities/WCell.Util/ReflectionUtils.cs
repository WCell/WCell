/*************************************************************************
 *
 *   file		: ReflectionUtils.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-02-16 12:30:51 +0800 (Mon, 16 Feb 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 757 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Reflection;

namespace WCell.Util
{
    /// <summary>
    /// Reflection utilities used in the network layer
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// Checks if a method's parameters match a given array of types
        /// </summary>
        /// <param name="generic_method_definition">the method to check</param>
        /// <param name="types">the types to check for</param>
        /// <returns>true if the method has the required types for its parameters</returns>
        public static bool SatisfiesGenericConstraints(MethodInfo generic_method_definition, params Type[] types)
        {
            Type[] generic_types = generic_method_definition.GetGenericArguments();

            if (generic_types.Length != types.Length)
                return false;

            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                Type generic_type = generic_types[i];

                if ((generic_type.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint)
                    != GenericParameterAttributes.None
                    && type.IsValueType == false && type.GetConstructor(Type.EmptyTypes) == null)
                    return false;

                if ((generic_type.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint)
                    != GenericParameterAttributes.None
                    && type.IsValueType == false)
                    return false;

                if ((generic_type.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint)
                    != GenericParameterAttributes.None
                    && type.IsValueType)
                    return false;

                foreach (Type required_parent in generic_type.GetGenericParameterConstraints())
                {
                    if (required_parent != type.BaseType
                        && type.GetInterface(required_parent.Name) == null)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if a delegate can be made from this given method for the given delegate type
        /// </summary>
        /// <typeparam name="DelegateType">the type of delegate to be created</typeparam>
        /// <param name="method">the method to be transformed into a delegate</param>
        /// <returns>true if the given method will be able to be of the given delegate type; false otherwise</returns>
        public static bool CanCreateDelegate<DelegateType>(MethodInfo method)
        {
            Type delegate_type = typeof (DelegateType);
            MethodInfo invoke = delegate_type.GetMethod("Invoke");

            if (invoke.ReturnType != method.ReturnType)
                return false;

            ParameterInfo[] method_params = method.GetParameters();
            ParameterInfo[] delegate_params = invoke.GetParameters();

            if (method_params.Length != delegate_params.Length)
                return false;

            for (int i = 0; i < method_params.Length; i++)
            {
                if (method_params[i].GetType() != delegate_params[i].GetType())
                    return false;

                // catch cases where people try to use RealmClient instead of IRealmClient, etc
                if (method_params[i].GetType().IsInterface &&
                    method_params[i].GetType().IsAssignableFrom(delegate_params[i].GetType()) &&
                    !delegate_params[i].GetType().IsAssignableFrom(method_params[i].GetType()))
                {
                    return false;
                }

            }

            return SatisfiesGenericConstraints(method, invoke.GetGenericArguments());
        }
    }
}
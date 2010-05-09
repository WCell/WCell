//
// Author: James Nies
// Contributor: Tobias Hertkorn
// Further Edited by Dominik Seifert, WCell
// Date: 5/31/2005
// Description: The GenericPropertyAccessor class provides fast dynamic access
//		to a property of a specified target class.
//
//
// *** This code was written by James Nies and has been provided to you, ***
// *** free of charge, for your use.  I assume no responsibility for any ***
// *** undesired events resulting from the use of this code or the		 ***
// *** information that has been provided with it .						 ***
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Collections;

namespace WCell.Util.DynamicProperties
{
	/// <summary>
	/// The GenericPropertyAccessor class provides fast dynamic access
	/// to a property of a specified target class.
	/// </summary>
	public class GenericPropertyAccessor<T, V> : IGenericPropertyAccessor<T, V>, IGetterSetter
	{
		/// <summary>
		/// Creates a new property accessor.
		/// </summary>
		/// <param name="property">Property name.</param>
		public GenericPropertyAccessor(string property)
		{
			this.mTargetType = typeof(T);
			this.mProperty = property;

			PropertyInfo propertyInfo =
				this.mTargetType.GetProperty(property);

			//
			// Make sure the property exists
			//
			if (propertyInfo == null)
			{
				throw new PropertyAccessorException(
					string.Format("Property \"{0}\" does not exist for type "
					              + "{1}.", property, this.mTargetType));
			}
			else
			{
				this.mCanRead = propertyInfo.CanRead;
				this.mCanWrite = propertyInfo.CanWrite;
				this.mPropertyType = propertyInfo.PropertyType;
			}
		}

		/// <summary>
		/// Gets the property value from the specified target.
		/// </summary>
		/// <param name="target">Target object.</param>
		/// <returns>Property value.</returns>
		public V Get(T target)
		{
			if (mCanRead)
			{
				if (this.mEmittedPropertyAccessor == null)
				{
					this.Init();
				}

				return this.mEmittedPropertyAccessor.Get(target);
			}
			else
			{
				throw new PropertyAccessorException(
					string.Format("Property \"{0}\" does not have a get method.",
					              mProperty));
			}
		}

		/// <summary>
		/// Sets the property for the specified target.
		/// </summary>
		/// <param name="target">Target object.</param>
		/// <param name="value">Value to set.</param>
		public void Set(T target, V value)
		{
			if (mCanWrite)
			{
				if (this.mEmittedPropertyAccessor == null)
				{
					this.Init();
				}

				//
				// Set the property value
				//
				this.mEmittedPropertyAccessor.Set(target, value);
			}
			else
			{
				throw new PropertyAccessorException(
					string.Format("Property \"{0}\" does not have a set method.",
					              mProperty));
			}
		}

		/// <summary>
		/// Whether or not the Property supports read access.
		/// </summary>
		public bool CanRead
		{
			get
			{
				return this.mCanRead;
			}
		}

		/// <summary>
		/// Whether or not the Property supports write access.
		/// </summary>
		public bool CanWrite
		{
			get
			{
				return this.mCanWrite;
			}
		}

		/// <summary>
		/// The Type of object this property accessor was
		/// created for.
		/// </summary>
		public Type TargetType
		{
			get
			{
				return this.mTargetType;
			}
		}

		/// <summary>
		/// The Type of the Property being accessed.
		/// </summary>
		public Type PropertyType
		{
			get
			{
				return this.mPropertyType;
			}
		}

		private static Hashtable mTypeHash;

		private Type mTargetType;
		private string mProperty;
		private Type mPropertyType;
		private IGenericPropertyAccessor<T, V> mEmittedPropertyAccessor;
		private bool mCanRead;
		private bool mCanWrite;

		/// <summary>
		/// This method generates creates a new assembly containing
		/// the Type that will provide dynamic access.
		/// </summary>
		private void Init()
		{
			// Create the assembly and an instance of the 
			// property accessor class.
			Assembly assembly = EmitAssembly();

			mEmittedPropertyAccessor =
				assembly.CreateInstance("Property") as IGenericPropertyAccessor<T, V>;

			if (mEmittedPropertyAccessor == null)
			{
				throw new Exception("Unable to create property accessor.");
			}
		}

		/// <summary>
		/// Thanks to Ben Ratzlaff for this snippet of code
		/// http://www.codeproject.com/cs/miscctrl/CustomPropGrid.asp
		/// 
		/// "Initialize a private hashtable with type-opCode pairs 
		/// so i dont have to write a long if/else statement when outputting msil"
		/// </summary>
		static GenericPropertyAccessor()
		{
			mTypeHash = new Hashtable();
			mTypeHash[typeof(sbyte)] = OpCodes.Ldind_I1;
			mTypeHash[typeof(byte)] = OpCodes.Ldind_U1;
			mTypeHash[typeof(char)] = OpCodes.Ldind_U2;
			mTypeHash[typeof(short)] = OpCodes.Ldind_I2;
			mTypeHash[typeof(ushort)] = OpCodes.Ldind_U2;
			mTypeHash[typeof(int)] = OpCodes.Ldind_I4;
			mTypeHash[typeof(uint)] = OpCodes.Ldind_U4;
			mTypeHash[typeof(long)] = OpCodes.Ldind_I8;
			mTypeHash[typeof(ulong)] = OpCodes.Ldind_I8;
			mTypeHash[typeof(bool)] = OpCodes.Ldind_I1;
			mTypeHash[typeof(double)] = OpCodes.Ldind_R8;
			mTypeHash[typeof(float)] = OpCodes.Ldind_R4;
		}

		/// <summary>
		/// Create an assembly that will provide the get and set methods.
		/// </summary>
		private Assembly EmitAssembly()
		{
			//
			// Create an assembly name
			//
			AssemblyName assemblyName = new AssemblyName();
			assemblyName.Name = "GenericPropertyAccessorAssembly";

			//
			// Create a new assembly with one module
			//
			AssemblyBuilder newAssembly = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			ModuleBuilder newModule = newAssembly.DefineDynamicModule("Module");

			//		
			//  Define a public class named "Property" in the assembly.
			//			
			TypeBuilder myType =
				newModule.DefineType("Property", TypeAttributes.Public | TypeAttributes.Sealed);

			//
			// Mark the class as implementing IPropertyAccessor. 
			//
			myType.AddInterfaceImplementation(typeof(IGenericPropertyAccessor<T, V>));

			// Add a constructor
			ConstructorBuilder constructor =
				myType.DefineDefaultConstructor(MethodAttributes.Public);

			//
			// Define a method for the get operation. 
			//
			Type[] getParamTypes = new Type[] { typeof(T) };
			Type getReturnType = typeof(V);
			MethodBuilder getMethod =
				myType.DefineMethod("Get",
				                    MethodAttributes.Public | MethodAttributes.Virtual,
				                    getReturnType,
				                    getParamTypes);

			//
			// From the method, get an ILGenerator. This is used to
			// emit the IL that we want.
			//
			ILGenerator getIL = getMethod.GetILGenerator();


			//
			// Emit the IL. 
			//
			MethodInfo targetGetMethod = this.mTargetType.GetMethod("get_" + this.mProperty);

			if (targetGetMethod != null)
			{
				getIL.DeclareLocal(typeof(V));
				getIL.Emit(OpCodes.Ldarg_1);								//Load the first argument 
				//(target object)

				getIL.EmitCall(OpCodes.Call, targetGetMethod, null);		//Get the property value

				getIL.Emit(OpCodes.Stloc_0);								//Store it

				getIL.Emit(OpCodes.Ldloc_0);
			}
			else
			{
				getIL.ThrowException(typeof(MissingMethodException));
			}

			getIL.Emit(OpCodes.Ret);


			//
			// Define a method for the set operation.
			//
			Type[] setParamTypes = new Type[] { typeof(T), typeof(V) };
			Type setReturnType = null;
			MethodBuilder setMethod =
				myType.DefineMethod("Set",
				                    MethodAttributes.Public | MethodAttributes.Virtual,
				                    setReturnType,
				                    setParamTypes);

			//
			// From the method, get an ILGenerator. This is used to
			// emit the IL that we want.
			//
			ILGenerator setIL = setMethod.GetILGenerator();
			//
			// Emit the IL. 
			//

			MethodInfo targetSetMethod = this.mTargetType.GetMethod("set_" + this.mProperty);
			if (targetSetMethod != null)
			{
				Type paramType = targetSetMethod.GetParameters()[0].ParameterType;

				setIL.DeclareLocal(paramType);
				setIL.Emit(OpCodes.Ldarg_1);						//Load the first argument 
				//(target object)

				setIL.Emit(OpCodes.Ldarg_2);						//Load the second argument 
				//(value object)

				setIL.EmitCall(OpCodes.Callvirt,
				               targetSetMethod, null);							//Set the property value
			}
			else
			{
				setIL.ThrowException(typeof(MissingMethodException));
			}

			setIL.Emit(OpCodes.Ret);

			//
			// Load the type
			//
			myType.CreateType();

			return newAssembly;
		}

		#region Implementation of IPropertyAccessor

		public object Get(object key)
		{
			return Get((T) key);
		}

		public void Set(object key, object value)
		{
			Set((T) key, (V) value);
		}

		#endregion
	}
}
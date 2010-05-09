//
// Author: James Nies
// Further Edited by Dominik Seifert, WCell
// Date: 3/22/2005
// Description: The PropertyAccessor class provides fast dynamic access
//		to a property of a specified target class.
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
	/// The PropertyAccessor class provides fast dynamic access
	/// to a property of a specified target class.
	/// </summary>
	public class PropertyAccessor : IGetterSetter
	{
		private static int nextTypeId;
		public static readonly Hashtable typesHashes;

		static PropertyAccessor()
		{
			typesHashes = new Hashtable();
			typesHashes[typeof(sbyte)] = OpCodes.Ldind_I1;
			typesHashes[typeof(byte)] = OpCodes.Ldind_U1;
			typesHashes[typeof(char)] = OpCodes.Ldind_U2;
			typesHashes[typeof(short)] = OpCodes.Ldind_I2;
			typesHashes[typeof(ushort)] = OpCodes.Ldind_U2;
			typesHashes[typeof(int)] = OpCodes.Ldind_I4;
			typesHashes[typeof(uint)] = OpCodes.Ldind_U4;
			typesHashes[typeof(long)] = OpCodes.Ldind_I8;
			typesHashes[typeof(ulong)] = OpCodes.Ldind_I8;
			typesHashes[typeof(bool)] = OpCodes.Ldind_I1;
			typesHashes[typeof(double)] = OpCodes.Ldind_R8;
			typesHashes[typeof(float)] = OpCodes.Ldind_R4;
		}


		private Type mTargetType;
		private PropertyInfo mProperty;
		private Type mPropertyType;
		private IGetterSetter mEmittedPropertyAccessor;
		private bool mCanRead;
		private bool mCanWrite;

		/// <summary>
		/// Creates a new property accessor.
		/// </summary>
		/// <param name="targetType">Target object type.</param>
		public PropertyAccessor(Type targetType, PropertyInfo propertyInfo)
		{
			mTargetType = targetType;
			mProperty = propertyInfo;
			mCanRead = propertyInfo.CanRead;
			mCanWrite = propertyInfo.CanWrite;
			mPropertyType = propertyInfo.PropertyType;
		}


		/// <summary>
		/// Creates a new property accessor.
		/// </summary>
		/// <param name="targetType">Target object type.</param>
		/// <param name="property">Property name.</param>
		public PropertyAccessor(Type targetType, string property)
		{
			mTargetType = targetType;

			PropertyInfo propertyInfo = 
				targetType.GetProperty(property);

			//
			// Make sure the property exists
			//
			if(propertyInfo == null)
			{
				throw new PropertyAccessorException(
					string.Format("Property \"{0}\" does not exist for type "
					+ "{1}.", property, targetType));
			}
			else
			{
				mProperty = propertyInfo;
				mCanRead = propertyInfo.CanRead;
				mCanWrite = propertyInfo.CanWrite;
				mPropertyType = propertyInfo.PropertyType;
			}
		}

		/// <summary>
		/// Gets the property value from the specified target.
		/// </summary>
		/// <param name="key">Target object.</param>
		/// <returns>Property value.</returns>
		public object Get(object key)
		{
			if(mCanRead)
			{
				if(mEmittedPropertyAccessor == null)
				{
					Init();
				}

				try
				{
					return mEmittedPropertyAccessor.Get(key);
				}
				catch (Exception e)
				{
					throw new PropertyAccessorException("Could not evaluate property - Is your class public and is it correctly accessed?", e);
				}
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
		/// <param name="key">Target object.</param>
		/// <param name="value">Value to set.</param>
		public void Set(object key, object value)
		{
			if(mCanWrite)
			{
				if(this.mEmittedPropertyAccessor == null)
				{
					this.Init();
				}

				//
				// Set the property value
				//
				this.mEmittedPropertyAccessor.Set(key, value);
			}
			else
			{
				throw new PropertyAccessorException(
					string.Format("Property \"{0}\" does not have a set method.",
					mProperty));
			}
		}

		public PropertyInfo Property
		{
			get { return mProperty; }
		}

		public string PropertyName
		{
			get { return mProperty.Name;  }
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

		/// <summary>
		/// This method generates creates a new assembly containing
		/// the Type that will provide dynamic access.
		/// </summary>
		private void Init()
		{
			// Create the assembly and an instance of the 
			// property accessor class.
			EmitAssembly();
		}

		/// <summary>
		/// Thanks to Ben Ratzlaff for this snippet of code
		/// http://www.codeproject.com/cs/miscctrl/CustomPropGrid.asp
		/// 
		/// "Initialize a private hashtable with type-opCode pairs 
		/// so i dont have to write a long if/else statement when outputting msil"
		/// </summary>

		/// <summary>
		/// Create an assembly that will provide the get and set methods.
		/// </summary>
		private void EmitAssembly()
		{
			var module = AccessorMgr.CreateModule();
			AddToModule(module);
		}

		internal void AddToModule(ModuleBuilder module)
		{
			string typeName = (nextTypeId++).ToString();

			//		
			//  Define a public class named "Property" in the assembly.
			//			
			TypeBuilder myType = 
				module.DefineType(typeName, TypeAttributes.Public);

			//
			// Mark the class as implementing IPropertyAccessor. 
			//
			myType.AddInterfaceImplementation(typeof(IGetterSetter));

			// Add a constructor
			//ConstructorBuilder constructor = myType.DefineDefaultConstructor(MethodAttributes.Public);

			//
			// Define a method for the get operation. 
			//
			Type[] getParamTypes = new [] {typeof(object)};
			Type getReturnType = typeof(object);
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
			var targetGetMethod = mProperty.GetGetMethod();

			if(targetGetMethod != null)
			{
				getIL.DeclareLocal(typeof(object));
				getIL.Emit(OpCodes.Ldarg_1);								//Load the first argument 
																			//(target object)
				getIL.Emit(OpCodes.Castclass, mTargetType);			//Cast to the source type
				getIL.EmitCall(OpCodes.Call, targetGetMethod, null);		//Get the property value
				if(targetGetMethod.ReturnType.IsValueType)
				{
					getIL.Emit(OpCodes.Box, targetGetMethod.ReturnType);	//Box if necessary
				}
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
			Type[] setParamTypes = new [] {typeof(object), typeof(object)};
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

			MethodInfo targetSetMethod = mProperty.GetSetMethod();

			if(targetSetMethod != null)
			{
				Type paramType = targetSetMethod.GetParameters()[0].ParameterType;

				setIL.DeclareLocal(paramType);
				setIL.Emit(OpCodes.Ldarg_1);						//Load the first argument 
																	//(target object)

				setIL.Emit(OpCodes.Castclass, mTargetType);	//Cast to the source type

				setIL.Emit(OpCodes.Ldarg_2);						//Load the second argument 
																	//(value object)

				if(paramType.IsValueType)
				{
					setIL.Emit(OpCodes.Unbox, paramType);			//Unbox it 	
					if(typesHashes[paramType]!=null)					//and load
					{
						OpCode load = (OpCode)typesHashes[paramType];
						setIL.Emit(load);
					}
					else
					{
						setIL.Emit(OpCodes.Ldobj,paramType);
					}
				}
				else
				{
					setIL.Emit(OpCodes.Castclass, paramType);		//Cast class
				}
			
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


			mEmittedPropertyAccessor = module.Assembly.CreateInstance(typeName) as IGetterSetter;
			if (mEmittedPropertyAccessor == null)
			{
				throw new Exception("Unable to create property accessor.");
			}
		}
	}
}
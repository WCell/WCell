using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace WCell.Util.DynamicProperties
{
	public static class AccessorMgr
	{
		private static int nextTypeId;
		public const BindingFlags DefaultBindingFlags =
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

		private static readonly Dictionary<Type, Dictionary<MemberInfo, IGetterSetter>> m_accessors =
			new Dictionary<Type, Dictionary<MemberInfo, IGetterSetter>>();
		public static readonly Dictionary<Type, OpCode> PropTypesHashes;

		static AccessorMgr()
		{
			PropTypesHashes = new Dictionary<Type, OpCode>();
			PropTypesHashes[typeof(sbyte)] = OpCodes.Ldind_I1;
			PropTypesHashes[typeof(byte)] = OpCodes.Ldind_U1;
			PropTypesHashes[typeof(char)] = OpCodes.Ldind_U2;
			PropTypesHashes[typeof(short)] = OpCodes.Ldind_I2;
			PropTypesHashes[typeof(ushort)] = OpCodes.Ldind_U2;
			PropTypesHashes[typeof(int)] = OpCodes.Ldind_I4;
			PropTypesHashes[typeof(uint)] = OpCodes.Ldind_U4;
			PropTypesHashes[typeof(long)] = OpCodes.Ldind_I8;
			PropTypesHashes[typeof(ulong)] = OpCodes.Ldind_I8;
			PropTypesHashes[typeof(bool)] = OpCodes.Ldind_I1;
			PropTypesHashes[typeof(double)] = OpCodes.Ldind_R8;
			PropTypesHashes[typeof(float)] = OpCodes.Ldind_R4;
		}

		/// <summary>
		/// Copies all public properties that have a setter and a getter and exist in the types of both objects from input to output.
		/// Ignores all properties that have the <see cref="DontCopyAttribute"/>.
		/// </summary>
		/// <returns></returns>
		//public static void CopyAll(object input, object output)
		//{
		//    var inputAccessors = GetOrCreateAccessors(input.GetType());
		//    var outputAccessors = GetOrCreateAccessors(output.GetType());

		//    PropertyAccessor outputAccessor;
		//    foreach (var inputAccessor in inputAccessors.Values)
		//    {
		//        if (outputAccessors.TryGetValue(inputAccessor.Property, out outputAccessor))
		//        {
		//            outputAccessor.Set(output, inputAccessor.Get(input));
		//        }
		//    }
		//}

		public static IGetterSetter GetOrCreateAccessor(Type type, PropertyInfo info)
		{
			var accessors = GetOrCreateAccessors(type);

			IGetterSetter accessor;
//#if DEBUG
			if (!accessors.TryGetValue(info, out accessor))
			{
				throw new Exception("Tried to get accessor for non-existing Property: " + info);
			}
//#endif
			return accessor;
		}

		public static ModuleBuilder CreateModule()
		{
			//
			// Create an assembly name
			//
			var assemblyName = new AssemblyName {Name = "PropertyAccessorAssembly"};

			//
			// Create a new assembly with one module
			//
			var asm = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			return asm.DefineDynamicModule("Module");
		}

		public static Dictionary<MemberInfo, IGetterSetter> GetOrCreateAccessors<T>()
		{
			return GetOrCreateAccessors(typeof(T));
		}

		public static Dictionary<MemberInfo, IGetterSetter> GetOrCreateAccessors(Type type)
		{
			Dictionary<MemberInfo, IGetterSetter> accessors;
			if (!m_accessors.TryGetValue(type, out accessors))
			{
				m_accessors.Add(type, accessors = CreateAccessors(type));
			}
			return accessors;
		}

		private static Dictionary<MemberInfo, IGetterSetter> CreateAccessors(Type type)
		{
			var accessors = new Dictionary<MemberInfo, IGetterSetter>();
			var members = type.GetMembers(DefaultBindingFlags);

			var module = CreateModule();
			foreach (var member in members)
			{
				var attributes = (DontCopyAttribute[])member.GetCustomAttributes(typeof(DontCopyAttribute), false);

				// only copy if not tagged otherwise
				if (attributes.Length == 0)
				{
					IGetterSetter accessor;
					if (member.MemberType == MemberTypes.Property)
					{
						accessor = AddToModule(module, (PropertyInfo)member);
					}
					else if (member.MemberType == MemberTypes.Field)
					{
						accessor = AddToModule(module, (FieldInfo)member);
					}
					else
					{
						continue;
					}

					accessors.Add(member, accessor);
				}
			}

			return accessors;
		}


		#region Other Dynamics

		public class X
		{
			public int a;
		}

		static void Test()
		{
			var x = new X();
			var fieldA = x.GetType().GetField("a");
			var ctor = x.GetType().GetConstructor(new Type[0]);
			var accessor = EmitAssembly(fieldA);
			var producer = EmitAssembly(ctor);
			var i = 1;

			Utility.Measure("Create object", 2000000, () => {
				var x2 = new X();
			});

			Utility.Measure("Dynamically create object", 2000000, () => {
				var x2 = (X)producer.Produce();
			});

			Utility.Measure("Create object through reflection", 2000000, () => {
				var x2 = Activator.CreateInstance<X>();
			});
		}

		private static IGetterSetter EmitAssembly(FieldInfo field)
		{
			var module = CreateModule();
			return AddToModule(module, field);
		}

		private static IProducer EmitAssembly(ConstructorInfo ctor)
		{
			var module = CreateModule();
			return AddToModule(module, ctor);
		}

		internal static IProducer AddToModule(ModuleBuilder module, ConstructorInfo ctor)
		{
			string typeName = "TestType";

			var targetType = ctor.DeclaringType;

			//		
			//  Define a public class named "Property" in the assembly.
			//			
			
			TypeBuilder myType =
				module.DefineType(typeName, TypeAttributes.Public);

			//
			// Mark the class as implementing IPropertyAccessor. 
			//
			myType.AddInterfaceImplementation(typeof(IProducer));

			// Add a constructor
			//ConstructorBuilder constructor = myType.DefineDefaultConstructor(MethodAttributes.Public);

			//
			// Define a method for the get operation. 
			//
			var getParamTypes = new Type[0];
			var getReturnType = typeof(object);
			var getMethod =
				myType.DefineMethod("Produce",
									MethodAttributes.Public | MethodAttributes.Virtual,
									getReturnType,
									getParamTypes);

			// Emit
			ILGenerator method = getMethod.GetILGenerator();
			method.DeclareLocal(targetType);
			method.Emit(OpCodes.Newobj, ctor);
			method.Emit(OpCodes.Stloc_0);
			method.Emit(OpCodes.Ldloc_0);
			method.Emit(OpCodes.Ret);

			myType.CreateType();

			var fieldAccessor = module.Assembly.CreateInstance(typeName) as IProducer;
			if (fieldAccessor == null)
			{
				throw new Exception("Unable to create producer.");
			}
			return fieldAccessor;
		}

		private static string NextTypeName(string s)
		{
			return "Dynamic" + s.Replace('.','_') + nextTypeId++;
		}


		public static IGetterSetter AddToModule(ModuleBuilder module, FieldInfo field)
		{
			var typeName = NextTypeName(field.GetMemberName());

			var targetType = field.DeclaringType;
			var fieldType = field.FieldType;

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
			Type[] getParamTypes = new[] { typeof(object) };
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

			getIL.DeclareLocal(typeof(object));
			getIL.Emit(OpCodes.Ldarg_1);						//Load the first argument 
			//(target object)

			getIL.Emit(OpCodes.Castclass, targetType);			//Cast to the source type

			getIL.Emit(OpCodes.Ldfld, field);					//Get the field value

			if (fieldType.IsValueType)
			{
				getIL.Emit(OpCodes.Box, fieldType);			//Box if necessary
			}
			getIL.Emit(OpCodes.Stloc_0);						//Store it

			// getIL.Emit(OpCodes.Br_S, );

			getIL.Emit(OpCodes.Ldloc_0);


			getIL.Emit(OpCodes.Ret);


			//
			// Define a method for the set operation.
			//
			Type[] setParamTypes = new[] { typeof(object), typeof(object) };
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

			setIL.DeclareLocal(fieldType);
			setIL.Emit(OpCodes.Ldarg_1);						//Load the first argument 
			//(target object)

			setIL.Emit(OpCodes.Castclass, targetType);			//Cast to the target type

			setIL.Emit(OpCodes.Ldarg_2);						//Load the second argument 
			//(value object)

			if (fieldType.IsValueType)
			{
				//setIL.Emit(OpCodes.Unbox, fieldType); //Unbox it 	
				setIL.Emit(OpCodes.Unbox_Any, fieldType); //Unbox it 	

				//if (typesHashes[paramType] != null) //and load
				//{
				//    OpCode load = (OpCode) typesHashes[paramType];
				//    setIL.Emit(load);
				//}
				//else
				//{
				//    setIL.Emit(OpCodes.Ldobj, paramType);
				//}
			}

			setIL.Emit(OpCodes.Stfld, field);

			setIL.Emit(OpCodes.Ret);

			//
			// Load the type
			//
			myType.CreateType();


			var fieldAccessor = module.Assembly.CreateInstance(typeName) as IGetterSetter;
			if (fieldAccessor == null)
			{
				throw new Exception("Unable to create Field accessor.");
			}
			return fieldAccessor;
		}

		public static IGetterSetter AddToModule(ModuleBuilder module, PropertyInfo prop)
		{
			var typeName = NextTypeName(prop.GetMemberName());

			var targetType = prop.DeclaringType;
			//		
			//  Define a public class named "Property" in the assembly.
			//			
			var myType =
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
			var getParamTypes = new[] { typeof(object) };
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
			var targetGetMethod = prop.GetGetMethod();

			if (targetGetMethod != null)
			{
				getIL.DeclareLocal(typeof(object));
				getIL.Emit(OpCodes.Ldarg_1);								//Load the first argument 
				//(target object)
				getIL.Emit(OpCodes.Castclass, targetType);			//Cast to the source type
				getIL.EmitCall(OpCodes.Call, targetGetMethod, null);		//Get the property value
				if (targetGetMethod.ReturnType.IsValueType)
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
			Type[] setParamTypes = new[] { typeof(object), typeof(object) };
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

			MethodInfo targetSetMethod = prop.GetSetMethod();

			if (targetSetMethod != null)
			{
				Type paramType = targetSetMethod.GetParameters()[0].ParameterType;

				setIL.DeclareLocal(paramType);
				setIL.Emit(OpCodes.Ldarg_1);						//Load the first argument 
				//(target object)

				setIL.Emit(OpCodes.Castclass, targetType);	//Cast to the source type

				setIL.Emit(OpCodes.Ldarg_2);						//Load the second argument 
				//(value object)

				if (paramType.IsValueType)
				{
					setIL.Emit(OpCodes.Unbox, paramType);			//Unbox it 	
					OpCode opCode;
					if (PropTypesHashes.TryGetValue(paramType, out opCode))					//and load
					{
						setIL.Emit(opCode);
					}
					else
					{
						setIL.Emit(OpCodes.Ldobj, paramType);
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


			var accessor = module.Assembly.CreateInstance(typeName) as IGetterSetter;
			if (accessor == null)
			{
				throw new Exception("Unable to create property accessor.");
			}
			return accessor;
		}
		#endregion
	}
}
/*
public static Func<T,R> GetFieldAccessor<T,R>(string fieldName)
{
ParameterExpression param = Expression.Parameter (typeof(T),"arg");
MemberExpression member = Expression.Field(param, fieldName);
LambdaExpression lambda = Expression.Lambda(typeof(Func<T,R>), member, param);
Func<T,R> compiled = (Func<T,R>)lambda.Compile();
return compiled;
}
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using WCell.Util.DB;

namespace WCell.Util.Data
{
	public interface IComplexBinaryPersistor : IBinaryPersistor
	{
	}

	public interface ISimpleBinaryPersistor : IBinaryPersistor
	{
		int BinaryLength { get; }
	}

	public interface IBinaryPersistor
	{
		void Write(BinaryWriter writer, object obj);

		object Read(BinaryReader reader);
	}

	public interface IStringPersistor
	{
		void WriteText(BinaryWriter writer, string text);

		object ReadText(BinaryReader reader);
	}

	public class ArrayPersistor : IComplexBinaryPersistor
	{
		private readonly ArrayDataField m_DataField;
		private IBinaryPersistor m_UnderlyingPersistor;

		public ArrayPersistor(ArrayDataField field)
		{
			m_DataField = field;
			m_UnderlyingPersistor = BinaryPersistors.GetPersistorNoArray(m_DataField);
		}

		public ArrayDataField DataField
		{
			get { return m_DataField; }
		}

		public IBinaryPersistor UnderlyingPersistor
		{
			get { return m_UnderlyingPersistor; }
		}

		//public int BinaryLength
		//{
		//    get { return m_UnderlyingPersistor.BinaryLength * m_DataField.Length; }
		//}

		public void Write(BinaryWriter writer, object obj)
		{
			var i = 0;
			if (obj != null)
			{
				for (; i < ((Array) obj).Length; i++)
				{
					var item = ((Array) obj).GetValue(i);
					m_UnderlyingPersistor.Write(writer, item);
				}
			}

			if (i < m_DataField.Length)
			{
				// write default Item as a filler
				var type = m_DataField.MappedMember.GetActualType();

				object deflt;
				if (type == typeof(string))
				{
					deflt = "";
				}
				else
				{
					deflt = Activator.CreateInstance(type);
				}

				for (; i < m_DataField.Length; i++)
				{
					m_UnderlyingPersistor.Write(writer, deflt);
				}
			}
		}

		public object Read(BinaryReader reader)
		{
			var arr = (Array)m_DataField.ArrayProducer.Produce();
			for (var i = 0; i < m_DataField.Length; i++)
			{
				object obj;
				if (m_UnderlyingPersistor is NestedPersistor)
				{
					obj = arr.GetValue(i);
					((NestedPersistor)m_UnderlyingPersistor).Read(reader, ref obj);
				}
				else
				{
					obj = m_UnderlyingPersistor.Read(reader);
				}
				ArrayUtil.SetValue(arr, i, obj);
			}
			return arr;
		}
	}

	public class NestedPersistor : IComplexBinaryPersistor
	{
		private readonly INestedDataField m_DataField;
		private IBinaryPersistor[] m_UnderlyingPersistors;
		private IGetterSetter[] m_accessors;

		public NestedPersistor(
			INestedDataField dataField)
		{
			m_DataField = dataField;
			m_UnderlyingPersistors = new IBinaryPersistor[m_DataField.InnerFields.Count];
			m_accessors = new IGetterSetter[m_DataField.InnerFields.Count];
			var i = 0;
			foreach (var field in m_DataField.InnerFields.Values)
			{
				var persistor = BinaryPersistors.GetPersistor(field);
				m_UnderlyingPersistors[i] = persistor;
				m_accessors[i] = field.Accessor;
				i++;
			}
		}

		public INestedDataField DataField
		{
			get { return m_DataField; }
		}

		public IBinaryPersistor[] UnderlyingPersistors
		{
			get { return m_UnderlyingPersistors; }
		}

		public void Write(BinaryWriter writer, object obj)
		{
			if (obj == null)
			{
				obj = m_DataField.Producer.Produce();
			}

			for (var i = 0; i < m_UnderlyingPersistors.Length; i++)
			{
				var persistor = m_UnderlyingPersistors[i];
				var val = m_accessors[i].Get(obj);
				persistor.Write(writer, val);
			}
		}

		public object Read(BinaryReader reader)
		{
			object obj = null;

			Read(reader, ref obj);

			return obj;
		}

		public void Read(BinaryReader reader, ref object obj)
		{
			if (obj == null)
			{
				if (m_DataField.Producer != null)
				{
					obj = m_DataField.Producer.Produce();
				}
				else
				{
					obj = Activator.CreateInstance(m_DataField.BelongsTo.GetActualType());
				}

			}
			for (var i = 0; i < m_UnderlyingPersistors.Length; i++)
			{
				var persistor = m_UnderlyingPersistors[i];
				var val = persistor.Read(reader);
				m_accessors[i].Set(obj, val);
			}
		}
	}

	public static class BinaryPersistors
	{
		public static readonly Dictionary<Type, ISimpleBinaryPersistor> SimplePersistors =
			new Dictionary<Type, ISimpleBinaryPersistor>();

		public static Encoding DefaultEncoding = Encoding.UTF8;

		static BinaryPersistors()
		{
			SimplePersistors[typeof(int)] = new Int32Persistor();
			SimplePersistors[typeof(uint)] = new UInt32Persistor();
			SimplePersistors[typeof(short)] = new Int16Persistor();
			SimplePersistors[typeof(ushort)] = new UInt16Persistor();
			SimplePersistors[typeof(byte)] = new BytePersistor();
			SimplePersistors[typeof(sbyte)] = new SBytePersistor();
			SimplePersistors[typeof(long)] = new Int64Persistor();
			SimplePersistors[typeof(ulong)] = new UInt64Persistor();
			SimplePersistors[typeof(float)] = new FloatPersistor();
			SimplePersistors[typeof(double)] = new DoublePersistor();
			SimplePersistors[typeof(string)] = new StringPersistor();
			SimplePersistors[typeof(bool)] = new BoolPersistor();
		}

		public static ISimpleBinaryPersistor GetSimplePersistor(Type type)
		{
			ISimpleBinaryPersistor persistor;
			if (type.IsEnum)
			{
				type = Enum.GetUnderlyingType(type);
			}
			SimplePersistors.TryGetValue(type, out persistor);
			if (persistor is FloatPersistor)
			{
				type.ToString();
			}
			return persistor;
		}

		/// <summary>
		/// Returns null if its a String field
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		public static IBinaryPersistor GetPersistor(IDataField field)
		{
			if (field.DataFieldType == DataFieldType.FlatArray ||
					field.DataFieldType == DataFieldType.NestedArray)
			{
				return new ArrayPersistor((ArrayDataField)field);
			}
			else
			{
				return GetPersistorNoArray(field);
			}
		}

		public static IBinaryPersistor GetPersistorNoArray(IDataField field)
		{
			var type = field.MappedMember.GetActualType();
			if (field is INestedDataField)
			{
				// nested
				return new NestedPersistor((INestedDataField)field);
			}
			else
			{
				// simple
				var persistor = GetSimplePersistor(type);
				if (persistor == null)
				{
					throw new DataHolderException("Simple Type did not have a binary Persistor: " + type.FullName);
				}
				return persistor;
			}
		}
	}

	public abstract class SimpleBinaryPersistor : ISimpleBinaryPersistor
	{
		public abstract int BinaryLength
		{
			get;
		}

		public abstract void Write(BinaryWriter writer, object obj);

		public abstract object Read(BinaryReader reader);

		public int InitPersistor(List<IGetterSetter> stringPersistors)
		{
			return BinaryLength;
		}
	}

	public class BoolPersistor : SimpleBinaryPersistor
	{
		public override int BinaryLength
		{
			get { return 1; }
		}

		public override void Write(BinaryWriter writer, object obj)
		{
			writer.Write((byte)((bool)obj ? 1 : 0));
		}

		public override object Read(BinaryReader reader)
		{
			return reader.ReadByte() == 1;
		}
	}

	public class Int32Persistor : SimpleBinaryPersistor
	{
		public override int BinaryLength
		{
			get { return 4; }
		}

		public override void Write(BinaryWriter writer, object obj)
		{
			writer.Write((int)obj);
		}

		public override object Read(BinaryReader reader)
		{
			return reader.ReadInt32();
		}
	}

	public class StringPersistor : SimpleBinaryPersistor
	{
		/// <summary>
		/// Redundant
		/// </summary>
		public override int BinaryLength
		{
			get { return -1; }
		}

		public override void Write(BinaryWriter writer, object obj)
		{
			if (string.IsNullOrEmpty(obj as string))
			{
				writer.Write((ushort)0);
			}
			else
			{
				var bytes = BinaryPersistors.DefaultEncoding.GetBytes((string) obj);
				writer.Write((ushort) bytes.Length);
				writer.Write(bytes);
			}
		}

		public override object Read(BinaryReader reader)
		{
			var len = reader.ReadUInt16();
			if (len == 0)
			{
				return "";
			}
			var bytes = reader.ReadBytes(len);
			return BinaryPersistors.DefaultEncoding.GetString(bytes);
		}
	}

	public class UInt32Persistor : SimpleBinaryPersistor
	{
		public override int BinaryLength
		{
			get { return 4; }
		}

		public override void Write(BinaryWriter writer, object obj)
		{
			writer.Write((uint)obj);
		}

		public override object Read(BinaryReader reader)
		{
			return reader.ReadUInt32();
		}
	}

	public class Int16Persistor : SimpleBinaryPersistor
	{
		public override int BinaryLength
		{
			get { return 2; }
		}

		public override void Write(BinaryWriter writer, object obj)
		{
			writer.Write((short)obj);
		}

		public override object Read(BinaryReader reader)
		{
			return reader.ReadInt16();
		}
	}

	public class UInt16Persistor : SimpleBinaryPersistor
	{
		public override int BinaryLength
		{
			get { return 2; }
		}

		public override void Write(BinaryWriter writer, object obj)
		{
			writer.Write((ushort)obj);
		}

		public override object Read(BinaryReader reader)
		{
			return reader.ReadUInt16();
		}
	}

	public class BytePersistor : SimpleBinaryPersistor
	{
		public override int BinaryLength
		{
			get { return 1; }
		}

		public override void Write(BinaryWriter writer, object obj)
		{
			writer.Write((byte)obj);
		}

		public override object Read(BinaryReader reader)
		{
			return reader.ReadByte();
		}
	}

	public class SBytePersistor : SimpleBinaryPersistor
	{
		public override int BinaryLength
		{
			get { return 1; }
		}

		public override void Write(BinaryWriter writer, object obj)
		{
			writer.Write((sbyte)obj);
		}

		public override object Read(BinaryReader reader)
		{
			return reader.ReadSByte();
		}
	}

	public class Int64Persistor : SimpleBinaryPersistor
	{
		public override int BinaryLength
		{
			get { return 8; }
		}

		public override void Write(BinaryWriter writer, object obj)
		{
			writer.Write((long)obj);
		}

		public override object Read(BinaryReader reader)
		{
			return reader.ReadInt64();
		}
	}

	public class UInt64Persistor : SimpleBinaryPersistor
	{
		public override int BinaryLength
		{
			get { return 8; }
		}

		public override void Write(BinaryWriter writer, object obj)
		{
			writer.Write((ulong)obj);
		}

		public override object Read(BinaryReader reader)
		{
			return reader.ReadUInt64();
		}
	}

	public class FloatPersistor : SimpleBinaryPersistor
	{
		public override int BinaryLength
		{
			get { return 4; }
		}

		public override void Write(BinaryWriter writer, object obj)
		{
			writer.Write((float)obj);
		}

		public override object Read(BinaryReader reader)
		{
			return reader.ReadSingle();
		}
	}

	public class DoublePersistor : SimpleBinaryPersistor
	{
		public override int BinaryLength
		{
			get { return 8; }
		}

		public override void Write(BinaryWriter writer, object obj)
		{
			writer.Write((double)obj);
		}

		public override object Read(BinaryReader reader)
		{
			return reader.ReadSingle();
		}
	}
}
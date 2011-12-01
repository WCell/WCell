/*************************************************************************
 *
 *   file		: SerializedFields.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-08-30 18:58:03 +0200 (sø, 30 aug 2009) $

 *   revision		: $Rev: 1054 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.UpdateFields
{
	[Serializable]
	public class SerializedFields
	{
		private CompoundType[] m_values;

		private SerializedFields()
		{
		}

		public CompoundType[] Values
		{
			get
			{
				return m_values;
			}
			set
			{
				m_values = value;
			}
		}

		public void SetValues(ObjectBase obj)
		{
			m_values = obj.UpdateValues;
		}

		public static byte[] GetSerializedFields(ObjectBase obj)
		{
			MemoryStream outputStream = new MemoryStream();
			BinaryFormatter bFormatter = new BinaryFormatter();

			SerializedFields sUpdates = new SerializedFields();
			sUpdates.SetValues(obj);

			bFormatter.Serialize(outputStream, sUpdates);

			return outputStream.ToArray();
		}

		public static CompoundType[] GetDeserializedFields(byte[] serializedFields)
		{
			MemoryStream inputStream = new MemoryStream(serializedFields);
			BinaryFormatter bFormatter = new BinaryFormatter();

			SerializedFields sUpdates;

			sUpdates = (SerializedFields)bFormatter.Deserialize(inputStream);

			return sUpdates.Values;
		}
	}
}
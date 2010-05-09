using System;
using System.Collections;
using Org.Mentalis.Files;

public class TestApp {
	public static void Main() {
		IniReader ini = new IniReader("c:\\test.ini");
		ini.Write("Section1", "KeyString", "MyString");
		ini.Write("Section1", "KeyInt", 5);
		ini.Write("Section2", "KeyBool", true);
		ini.Write("Section2", "KeyBytes", new byte[] {0, 123, 255});
		ini.Write("Section3", "KeyLong", (long)123456789101112);
		ini.Section = "Section1";
		Console.WriteLine("String: " + ini.ReadString("KeyString"));
		Console.WriteLine("Int: " + ini.ReadInteger("KeyInt", 0).ToString());
		Console.WriteLine("Bool: " + ini.ReadBoolean("Section2", "KeyBool", false).ToString());
		Console.WriteLine("Long: " + ini.ReadLong("Section3", "KeyLong", 0).ToString());
		Console.WriteLine("Byte 1 in byte array: " + ini.ReadByteArray("Section2", "KeyBytes")[1].ToString());
		ini.DeleteKey("Section2", "KeyBytes");
		ini.DeleteSection("Section3");
		IEnumerator e = ini.GetSectionNames().GetEnumerator();
		while(e.MoveNext()) {
			Console.WriteLine(e.Current);
		}
		Console.WriteLine("Press enter to continue...");
		Console.ReadLine();
	}
}

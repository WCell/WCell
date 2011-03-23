using System.Xml;
using WCell.Util.Logging;
using WCell.Util.Variables;
using WCell.Intercommunication.DataTypes;

namespace WCell.Core.Variables
{
	public class WCellVariableDefinition : TypeVariableDefinition
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		//public RoleGroup RequiredReadPrivileges;
		//public RoleGroup RequiredWritePrivileges;

		//private string readRoleName, writeRoleName;

		//public bool MaySet(RoleGroup group)
		//{
		//    return group >= RequiredWritePrivileges;
		//}

		//public bool MayGet(RoleGroup group)
		//{
		//    return group >= RequiredReadPrivileges;
		//}

		//public override void ReadXml(XmlReader reader)
		//{
		//    if (reader.MoveToAttribute("Read") && reader.ReadAttributeValue())
		//    {
		//        readRoleName = reader.ReadContentAsString();
		//    }
		//    if (reader.MoveToAttribute("Write"))
		//    {
		//        writeRoleName = reader.ReadString();
		//    }
		//    base.ReadXml(reader);
		//}
		
		//internal void Initialize()
		//{
		//    if (!string.IsNullOrEmpty(readRoleName))
		//    {
		//        RequiredReadPrivileges = PrivilegeMgr.Instance.GetRoleOrDefault(readRoleName);
		//        if (RequiredReadPrivileges == null)
		//        {
		//            RealmServerConfiguration.OnError("Invalid Read - RoleGroup \"{0}\" in Variable: {1}", readRoleName, this);
		//        }
		//    }
		//    if (!string.IsNullOrEmpty(writeRoleName))
		//    {
		//        RequiredWritePrivileges = PrivilegeMgr.Instance.GetRoleOrDefault(writeRoleName);
		//        if (RequiredWritePrivileges == null)
		//        {
		//            RealmServerConfiguration.OnError("Invalid Write - RoleGroup \"{0}\" in Variable: {1}", writeRoleName, this);
		//        }
		//    }

		//    if (RequiredReadPrivileges == null)
		//    {
		//        RequiredReadPrivileges = null;
		//    }
		//    if (RequiredWritePrivileges == null)
		//    {
		//        RequiredWritePrivileges = null;
		//    }
		//}
	}
}
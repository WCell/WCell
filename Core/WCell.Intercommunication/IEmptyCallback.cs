using System.ServiceModel;

namespace WCell.Intercommunication
{
	interface IEmptyCallback
	{
		[OperationContract]
		void Ping();
	}
}

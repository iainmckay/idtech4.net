#pragma once

namespace idTech4
{
	public ref class idNetworkSystem
	{
	public:
		idNetworkSystem()
		{

		}

		void ServerSendReliableMessage(int clientNum, idBitMsg^ msg)
		{
			networkSystem->ServerSendReliableMessage(clientNum, msg->GetNativeRef());		
		}
	};
}
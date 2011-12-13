#pragma once

namespace idTech4
{
	public ref class idCollisionModelManager
	{
	public:
		idCollisionModelManager()
		{

		}

		void LoadMap(idTech4::Editor::idMapFile^ mapFile)
		{
			collisionModelManager->LoadMap(mapFile->GetNative());
		}

		int LoadModel(String^ modelName, bool precache)
		{
			char* tmpName = (char*) Marshal::StringToHGlobalAnsi(modelName).ToPointer();
			
			int ret = collisionModelManager->LoadModel(tmpName, precache);

			Marshal::FreeHGlobal((IntPtr) tmpName);

			return ret;
		}
	};
}
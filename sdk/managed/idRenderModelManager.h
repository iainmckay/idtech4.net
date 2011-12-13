#pragma once

namespace idTech4
{
	public ref class idRenderModel
	{
	private:
		::idRenderModel* _native;

	internal:
		idRenderModel(::idRenderModel* native)
		{
			_native = native;
		}
	};

	public ref class idRenderModelManager
	{
	public:
		idRenderModelManager()
		{

		}

		idRenderModel^ FindModel(String^ name)
		{
			if((name == nullptr) || (name == String::Empty))
			{
				return nullptr;
			}

			char* tmpName = (char*) Marshal::StringToHGlobalAnsi(name).ToPointer();
			
			::idRenderModel* model = renderModelManager->FindModel(tmpName);
			idRenderModel^ ret = gcnew idRenderModel(model);

			Marshal::FreeHGlobal((IntPtr) tmpName);

			return ret;
		}
	};
}
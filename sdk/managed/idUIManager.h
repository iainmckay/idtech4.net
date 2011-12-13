#pragma once

namespace idTech4
{
	public ref class idUserInterface
	{
	private:
		::idUserInterface* _native;

	internal:
		idUserInterface(::idUserInterface* native)
		{
			_native = native;
		}

		::idUserInterface* GetNative()
		{
			return _native;
		}

	public:
		String^ Activate(bool activate, int time)
		{
			return gcnew String(GetNative()->Activate(activate, time));
		}

		bool InitFromFile(String^ qpath)
		{
			return InitFromFile(qpath, true);
		}

		bool InitFromFile(String^ qpath, bool rebuild)
		{
			return InitFromFile(qpath, rebuild, true);
		}

		bool InitFromFile(String^ qpath, bool rebuild, bool cache)
		{
			char* tmpPath = (char*) Marshal::StringToHGlobalAnsi(qpath).ToPointer();
			bool ret = GetNative()->InitFromFile(tmpPath, rebuild, cache);

			Marshal::FreeHGlobal((IntPtr) tmpPath);

			return ret;
		}

		void SetState(String^ key, String^ value)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			char* tmpValue = (char*) Marshal::StringToHGlobalAnsi(value).ToPointer();

			GetNative()->SetStateString(tmpKey, tmpValue);
			
			Marshal::FreeHGlobal((IntPtr) tmpKey);
			Marshal::FreeHGlobal((IntPtr) tmpValue);
		}

		void SetState(String^ key, bool value)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();

			GetNative()->SetStateBool(tmpKey, value);
			
			Marshal::FreeHGlobal((IntPtr) tmpKey);
		}
		
		void SetState(String^ key, int value)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();

			GetNative()->SetStateInt(tmpKey, value);
			
			Marshal::FreeHGlobal((IntPtr) tmpKey);
		}

		void SetState(String^ key, float value)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();

			GetNative()->SetStateFloat(tmpKey, value);
			
			Marshal::FreeHGlobal((IntPtr) tmpKey);
		}

		void StateChanged(int time)
		{
			StateChanged(time, false);
		}

		void StateChanged(int time, bool redraw)
		{
			GetNative()->StateChanged(time, redraw);
		}
	};

	public ref class idUIManager
	{
	public:
		idUIManager()
		{

		}

		idUserInterface^ Alloc()
		{
			::idUserInterface* gui = uiManager->Alloc();

			if(gui == NULL)
			{
				return nullptr;
			}

			return gcnew idUserInterface(gui);
		}

		void DeAlloc(idUserInterface^ gui)
		{
			uiManager->DeAlloc(gui->GetNative());
		}

		idUserInterface^ FindGui(String^ qpath)
		{
			return FindGui(qpath, false);
		}

		idUserInterface^ FindGui(String^ qpath, bool autoLoad)
		{
			return FindGui(qpath, autoLoad, false);
		}

		idUserInterface^ FindGui(String^ qpath, bool autoLoad, bool needUnique)
		{
			return FindGui(qpath, autoLoad, needUnique, false);
		}

		idUserInterface^ FindGui(String^ qpath, bool autoLoad, bool needUnique, bool forceUnique)
		{
			char* tmpPath = (char*) Marshal::StringToHGlobalAnsi(qpath).ToPointer();
			::idUserInterface* native = uiManager->FindGui(tmpPath, autoLoad, needUnique, forceUnique);

			Marshal::FreeHGlobal((IntPtr) tmpPath);
			
			if(native == NULL)
			{
				return nullptr;
			}

			return gcnew idUserInterface(native);
		}
	};
}
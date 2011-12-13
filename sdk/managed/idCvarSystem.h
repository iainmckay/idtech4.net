namespace idTech4
{
	[Flags]
	public enum class CvarFlags
	{
		All = -1,
		Bool = 1 << 0,
		Integer = 1 << 1,
		Float = 1 << 2,
		Syste = 1 << 3,
		Renderer = 1 << 4,
		Sound = 1 << 5,
		Gui = 1 << 6,
		Game = 1 << 7,
		Tool = 1 << 8,
		UserInfo = 1 << 9,
		ServerInfo = 1 << 10,
		NetworkSync = 1 << 11,
		Static = 1 << 12,
		Cheat = 1 << 13,
		NoCheat = 1 << 14,
		Init = 1 << 15,
		ReadOnly = 1 << 16,
		Archive = 1 << 17,
		Modified = 1 << 18
	};

	public ref class idCvarSystem
	{
	public:
		bool GetBool(String^ key)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			bool ret = cvarSystem->GetCVarBool(tmpKey);

			Marshal::FreeHGlobal((IntPtr) tmpKey);

			return ret;
		}

		float GetFloat(String^ key)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			float ret = cvarSystem->GetCVarFloat(tmpKey);

			Marshal::FreeHGlobal((IntPtr) tmpKey);

			return ret;
		}

		int GetInteger(String^ key)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			int ret = cvarSystem->GetCVarInteger(tmpKey);

			Marshal::FreeHGlobal((IntPtr) tmpKey);

			return ret;
		}

		String^ GetString(String^ key)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			String^ ret = gcnew String(cvarSystem->GetCVarString(tmpKey));

			Marshal::FreeHGlobal((IntPtr) tmpKey);

			return ret;
		}

		void Set(String^ key, bool v)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			
			cvarSystem->SetCVarBool(tmpKey, v);

			Marshal::FreeHGlobal((IntPtr) tmpKey);
		}

		void Set(String^ key, String^ v)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			char* tmpValue = (char*) Marshal::StringToHGlobalAnsi(v).ToPointer();
			
			cvarSystem->SetCVarString(tmpKey, tmpValue);

			Marshal::FreeHGlobal((IntPtr) tmpKey);
			Marshal::FreeHGlobal((IntPtr) tmpValue);
		}
	};
	
	public ref class idCvar
	{
	public:
		idCvar(String^ name, String^ value, String^ description, CvarFlags flags)
		{
			idCvar(name, value, nullptr, description, flags);
		}

		idCvar(String^ name, String^ value, ArgCompletion^ valueCompletion, String^ description, CvarFlags flags)
		{
			char* tmpName = (char*) Marshal::StringToHGlobalAnsi(name).ToPointer();
			char* tmpValue = (char*) Marshal::StringToHGlobalAnsi(value).ToPointer();
			char* tmpDescription = (char*) Marshal::StringToHGlobalAnsi(description).ToPointer();

			argCompletion_t callback = NULL;

			if(valueCompletion != nullptr)
			{
				callback = valueCompletion->GetCallback();
			}

			cvarSystem->Register(&idCVar(tmpName, tmpValue, (int) flags, tmpDescription, callback));

			Marshal::FreeHGlobal((IntPtr) tmpName);
			Marshal::FreeHGlobal((IntPtr) tmpValue);
			Marshal::FreeHGlobal((IntPtr) tmpDescription);
		}

		idCvar(String^ name, String^ value, float valueMin, float valueMax, String^ description, CvarFlags flags)
		{
			char* tmpName = (char*) Marshal::StringToHGlobalAnsi(name).ToPointer();
			char* tmpValue = (char*) Marshal::StringToHGlobalAnsi(value).ToPointer();
			char* tmpDescription = (char*) Marshal::StringToHGlobalAnsi(description).ToPointer();

			cvarSystem->Register(&idCVar(tmpName, tmpValue, (int) flags, tmpDescription, valueMin, valueMax));

			Marshal::FreeHGlobal((IntPtr) tmpName);
			Marshal::FreeHGlobal((IntPtr) tmpValue);
			Marshal::FreeHGlobal((IntPtr) tmpDescription);
		}

		idCvar(String^ name, String^ value, float valueMin, float valueMax, ArgCompletion^ valueCompletion, String^ description, CvarFlags flags)
		{
			char* tmpName = (char*) Marshal::StringToHGlobalAnsi(name).ToPointer();
			char* tmpValue = (char*) Marshal::StringToHGlobalAnsi(value).ToPointer();
			char* tmpDescription = (char*) Marshal::StringToHGlobalAnsi(description).ToPointer();

			argCompletion_t callback = NULL;

			if(valueCompletion != nullptr)
			{
				callback = valueCompletion->GetCallback();
			}

			cvarSystem->Register(&idCVar(tmpName, tmpValue, (int) flags, tmpDescription, valueMin, valueMax, callback));

			Marshal::FreeHGlobal((IntPtr) tmpName);
			Marshal::FreeHGlobal((IntPtr) tmpValue);
			Marshal::FreeHGlobal((IntPtr) tmpDescription);
		}

		/*idCvar(String^ name, String^ value, array<String^>^ valueStrings, String^ description, CvarFlags flags)
		{

		}*/

		idCvar(String^ name, String^ value, array<String^>^ valueStrings, ArgCompletion^ valueCompletion, String^ description, CvarFlags flags)
		{
			char* tmpName = (char*) Marshal::StringToHGlobalAnsi(name).ToPointer();
			char* tmpValue = (char*) Marshal::StringToHGlobalAnsi(value).ToPointer();
			char* tmpDescription = (char*) Marshal::StringToHGlobalAnsi(description).ToPointer();

			argCompletion_t callback = NULL;

			if(valueCompletion != nullptr)
			{
				callback = valueCompletion->GetCallback();
			}

			cvarSystem->Register(&idCVar(tmpName, tmpValue, (int) flags, tmpDescription, callback));

			Marshal::FreeHGlobal((IntPtr) tmpName);
			Marshal::FreeHGlobal((IntPtr) tmpValue);
			Marshal::FreeHGlobal((IntPtr) tmpDescription);
		}
	};
}
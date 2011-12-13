namespace idTech4
{
	public ref class idE
	{
	public:
		static const int MaxClients = 32;
		static const int MaxGameEntities = 4096;

		static const int MaxRenderEntityGui = 3;

		static const int GameEntityBits = 12;

		static property String^ EngineVersion
		{
			String^ get()
			{
				return gcnew String(ENGINE_VERSION);
			}
		}

		static property int BuildNumber
		{
			int get()
			{
				return BUILD_NUMBER;
			}
		}

		static property String^ BuildType
		{
			String^ get()
			{
#if defined( _DEBUG )
				return "-debug";
#else
				return "-release"
#endif
			}
		}
		
		static property String^ BuildArch
		{
			String^ get()
			{
				return gcnew String(BUILD_STRING);
			}
		}

		static property String^ BuildDate
		{
			String^ get()
			{
				return gcnew String(__DATE__);
			}
		}

		static property String^ BuildTime
		{
			String^ get()
			{
				return gcnew String(__TIME__);
			}
		}

		static property idDeclManager^ DeclManager
		{
			idDeclManager^ get()
			{
				return _declManager;
			}
		};

		static property idCvarSystem^ CvarSystem
		{
			idCvarSystem^ get()
			{
				return _cvarSystem;
			}
		};

		static property idNetworkSystem^ NetworkSystem
		{
			idNetworkSystem^ get()
			{
				return _networkSystem;
			}
		}

		static property idCollisionModelManager^ CollisionModelManager
		{
			idCollisionModelManager^ get()
			{
				return _collisionModelManager;
			}
		}

		static property idGame^ Game
		{
			idGame^ get()
			{
				return _game;
			}
		}

		static property idLangDict^ Language
		{
			idLangDict^ get()
			{
				return _langDict;
			}
		}

		static property idFileSystem^ FileSystem
		{
			idFileSystem^ get()
			{
				return _fileSystem;
			}
		}

		static property idRenderModelManager^ RenderModelManager
		{
			idRenderModelManager^ get()
			{
				return _renderModelManager;
			}
		}

		static property idUIManager^ UIManager
		{
			idUIManager^ get()
			{
				return _uiManager;
			}
		}

	private:
		static idDeclManager^ _declManager;
		static idCvarSystem^ _cvarSystem;
		static idNetworkSystem^ _networkSystem;
		static idCollisionModelManager^ _collisionModelManager;
		static idRenderModelManager^ _renderModelManager;
		static idFileSystem^ _fileSystem;
		static idUIManager^ _uiManager;

		static idLangDict^ _langDict;

	internal:
		static idGame^ _game;
		/*static idGameEdit^ _gameEdit;*/

	public:
		static idE()
		{
			_declManager = gcnew idDeclManager();
			_cvarSystem = gcnew idCvarSystem();
			_networkSystem = gcnew idNetworkSystem();
			_collisionModelManager = gcnew idCollisionModelManager();
			_renderModelManager = gcnew idRenderModelManager();
			_fileSystem = gcnew idFileSystem();
			_uiManager = gcnew idUIManager();
			_langDict = gcnew idLangDict();
		}

		static void Write(String^ format, ... array<Object^>^ args)
		{
			char* tmp = (char*) Marshal::StringToHGlobalAnsi(String::Format(format, args)).ToPointer();
     	
			common->Printf(tmp);
			Marshal::FreeHGlobal((IntPtr) tmp);
		}

		static void WriteLine(String^ format, ... array<Object^>^ args)
		{
			idE::Write(format + "\n", args);
		}

		static void DWrite(String^ format, ... array<Object^>^ args)
		{
			char* tmp = (char*) Marshal::StringToHGlobalAnsi(String::Format(format, args)).ToPointer();
     	
			common->DPrintf(tmp);
			Marshal::FreeHGlobal((IntPtr) tmp);
		}

		static void DWriteLine(String^ format, ... array<Object^>^ args)
		{
			idE::DWrite(format + "\n", args);
		}

		static void Warning(String^ format, ... array<Object^>^ args)
		{
			char* tmp = (char*) Marshal::StringToHGlobalAnsi(String::Format(format, args)).ToPointer();
     	
			common->Warning(tmp);
			Marshal::FreeHGlobal((IntPtr) tmp);
		}

		static void WarningWriteLine(String^ format, ... array<Object^>^ args)
		{
			idE::Warning(format + "\n", args);
		}
		
		static void DWarning(String^ format, ... array<Object^>^ args)
		{
			char* tmp = (char*) Marshal::StringToHGlobalAnsi(String::Format(format, args)).ToPointer();
     	
			common->DWarning(tmp);
			Marshal::FreeHGlobal((IntPtr) tmp);
		}

		static void DWarningWriteLine(String^ format, ... array<Object^>^ args)
		{
			idE::DWarning(format + "\n", args);
		}

		static void Error(String^ format, ... array<Object^>^ args)
		{
			char* tmp = (char*) Marshal::StringToHGlobalAnsi(String::Format(format, args)).ToPointer();

			common->Error(tmp);
			Marshal::FreeHGlobal((IntPtr) tmp);
		}
	};
}
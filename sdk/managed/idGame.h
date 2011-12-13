namespace idTech4 
{
	public ref struct Angles
	{
	public:
		float Pitch;
		float Yaw;
		float Roll;

		Angles()
		{

		}

		Angles(float pitch, float yaw, float roll)
		{
			Pitch = pitch;
			Yaw = yaw;
			Roll = roll;
		}
	};

	public ref class idUserCommand
	{
	public:
		int GameFrame;
		int GameTime;
		int DuplicateCount;
		
		byte Buttons;

		unsigned char ForwardMove;
		unsigned char RightMove;
		unsigned char UpMove;

		array<short>^ Angles;

		short MouseX;
		short MouseY;

		unsigned char Impulse;
		byte Flags;

		int Sequence;

	public:
		idUserCommand(usercmd_t cmd)
		{
			GameFrame = cmd.gameFrame;
			GameTime = cmd.gameTime;

			DuplicateCount = cmd.duplicateCount;
			Buttons = cmd.buttons;

			ForwardMove = cmd.forwardmove;
			RightMove = cmd.rightmove;
			UpMove = cmd.upmove;

			Angles = gcnew array<short>(3);
			Angles[0] = cmd.angles[0];
			Angles[1] = cmd.angles[1];
			Angles[2] = cmd.angles[2];

			MouseX = cmd.mx;
			MouseY = cmd.my;

			Impulse = cmd.impulse;
			Flags = cmd.flags;
			Sequence = cmd.sequence;
		}
	};

	public ref class idGameReturn
	{
	public:
		String^ SessionCommand;
		int ConsistencyHash;
		int Health;
		int HeartRate;
		int Stamina;
		int Combat;
		bool SyncNextGameFrame;

	public:
		idGameReturn()
		{
			SessionCommand = String::Empty;
		}
	};

	public ref class idGame abstract
	{
	public:
		virtual void Init() = 0;
		virtual void InitFromNewMap(String^ mapName, idRenderWorld^ renderWorld, idSoundWorld^ soundWorld, bool isServer, bool isClient, int randomSeed) = 0;

		virtual idGameReturn^ RunFrame(array<idUserCommand^>^ userCommands) = 0;
		virtual bool Draw(int clientIndex) = 0;

		virtual void CacheDictionaryMedia(idDict^ dict) = 0;

		virtual void HandleMainMenuCommands(String^ menuCommand, idUserInterface^ gui) = 0;

		virtual String^ GetMapLoadingGui(String^ defaultGui) = 0;
		virtual String^ GetBestGameType(String^ map, String^ gameType) = 0;

		virtual void SetLocalClient(int clientIndex) = 0;
		virtual void SetServerInfo(idDict^ serverInfo) = 0;
		virtual idDict^ SetUserInfo(int clientIndex, idDict^ userInfo, bool isClient, bool canModify) = 0;

		virtual void ServerClientConnect(int clientIndex, String^ guid) = 0;
		virtual void ServerClientBegin(int clientIndex) = 0;
	};
}
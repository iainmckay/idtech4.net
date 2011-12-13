#include "../game/Game_local.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace idTech4
{
	public ref class idConsole
	{
	public:
		static void Write(String^ format, ... array<Object^>^ args)
		{
			char* tmp = (char*) Marshal::StringToHGlobalAnsi(String::Format(format, args)).ToPointer();
     	
			common->Printf(tmp);
			Marshal::FreeHGlobal((IntPtr) tmp);
		}

		static void WriteLine(String^ format, ... array<Object^>^ args)
		{
			idConsole::Write(format + "\n", args);
		}

		static void Error(String^ format, ... array<Object^>^ args)
		{
			char* tmp = (char*) Marshal::StringToHGlobalAnsi(String::Format(format, args)).ToPointer();

			common->Error(tmp);
			Marshal::FreeHGlobal((IntPtr) tmp);
		}
	};
}
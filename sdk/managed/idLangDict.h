#pragma once

#using <system.dll>

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Text::RegularExpressions;

namespace idTech4
{
	public ref class idLangDict
	{
	private:
		static int _index;

		static String^ ReplaceHandler(Match^ match)
		{
			return String::Format("{{{0}}}", _index++);
		}

	public:
		idLangDict()
		{

		}

		String^ GetString(String^ str)
		{
			char* tmp = (char*) Marshal::StringToHGlobalAnsi(str).ToPointer();

			String^ ret = gcnew String(common->GetLanguageDict()->GetString(tmp));			
			_index = 0;
			
			ret = Regex::Replace(ret, "%s|%d|%x", gcnew MatchEvaluator(ReplaceHandler));

			Marshal::FreeHGlobal((IntPtr) tmp);

			return ret;
		}
	};
}
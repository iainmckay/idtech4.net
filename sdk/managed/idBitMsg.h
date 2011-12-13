#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

namespace idTech4
{
	public ref class idBitMsg
	{
	private:
		AutoPtr<::idBitMsg> _native;

	internal:
		::idBitMsg& GetNativeRef()
		{
			return _native.GetRef();
		}

	public:
		idBitMsg()
		{
			_native.Reset(new ::idBitMsg());			
		}

		void InitGame()
		{
			byte buf[8192];

			_native.GetPointer()->Init(buf, sizeof(buf));
		}

		void BeginWriting()
		{
			_native.GetPointer()->BeginWriting();
		}

		void WriteByte(int c)
		{
			_native.GetPointer()->WriteByte(c);
		}

		void WriteString(String^ str)
		{
			this->WriteString(str, -1);
		}

		void WriteString(String^ str, int maxLength)
		{
			this->WriteString(str, maxLength, true);
		}

		void WriteString(String^ str, int maxLength, bool make7Bit)
		{
			char* tmp = (char*) Marshal::StringToHGlobalAnsi(str).ToPointer();

			_native.GetPointer()->WriteString(tmp, maxLength, make7Bit);

			Marshal::FreeHGlobal((IntPtr) tmp);
		}

		void WriteLong(long v)
		{
			_native.GetPointer()->WriteLong(v);
		}

		void WriteDeltaDict(idDict^ dict, idDict^ base)
		{
			::idDict* baseNative = NULL;

			if(base != nullptr)
			{
				baseNative = base->GetNative();
			}

			_native.GetPointer()->WriteDeltaDict(dict->GetNativeRef(), baseNative);
		}
	};
}
#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

namespace idTech4
{
	[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
	private delegate void ArgCompletionCallback(const idCmdArgs &args, void(*callback)(const char *s));

	public ref class ArgCompletion abstract
	{
	internal:
		virtual argCompletion_t GetCallback() 
		{
			return NULL;
		}
	};

	public ref class MapNameArgCompletion : ArgCompletion
	{
	internal:
		virtual argCompletion_t GetCallback() override
		{
			return idCmdSystem::ArgCompletion_MapName;
		}
	};

	public ref class IntegerArgCompletion : ArgCompletion
	{
	private:
		int _min;
		int _max;
		ArgCompletionCallback^ _callback;
		GCHandle _handle;

	public:
		IntegerArgCompletion(int min, int max)
		{
			_min = min;
			_max = max;
			_callback = gcnew ArgCompletionCallback(this, &IntegerArgCompletion::ArgCompletion_Integer);
			_handle = GCHandle::Alloc(_callback);
		}

	internal:
		virtual argCompletion_t GetCallback() override
		{
			return ((argCompletion_t) Marshal::GetFunctionPointerForDelegate(_callback).ToPointer());
		}

	private:
		void ArgCompletion_Integer(const idCmdArgs &args, void(*callback)(const char *s)) 
		{
			for(int i = _min; i <= _max; i++)
			{
				callback(va("%s %d", args.Argv(0), i));
			}
		}
	};

public ref class StringArgCompletion : ArgCompletion
	{
	private:
		array<String^>^ _list;
		ArgCompletionCallback^ _callback;
		GCHandle _handle;

	public:
		StringArgCompletion(array<String^>^ list)
		{
			_list = list;
			_callback = gcnew ArgCompletionCallback(this, &StringArgCompletion::ArgCompletion_String);
			_handle = GCHandle::Alloc(_callback);
		}

	internal:
		virtual argCompletion_t GetCallback() override
		{
			return ((argCompletion_t) Marshal::GetFunctionPointerForDelegate(_callback).ToPointer());
		}

	private:
		void ArgCompletion_String(const idCmdArgs &args, void(*callback)(const char *s)) 
		{
			char* tmpName = NULL;

			for(int i = 0; i < _list->Length; i++)
			{
				tmpName = (char*) Marshal::StringToHGlobalAnsi(_list[i]).ToPointer();

				callback(va("%s %s", args.Argv(0), tmpName));

				Marshal::FreeHGlobal((IntPtr) tmpName);
			}
		}
	};
}
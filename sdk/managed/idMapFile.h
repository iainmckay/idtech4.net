#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

namespace idTech4
{
	namespace Editor
	{
		public ref class idMapEntity
		{
		private:
			::idMapEntity* _native;

		internal:
			idMapEntity(::idMapEntity* native)
			{
				_native = native;
			}

			::idMapEntity* GetNative()
			{
				return _native;
			}

		public:
			property idDict^ Dict
			{
				idDict^ get()
				{
					return gcnew idDict(GetNative()->epairs);
				}
			}
		};

		public ref class idMapFile
		{
		private:
			AutoPtr<::idMapFile> _native;

		internal:
			::idMapFile* GetNative()
			{
				return _native.GetPointer();
			}

		public:
			idMapFile()
			{
				_native.Reset(new ::idMapFile());
			}

			bool Parse(String^ fileName)
			{
				return this->Parse(fileName, false, false);
			}

			bool Parse(String^ fileName, bool ignoreRegion)
			{
				return this->Parse(fileName, ignoreRegion, false);
			}

			bool Parse(String^ fileName, bool ignoreRegion, bool osPath)
			{
				char* tmp = (char*) Marshal::StringToHGlobalAnsi(fileName).ToPointer();
				bool ret = GetNative()->Parse(tmp, ignoreRegion, osPath);

				Marshal::FreeHGlobal((IntPtr) tmp);
					
				return ret;
			}

			void RemovePrimitiveData()
			{
				GetNative()->RemovePrimitiveData();
			}

			idMapEntity^ GetEntity(int index)
			{
				return gcnew idMapEntity(GetNative()->GetEntity(index));
			}

			property bool NeedsReload
			{
				bool get()
				{
					return _native->NeedsReload();
				}
			}

			property String^ Name
			{
				String^ get()
				{
					return gcnew String(_native->GetName());
				}
			}

			property int EntityCount
			{
				int get()
				{
					return GetNative()->GetNumEntities();
				}
			}
		};
	}
}
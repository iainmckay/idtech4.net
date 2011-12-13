#pragma once

namespace idTech4
{
	public ref class idFile
	{
	private:
		::idFile* _native;

	internal:
		idFile(::idFile* native)
		{
			_native = native;
		}

		::idFile* GetNative()
		{
			return _native;
		}
	};

	public ref class idFileSystem
	{
	public:
		idFileSystem()
		{

		}

		idFile^ OpenFileRead(String^ relativePath)
		{
			return OpenFileRead(relativePath, true);
		}

		idFile^ OpenFileRead(String^ relativePath, bool allowCopyFiles)
		{
			return OpenFileRead(relativePath, allowCopyFiles, nullptr);
		}

		idFile^ OpenFileRead(String^ relativePath, bool allowCopyFiles, String^ gameDir)
		{
			char* tmpPath = (char*) Marshal::StringToHGlobalAnsi(relativePath).ToPointer();
			char* tmpGameDir = NULL;

			if(gameDir != nullptr)
			{
				tmpGameDir = (char*) Marshal::StringToHGlobalAnsi(gameDir).ToPointer();
			}

			::idFile* file = fileSystem->OpenFileRead(tmpPath, allowCopyFiles, tmpGameDir);
			idFile^ ret = gcnew idFile(file);

			Marshal::FreeHGlobal((IntPtr) tmpPath);

			if(gameDir != nullptr)
			{
				Marshal::FreeHGlobal((IntPtr) tmpGameDir);
			}

			return ret;
		}

		void CloseFile(idFile^ file)
		{
			fileSystem->CloseFile(file->GetNative());
		}
	};
}
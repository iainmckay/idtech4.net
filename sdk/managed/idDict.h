using namespace System;
using namespace System::Runtime::InteropServices;

using namespace Microsoft::Xna::Framework;

namespace idTech4 
{
	public ref class idKeyValue
	{
		const ::idKeyValue* _native;

	internal:
		idKeyValue(const ::idKeyValue* native)
		{
			_native = native;
		}

		const ::idKeyValue* GetNative()
		{
			return _native;
		}

	public:
		property String^ Key
		{
			String^ get()
			{
				return gcnew String(_native->GetKey());
			}
		}

		property String^ Value
		{
			String^ get()
			{
				return gcnew String(_native->GetValue());
			}
		}
	};

	public ref class idDict
	{
	private:
		AutoPtr<::idDict> _native;

	internal:
		idDict(::idDict native)
		{
			// i assume that whenever we're working on a dictionary from the engine; we never
			// directly modify that dictionary, we always return a new instance.
			_native.Reset(new ::idDict());
			_native.GetRef().Copy(native);
		}

		::idDict* GetNative()
		{
			return _native.GetPointer();
		}

		::idDict& GetNativeRef()
		{
			return _native.GetRef();
		}

	public:
		idDict()
		{
			_native.Reset(new ::idDict());
		}
		
		void Clear()
		{
			_native.GetRef().Clear();
		}

		void Set(String^ key, String^ value)
		{
			char* tmp = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			char* tmp2 = (char*) Marshal::StringToHGlobalAnsi(value).ToPointer();
			
			_native.GetRef().Set(tmp, tmp2);

			Marshal::FreeHGlobal((IntPtr) tmp);
			Marshal::FreeHGlobal((IntPtr) tmp2);
		}

		void Set(String^ key, bool value)
		{
			char* tmp = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			
			_native.GetRef().SetBool(tmp, value);

			Marshal::FreeHGlobal((IntPtr) tmp);
		}

		void Set(String^ key, int v)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();

			_native.GetRef().SetInt(tmpKey, v);

			Marshal::FreeHGlobal((IntPtr) tmpKey);
		}

		void SetDefaults(idDict^ dict)
		{
			_native.GetRef().SetDefaults(dict->GetNative());
		}

		String^ GetString(String^ key)
		{
			char* tmp = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
     	
			String^ ret = gcnew String(_native.GetRef().GetString(tmp));

			Marshal::FreeHGlobal((IntPtr) tmp);

			return ret;
		}

		int GetInt(String^ key)
		{
			char* tmp = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
     	
			int ret = _native.GetRef().GetInt(tmp);

			Marshal::FreeHGlobal((IntPtr) tmp);

			return ret;
		}

		int GetInt(String^ key, String^ defaultValue)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			char* tmpValue = (char*) Marshal::StringToHGlobalAnsi(defaultValue).ToPointer();

			int ret = _native.GetRef().GetInt(tmpKey, tmpValue);

			Marshal::FreeHGlobal((IntPtr) tmpKey);
			Marshal::FreeHGlobal((IntPtr) tmpValue);

			return ret;
		}

		String^ GetString(String^ key, String^ defaultValue)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			char* tmpValue = (char*) Marshal::StringToHGlobalAnsi(defaultValue).ToPointer();

			const char* ret = _native.GetRef().GetString(tmpKey, tmpValue);

			Marshal::FreeHGlobal((IntPtr) tmpKey);
			Marshal::FreeHGlobal((IntPtr) tmpValue);

			return gcnew String(ret);
		}

		bool GetBool(String^ key)
		{
			char* tmp = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
     	
			bool ret = _native.GetRef().GetBool(tmp);

			Marshal::FreeHGlobal((IntPtr) tmp);

			return ret;
		}

		bool GetBool(String^ key, String^ defaultValue)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			char* tmpValue = (char*) Marshal::StringToHGlobalAnsi(defaultValue).ToPointer();

			bool ret = _native.GetRef().GetBool(tmpKey, tmpValue);

			Marshal::FreeHGlobal((IntPtr) tmpKey);
			Marshal::FreeHGlobal((IntPtr) tmpValue);

			return ret;
		}

		float GetFloat(String^ key)
		{
			char* tmp = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
     	
			float ret = _native.GetRef().GetFloat(tmp);

			Marshal::FreeHGlobal((IntPtr) tmp);

			return ret;
		}

		float GetFloat(String^ key, String^ defaultValue)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			char* tmpValue = (char*) Marshal::StringToHGlobalAnsi(defaultValue).ToPointer();

			float ret = _native.GetRef().GetFloat(tmpKey, tmpValue);

			Marshal::FreeHGlobal((IntPtr) tmpKey);
			Marshal::FreeHGlobal((IntPtr) tmpValue);

			return ret;
		}

		Vector3 GetVector(String^ key, String^ defaultValue)
		{
			if(defaultValue == nullptr)
			{
				defaultValue = "0 0 0";
			}

			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			char* tmpValue = (char*) Marshal::StringToHGlobalAnsi(defaultValue).ToPointer();

			idVec3 vec = _native.GetRef().GetVector(tmpKey, tmpValue);
			Vector3 ret = Vector3(vec.x, vec.y, vec.z);

			Marshal::FreeHGlobal((IntPtr) tmpKey);
			Marshal::FreeHGlobal((IntPtr) tmpValue);

			return ret;
		}

		Matrix GetMatrix(String^ key, String^ defaultValue)
		{
			if(defaultValue == nullptr)
			{
				defaultValue = "1 0 0 0 1 0 0 0 1";
			}

			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			char* tmpValue = (char*) Marshal::StringToHGlobalAnsi(defaultValue).ToPointer();

			idMat3 mat = _native.GetRef().GetMatrix(tmpKey, tmpValue);
			Matrix ret = Matrix(mat[0].x, mat[0].y, mat[0].z, 0, mat[1].x, mat[1].y, mat[1].z, 0, mat[2].x, mat[2].y, mat[2].z, 0, 0, 0, 0, 0);

			Marshal::FreeHGlobal((IntPtr) tmpKey);
			Marshal::FreeHGlobal((IntPtr) tmpValue);

			return ret;
		}

		void TransferKeyValues(idDict^ dict)
		{
			_native.GetRef().TransferKeyValues(dict->GetNativeRef());
		}

		bool ContainsKey(String^ key)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			const ::idKeyValue *kv = _native.GetRef().FindKey(tmpKey);
	
			Marshal::FreeHGlobal((IntPtr) tmpKey);

			if(kv) 
			{
				return true;
			}
			
			return false;
		}

		idKeyValue^ MatchPrefix(String^ prefix)
		{
			return MatchPrefix(prefix, nullptr);
		}

		idKeyValue^ MatchPrefix(String^ prefix, idKeyValue^ lastMatch)
		{
			char* tmpPrefix = (char*) Marshal::StringToHGlobalAnsi(prefix).ToPointer();
			const ::idKeyValue* nativeLastMatch = NULL;

			if(lastMatch != nullptr)
			{
				nativeLastMatch = lastMatch->GetNative();
			}

			const ::idKeyValue* kv = _native.GetRef().MatchPrefix(tmpPrefix, nativeLastMatch);
			idKeyValue^ ret = nullptr;

			if(kv != NULL)
			{
				ret = gcnew idKeyValue(kv);
			}

			Marshal::FreeHGlobal((IntPtr) tmpPrefix);

			return ret;
		}

		idKeyValue^ FindKey(String^ key)
		{
			char* tmpKey = (char*) Marshal::StringToHGlobalAnsi(key).ToPointer();
			const ::idKeyValue* kv = _native.GetRef().FindKey(tmpKey);
			idKeyValue^ ret = nullptr;

			if(kv != NULL) 
			{
				ret = gcnew idKeyValue(kv);
			}

			Marshal::FreeHGlobal((IntPtr) tmpKey);

			return ret;
		}
	};
}
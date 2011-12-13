#pragma once

namespace idTech4 
{
	public enum class DeclType
	{
		Table,
		Material,
		Skin,
		Sound,
		EntityDef,
		ModelDef,
		Fx,
		Particle,
		Af,
		Pda,
		Video,
		Audio,
		Email,
		ModelExport,
		MapDef,

		MaxTypes = 32
	};

	public ref class idDecl
	{
	private:
		const ::idDecl* _native;

	public:
		idDecl()
		{

		}

	internal:
		idDecl(const ::idDecl* native)
		{
			SetInternal(native);
		}

		void SetInternal(const ::idDecl* native)
		{
			_native = native;
		}

		const ::idDecl* GetNative()
		{
			return _native;
		}

	public:
		property String^ Name
		{
			String^ get()
			{
				return gcnew String(GetNative()->GetName());
			}
		}

		property bool IsImplicit
		{
			bool get()
			{
				return GetNative()->IsImplicit();
			}
		}

		property int Index
		{
			int get()
			{
				return GetNative()->Index();
			}
		}
	};

	public ref class idDeclEntityDef : public idDecl
	{
	private:
		const ::idDeclEntityDef* _native;
		idDict^ _dict;

	public:
		idDeclEntityDef() : idDecl()
		{

		}

	internal:
		void SetNative(const ::idDeclEntityDef* native)
		{
			_native = native;
			_dict = gcnew idDict(native->dict);
		}

	public:
		property idDict^ Dict
		{
			idDict^ get()
			{
				return _dict;
			}
		}
	};

	public ref class idDeclSkin : public idDecl
	{
	private:
		const ::idDeclSkin* _native;

	public:
		idDeclSkin() : idDecl()
		{

		}

	internal:
		idDeclSkin(::idDeclSkin* native) : idDecl()
		{
			SetNative(native);
		}

		void SetNative(const ::idDeclSkin* native)
		{
			_native = native;
		}

		const ::idDeclSkin* GetNative()
		{
			return _native;
		}
	};

	public ref class idMaterial : public idDecl
	{
	private:
		const ::idMaterial* _native;

	public:
		idMaterial() : idDecl()
		{

		}

	internal:
		idMaterial(const ::idMaterial* native) : idDecl()
		{
			SetNative(native);
		}

		void SetNative(const ::idMaterial* native)
		{
			_native = native;
		}
		
		const ::idMaterial* GetNative()
		{
			return _native;
		}
	};

	public ref class idDeclManager
	{
	public:
		int GetDeclTypeCount()
		{
			return declManager->GetNumDeclTypes();
		}

		int GetDeclCount(DeclType type)
		{
			return declManager->GetNumDecls((declType_t) type);
		}

		generic<typename T> where T:ref class,gcnew() T FindType(DeclType type, String^ name)
		{
			return FindType<T>(type, name, true);
		}

		generic<typename T> where T:ref class,gcnew() T FindType(DeclType type, String^ name, bool makeDefault)
		{
			if(name == nullptr)
			{
				return T();
			}

			char* tmpName = (char*) Marshal::StringToHGlobalAnsi(name).ToPointer();

			const ::idDecl* decl = declManager->FindType((declType_t) type, tmpName, makeDefault);
			T ret = T();
			
			if(decl != NULL)
			{
				ret = gcnew T();

				if(ret->GetType() == idDeclEntityDef::typeid)
				{
					((idDeclEntityDef^) ret)->SetNative((const ::idDeclEntityDef*) decl);
				}

				((idDecl^) ret)->SetInternal(decl);
			}

			Marshal::FreeHGlobal((IntPtr) tmpName);

			return ret;
		}

		idDeclSkin^ FindSkin(String^ name)
		{
			return FindSkin(name, true);
		}

		idDeclSkin^ FindSkin(String^ name, bool makeDefault)
		{
			return FindType<idDeclSkin^>(DeclType::Skin, name, makeDefault);
		}

		idMaterial^ FindMaterial(String^ name)
		{
			return FindMaterial(name, true);
		}

		idMaterial^ FindMaterial(String^ name, bool makeDefault)
		{
			return FindType<idMaterial^>(DeclType::Material, name, makeDefault);
		}

		idDecl^ DeclByIndex(DeclType type, int index, bool forceParse)
		{
			const ::idDecl* decl = declManager->DeclByIndex((declType_t) type, index, forceParse);

			if(decl == NULL)
			{
				return nullptr;
			}

			return gcnew idDecl(decl);
		}
		
		void RegisterDeclFolder(String^ folder, String^ extension, DeclType defaultType)
		{
			char* tmpFolder = (char*) Marshal::StringToHGlobalAnsi(folder).ToPointer();
			char* tmpExtension = (char*) Marshal::StringToHGlobalAnsi(extension).ToPointer();

			declManager->RegisterDeclFolder(tmpFolder, tmpExtension, (declType_t) defaultType);

			Marshal::FreeHGlobal((IntPtr) tmpFolder);
			Marshal::FreeHGlobal((IntPtr) tmpExtension);
		}

		void MediaPrint(String^ media)
		{
			media += '\n';
			char* tmpMedia = (char*) Marshal::StringToHGlobalAnsi(media).ToPointer();

			declManager->MediaPrint(tmpMedia);

			Marshal::FreeHGlobal((IntPtr) tmpMedia);
		}
	};
}
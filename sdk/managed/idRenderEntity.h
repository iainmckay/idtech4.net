#pragma once

using namespace Microsoft::Xna::Framework;

namespace idTech4
{
	public ref class idRenderEntity
	{
	private:
		AutoPtr<renderEntity_t> _native;
		
	public:
		property int EntityIndex
		{
			int get()
			{
				return _native.GetRef().entityNum;
			}
			void set(int v)
			{
				_native.GetRef().entityNum = v;
			}
		}

		property int SuppressSurfaceInViewID
		{
			int get()
			{
				return _native.GetRef().suppressSurfaceInViewID;
			}
			void set(int v)
			{
				_native.GetRef().suppressSurfaceInViewID = v;
			}
		}

		property int SuppressShadowInViewID
		{
			int get()
			{
				return _native.GetRef().suppressShadowInViewID;
			}
			void set(int v)
			{
				_native.GetRef().suppressShadowInViewID = v;
			}
		}

		property int SuppressShadowInLightID
		{
			int get()
			{
				return _native.GetRef().suppressShadowInLightID;
			}
			void set(int v)
			{
				_native.GetRef().suppressShadowInLightID = v;
			}
		}

		property int AllowSurfaceInViewID
		{
			int get()
			{
				return _native.GetRef().allowSurfaceInViewID;
			}
			void set(int v)
			{
				_native.GetRef().allowSurfaceInViewID = v;
			}
		}

		property idMaterial^ CustomShader
		{
			idMaterial^ get()
			{
				if(_native.GetRef().customShader != NULL)
				{
					return gcnew idMaterial(_native.GetRef().customShader);
				}

				return nullptr;
			}
			void set(idMaterial^ v)
			{
				if(v != nullptr)
				{
					_native.GetRef().customShader = v->GetNative();
				}

				_native.GetRef().customShader = NULL;
			}
		}

		property idMaterial^ ReferenceShader
		{
			idMaterial^ get()
			{
				if(_native.GetRef().referenceShader != NULL)
				{
					return gcnew idMaterial(_native.GetRef().referenceShader);
				}

				return nullptr;
			}
			void set(idMaterial^ v)
			{
				if(v != nullptr)
				{
					_native.GetRef().referenceShader = v->GetNative();
				}

				_native.GetRef().referenceShader = NULL;
			}
		}

		property idDeclSkin^ CustomSkin
		{
			idDeclSkin^ get()
			{
				if(_native.GetRef().customSkin != NULL)
				{
					return gcnew idDeclSkin(const_cast<::idDeclSkin*>(_native.GetRef().customSkin));
				}

				return nullptr;
			}
			void set(idDeclSkin^ v)
			{
				if(v != nullptr)
				{
					_native.GetRef().customSkin = v->GetNative();
				}

				_native.GetRef().customSkin = NULL;
			}
		}
		
		property bool NoSelfShadow
		{
			bool get()
			{
				return _native.GetRef().noSelfShadow;
			}
			void set(bool v)
			{
				_native.GetRef().noSelfShadow = v;
			}
		}

		property bool NoShadow
		{
			bool get()
			{
				return _native.GetRef().noShadow;
			}
			void set(bool v)
			{
				_native.GetRef().noShadow = v;
			}
		}

		property bool NoDynamicInteractions
		{
			bool get()
			{
				return _native.GetRef().noDynamicInteractions;
			}
			void set(bool v)
			{
				_native.GetRef().noDynamicInteractions = v;
			}
		}

		property Vector3 Origin
		{
			Vector3 get()
			{
				idVec3 vec = _native.GetRef().origin;

				return Vector3(vec[0], vec[1], vec[2]);
			}
			void set(Vector3 v)
			{
				idVec3 vec = _native.GetRef().origin;

				vec[0] = v.X;
				vec[1] = v.Y;
				vec[2] = v.Z;

				_native.GetRef().origin = vec;
			}
		}

		property Matrix Axis
		{
			Matrix get()
			{
				idMat3 mat = _native.GetRef().axis;

				return Matrix(mat[0].x, mat[0].y, mat[0].z, 0, mat[1].x, mat[1].y, mat[1].z, 0, mat[2].x, mat[2].y, mat[2].z, 0, 0, 0, 0, 0);
			}
			void set(Matrix v)
			{
				idMat3 mat = _native.GetRef().axis;

				mat[0].x = v.M11;
				mat[0].y = v.M12;
				mat[0].z = v.M13;

				mat[1].x = v.M21;
				mat[1].y = v.M22;
				mat[1].z = v.M23;

				mat[2].x = v.M31;
				mat[2].y = v.M32;
				mat[2].z = v.M33;

				_native.GetRef().axis = mat;
			}
		}

		property array<float>^ ShaderParms
		{
			array<float>^ get()
			{
				array<float>^ ret = gcnew array<float>(MAX_ENTITY_SHADER_PARMS);

				for(int i = 0; i < MAX_ENTITY_SHADER_PARMS; i++)
				{
					ret[i] = _native.GetRef().shaderParms[i];
				}
				
				return ret;
			}
			void set(array<float>^ v)
			{
				for(int i = 0; i < v->Length; i++)
				{
					_native.GetRef().shaderParms[i] = v[i];
				}
			}
		}

		idRenderEntity()
		{
			_native.Reset(&renderEntity_t());
		}

		idRenderEntity(const renderEntity_t* ent)
		{
			renderEntity_t* tmp = const_cast<renderEntity_t*>(ent);
			_native.Reset(tmp);
		}

		void Clear()
		{

		}
	};
}
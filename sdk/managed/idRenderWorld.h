#pragma once

namespace idTech4
{
	public enum class ShaderParameter
	{
		Red = 0,
		Green = 1,
		Blue = 2,
		Alpha = 3,
		TimeScale = 3,
		TimeOffset = 4,
		Diversity = 5,
		Mode = 7,
		TimeOfDeath = 7,

		MD5SkinScale = 8,

		MD3Frame = 8,
		MD3LastFrame = 9,
		MD3BackLerp = 10,

		BeamEndX = 8,
		BeamEndY = 9,
		BeamEndZ = 10,
		BeamWidth = 11,

		SpriteWidth = 8,
		SpriteHeight = 9,

		ParticleStopTime = 8
	};

	public ref class idRenderWorld
	{
	private:
		::idRenderWorld* _native;

	internal:
		idRenderWorld(::idRenderWorld* native)
		{
			_native = native;
		}

	public:
		property idRenderView^ RenderView
		{
			void set(idRenderView^ v)
			{
				_native->SetRenderView(v->GetNative());
			}
		}

		void DebugClearLines(int time)
		{
			_native->DebugClearLines(time);
		}

		void DebugClearPolygons(int time)
		{
			_native->DebugClearPolygons(time);
		}

		idRenderEntity^ GetRenderEntity(int handle)
		{
			return gcnew idRenderEntity(_native->GetRenderEntity(handle));
		}
	};
}
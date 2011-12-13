#pragma once

namespace idTech4
{
	public ref class idSoundWorld
	{
	private:
		::idSoundWorld* _native;

	internal:
		idSoundWorld(::idSoundWorld* native)
		{
			_native = native;
		}

	public:
		void ClearAllSoundEmitters()
		{
			_native->ClearAllSoundEmitters();
		}
	};
}
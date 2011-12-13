#pragma once

namespace idTech4
{
	public ref class idRenderView
	{
	private:
		AutoPtr<renderView_t> _native;

	internal:
		renderView_t* GetNative()
		{
			return _native.GetPointer();
		}

	public:
		idRenderView()
		{

		}
	};
}
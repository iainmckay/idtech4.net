using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace idTech4.Renderer
{
	public interface IRenderBackend
	{
		void Init();
		
		/// <summary>
		/// We want to exit this with the GPU idle, right at vsync
		/// </summary>
		void BlockingSwapBuffers();
	}
}

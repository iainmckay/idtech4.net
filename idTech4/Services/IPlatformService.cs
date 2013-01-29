/*
===========================================================================

Doom 3 BFG Edition GPL Source Code
Copyright (C) 1993-2012 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 BFG Edition GPL Source Code ("Doom 3 BFG Edition Source Code").  

Doom 3 BFG Edition Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 BFG Edition Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 BFG Edition Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 BFG Edition Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 BFG Edition Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using Microsoft.Xna.Framework;

using idTech4.Renderer;

namespace idTech4.Services
{
	public enum CpuCapabilities
	{
		None = 0x00000,

		/// <summary>Unsupported (386/486).</summary>
		Unsupported = 0x00001,

		/// <summary>Unrecognized processor.</summary>
		Generic = 0x00002,

		/// <summary>Intel.</summary>
		Intel = 0x00004,

		/// <summary>AMD.</summary>
		AMD = 0x00008,

		/// <summary>Multi Media Extensions.</summary>
		MMX = 0x00010,

		/// <summary>3DNow!</summary>
		_3DNow = 0x00020,

		/// <summary>Streaming SIMD Extensions</summary>
		SSE = 0x00040,

		/// <summary>Streaming SIMD Extensions 2</summary>
		SSE2 = 0x00080,

		/// <summary>Streaming SIMD Extensions 3 aka Prescott's New Instructions</summary>
		SSE3 = 0x00100,

		/// <summary>AltiVec</summary>
		AltiVec = 0x00200,

		/// <summary>Hyper-Threading Technology</summary>
		HyperThreading = 0x01000,

		/// <summary>Conditional Move (CMOV) and fast floating point comparison (FCOMI) instructions.</summary>
		ConditionalMove = 0x02000,

		/// <summary>Flush-To-Zero mode (denormal results are flushed to zero).</summary>
		FlushToZero = 0x04000,

		/// <summary>Denormals-Are-Zero mode (denormal source operands are set to zero).</summary>
		DenormalsAreZero = 0x08000
	}

	public interface IPlatformService
	{
		#region Properties
		bool IsDebug { get; }
		bool Is64Bit { get; }
		bool IsIntel { get; }
		bool IsAMD { get; }

		bool IsWindows { get; }
		bool IsXbox { get; }
		bool IsLinux { get; }
		bool IsMac { get; }
		
		uint ClockSpeed { get; }
		uint CoreCount { get; }
		uint ThreadCount { get; }

		uint TotalPhysicalMemory { get; }
		uint TotalVideoMemory { get; }

		CpuCapabilities CpuCapabilities { get; }

		string Name { get; }
		string TagName { get; }
		#endregion

		#region Methods
		IRenderBackend CreateRenderBackend();

		#region Initialization
		#region Properties
		bool IsInitialized { get; }
		#endregion

		#region Methods
		void Initialize();
		#endregion
		#endregion
		#endregion
	}
}
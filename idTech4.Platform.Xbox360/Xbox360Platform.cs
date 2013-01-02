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
using idTech4.Services;

namespace idTech4.Platform.Xbox360
{
	public sealed class Xbox360Platform : BasePlatform
	{
		#region Properties
		public CpuCapabilities CpuCapabilities
		{
			get
			{
				CpuCapabilities caps = 0;

				// check for an AMD
				if(this.IsAMD == true)
				{
					caps = CpuCapabilities.AMD;
				}
				else
				{
					caps = CpuCapabilities.Intel;
				}

				// FIXME: we can't actually make use of any of these features but it would be nice
				// to still identify them.

				// check for Multi Media Extensions
				/*if(HasMMX())
				{
					flags |= CPUID_MMX;
				}

				// check for 3DNow!
				if(Has3DNow())
				{
					flags |= CPUID_3DNOW;
				}

				// check for Streaming SIMD Extensions
				if(HasSSE())
				{
					flags |= CPUID_SSE | CPUID_FTZ;
				}

				// check for Streaming SIMD Extensions 2
				if(HasSSE2())
				{
					flags |= CPUID_SSE2;
				}

				// check for Streaming SIMD Extensions 3 aka Prescott's New Instructions
				if(HasSSE3())
				{
					flags |= CPUID_SSE3;
				}

				// check for Hyper-Threading Technology
				if(HasHTT())
				{
					flags |= CPUID_HTT;
				}

				// check for Conditional Move (CMOV) and fast floating point comparison (FCOMI) instructions
				if(HasCMOV())
				{
					flags |= CPUID_CMOV;
				}

				// check for Denormals-Are-Zero mode
				if(HasDAZ())
				{
					flags |= CPUID_DAZ;
				}*/

				return caps;
			}
		}

		public bool IsXbox
		{
			get
			{
				return true;
			}
		}

		public bool IsIntel
		{
			get
			{
				return true;
			}
		}

		public bool IsAMD
		{
			get
			{
				return (this.IsIntel == false);
			}
		}

		public uint ClockSpeed
		{
			get
			{
				return 3200;
			}
		}

		public uint CoreCount
		{
			get
			{
				return 3;
			}
		}

		public uint ThreadCount
		{
			get
			{
				return 6;
			}
		}

		public uint TotalPhysicalMemory
		{
			get
			{
				return 512;
			}
		}

		public uint TotalVideoMemory
		{
			get
			{
				return 512;
			}
		}

		public string Name
		{
			get
			{
				return "Xbox360";
			}
		}

		public string TagName
		{
			get
			{
				return "xbox";
			}
		}
		#endregion

		#region Constructor
		public Xbox360Platform()
			: base()
		{

		}
		#endregion
	}
}
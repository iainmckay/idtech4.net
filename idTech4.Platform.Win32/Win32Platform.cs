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
using System;
using System.Management;

using Microsoft.Xna.Framework;

using idTech4.Renderer;
using idTech4.Services;

namespace idTech4.Platform.Win32
{
	public sealed class Win32Platform : BasePlatform
	{
		#region Members
		private uint _currentClockSpeed;
		private uint _coreCount;
		private uint _threadCount;

		private uint _totalPhysicalMemory;
		private uint _totalVideoMemory;

		private bool _isIntel;
		#endregion

		#region Constructor
		public Win32Platform()
			: base()
		{
			using(ManagementObjectSearcher mosInfo = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
			{
				// FIXME: ha! going to limit this to one cpu, who in the universe would have more than one, eh?!
				// might come to regret this.
				foreach(ManagementObject mosObj in mosInfo.Get())
				{
					_currentClockSpeed = (uint) mosObj["CurrentClockSpeed"];

					// only vista and above support these properties
					if(Environment.OSVersion.Version.Major >= 6)
					{
						_coreCount = (uint) mosObj["NumberOfCores"];
						_threadCount = (uint) mosObj["NumberOfLogicalProcessors"];
					}
					else
					{
						_coreCount = 1;
						_threadCount = 1;
					}

					string desc = (string) mosObj["Description"];

					_isIntel = desc.Contains("Intel");

					mosObj.Dispose();
					break;
				}
			}

			using(ManagementObjectSearcher mosInfo = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
			{
				foreach(ManagementObject mosObj in mosInfo.Get())
				{
					_totalPhysicalMemory = (uint) ((ulong) mosObj["TotalPhysicalMemory"] / 1024 / 1024);
				}
			}

			using(ManagementObjectSearcher mosInfo = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
			{
				// we only check one graphics card.
				foreach(ManagementObject mosObj in mosInfo.Get())
				{
					_totalVideoMemory = (uint) mosObj["AdapterRAM"] / 1024 / 1024;
				}
			}
		}
		#endregion

		#region BasePlatform implementation
		#region Properties
		public override CpuCapabilities CpuCapabilities
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

		public override bool IsWindows
		{
			get
			{
				return true;
			}
		}

		public override bool IsIntel
		{
			get
			{
				return _isIntel;
			}
		}

		public override bool IsAMD
		{
			get
			{
				return (this.IsIntel == false);
			}
		}

		public override uint ClockSpeed
		{
			get
			{
				return _currentClockSpeed;
			}
		}

		public override uint CoreCount
		{
			get
			{
				return _coreCount;
			}
		}

		public override uint ThreadCount
		{
			get
			{
				return _threadCount;
			}
		}

		public override uint TotalPhysicalMemory
		{
			get
			{
				return _totalPhysicalMemory;
			}
		}

		public override uint TotalVideoMemory
		{
			get
			{
				return _totalVideoMemory;
			}
		}

		public override string Name
		{
			get
			{
				return "Windows";
			}
		}

		public override string TagName
		{
			get
			{
				return "win";
			}
		}
		#endregion

		#region Methods
		public IRenderBackend CreateRenderBackend(GraphicsDeviceManager deviceManager)
		{
			return new Renderer.Backend.XNARenderBackend(deviceManager);
		}
		#endregion
		#endregion
	}
}
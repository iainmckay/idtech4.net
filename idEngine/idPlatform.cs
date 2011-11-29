/*
===========================================================================

Doom 3 GPL Source Code
Copyright (C) 1999-2011 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 GPL Source Code (?Doom 3 Source Code?).  

Doom 3 Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace idTech4
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

	public class idPlatform
	{
		#region Properties
		public bool Is64Bit
		{
			get
			{
				return (IntPtr.Size == 8);
			}
		}

		public bool IsDebug
		{
			get
			{
#if DEBUG
				return true;
#else
				return false;
#endif
			}
		}

		public bool IsIntel
		{
			get
			{
				return _isIntel;
			}
		}

		public bool IsAMD
		{
			get
			{
				return (_isIntel == false);
			}
		}

		public uint ClockSpeed
		{
			get
			{
				return _currentClockSpeed;
			}
		}

		public uint CoreCount
		{
			get
			{
				return _coreCount;
			}
		}

		public uint ThreadCount
		{
			get
			{
				return _threadCount;
			}
		}

		public ulong TotalPhysicalMemory
		{
			get
			{
				return _totalPhysicalMemory;
			}
		}

		public ulong TotalVideoMemory
		{
			get
			{
				return _totalVideoMemory;
			}
		}
		#endregion

		#region Members
		private uint _currentClockSpeed;
		private uint _coreCount;
		private uint _threadCount;

		private ulong _totalPhysicalMemory;
		private ulong _totalVideoMemory;

		private bool _isIntel;
		#endregion

		#region Constructor
		public idPlatform()
		{
			ManagementObjectSearcher mosInfo = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");

			// ha! going to limit this to one cpu, who in the universe would have more than one, eh?!
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

				break;
			}

			mosInfo = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");

			foreach(ManagementObject mosObj in mosInfo.Get())
			{
				_totalPhysicalMemory = (ulong) mosObj["TotalPhysicalMemory"] / 1024 / 1024;
			}

			mosInfo = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");

			// we only check one graphics card.
			foreach(ManagementObject mosObj in mosInfo.Get())
			{
				_totalVideoMemory = (uint) mosObj["AdapterRAM"] / 1024 / 1024;
			}
		}
		#endregion

		#region Methods
		public CpuCapabilities GetCpuCapabilities()
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

			// TODO: we can't actually make use of any of these features but it would be nice
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
		#endregion
	}
}

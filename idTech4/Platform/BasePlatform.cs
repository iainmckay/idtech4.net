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

using Microsoft.Xna.Framework;

using idTech4.Renderer;
using idTech4.Services;

namespace idTech4.Platform
{
	public abstract class BasePlatform : IPlatformService
	{
		#region Members
		private bool _initialized;
		#endregion

		#region Constructor
		public BasePlatform()
		{

		}
		#endregion

		#region IPlatformService implementation
		#region Properties
		public abstract CpuCapabilities CpuCapabilities { get; }

		public virtual bool Is64Bit
		{
			get
			{
				return (IntPtr.Size == 8);
			}
		}

		public virtual bool IsWindows
		{
			get
			{
				return false;
			}
		}

		public virtual bool IsLinux
		{
			get
			{
				return false;
			}
		}

		public virtual bool IsMac
		{
			get
			{
				return false;
			}
		}

		public virtual bool IsXbox
		{
			get
			{
				return false;
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

		public abstract bool IsAMD { get; }
		public abstract bool IsIntel { get; }

		public abstract uint ClockSpeed { get; }

		public abstract uint CoreCount { get; }
		public abstract uint ThreadCount { get; }

		public abstract uint TotalPhysicalMemory { get; }
		public abstract uint TotalVideoMemory { get; }

		public abstract string Name { get; }
		public abstract string TagName { get; }
		#endregion

		#region Methods
		public virtual IRenderBackend CreateRenderBackend(GraphicsDeviceManager graphicsDeviceManager)
		{
			return null;
		}

		#region Initialization
		#region Properties
		public bool IsInitialized
		{
			get
			{
				return _initialized;
			}
		}
		#endregion

		#region Methods
		public virtual void Initialize()
		{
			if(this.IsInitialized == true)
			{
				throw new Exception("BasePlatform has already been initialized.");
			}

			_initialized = true;
		}
		#endregion
		#endregion
		#endregion
		#endregion
	}
}
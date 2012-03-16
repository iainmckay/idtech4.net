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

namespace idTech4.Input
{
	public sealed class idUserCommand
	{
		/// <summary>Frame number.</summary>
		public int GameFrame;

		/// <summary>Game time.</summary>
		public int	GameTime;

		/// <summary>Duplication count for networking</summary>
		public int	DuplicateCount;

		/// <summary>Buttons</summary>
		public Button Buttons;

		/// <summary>Forward/backward movement.</summary>
		public bool ForwardMove;

		/// <summary>Left/right movement.</summary>
		public bool RightMove;

		/// <summary>Up/down movement.</summary>
		public bool UpMove;

		/// <summary>View angles.</summary>
		public short[]	Angles = new short[3];

		/// <summary>Mouse delta X.</summary>
		public short MouseX;

		/// <summary>Mouse delta Y</summary>
		public short MouseY;

		/// <summary>Impulse command.</summary>
		public Impulse Impulse;

		/// <summary>Additional flags.</summary>
		public byte Flags;

		/// <summary>Just for debugging.</summary>
		public int	Sequence;

		public static bool operator ==(idUserCommand c1, idUserCommand c2)
		{
			return ((c1.Buttons == c2.Buttons)
				&& (c1.ForwardMove == c2.ForwardMove)
				&& (c1.RightMove == c2.RightMove)
				&& (c1.UpMove == c2.UpMove)
				&& (c1.Angles[0] == c2.Angles[0])
				&& (c1.Angles[1] == c2.Angles[1])
				&& (c1.Angles[2] == c2.Angles[2])
				&& (c1.Impulse == c2.Impulse)
				&& (c1.Flags == c2.Flags)
				&& (c1.MouseX == c2.MouseX)
				&& (c1.MouseY == c2.MouseY));
		}

		public static bool operator !=(idUserCommand c1, idUserCommand c2)
		{
			return !(c1 == c2);
		}
	}

	[Flags]
	public enum Button : byte
	{
		Attack = 1 << 0,
		Run = 1 << 1,
		Zoom = 1 << 2,
		Scores = 1 << 3,
		MouseLook = 1 << 4,
		B5 = 1 << 5,
		B6 = 1 << 6,
		B7 = 1 << 7
	}

	[Flags]
	public enum Impulse
	{
		Weapon0 = 0,
		Weapon1,
		Weapon2,
		Weapon3,
		Weapon4,
		Weapon5,
		Weapon6,
		Weapon7,
		Weapon8,
		Weapon9,
		Weapon10,
		Weapon11,
		Weapon12,
		WeaponReload,
		WeaponNext,
		WeaponPrevious,
		Unused,
		Ready,
		CenterView,
		ShowInterface,
		ToggleTeam,
		Unused2,
		Spectate,
		Unused3,
		Unused4,
		Unused5,
		Unused6,
		Unused7,
		VoteYes,
		VoteNo,
		UseVehicle
	}
}
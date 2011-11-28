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

using Microsoft.Xna.Framework;

namespace idTech4
{
	public enum ContactType
	{
		/// <summary>No contact.</summary>
		None,
		/// <summary>Trace model edge hits model edge.</summary>
		Edge,
		/// <summary>Model vertex hits trace model polygon.</summary>
		ModelVertex,
		/// <summary>Trace model vertex hits model polygon.</summary>
		TraceModelVertex
	}

	public struct ContactInfo
	{
		public ContactType Type;

		/// <summary>
		/// Point of contact.
		/// </summary>
		public Vector3 Point;

		/// <summary>
		/// Contact plane normal.
		/// </summary>
		public Vector3 Normal;

		/// <summary>
		/// Contact plane distance.
		/// </summary>
		public float Distance;

		/// <summary>
		/// Contents at the other side of the surface.
		/// </summary>
		public ContentFlags Contents;

		/// <summary>
		/// Surface material.
		/// </summary>
		public idMaterial Material;

		/// <summary>
		/// Contact feature on model.
		/// </summary>
		public int ModelFeature;

		/// <summary>
		/// Contact feature on the trace model.
		/// </summary>
		public int TraceModelFeature;

		/// <summary>
		/// Entity the contact surface is a part of.
		/// </summary>
		public int EntityIndex;

		/// <summary>
		/// ID of the clip model the contact surface is part of.
		/// </summary>
		public int ID;
	}

	public struct TraceResult
	{
		/// <summary>
		/// Fraction of movement completed, 1.0 = didn't hit anything.
		/// </summary>
		public float Fraction;

		/// <summary>
		/// Final position of the trace model.
		/// </summary>
		public Vector3 EndPosition;

		/// <summary>
		/// Final axis of the trace model.
		/// </summary>
		public Matrix EndAxis;

		/// <summary>
		/// Contact information, only valid if fraction < 1.0.
		/// </summary>
		public ContactInfo ContactInformation;
	}

	/// <summary>
	/// Contents flags.
	/// </summary>
	/// <remarks>
	/// Make sure to keep the defines in doom_defs.script up to date with these!
	/// </remarks>
	[Flags]
	public enum ContentFlags
	{
		None = -1,
		/// <summary>An eye is never valid in a solid.</summary>
		Solid = 1 << 0,
		/// <summary>Blocks visibility (for AI).</summary>
		Opaque = 1 << 1,
		/// <summary>Used for water.</summary>
		Water = 1 << 2,
		/// <summary>Solid to players.</summary>
		PlayerClip = 1 << 3,
		/// <summary>Solid to monsters.</summary>
		MonsterClip = 1 << 4,
		/// <summary>Solid to moveable entities.</summary>
		MoveableClip = 1 << 5,
		/// <summary>Solid to IK.</summary>
		IkClip = 1 << 6,
		/// <summary>Used to detect blood decals.</summary>
		Blood = 1 << 7,
		/// <summary>Used for actors.</summary>
		Body = 1 << 8,
		/// <summary>Used for projectiles.</summary>
		Projectile = 1 << 9,
		/// <summary>Used for dead bodies.</summary>
		Corpse = 1 << 10,
		/// <summary>Used for render models for collision detection.</summary>
		RenderModel = 1 << 11,
		/// <summary>Used for triggers.</summary>
		Trigger = 1 << 12,
		/// <summary>Solid for AAS.</summary>
		AasSolid = 1 << 13,
		/// <summary>Used to compile an obstacle into AAS that can be enabled/disabled.</summary>
		AasObstacle = 1 << 14,
		/// <summary>Used for triggers that are activated by the flashlight.</summary>
		FlashlightTrigger = 1 << 15,

		/// <summary>Portal separating renderer areas.</summary>
		AreaPortal = 1 << 20,
		/// <summary>Don't cut this brush with CSG operations in the editor.</summary>
		NoCsg = 1 << 21,

		MaskAll = -1,
		MaskSolid = Solid,
		MaskMonsterSolid = Solid | MonsterClip | Body,
		MaskPlayerSolid = Solid | PlayerClip | Body,
		MaskDeadSolid = Solid | PlayerClip,
		MaskWater = Water,
		MaskOpaque = Opaque,
		MaskShotRenderModel = Solid | RenderModel,
		MaskShotBoundingBox = Solid | Body
	}
}
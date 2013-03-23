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
using idTech4.Renderer;
using idTech4.Text;

namespace idTech4.Content.Pipeline.Intermediate.Material
{
	public class MaterialContent
	{
		public string Description;
		public ContentFlags ContentFlags;
		public SurfaceFlags SurfaceFlags;
		public float Sort;
		public MaterialCoverage Coverage;
		public CullType CullType;
		public DeformType DeformType;
		public int[] DeformRegisters;

		public float EditorAlpha;
		public string EditorImageName;

		public DecalInfo DecalInfo;

		public bool[] RegisterIsTemporary           = new bool[Constants.MaxExpressionRegisters];
		public float[] ShaderRegisters              = new float[Constants.MaxExpressionRegisters];

		public List<ExpressionOperation> Operations;
		public List<MaterialStage> Stages;

		public bool RegistersAreConstant;
		public bool ForceOverlays;

		public int StageCount;
		public int RegisterCount;
		public int AmbientStageCount;

		public MaterialContent()
		{
			this.Description         = "<none>";
			this.ContentFlags        = ContentFlags.Solid;
			this.SurfaceFlags        = SurfaceFlags.None;
			this.Coverage            = MaterialCoverage.Bad;
			this.CullType            = CullType.Front;

			this.DeformType          = DeformType.None;
			this.DeformRegisters     = new int[4];

			this.Sort                = (float) MaterialSort.Bad;
			this.EditorAlpha         = 1.0f;

			this.DecalInfo.StayTime  = 10000;
			this.DecalInfo.FadeTime  = 4000;
			this.DecalInfo.Start     = new float[] { 1, 1, 1, 1 };
			this.DecalInfo.End       = new float[] { 0, 0, 0, 0 };

			this.Operations          = new List<ExpressionOperation>();
			this.Stages              = new List<MaterialStage>();
		}

		public bool MatchToken(idLexer lexer, string match)
		{
			if(lexer.ExpectTokenString(match) == false)
			{
				this.MaterialFlags |= MaterialFlags.Defaulted;

				return false;
			}

			return true;
		}
	}
}
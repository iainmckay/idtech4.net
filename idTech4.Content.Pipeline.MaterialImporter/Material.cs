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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using idTech4.Content.Pipeline.Intermediate.Material;

using idTech4.Services;
using idTech4.Sound;
using idTech4.Text;
using idTech4.Text.Decls;

namespace idTech4.Content.Pipeline
{
	public class Material
	{
		/// <summary>
		/// Parses the current material definition and finds all necessary images.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static MaterialContent LoadFrom(string fileName)
		{
		
			ParseMaterial(lexer);

			// TODO: fs_copyFiles
			// if we are doing an fs_copyfiles, also reference the editorImage
			/*if ( cvarSystem->GetCVarInteger( "fs_copyFiles" ) ) {
				GetEditorImage();
			}*/

			// count non-lit stages.
			_ambientStageCount = 0;
			_stageCount = _parsingData.Stages.Count;

			for(int i = 0; i < _stageCount; i++)
			{
				if(_parsingData.Stages[i].Lighting == StageLighting.Ambient)
				{
					_ambientStageCount++;
				}
			}

			// see if there is a subview stage
			if(_sort == (float) MaterialSort.Subview)
			{
				_hasSubview = true;
			}
			else
			{
				_hasSubview = false;
				int count   = _parsingData.Stages.Count;

				for(int i = 0; i < count; i++)
				{
					if(_parsingData.Stages[i].Texture.Dynamic != null)
					{
						_hasSubview = true;
					}
				}
			}

			// automatically determine coverage if not explicitly set
			if(_coverage == MaterialCoverage.Bad)
			{
				// automatically set MC_TRANSLUCENT if we don't have any interaction stages and 
				// the first stage is blended and not an alpha test mask or a subview.
				if(_stageCount == 0)
				{
					// non-visible.
					_coverage = MaterialCoverage.Translucent;
				}
				else if(_stageCount != _ambientStageCount)
				{
					// we have an interaction draw.
					_coverage = MaterialCoverage.Opaque;
				}
				else
				{
					MaterialStates drawStateBits = _parsingData.Stages[0].DrawStateBits;

					if(((drawStateBits & MaterialStates.DestinationBlendBits) != MaterialStates.DestinationBlendZero)
						|| ((drawStateBits & MaterialStates.SourceBlendBits) == MaterialStates.SourceBlendDestinationColor)
						|| ((drawStateBits & MaterialStates.SourceBlendBits) == MaterialStates.SourceBlendOneMinusDestinationColor)
						|| ((drawStateBits & MaterialStates.SourceBlendBits) == MaterialStates.SourceBlendDestinationAlpha)
						|| ((drawStateBits & MaterialStates.SourceBlendBits) == MaterialStates.SourceBlendOneMinusDestinationAlpha))
					{
						// blended with the destination
						_coverage = MaterialCoverage.Translucent;
					}
					else
					{
						_coverage = MaterialCoverage.Opaque;
					}
				}
			}

			// translucent automatically implies noshadows
			if(_coverage == MaterialCoverage.Translucent)
			{
				this.MaterialFlag = MaterialFlags.NoShadows;
			}
			else
			{
				// mark the contents as opaque
				_contentFlags |= ContentFlags.Opaque;
			}

			// if we are translucent, draw with an alpha in the editor
			if(_coverage == MaterialCoverage.Translucent)
			{
				_editorAlpha = 0.5f;
			}
			else
			{
				_editorAlpha = 1.0f;
			}

			// the sorts can make reasonable defaults
			if(_sort == (float) MaterialSort.Bad)
			{
				if(TestMaterialFlag(MaterialFlags.PolygonOffset) == true)
				{
					_sort = (float) MaterialSort.Decal;
				}
				else if(_coverage == MaterialCoverage.Translucent)
				{
					_sort = (float) MaterialSort.Medium;
				}
				else
				{
					_sort = (float) MaterialSort.Opaque;
				}
			}

			// anything that references _currentRender will automatically get sort = SS_POST_PROCESS
			// and coverage = MC_TRANSLUCENT.
			for(int i = 0; i < _stageCount; i++)
			{
				MaterialStage stage = _parsingData.Stages[i];

				idLog.Warning("TODO: if(stage.Texture.Image == ImageManager.OriginalCurrentRenderImage)");
				/*if(stage.Texture.Image == ImageManager.OriginalCurrentRenderImage)
				{
					if(_sort != (float) MaterialSort.PortalSky)
					{
						_sort = (float) MaterialSort.PostProcess;
						_coverage = MaterialCoverage.Translucent;
					}

					break;
				}*/

				if(stage.NewStage.IsEmpty == false)
				{
					NewMaterialStage newShaderStage = stage.NewStage;
					int imageCount                  = newShaderStage.FragmentProgramImages.Length;

					for(int j = 0; j < imageCount; j++)
					{
						idLog.Warning("TODO: ImageManager.OriginalCurrentRenderImage)");
						/*if(newShaderStage.FragmentProgramImages[j] == ImageManager.OriginalCurrentRenderImage)
						{
							if(_sort != (float) MaterialSort.PortalSky)
							{
								_sort = (float) MaterialSort.PostProcess;
								_coverage = MaterialCoverage.Translucent;
							}

							i = _stageCount;
							break;
						}*/
					}
				}
			}

			// set the drawStateBits depth flags
			for(int i = 0; i < _stageCount; i++)
			{
				MaterialStage stage = _parsingData.Stages[i];

				if(_sort == (float) MaterialSort.PostProcess)
				{
					// post-process effects fill the depth buffer as they draw, so only the
					// topmost post-process effect is rendered.
					stage.DrawStateBits |= MaterialStates.DepthFunctionLess;
				}
				else if((_coverage == MaterialCoverage.Translucent) || (stage.IgnoreAlphaTest == true))
				{
					// translucent surfaces can extend past the exactly marked depth buffer
					stage.DrawStateBits |= MaterialStates.DepthFunctionLess | MaterialStates.DepthMask;
				}
				else
				{
					// opaque and perforated surfaces must exactly match the depth buffer,
					// which gets alpha test correct.
					stage.DrawStateBits |= MaterialStates.DepthFunctionEqual | MaterialStates.DepthMask;
				}

				_parsingData.Stages[i] = stage;
			}

			// determine if this surface will accept overlays / decals
			if(_parsingData.ForceOverlays == true)
			{
				// explicitly flaged in material definition
				_allowOverlays = true;
			}
			else
			{
				if(this.IsDrawn == false)
				{
					_allowOverlays = false;
				}

				if(this.Coverage != MaterialCoverage.Opaque)
				{
					_allowOverlays = false;
				}

				if((this.SurfaceFlags & Renderer.SurfaceFlags.NoImpact) != 0)
				{
					_allowOverlays = false;
				}
			}

			// add a tiny offset to the sort orders, so that different materials
			// that have the same sort value will at least sort consistantly, instead
			// of flickering back and forth.

			/* this messed up in-game guis
			if ( sort != SS_SUBVIEW ) {
				int	hash, l;

				l = name.Length();
				hash = 0;
				for ( int i = 0 ; i < l ; i++ ) {
					hash ^= name[i];
				}
				sort += hash * 0.01;
			}
			*/

			if(_stageCount > 0)
			{
				_stages = _parsingData.Stages.ToArray();
			}

			if(_parsingData.Operations.Count > 0)
			{
				_ops = _parsingData.Operations.ToArray();
			}

			if(_registerCount > 0)
			{
				_expressionRegisters = new float[_registerCount];

				Array.Copy(_parsingData.ShaderRegisters, _expressionRegisters, _registerCount);
			}

			// see if the registers are completely constant, and don't need to be evaluated per-surface.
			CheckForConstantRegisters();

			_parsingData = null;

			// finish things up
			if(TestMaterialFlag(MaterialFlags.Defaulted) == true)
			{
				MakeDefault();
				return false;
			}

			return true;
		}

		
	}
}
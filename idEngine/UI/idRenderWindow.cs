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

using idTech4.Math;
using idTech4.Renderer;

namespace idTech4.UI
{
	public sealed class idRenderWindow : idWindow
	{
		#region Members
		private idRenderWorld _world;
		private idRenderView _renderView = new idRenderView();

		private string _animationClass;
		private int _animationLength;
		private int _animationEndTime;
		private bool _updateAnimation;
		
		private idWinBool _needsRender = new idWinBool("needsRender");

		private idWinString _animationName = new idWinString("anim");
		private idWinString _modelName = new idWinString("model");

		private idWinVector4 _lightOrigin = new idWinVector4("lightOrigin");
		private idWinVector4 _lightColor = new idWinVector4("lightColor");
		private idWinVector4 _modelOrigin = new idWinVector4("modelOrigin");
		private idWinVector4 _modelRotate = new idWinVector4("modelRotate");
		private idWinVector4 _viewOffset = new idWinVector4("viewOffset");
		#endregion

		#region Constructor
		public idRenderWindow(idUserInterface gui)
			: base(gui)
		{
			Init();
		}

		public idRenderWindow(idDeviceContext context, idUserInterface gui)
			: base(gui, context)
		{
			Init();
		}
		#endregion

		#region Methods
		#region Private
		private void Init()
		{
			_world = idE.RenderSystem.CreateRenderWorld();
			_needsRender.Set(true);

			_lightOrigin.Set(new Vector4(-128.0f, 0.0f, 0.0f, 1.0f));
			_lightColor.Set(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
			_modelOrigin.Set(Vector4.Zero);
			_viewOffset.Set(new Vector4(-128.0f, 0.0f, 0.0f, 1.0f));
			//_modelAnimation = null;

			_animationLength = 0;
			_animationEndTime = -1;

			//modelDef = -1;
			_updateAnimation = true;
		}
		#endregion
		#endregion

		#region idWindow implementation
		#region Public
		public override void Draw(float x, float y)
		{
			//idConsole.Warning("TODO: RenderWindow Draw");

			/*PreRender();
			Render(time);

			 */

			_renderView.Clear();
			_renderView.ViewOrigin = _viewOffset.ToVector3();
			//refdef.vieworg.Set(-128, 0, 0);

			_renderView.ViewAxis = Matrix.Identity;
			_renderView.MaterialParameters[0] = 1;
			_renderView.MaterialParameters[1] = 1;
			_renderView.MaterialParameters[2] = 1;
			_renderView.MaterialParameters[3] = 1;

			idRectangle drawRect = this.DrawRectangle;

			_renderView.X = (int) drawRect.X;
			_renderView.Y = (int) drawRect.Y;
			_renderView.Width = (int) drawRect.Width;
			_renderView.Height = (int) drawRect.Height;

			_renderView.FovX = 90;
			_renderView.FovY = 2.0f * (float) System.Math.Atan(drawRect.Height / drawRect.Width) * idMath.Rad2Deg;

			_renderView.Time = this.UserInterface.Time;;

			_world.RenderScene(_renderView);
		}

		public override idWindowVariable GetVariableByName(string name, bool fixup, ref DrawWindow owner)
		{
			string nameLower = name.ToLower();

			if(nameLower == "model")
			{
				return _modelName;
			}
			else if(nameLower == "anim")
			{
				return _animationName;
			}
			else if(nameLower == "lightorigin")
			{
				return _lightOrigin;
			}
			else if(nameLower == "lightcolor")
			{
				return _lightColor;
			}
			else if(nameLower == "modelorigin")
			{
				return _modelOrigin;
			}
			else if(nameLower == "modelrotate")
			{
				return _modelRotate;
			}
			else if(nameLower == "viewoffset")
			{
				return _viewOffset;
			}
			else if(nameLower == "needsrender")
			{
				return _needsRender;
			}

			return base.GetVariableByName(name, fixup, ref owner);
		}
		#endregion

		#region Protected
		protected override bool ParseInternalVariable(string name, Text.idScriptParser parser)
		{
			string nameLower = name.ToLower();

			if(nameLower == "animclass")
			{
				_animationClass = ParseString(parser);
			}
			else
			{
				return base.ParseInternalVariable(name, parser);
			}

			return true;
		}
		#endregion
		#endregion
	}
}
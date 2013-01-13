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

namespace idTech4.Text.Decls
{
	public class idDeclFX : idDecl
	{
		#region Constructor
		public idDeclFX()
			: base()
		{

		}
		#endregion

		#region Methods
		#region Private
		private void ParseSingleAction(idLexer lexer /*idFXSingleAction& FXAction*/)
		{
			idToken token;
			string tokenValue;

			/*FXAction.type = -1;
			FXAction.sibling = -1;

			FXAction.data = "<none>";
			FXAction.name = "<none>";
			FXAction.fire = "<none>";

			FXAction.delay = 0.0f;
			FXAction.duration = 0.0f;
			FXAction.restart = 0.0f;
			FXAction.size = 0.0f;
			FXAction.fadeInTime = 0.0f;
			FXAction.fadeOutTime = 0.0f;
			FXAction.shakeTime = 0.0f;
			FXAction.shakeAmplitude = 0.0f;
			FXAction.shakeDistance = 0.0f;
			FXAction.shakeFalloff = false;
			FXAction.shakeImpulse = 0.0f;
			FXAction.shakeIgnoreMaster = false;
			FXAction.lightRadius = 0.0f;
			FXAction.rotate = 0.0f;
			FXAction.random1 = 0.0f;
			FXAction.random2 = 0.0f;

			FXAction.lightColor = vec3_origin;
			FXAction.offset = vec3_origin;
			FXAction.axis = mat3_identity;

			FXAction.bindParticles = false;
			FXAction.explicitAxis = false;
			FXAction.noshadows = false;
			FXAction.particleTrackVelocity = false;
			FXAction.trackOrigin = false;
			FXAction.soundStarted = false;*/

			while(true)
			{
				if((token = lexer.ReadToken()) == null)
				{
					break;
				}

				tokenValue = token.ToString().ToLower();

				if(tokenValue == "}")
				{
					break;
				}
				else if(tokenValue == "angle")
				{
					/*idAngles a;*/
					/*a[0] = */
					lexer.ParseFloat();
					lexer.ExpectTokenString(",");
					/*a[1] = */
					lexer.ParseFloat();
					lexer.ExpectTokenString(",");
					/*a[2] = */
					lexer.ParseFloat();
					/*FXAction.axis = a.ToMat3();
					FXAction.explicitAxis = true;*/
				}
				else if(tokenValue == "attachentity")
				{
					token = lexer.ReadToken();

					/*FXAction.data = token;
					FXAction.type = FX_ATTACHENTITY;

					// precache the model
					renderModelManager->FindModel( FXAction.data );*/
				}
				else if(tokenValue == "attachlight")
				{
					token = lexer.ReadToken();

					/*FXAction.data = token;
					FXAction.type = FX_ATTACHLIGHT;

					// precache it
					declManager->FindMaterial( FXAction.data );*/
				}
				else if(tokenValue == "axis")
				{
					/*idVec3 v;*/
					/*v.x = */
					lexer.ParseFloat();
					lexer.ExpectTokenString(",");
					/*v.y = */
					lexer.ParseFloat();
					lexer.ExpectTokenString(",");
					/*v.z = */
					lexer.ParseFloat();
					/*v.Normalize();
					FXAction.axis = v.ToMat3();
					FXAction.explicitAxis = true;*/
				}
				else if(tokenValue == "decal")
				{
					token = lexer.ReadToken();

					/*FXAction.data = token;
					FXAction.type = FX_DECAL;

					// precache it
					declManager->FindMaterial( FXAction.data );*/
				}
				else if(tokenValue == "delay")
				{
					/*FXAction.delay = */
					lexer.ParseFloat();
				}
				else if(tokenValue == "duration")
				{
					/*FXAction.duration = */
					lexer.ParseFloat();
				}
				else if(tokenValue == "fadein")
				{
					/*FXAction.fadeInTime = */
					lexer.ParseFloat();
				}
				else if(tokenValue == "fadeout")
				{
					/*FXAction.fadeOutTime = */
					lexer.ParseFloat();
				}
				else if(tokenValue == "fire")
				{
					token = lexer.ReadToken();
					// TODO: FXAction.fire = token;
				}
				else if(tokenValue == "ignoremaster")
				{
					/*FXAction.shakeIgnoreMaster = true;*/
				}
				else if(tokenValue == "launch")
				{
					token = lexer.ReadToken();

					/*FXAction.data = token;
					FXAction.type = FX_LAUNCH;

					// precache the entity def
					declManager->FindType( DECL_ENTITYDEF, FXAction.data );*/
				}				
				else if(tokenValue == "light")
				{
					token = lexer.ReadToken();

					/*FXAction.data = token;*/
					lexer.ExpectTokenString(",");
					/*FXAction.lightColor[0] = */
					lexer.ParseFloat();
					lexer.ExpectTokenString(",");
					/*FXAction.lightColor[1] = */
					lexer.ParseFloat();
					lexer.ExpectTokenString(",");
					/*FXAction.lightColor[2] = */
					lexer.ParseFloat();
					lexer.ExpectTokenString(",");
					/*FXAction.lightRadius = */
					lexer.ParseFloat();
					/*FXAction.type = FX_LIGHT;

					// precache the light material
					declManager->FindMaterial( FXAction.data );*/
				}
				else if(tokenValue == "model")
				{
					token = lexer.ReadToken();

					/*FXAction.data = token;
					FXAction.type = FX_MODEL;

					// precache it
					renderModelManager->FindModel( FXAction.data );*/
				}
				else if(tokenValue == "name")
				{
					token = lexer.ReadToken();
					// TODO: FXAction.name = token;
				}
				else if(tokenValue == "noshadows")
				{
					// TODO: FXAction.noshadows = true;
				}
				else if(tokenValue == "offset")
				{
					/*FXAction.offset.x = */
					lexer.ParseFloat();
					lexer.ExpectTokenString(",");
					/*FXAction.offset.y = */
					lexer.ParseFloat();
					lexer.ExpectTokenString(",");
					/*FXAction.offset.z = */
					lexer.ParseFloat();
				}
				else if(tokenValue == "particle") // FIXME: now the same as model
				{
					token = lexer.ReadToken();

					/*FXAction.data = token;
					FXAction.type = FX_PARTICLE;

					// precache it
					renderModelManager->FindModel( FXAction.data );*/
				}				
				else if(tokenValue == "particletrackvelocity")
				{
					// TODO: FXAction.particleTrackVelocity = true;
				}
				else if(tokenValue == "random")
				{
					/*FXAction.random1 = */
					lexer.ParseFloat();
					lexer.ExpectTokenString(",");
					/*FXAction.random2 = */
					lexer.ParseFloat();
					// FXAction.delay = 0.0f;		// check random
				}
				else if(tokenValue == "restart")
				{
					/*FXAction.restart = */
					lexer.ParseFloat();
				}
				else if(tokenValue == "rotate")
				{
					/*FXAction.rotate = */
					lexer.ParseFloat();
				}
				else if(tokenValue == "shake")
				{
					/*FXAction.type = FX_SHAKE;*/
					/*FXAction.shakeTime = */
					lexer.ParseFloat();
					lexer.ExpectTokenString(",");
					/*FXAction.shakeAmplitude = */
					lexer.ParseFloat();
					lexer.ExpectTokenString(",");
					/*FXAction.shakeDistance = */
					lexer.ParseFloat();
					lexer.ExpectTokenString(",");
					/*FXAction.shakeFalloff = */
					lexer.ParseBool();
					lexer.ExpectTokenString(",");
					/*FXAction.shakeImpulse = */
					lexer.ParseFloat();
				}
				else if(tokenValue == "shockwave")
				{
					token = lexer.ReadToken();

					/*FXAction.data = token;
					FXAction.type = FX_SHOCKWAVE;

					// precache the entity def
					declManager->FindType( DECL_ENTITYDEF, FXAction.data );*/
				}
				else if(tokenValue == "size")
				{
					/*FXAction.size = */
					lexer.ParseFloat();
				}
				else if(tokenValue == "sound")
				{
					token = lexer.ReadToken();

					/*FXAction.data = token;
					FXAction.type = FX_SOUND;

					// precache it
					declManager->FindSound( FXAction.data );*/
				}
				else if(tokenValue == "trackorigin")
				{
					/*FXAction.trackOrigin = */
					lexer.ParseBool();
				}
				else if(tokenValue == "uselight")
				{
					token = lexer.ReadToken();

					/*FXAction.data = token;
					for( int i = 0; i < events.Num(); i++ ) {
						if ( events[i].name.Icmp( FXAction.data ) == 0 ) {
							FXAction.sibling = i;
							FXAction.lightColor = events[i].lightColor;
							FXAction.lightRadius = events[i].lightRadius;
						}
					}
					FXAction.type = FX_LIGHT;

					// precache the light material
					declManager->FindMaterial( FXAction.data );*/
				}		
				else if(tokenValue == "usemodel")
				{
					token = lexer.ReadToken();

					/*FXAction.data = token;
					for( int i = 0; i < events.Num(); i++ ) {
						if ( events[i].name.Icmp( FXAction.data ) == 0 ) {
							FXAction.sibling = i;
						}
					}
					FXAction.type = FX_MODEL;

					// precache the model
					renderModelManager->FindModel( FXAction.data );*/
				}
				else
				{
					lexer.Warning("FX File: bad token");
				}
			}
		}
		#endregion
		#endregion

		#region idDecl implementation
		#region Properties
		public override string DefaultDefinition
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return "{\n\t{\n\t\tduration\t5\n\t\tmodel\t\t_default\n\t}\n}";
			}
		}

		public override int MemoryUsage
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idLog.Warning("TODO: idDeclFX.MemoryUsage");
				return 0;
			}
		}
		#endregion

		#region Methods
		#region Public
		public override bool Parse(string text)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idLexer lexer = new idLexer(idDeclFile.LexerOptions);
			lexer.LoadMemory(text, this.FileName, this.LineNumber);
			lexer.SkipUntilString("{");

			idToken token;
			string tokenValue;

			idLog.Warning("TODO: actual fx parsing, we only step over the block");

			while(true)
			{
				if((token = lexer.ReadToken()) == null)
				{
					break;
				}

				tokenValue = token.ToString().ToLower();

				if(tokenValue == "}")
				{
					break;
				}

				if(tokenValue == "bindto")
				{
					token = lexer.ReadToken();

					idLog.Warning("TODO: FX: joint = token;");
				}
				else if(tokenValue == "{")
				{
					idLog.Warning("TODO: FX: idFXSingleAction action;");
					ParseSingleAction(lexer/*, action*/);
					// events.Append(action);
					continue;
				}
			}

			if(lexer.HadError == true)
			{
				lexer.Warning("FX decl '{0}' had a parse error", this.Name);
				return false;
			}

			return true;
		}
		#endregion

		#region Protected
		protected override void ClearData()
		{
			base.ClearData();

			// TODO: events.Clear();
		}
		#endregion
		#endregion
		#endregion
	}
}
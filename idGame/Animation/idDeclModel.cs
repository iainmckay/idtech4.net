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

using idTech4.Renderer;
using idTech4.Text;
using idTech4.Text.Decl;

namespace idTech4.Game.Animation
{
	public class idDeclModel : idDecl
	{
		#region Properties
		public idDeclSkin DefaultSkin
		{
			get
			{
				return _skin;
			}
		}

		public idRenderModel Model
		{
			get
			{
				return _model;
			}
		}
		#endregion

		#region Members
		private idDeclSkin _skin;
		private idRenderModel _model;
		#endregion

		#region Constructor
		public idDeclModel()
			: base()
		{
			idConsole.WriteLine("TODO: idDeclModel");
			/*modelHandle	= NULL;
			skin		= NULL;
			offset.Zero();
			for ( int i = 0; i < ANIM_NumAnimChannels; i++ ) {
				channelJoints[i].Clear();
			}*/
		}
		#endregion

		#region idDecl implementation
		#region Properties
		public override int Size
		{
			get
			{
				idConsole.WriteLine("TODO: idDeclModel.Size");
				return 0;
			}
		}
		#endregion

		#region Methods
		#region Public
		public override string GetDefaultDefinition()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idDeclModel");
			}

			return "{ }";
		}

		public override bool Parse(string text)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idDeclModel");
			}

			idLexer lexer = new idLexer(idDeclFile.LexerOptions);
			lexer.LoadMemory(text, this.FileName, this.LineNumber);
			lexer.SkipUntilString("{");

			int defaultAnimationCount = 0;
			idToken token;
			idToken token2;
			string tokenValue;

			while(true)
			{
				if((token = lexer.ReadToken()) == null)
				{
					break;
				}

				tokenValue = token.ToString();

				if(tokenValue == "}")
				{
					break;
				}

				if(tokenValue == "inherit")
				{
					idConsole.WriteLine("TODO: inherit");

					/*if( !src.ReadToken( &token2 ) ) {
						src.Warning( "Unexpected end of file" );
						MakeDefault();
						return false;
					}
			
					const idDeclModelDef *copy = static_cast<const idDeclModelDef *>( declManager->FindType( DECL_MODELDEF, token2, false ) );
					if ( !copy ) {
						common->Warning( "Unknown model definition '%s'", token2.c_str() );
					} else if ( copy->GetState() == DS_DEFAULTED ) {
						common->Warning( "inherited model definition '%s' defaulted", token2.c_str() );
						MakeDefault();
						return false;
					} else {
						CopyDecl( copy );
						numDefaultAnims = anims.Num();
					}*/
				} 
				else if(tokenValue == "skin") 
				{
					idConsole.WriteLine("TODO: skin");
					
					/*if( !src.ReadToken( &token2 ) ) {
						src.Warning( "Unexpected end of file" );
						MakeDefault();
						return false;
					}
					skin = declManager->FindSkin( token2 );
					if ( !skin ) {
						src.Warning( "Skin '%s' not found", token2.c_str() );
						MakeDefault();
						return false;
					}*/
				} 
				else if(tokenValue == "mesh")
				{
					idConsole.WriteLine("TODO: mesh");
					/*if( !src.ReadToken( &token2 ) ) {
						src.Warning( "Unexpected end of file" );
						MakeDefault();
						return false;
					}
					filename = token2;
					filename.ExtractFileExtension( extension );
					if ( extension != MD5_MESH_EXT ) {
						src.Warning( "Invalid model for MD5 mesh" );
						MakeDefault();
						return false;
					}
					modelHandle = renderModelManager->FindModel( filename );
					if ( !modelHandle ) {
						src.Warning( "Model '%s' not found", filename.c_str() );
						MakeDefault();
						return false;
					}

					if ( modelHandle->IsDefaultModel() ) {
						src.Warning( "Model '%s' defaulted", filename.c_str() );
						MakeDefault();
						return false;
					}

					// get the number of joints
					num = modelHandle->NumJoints();
					if ( !num ) {
						src.Warning( "Model '%s' has no joints", filename.c_str() );
					}

					// set up the joint hierarchy
					joints.SetGranularity( 1 );
					joints.SetNum( num );
					jointParents.SetNum( num );
					channelJoints[0].SetNum( num );
					md5joints = modelHandle->GetJoints();
					md5joint = md5joints;
					for( i = 0; i < num; i++, md5joint++ ) {
						joints[i].channel = ANIMCHANNEL_ALL;
						joints[i].num = static_cast<jointHandle_t>( i );
						if ( md5joint->parent ) {
							joints[i].parentNum = static_cast<jointHandle_t>( md5joint->parent - md5joints );
						} else {
							joints[i].parentNum = INVALID_JOINT;
						}
						jointParents[i] = joints[i].parentNum;
						channelJoints[0][i] = i;
					}*/
				}
				else if(tokenValue == "remove")
				{
					idConsole.WriteLine("TODO: remove");

					// removes any anims whos name matches
					/*if( !src.ReadToken( &token2 ) ) {
						src.Warning( "Unexpected end of file" );
						MakeDefault();
						return false;
					}
					num = 0;
					for( i = 0; i < anims.Num(); i++ ) {
						if ( ( token2 == anims[ i ]->Name() ) || ( token2 == anims[ i ]->FullName() ) ) {
							delete anims[ i ];
							anims.RemoveIndex( i );
							if ( i >= numDefaultAnims ) {
								src.Warning( "Anim '%s' was not inherited.  Anim should be removed from the model def.", token2.c_str() );
								MakeDefault();
								return false;
							}
							i--;
							numDefaultAnims--;
							num++;
							continue;
						}
					}
					if ( !num ) {
						src.Warning( "Couldn't find anim '%s' to remove", token2.c_str() );
						MakeDefault();
						return false;
					}*/
				} 
				else if(tokenValue == "anim") 
				{
					idConsole.WriteLine("TODO: anim");
					/*if ( !modelHandle ) {
						src.Warning( "Must specify mesh before defining anims" );
						MakeDefault();
						return false;
					}
					if ( !ParseAnim( src, numDefaultAnims ) ) {
						MakeDefault();
						return false;
					}*/
				} 
				else if(tokenValue == "offset") 
				{
					idConsole.WriteLine("TODO: offset");
					/*if ( !src.Parse1DMatrix( 3, offset.ToFloatPtr() ) ) {
						src.Warning( "Expected vector following 'offset'" );
						MakeDefault();
						return false;
					}*/
				} 
				else if(tokenValue == "channel") 
				{
					idConsole.WriteLine("TODO: channel");
					/*if ( !modelHandle ) {
						src.Warning( "Must specify mesh before defining channels" );
						MakeDefault();
						return false;
					}

					// set the channel for a group of joints
					if( !src.ReadToken( &token2 ) ) {
						src.Warning( "Unexpected end of file" );
						MakeDefault();
						return false;
					}
					if ( !src.CheckTokenString( "(" ) ) {
						src.Warning( "Expected { after '%s'\n", token2.c_str() );
						MakeDefault();
						return false;
					}

					for( i = ANIMCHANNEL_ALL + 1; i < ANIM_NumAnimChannels; i++ ) {
						if ( !idStr::Icmp( channelNames[ i ], token2 ) ) {
							break;
						}
					}

					if ( i >= ANIM_NumAnimChannels ) {
						src.Warning( "Unknown channel '%s'", token2.c_str() );
						MakeDefault();
						return false;
					}

					channel = i;
					jointnames = "";

					while( !src.CheckTokenString( ")" ) ) {
						if( !src.ReadToken( &token2 ) ) {
							src.Warning( "Unexpected end of file" );
							MakeDefault();
							return false;
						}
						jointnames += token2;
						if ( ( token2 != "*" ) && ( token2 != "-" ) ) {
							jointnames += " ";
						}
					}

					GetJointList( jointnames, jointList );

					channelJoints[ channel ].SetNum( jointList.Num() );
					for( num = i = 0; i < jointList.Num(); i++ ) {
						jointnum = jointList[ i ];
						if ( joints[ jointnum ].channel != ANIMCHANNEL_ALL ) {
							src.Warning( "Joint '%s' assigned to multiple channels", modelHandle->GetJointName( jointnum ) );
							continue;
						}
						joints[ jointnum ].channel = channel;
						channelJoints[ channel ][ num++ ] = jointnum;
					}
					channelJoints[ channel ].SetNum( num );*/
				}
				else
				{
					lexer.Warning("unknown token '{0}'", token.ToString());
					MakeDefault();

					return false;
				}
			}
		
			return true;
		}
		#endregion

		#region Protected
		protected override void ClearData()
		{
			base.ClearData();

			idConsole.WriteLine("TODO: idDeclModel.ClearData");
			/*anims.DeleteContents( true );
			joints.Clear();
			jointParents.Clear();
			modelHandle	= NULL;
			skin = NULL;
			offset.Zero();
			for ( int i = 0; i < ANIM_NumAnimChannels; i++ ) {
				channelJoints[i].Clear();
			}*/
		}
		#endregion
		#endregion
		#endregion
	}
}
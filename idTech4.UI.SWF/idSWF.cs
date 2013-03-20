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
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using idTech4.Renderer;
using idTech4.Services;

namespace idTech4.UI.SWF
{
	public class idSWF
	{		
		#region Members
		// mouse coords for all flash files
		private static int _mouseX = -1;
		private static int _mouseY = -1;

		private string _fileName;

		private float _frameWidth;
		private float _frameHeight;
		private ushort _frameRate;
		private float _renderBorder;
		private float _swfScale;

		private Vector2	_scaleToVirtual;

		private int	_lastRenderTime;

		private bool _isActive;
		private bool _inhibitControl;
		private bool _useInhibtControl;

		// certain screens need to be rendered when the pause menu is up so if this flag is
		// set on the gui we will allow it to render at a paused state;
		private bool _pausedRender;

		private bool _mouseEnabled;
		private bool _useMouse;

		private bool _blackbars;
		private bool _crop;
		private bool _paused;
		private bool _hasHitObject;

		private idMaterial _atlasMaterial;
		private idMaterial _guiSolid;
		private idMaterial _guiCursorArrow;
		private idMaterial _guiCursorHand;
		private idMaterial _white;

		private Random _random;
		#endregion

		#region Constructor
		public idSWF()
		{
			
		}

		public idSWF(string fileName /*, idSoundWorld * soundWorld_*/)
		{
			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();
			ICVarSystem cvarSystem   = idEngine.Instance.GetService<ICVarSystem>();

			_atlasMaterial  = null;
			_swfScale       = 1.0f;
			_scaleToVirtual = new Vector2(1, 1);

			_random			= new Random();

			_guiSolid       = declManager.FindMaterial("guiSolid");
			_guiCursorArrow = declManager.FindMaterial("ui/assets/guicursor_arrow");
			_guiCursorHand  = declManager.FindMaterial("ui/assets/guicursor_hand");
			_white          = declManager.FindMaterial("_white");

			// TODO:
			/*tooltipButtonImage.Append( keyButtonImages_t( "<JOY1>", "guis/assets/hud/controller/xb360/a", "guis/assets/hud/controller/ps3/cross", 37, 37, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY2>", "guis/assets/hud/controller/xb360/b", "guis/assets/hud/controller/ps3/circle", 37, 37, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY3>", "guis/assets/hud/controller/xb360/x", "guis/assets/hud/controller/ps3/square", 37, 37, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY4>", "guis/assets/hud/controller/xb360/y", "guis/assets/hud/controller/ps3/triangle", 37, 37, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY_TRIGGER2>", "guis/assets/hud/controller/xb360/rt", "guis/assets/hud/controller/ps3/r2", 64, 52, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY_TRIGGER1>", "guis/assets/hud/controller/xb360/lt", "guis/assets/hud/controller/ps3/l2", 64, 52, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY5>", "guis/assets/hud/controller/xb360/lb", "guis/assets/hud/controller/ps3/l1", 52, 32, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY6>", "guis/assets/hud/controller/xb360/rb", "guis/assets/hud/controller/ps3/r1", 52, 32, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<MOUSE1>", "guis/assets/hud/controller/mouse1", "", 64, 52, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<MOUSE2>", "guis/assets/hud/controller/mouse2", "", 64, 52, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<MOUSE3>", "guis/assets/hud/controller/mouse3", "", 64, 52, 0 ) );
	 	
		   for ( int index = 0; index < tooltipButtonImage.Num(); index++ ) {
			   if ( ( tooltipButtonImage[index].xbImage != NULL ) && ( tooltipButtonImage[index].xbImage[0] != '\0' ) ) {
				   declManager->FindMaterial( tooltipButtonImage[index].xbImage );
			   }
			   if ( ( tooltipButtonImage[index].psImage != NULL ) && ( tooltipButtonImage[index].psImage[0] != '\0' ) ) {
				   declManager->FindMaterial( tooltipButtonImage[index].psImage );
			   }
		   }*/

			_useInhibtControl = true;
			_useMouse = true;

			if(_fileName.StartsWith("swf/") == false)
			{
				// if it doesn't already have swf/ in front of it, add it
				_fileName = "swf/" + fileName;
			}
			else
			{
				_fileName = fileName;
			}

			_fileName = _fileName.ToLower();

			if(_fileName.EndsWith(".swf") == false)
			{
				_fileName += ".swf";
			}

			// TODO: timestamp = fileSystem->GetTimestamp( filename );

			// TODO
			/*mainsprite = new (TAG_SWF) idSWFSprite( this );
			mainspriteInstance = NULL;*/

			string binaryFileName = Path.Combine("generated", Path.GetFileNameWithoutExtension(fileName));

			if(cvarSystem.GetBool("swf_loadBinary") == true)
			{
				throw new Exception("HELLO");
				/*ID_TIME_T sourceTime = fileSystem->GetTimestamp(filename);
				if(!LoadBinary(binaryFileName, sourceTime))
				{
					if(LoadSWF(filename))
					{
						WriteBinary(binaryFileName);
					}
				}*/
			}
			else
			{
				idLog.Warning("TODO: LoadSWF( filename );");
			}
 
			_atlasMaterial = declManager.FindMaterial(Path.GetFileNameWithoutExtension(binaryFileName));

			// TODO: 
			/*globals = idSWFScriptObject::Alloc();
			globals->Set( "_global", globals );

			globals->Set( "Object", &scriptFunction_Object );

			mainspriteInstance = spriteInstanceAllocator.Alloc();
			mainspriteInstance->Init( mainsprite, NULL, 0 );

			shortcutKeys = idSWFScriptObject::Alloc();
			scriptFunction_shortcutKeys_clear.Bind( this );
			scriptFunction_shortcutKeys_clear.Call( shortcutKeys, idSWFParmList() );
			globals->Set( "shortcutKeys", shortcutKeys );

			globals->Set( "deactivate", scriptFunction_deactivate.Bind( this ) );
			globals->Set( "inhibitControl", scriptFunction_inhibitControl.Bind( this ) );
			globals->Set( "useInhibit", scriptFunction_useInhibit.Bind( this ) );
			globals->Set( "precacheSound", scriptFunction_precacheSound.Bind( this ) );
			globals->Set( "playSound", scriptFunction_playSound.Bind( this ) );
			globals->Set( "stopSounds",scriptFunction_stopSounds.Bind( this ) );
			globals->Set( "getPlatform", scriptFunction_getPlatform.Bind( this ) );
			globals->Set( "getTruePlatform", scriptFunction_getTruePlatform.Bind( this ) );
			globals->Set( "getLocalString", scriptFunction_getLocalString.Bind( this ) );
			globals->Set( "swapPS3Buttons", scriptFunction_swapPS3Buttons.Bind( this ) );
			globals->Set( "_root", mainspriteInstance->scriptObject );
			globals->Set( "strReplace", scriptFunction_strReplace.Bind( this ) );
			globals->Set( "getCVarInteger", scriptFunction_getCVarInteger.Bind( this ) );
			globals->Set( "setCVarInteger", scriptFunction_setCVarInteger.Bind( this ) );

			globals->Set( "acos", scriptFunction_acos.Bind( this ) );
			globals->Set( "cos", scriptFunction_cos.Bind( this ) );
			globals->Set( "sin", scriptFunction_sin.Bind( this ) );
			globals->Set( "round", scriptFunction_round.Bind( this ) );
			globals->Set( "pow", scriptFunction_pow.Bind( this ) );
			globals->Set( "sqrt", scriptFunction_sqrt.Bind( this ) );
			globals->Set( "abs", scriptFunction_abs.Bind( this ) );
			globals->Set( "rand", scriptFunction_rand.Bind( this ) );
			globals->Set( "floor", scriptFunction_floor.Bind( this ) );
			globals->Set( "ceil", scriptFunction_ceil.Bind( this ) );
			globals->Set( "toUpper", scriptFunction_toUpper.Bind( this ) );

			globals->SetNative( "platform", swfScriptVar_platform.Bind( &scriptFunction_getPlatform ) );
			globals->SetNative( "blackbars", swfScriptVar_blackbars.Bind( this ) );
			globals->SetNative( "cropToHeight", swfScriptVar_crop.Bind( this ) );
			globals->SetNative( "cropToFit", swfScriptVar_crop.Bind( this ) );
			globals->SetNative( "crop", swfScriptVar_crop.Bind( this ) );*/

			// Do this to touch any external references (like sounds)
			// But disable script warnings because many globals won't have been created yet
			int debug = cvarSystem.GetInt("swf_debug");
			cvarSystem.Set("swf_debug", 0);

			/*mainspriteInstance->Run();
			mainspriteInstance->RunActions();
			mainspriteInstance->RunTo( 0 );*/

			cvarSystem.Set("swf_debug", debug);

			if(_mouseX == -1)
			{
				_mouseX = (int) (_frameWidth / 2);
			}

			if(_mouseY == -1)
			{
				_mouseY = (int) (_frameHeight / 2);
			}

			// TODO: soundWorld = soundWorld_;
		}

		// TODO: cleanup
		/*idSWF::~idSWF() {
			spriteInstanceAllocator.Free( mainspriteInstance );
			delete mainsprite;

			for ( int i = 0 ; i < dictionary.Num() ; i++ ) {
				if ( dictionary[i].sprite ) {
					delete dictionary[i].sprite;
					dictionary[i].sprite = NULL;
				}
				if ( dictionary[i].shape ) {
					delete dictionary[i].shape;
					dictionary[i].shape = NULL;
				}
				if ( dictionary[i].font ) {
					delete dictionary[i].font;
					dictionary[i].font = NULL;
				}
				if ( dictionary[i].text ) {
					delete dictionary[i].text;
					dictionary[i].text = NULL;
				}
				if ( dictionary[i].edittext ) {
					delete dictionary[i].edittext;
					dictionary[i].edittext = NULL;
				}
			}
	
			globals->Clear();
			tooltipButtonImage.Clear();
			globals->Release();

			shortcutKeys->Clear();
			shortcutKeys->Release();
		}*/
		#endregion

		#region State
		#region Properties
		public bool IsActive
		{
			get
			{
				return _isActive;
			}
		}
		#endregion
		#endregion
	}
}
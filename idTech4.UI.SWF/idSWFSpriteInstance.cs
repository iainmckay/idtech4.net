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
using System.Collections.Generic;

using idTech4.Renderer;

namespace idTech4.UI.SWF
{
	public class idSWFSpriteInstance
	{
		#region Properties
		public bool ChildrenRunning
		{
			get
			{
				return _childrenRunning;
			}
			protected set
			{
				_childrenRunning = true;
			}
		}

		public idSWFSpriteInstance Parent
		{
			get
			{
				return _parent;
			}
		}
		#endregion

		#region Members
		private bool _isPlaying;
		private bool _isVisible;
		private bool _childrenRunning;
		private bool _firstRun;

		// currentFrame is the frame number currently in the displayList
		// we use 1 based frame numbers because currentFrame = 0 means nothing is in the display list
		// it's also convenient because Flash also uses 1 based frame numbers
		private ushort _currentFrame;
		private ushort _frameCount;
	
		// the sprite this is an instance of
		private idSWFSprite _sprite;

		// sprite instances can be nested
		private idSWFSpriteInstance _parent;

		// depth of this sprite instance in the parent's display list
		private int _depth;

		// if this is set, apply this material when rendering any child shapes
		private int _itemIndex;

		private idMaterial _materialOverride;
		private ushort _materialWidth;
		private ushort materialHeight;

		private float _xOffset;
		private float _yOffset;

		private float _moveToXScale;
		private float _moveToYScale;
		private float _moveToSpeed;

		private int _stereoDepth;
	
		// name of this sprite instance
		private string _name;

		// children display entries
		public List<DisplayEntry> _displayList = new List<DisplayEntry>();
		#endregion

		#region Constructor
		public idSWFSpriteInstance()
		{
			_isPlaying       = true;
			_isVisible       = true;
			_childrenRunning = true;

			_moveToXScale    = 1.0f;
			_moveToYScale    = 1.0f;
			_moveToSpeed     = 1.0f;
		}

		// TODO: cleanup
		/*idSWFSpriteInstance::~idSWFSpriteInstance() {
			if ( parent != NULL ) {
				parent->scriptObject->Set( name, idSWFScriptVar() );
			}
			FreeDisplayList();
			displayList.Clear();
			scriptObject->SetSprite( NULL );
			scriptObject->Clear();
			scriptObject->Release();
			actionScript->Release();
		}*/
		#endregion

		#region Initialization
		public void Initialize(idSWFSprite sprite, idSWFSpriteInstance parent, int depth)
		{
			_sprite = sprite;
			_parent = parent;
			_depth  = depth;

			_frameCount = sprite.FrameCount;

			idLog.Warning("TODO: idSWFSpriteInstance.Initialize");

			/*scriptObject = idSWFScriptObject::Alloc();
			scriptObject->SetPrototype( &spriteInstanceScriptObjectPrototype );
			scriptObject->SetSprite( this );*/

			_firstRun = true;

			/*actionScript = idSWFScriptFunction_Script::Alloc();

			idList<idSWFScriptObject *, TAG_SWF> scope;
			scope.Append( sprite->swf->globals );
			scope.Append( scriptObject );
			actionScript->SetScope( scope );
			actionScript->SetDefaultSprite( this );

			for	(int i = 0; i < sprite->doInitActions.Num(); i++) {
				actionScript->SetData( sprite->doInitActions[i].Ptr(), sprite->doInitActions[i].Length() );
				actionScript->Call( scriptObject, idSWFParmList() );
			}*/

			Play();
		}
		#endregion

		#region Frame
		public void Play()
		{
			for(idSWFSpriteInstance p = _parent; p != null; p = p.Parent)
			{
				p.ChildrenRunning = true;
			}

			_isPlaying = true;
		}

		public bool Run()
		{
			if(_isVisible == false)
			{
				return false;
			}

			if(_childrenRunning == true)
			{
				_childrenRunning = false;
				
				for(int i = 0; i < _displayList.Count; i++)
				{
					if(_displayList[i].SpriteInstance != null)
					{
						_childrenRunning |= _displayList[i].SpriteInstance.Run();
					}
				}
			}

			if(_isPlaying == true)
			{
				if(_currentFrame == _frameCount)
				{
					if(_frameCount > 1)
					{
						ClearDisplayList();
						RunTo(1);
					}
				} 
				else 
				{
					RunTo(_currentFrame + 1);
				}
			}

			return ((_childrenRunning == true) || (_isPlaying == true));
		}

		public bool RunActions()
		{
			if(_isVisible == false)
			{
				idLog.Warning("TODO: _actions.Clear();");
				return false;
			}

			idLog.Warning("TODO: RunActions");

			/*if ( firstRun && scriptObject->HasProperty( "onLoad" ) ) {
				firstRun = false;
				idSWFScriptVar onLoad = scriptObject->Get( "onLoad" );
				onLoad.GetFunction()->Call( scriptObject, idSWFParmList() );
			}

			if ( onEnterFrame.IsFunction() ) {
				onEnterFrame.GetFunction()->Call( scriptObject, idSWFParmList() );
			}

			for ( int i = 0; i < actions.Num(); i++ ) {
				actionScript->SetData( actions[i].data, actions[i].dataLength );
				actionScript->Call( scriptObject, idSWFParmList() );
			}
			actions.SetNum( 0 );

			for ( int i = 0; i < displayList.Num(); i++ ) {
				if ( displayList[i].spriteInstance != NULL ) {
					Prefetch( displayList[i].spriteInstance, 0 );
				}
			}
			for ( int i = 0; i < displayList.Num(); i++ ) {
				if ( displayList[i].spriteInstance != NULL ) {
					displayList[i].spriteInstance->RunActions();
				}
			}*/

			return true;
		}

		public void RunTo(int targetFrame)
		{
			if(targetFrame == _currentFrame)
			{
				return; // otherwise we'll re-run the current frame
			}
			
			if(targetFrame < _currentFrame)
			{
				ClearDisplayList();
			}
			
			if(targetFrame < 1)
			{
				return;
			}

			if(targetFrame > (_sprite.FrameOffsetCount - 1))
			{
				targetFrame = _sprite.FrameOffsetCount - 1;
			}

			//actions.Clear();

			uint firstActionCommand = _sprite.GetFrameOffset(targetFrame - 1);

			for(uint c = _sprite.GetFrameOffset(_currentFrame); c < _sprite.GetFrameOffset(targetFrame); c++)
			{
				idSWFSpriteCommand command = _sprite.GetCommand(c);

				if((command.Tag == idSWFTag.DoAction) && (c < firstActionCommand))
				{
					// Skip DoAction up to the firstActionCommand
					// This is to properly support skipping to a specific frame
					// for example if we're on frame 3 and skipping to frame 10, we want
					// to run all the commands PlaceObject commands for frames 4-10 but
					// only the DoAction commands for frame 10
					continue;
				}

				command.Stream.Seek(0, System.IO.SeekOrigin.Begin);

				switch(command.Tag)
				{
					case idSWFTag.PlaceObject2:
						idLog.Warning("TODO: PlaceObject2(command.Stream);");
						break;

					case idSWFTag.PlaceObject3:
						idLog.Warning("TODO: PlaceObject3(command.Stream);");
						break;

					case idSWFTag.RemoveObject:
						idLog.Warning("TODO: RemoveObject(command.Stream);");
						break;

					case idSWFTag.StartSound:
						idLog.Warning("TODO: StartSound(command.Stream);");
						break;

					case idSWFTag.DoAction:
						idLog.Warning("TODO: DoAction(command.Stream);");
						break;

					default:
						idLog.Warning("TODO: Run Sprite: Unhandled tag {0}", command.Tag);
						break;
				}
			}

			_currentFrame = (ushort) targetFrame;
		}
		#endregion

		#region Misc.
		private void ClearDisplayList()
		{
			for(int i = 0; i < _displayList.Count; i++)
			{
				_displayList[i].SpriteInstance = null;
				// TODO: _displayList[i].TextInstance = null;
			}

			_displayList.Clear();
			_currentFrame = 0;
		}
		#endregion

		#region DisplayEntry
		public class DisplayEntry
		{
			public ushort CharacterID;
			public ushort Depth;
			public ushort ClipDepth;
			public ushort BlendMode;
			public float Ratio;

			public idSWFMatrix Matrix;

			// TODO: swfColorXform_t cxf;

			/// <summary>
			/// If this entry is a sprite, then this will point to the specific instance of that sprite.
			/// </summary>
			public idSWFSpriteInstance SpriteInstance;

			/// <summary>
			/// If this entry is text, then this will point to the specific instance of the text.
			/// </summary>
			// TODO:	class idSWFTextInstance * textInstance;
		}
		#endregion
	}
}
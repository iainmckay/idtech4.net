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

using Microsoft.Xna.Framework;

using idTech4.Renderer;
using idTech4.UI.SWF.Scripting;

namespace idTech4.UI.SWF
{
	public class idSWFSpriteInstance
	{
		#region Properties
		public float Alpha
		{
			get
			{
				if(_parent == null)
				{
					return 1.0f;
				}

				idSWFDisplayEntry displayEntry = _parent.FindDisplayEntry(_depth);

				if((displayEntry == null) || (displayEntry.SpriteInstance != this))
				{
					idLog.Warning("_alpha: Couldn't find our display entry in our parents display list");
					return 1.0f;
				}

				return displayEntry.ColorXForm.Mul.W;
			}
			set
			{
				if(_parent == null)
				{
					return;
				}

				idSWFDisplayEntry displayEntry = _parent.FindDisplayEntry(_depth);

				if((displayEntry == null) || (displayEntry.SpriteInstance != this))
				{
					idLog.Warning("_alpha: Couldn't find our display entry in our parents display list" );
					return;
				}

				displayEntry.ColorXForm.Mul.W = value;
			}
		}

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

		public ushort CurrentFrame
		{
			get
			{
				return _currentFrame;
			}
		}

		public int Depth
		{
			get
			{
				return _depth;
			}
		}

		public List<idSWFDisplayEntry> DisplayList
		{
			get
			{
				return _displayList;
			}
		}

		public int FrameCount
		{
			get
			{
				return _frameCount;
			}
		}

		public bool IsPlaying
		{
			get
			{
				return _isPlaying;
			}
		}

		public bool IsVisible
		{
			get
			{
				return _isVisible;
			}
			set
			{
				_isVisible = value;

				if(_isVisible == true)
				{
					for(idSWFSpriteInstance p = _parent; p != null; p = p.Parent)
					{
						p.ChildrenRunning = true;
					}
				}
			}
		}

		public int ItemIndex
		{
			get
			{
				return _itemIndex;
			}
			set
			{
				_itemIndex = value;
			}
		}

		public idMaterial MaterialOverride
		{
			get
			{
				return _materialOverride;
			}
			set
			{
				_materialOverride = value;
			}
		}

		public ushort MaterialWidth
		{
			get
			{
				return _materialWidth;
			}
			set
			{
				_materialWidth = value;
			}
		}

		public ushort MaterialHeight
		{
			get
			{
				return _materialHeight;
			}
			set
			{
				_materialHeight = value;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public float OffsetX
		{
			get
			{
				return _xOffset;
			}
			set
			{
				_xOffset = value;
			}
		}

		public float OffsetY
		{
			get
			{
				return _yOffset;
			}
			set
			{
				_yOffset = value;
			}
		}

		public idSWFScriptVariable OnEnterFrame
		{
			get
			{
				return _onEnterFrame;
			}
			set
			{
				_onEnterFrame = value;
			}
		}

		public float PositionX
		{
			get
			{
				if(_parent == null)
				{
					return 0.0f;
				}

				idSWFDisplayEntry displayEntry = _parent.FindDisplayEntry(_depth);

				if((displayEntry == null) || (displayEntry.SpriteInstance != this))
				{
					idLog.Warning("GetXPos: Couldn't find our display entry in our parent's display list for depth {0}", _depth);
					return 0.0f;
				}

				return displayEntry.Matrix.TX;
			}
			set
			{
				if(_parent == null)
				{
					return;
				}

				idSWFDisplayEntry displayEntry = _parent.FindDisplayEntry(_depth);

				if((displayEntry == null) || (displayEntry.SpriteInstance != this))
				{
					idLog.Warning("_x: Couldn't find our display entry in our parents display list" );
					return;
				}

				displayEntry.Matrix.TX = value;
			}
		}

		public float PositionY
		{
			get
			{
				if(_parent == null)
				{
					return 0.0f;
				}

				idSWFDisplayEntry displayEntry = _parent.FindDisplayEntry(_depth);

				if((displayEntry == null) || (displayEntry.SpriteInstance != this))
				{
					idLog.Warning("GetYPos: Couldn't find our display entry in our parents display list for depth {0}", _depth);
					return 0.0f;
				}

				return displayEntry.Matrix.TY;
			}
			set
			{
				if(_parent == null)
				{
					return;
				}

				idSWFDisplayEntry displayEntry = _parent.FindDisplayEntry(_depth);

				if((displayEntry == null) || (displayEntry.SpriteInstance != this))
				{
					idLog.Warning("_y: Couldn't find our display entry in our parents display list" );
					return;
				}

				displayEntry.Matrix.TY = value;
			}
		}

		public idSWFSpriteInstance Parent
		{
			get
			{
				return _parent;
			}
		}

		public idSWFScriptObject ScriptObject
		{
			get
			{
				return _scriptObject;
			}
		}

		public StereoDepthType StereoDepth
		{
			get
			{
				return _stereoDepth;
			}
			set
			{
				_stereoDepth = value;
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

		private idSWFScriptFunction_Script _actionScript;
		private idSWFScriptObject _scriptObject;
		private static idSWFScriptObject_SpriteInstancePrototype _scriptObjectPrototype = new idSWFScriptObject_SpriteInstancePrototype();

		private idSWFScriptVariable _onEnterFrame;

		// depth of this sprite instance in the parent's display list
		private int _depth;

		// if this is set, apply this material when rendering any child shapes
		private int _itemIndex;

		private idMaterial _materialOverride;
		private ushort _materialWidth;
		private ushort _materialHeight;

		private float _xOffset;
		private float _yOffset;

		private float _moveToXScale;
		private float _moveToYScale;
		private float _moveToSpeed;

		private StereoDepthType _stereoDepth;
	
		// name of this sprite instance
		private string _name;

		// children display entries
		private List<idSWFDisplayEntry> _displayList = new List<idSWFDisplayEntry>();
		private List<byte[]> _actions                = new List<byte[]>();
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

			_name            = string.Empty;
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

			_frameCount   = sprite.FrameCount;			
			_firstRun     = true;
			_scriptObject = new idSWFScriptObject(this, _scriptObjectPrototype);

			List<idSWFScriptObject> scope = new List<idSWFScriptObject>();
			scope.Add(_sprite.Owner.Globals);
			scope.Add(_scriptObject);
						
			_actionScript = new idSWFScriptFunction_Script(scope, this);
			
			for(int i = 0; i < _sprite.DoInitActions.Length; i++)
			{
				_actionScript.Data = _sprite.DoInitActions[i];
				_actionScript.Invoke(_scriptObject, new idSWFParameterList());
			}

			Play();
		}
		#endregion

		#region Frame
		public int FindFrame(string labelName)
		{
			int frameNum;
			int.TryParse(labelName, out frameNum);

			if(frameNum > 0)
			{
				return frameNum;
			}

			for(int i = 0; i < _sprite.FrameLabels.Length; i++)
			{
				if(_sprite.FrameLabels[i].Label.Equals(labelName, StringComparison.OrdinalIgnoreCase) == true)
				{
					return _sprite.FrameLabels[i].FrameNumber;
				}
			}

			idLog.Warning("Could not find frame '{0}' in sprite '{1}'", labelName, this.Name);

			return _currentFrame;
		}

		public bool FrameExists(string label)
		{
			int frameNum;

			if(int.TryParse(label, out frameNum) == true)
			{
				return (frameNum <= _sprite.FrameCount);
			}

			for(int i = 0; i < _sprite.FrameLabels.Length; i++) 
			{
				if(_sprite.FrameLabels[i].Label.Equals(label, StringComparison.OrdinalIgnoreCase) == true)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Checks if the current frame is between the given inclusive range.
		/// </summary>
		/// <param name="frame1"></param>
		/// <param name="frame2"></param>
		/// <returns></returns>
		public bool IsBetweenFrames(string frame1, string frame2)
		{
			return ((_currentFrame >= FindFrame(frame1)) && (_currentFrame <= FindFrame(frame2)));
		}

		public void NextFrame()
		{
			if(_currentFrame < _frameCount)
			{
				RunTo(_currentFrame + 1);
			}
		}

		public void PreviousFrame()
		{
			if(_currentFrame > 1)
			{
				RunTo(_currentFrame - 1);
			}
		}

		public void Play()
		{
			for(idSWFSpriteInstance p = _parent; p != null; p = p.Parent)
			{
				p.ChildrenRunning = true;
			}

			_isPlaying = true;
		}

		public void PlayFrame(idSWFParameterList parms)
		{
			if(parms.Count > 0)
			{
				_actions.Clear();

				RunTo((int) FindFrame(parms[0].ToString()));
				Play();
			}
			else
			{
				idLog.Warning("gotoAndPlay: expected 1 parameter");
			}
		}

		public void PlayFrame(string name)
		{
			idSWFParameterList parms = new idSWFParameterList(1);
			parms[0].Set(name);

			PlayFrame(parms);
		}

		public void PlayFrame(int num)
		{
			idSWFParameterList parms = new idSWFParameterList(1);
			parms[0].Set(num);

			PlayFrame(parms);
		}

		public void StopFrame(idSWFParameterList parameters)
		{
			if(parameters.Count > 0)
			{
				if((parameters[0].IsNumeric == true) && (parameters[0].ToInt32() < 1))
				{
					RunTo(FindFrame("1"));
				}
				else
				{
					RunTo(FindFrame(parameters[0].ToString()));
				}
		
				Stop();
	
			} 
			else 
			{
				idLog.Warning("gotoAndStop: expected 1 paramater");
			}
		}

		public void StopFrame(string name)
		{
			idSWFParameterList parameters = new idSWFParameterList(1);
			parameters[0].Set(name);

			StopFrame(parameters);
		}

		public void StopFrame(int index)
		{
			idSWFParameterList parameters = new idSWFParameterList(1);
			parameters[0].Set(index);

			StopFrame(parameters);
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
				_actions.Clear();
				return false;
			}

			if((_firstRun == true) && (_scriptObject.HasProperty("onLoad") == true))
			{
				_firstRun = false;

				idSWFScriptVariable onLoad = _scriptObject.Get("onLoad");
				onLoad.Function.Invoke(_scriptObject, new idSWFParameterList());
			}

			if((_onEnterFrame != null) && (_onEnterFrame.IsFunction == true))
			{
				_onEnterFrame.Function.Invoke(_scriptObject, new idSWFParameterList());
			}

			for(int i = 0; i < _actions.Count; i++)
			{
				_actionScript.Data = _actions[i];
				_actionScript.Invoke(_scriptObject, new idSWFParameterList());
			}

			_actions.Clear();

			for(int i = 0; i < _displayList.Count; i++)
			{
				if(_displayList[i].SpriteInstance != null)
				{
					_displayList[i].SpriteInstance.RunActions();
				}
			}

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

				command.Stream.Rewind();

				switch(command.Tag)
				{
					case idSWFTag.PlaceObject2:
						Tag_PlaceObject2(command.Stream);
						break;

					case idSWFTag.PlaceObject3:
						Tag_PlaceObject3(command.Stream);
						break;

					case idSWFTag.RemoveObject2:
						Tag_RemoveObject2(command.Stream);
						break;

					case idSWFTag.StartSound:
						idLog.Warning("TODO: StartSound(command.Stream);");
						break;

					case idSWFTag.DoAction:
						Tag_DoAction(command.Stream);
						break;

					default:
						idLog.Warning("TODO: Run Sprite: Unhandled tag {0}", command.Tag);
						break;
				}
			}

			_currentFrame = (ushort) targetFrame;
		}

		public void Stop()
		{
			_isPlaying = false;
		}
		#endregion

		#region Misc.
		public idSWFDisplayEntry AddDisplayEntry(int depth, int characterID)
		{
			int i = 0;

			for(; i < _displayList.Count; i++)
			{
				if(_displayList[i].Depth == depth)
				{
					return null;
				}
				if(_displayList[i].Depth > depth)
				{
					break;
				}
			}

			idSWFDisplayEntry display = new idSWFDisplayEntry();
			display.Depth             = (ushort) depth;
			display.CharacterID       = (ushort) characterID;

			_displayList.Insert(i, display);

			idSWFDictionaryEntry dictEntry = _sprite.Owner.FindDictionaryEntry(characterID);

			if(dictEntry != null)
			{
				if(dictEntry is idSWFSprite)
				{
					display.SpriteInstance = new idSWFSpriteInstance();
					display.SpriteInstance.Initialize((idSWFSprite) dictEntry, this, depth);
					display.SpriteInstance.RunTo(1);
				}
				else if(dictEntry is idSWFEditText)
				{
					display.TextInstance = new idSWFTextInstance();
					display.TextInstance.Initialize((idSWFEditText) dictEntry, _sprite.Owner);
				}
			}

			return display;
		}

		public void ClearActions()
		{
			_actions.Clear();
		}

		public idSWFDisplayEntry FindDisplayEntry(int depth)
		{
			int length = _displayList.Count;
			int mid = length;
			int offset = 0;

			while(mid > 0)
			{
				mid = length >> 1;

				if(_displayList[offset + mid].Depth <= depth)
				{
					offset += mid;
				}

				length -= mid;
			}

			if(_displayList[offset].Depth == depth)
			{
				return _displayList[offset];
			}

			return null;
		}

		public void RemoveDisplayEntry(int depth) 
		{
			idSWFDisplayEntry entry = FindDisplayEntry(depth);

			if(entry != null)
			{
				entry.SpriteInstance = null;
				entry.TextInstance   = null;

				_displayList.RemoveAt(_displayList.IndexOf(entry));
			}
		}

		public void ClearDisplayList()
		{
			for(int i = 0; i < _displayList.Count; i++)
			{
				_displayList[i].SpriteInstance = null;
				_displayList[i].TextInstance   = null;
			}

			_displayList.Clear();
			_currentFrame = 0;
		}

		public void SetAlignment(float x, float y)
		{
			_xOffset = x;
			_yOffset = y;
		}

		public void SetScale(float x, float y)
		{
			if(_parent == null)
			{
				return;
			}

			idSWFDisplayEntry displayEntry = _parent.FindDisplayEntry(_depth);

			if((displayEntry == null) || (displayEntry.SpriteInstance != this))
			{
				idLog.Warning("scale: Couldn't find our display entry in our parents display list");
				return;
			}

			float newScale = x / 100.0f;

			// this is done funky to maintain the current rotation
			Vector2 currentScale = displayEntry.Matrix.Scale(new Vector2(1, 0));
			currentScale.Normalize();

			if(currentScale.Length() == 0.0f)
			{
				displayEntry.Matrix.XX = newScale;
				displayEntry.Matrix.YX = 0.0f;
			}
			else
			{
				displayEntry.Matrix.XX = currentScale.X * newScale;
				displayEntry.Matrix.YX = currentScale.Y * newScale;
			}
		
			newScale = y / 100.0f;

			// this is done funky to maintain the current rotation
			currentScale = displayEntry.Matrix.Scale(new Vector2(0, 1));
			currentScale.Normalize();

			if(currentScale.Length() == 0.0f)
			{
				displayEntry.Matrix.YY = newScale;
				displayEntry.Matrix.XY = 0.0f;
			}
			else
			{
				displayEntry.Matrix.YY = currentScale.Y * newScale;
				displayEntry.Matrix.XY = currentScale.X * newScale;
			}
		}

		public void SetMaterial(idMaterial material, int width = -1, int height = -1)
		{
			_materialOverride = material;

			if(_materialOverride != null)
			{
				// converting this to a short should be safe since we don't support images larger than 8k anyway
				if((_materialOverride.GetStage(0) != null) && (_materialOverride.GetStage(0).Texture.Cinematic != null))
				{
					_materialWidth  = 256;
					_materialHeight = 256;
				} 
				else 
				{
					Debug.Assert((_materialOverride.ImageWidth > 0) && (_materialOverride.ImageHeight > 0));
					Debug.Assert((_materialOverride.ImageWidth <= 8192) && (_materialOverride.ImageHeight <= 8192));

					_materialWidth  = (ushort) _materialOverride.ImageWidth;
					_materialHeight = (ushort) _materialOverride.ImageHeight;
				}
			} 
			else
			{
				_materialWidth  = 0;
				_materialHeight = 0;
			}

			if(width >= 0) 
			{
				_materialWidth = (ushort) width;
			}

			if(height >= 0) 
			{
				_materialHeight = (ushort) height;
			}
		}
		#endregion

		#region Tag handling
		private void Tag_PlaceObject2(idSWFBitStream stream)
		{
			// TODO: c_PlaceObject2++;

			PlaceObjectFlags flags = (PlaceObjectFlags) stream.ReadByte();
			int depth              = stream.ReadUInt16();
			int characterID        = -1;

			if((flags & PlaceObjectFlags.HasCharacter) != 0)
			{
				characterID = stream.ReadUInt16();
			}

			idSWFDisplayEntry display = null;

			if((flags & PlaceObjectFlags.Move) != 0)
			{
				// modify an existing entry
				display = FindDisplayEntry(depth);

				if(display == null)
				{
					idLog.Warning("PlaceObject2: Trying to modify entry {0}, which doesn't exist", depth);
					return;
				}

				if(characterID >= 0)
				{
					// We are very picky about what kind of objects can change characters
					// Shapes can become other shapes, but sprites can never change
					if((display.SpriteInstance != null) || (display.TextInstance != null))
					{
						idLog.Warning("PlaceObject2: Trying to change the character of a sprite after it's been created");
						return;
					}

					idSWFDictionaryEntry dictEntry = _sprite.Owner.FindDictionaryEntry(characterID);

					if(dictEntry != null)
					{
						if((dictEntry is idSWFSprite) || (dictEntry is idSWFEditText))
						{
							idLog.Warning("PlaceObject2: Trying to change the character of a shape to a sprite");
							return;
						}
					}

					display.CharacterID = (ushort) characterID;
				}
			}
			else
			{
				if(characterID < 0)
				{
					idLog.Warning("PlaceObject2: Trying to create a new object without a character");
					return;
				}

				// create a new entry
				display = AddDisplayEntry(depth, characterID);

				if(display == null)
				{
					idLog.Warning("PlaceObject2: Trying to create a new entry at {0}, but an item already exists there", depth);
					return;
				}
			}

			if((flags & PlaceObjectFlags.HasMatrix) != 0)
			{
				display.Matrix = stream.ReadMatrix();
			}

			if((flags & PlaceObjectFlags.HasColorTransform) != 0)
			{
				display.ColorXForm = stream.ReadColorXFormRGBA();
			}

			if((flags & PlaceObjectFlags.HasRatio) != 0)
			{
				display.Ratio = (stream.ReadUInt16() * (1.0f / 65535.0f));
			}

			if((flags & PlaceObjectFlags.HasName) != 0)
			{
				string name = stream.ReadString();

				if(display.SpriteInstance != null)
				{
					display.SpriteInstance.Name = name;

					_scriptObject.Set(name, display.SpriteInstance.ScriptObject);
				} 
				else if(display.TextInstance != null)
				{
					_scriptObject.Set(name, display.TextInstance.ScriptObject);
				}
			}

			if((flags & PlaceObjectFlags.HasClipDepth) != 0)
			{
				display.ClipDepth = stream.ReadUInt16();
			}

			if((flags & PlaceObjectFlags.HasClipActions) != 0)
			{
				// FIXME: clip actions
			}
		}

		private void Tag_PlaceObject3(idSWFBitStream stream)
		{
			// TODO: c_PlaceObject3++;

			PlaceObjectFlags flags1  = (PlaceObjectFlags) stream.ReadByte();
			PlaceObjectFlags2 flags2 = (PlaceObjectFlags2) stream.ReadByte();
			ushort depth             = stream.ReadUInt16();

			if(((flags2 & PlaceObjectFlags2.HasClassName) != 0) || (((flags2 & PlaceObjectFlags2.HasImage) != 0) && ((flags1 & PlaceObjectFlags.HasCharacter) != 0)))
			{
				stream.ReadString(); // ignored
			}

			int characterID = -1;

			if((flags1 & PlaceObjectFlags.HasCharacter) != 0)
			{
				characterID = stream.ReadUInt16();
			}

			idSWFDisplayEntry display = null;

			if((flags1 & PlaceObjectFlags.Move) != 0)
			{
				// modify an existing entry
				display = FindDisplayEntry(depth);

				if(display == null)
				{
					idLog.Warning("PlaceObject3: Trying to modify entry {0}, which doesn't exist", depth);
					return;
				}

				if(characterID >= 0)
				{
					// We are very picky about what kind of objects can change characters
					// Shapes can become other shapes, but sprites can never change
					if((display.SpriteInstance != null) || (display.TextInstance != null))
					{
						idLog.Warning("PlaceObject3: Trying to change the character of a sprite after it's been created");
						return;
					}

					idSWFDictionaryEntry dictEntry = _sprite.Owner.FindDictionaryEntry(characterID);

					if(dictEntry != null)
					{
						if((dictEntry is idSWFSprite) || (dictEntry is idSWFEditText))
						{
							idLog.Warning("PlaceObject3: Trying to change the character of a shape to a sprite");
							return;
						}
					}

					display.CharacterID = (ushort) characterID;
				}
			} 
			else 
			{
				if(characterID < 0)
				{
					idLog.Warning("PlaceObject3: Trying to create a new object without a character");
					return;
				}


				// create a new entry
				display = AddDisplayEntry(depth, characterID);

				if(display == null)
				{
					idLog.Warning("PlaceObject3: Trying to create a new entry at {0}, but an item already exists there", depth);
					return;
				}
			}

			if((flags1 & PlaceObjectFlags.HasMatrix) != 0)
			{
				display.Matrix = stream.ReadMatrix();
			}

			if((flags1 & PlaceObjectFlags.HasColorTransform) != 0)
			{
				display.ColorXForm = stream.ReadColorXFormRGBA();
			}

			if((flags1 & PlaceObjectFlags.HasRatio) != 0)
			{
				display.Ratio = (stream.ReadUInt16() * (1.0f / 65535.0f));
			}

			if((flags1 & PlaceObjectFlags.HasName) != 0)
			{
				string name = stream.ReadString();

				if(display.SpriteInstance != null)
				{
					display.SpriteInstance.Name = name;

					_scriptObject.Set(name, display.SpriteInstance.ScriptObject);
				}
				else if(display.TextInstance != null) 
				{
					_scriptObject.Set(name, display.TextInstance.ScriptObject);
				}
			}

			if((flags1 & PlaceObjectFlags.HasClipDepth) != 0)
			{
				display.ClipDepth = stream.ReadUInt16();
			}

			if((flags2 & PlaceObjectFlags2.HasFilterList) != 0)
			{
				// we don't support filters and because the filter list is variable length we
				// can't support anything after the filter list either (blend modes and clip actions)
				idLog.Warning("PlaceObject3: has filters");
				return;
			}

			if((flags2 & PlaceObjectFlags2.HasBlendMode) != 0)
			{
				display.BlendMode = stream.ReadByte();
			}

			if((flags1 & PlaceObjectFlags.HasClipActions) != 0)
			{
				// FIXME:
			}
		}

		private void Tag_DoAction(idSWFBitStream bitStream)
		{
			_actions.Add(bitStream.ReadData(bitStream.Length));
		}

		private void Tag_RemoveObject2(idSWFBitStream bitStream)
		{
			RemoveDisplayEntry(bitStream.ReadUInt16());
		}
		#endregion

		#region PlaceObjectFlags
		private enum PlaceObjectFlags : ulong
		{
			HasClipActions    = 1ul << 7,
			HasClipDepth      = 1ul << 6,
			HasName           = 1ul << 5,
			HasRatio          = 1ul << 4,
			HasColorTransform = 1ul << 3,
			HasMatrix         = 1ul << 2,
			HasCharacter      = 1ul << 1,
			Move              = 1ul << 0
		}
		
		private enum PlaceObjectFlags2 : ulong
		{
			Pad0          = 1ul << 7,
			Pad1          = 1ul << 6,
			Pad2          = 1ul << 5,
			HasImage      = 1ul << 4,
			HasClassName  = 1ul << 3,
			CacheAsBitmap = 1ul << 2,
			HasBlendMode  = 1ul << 1,
			HasFilterList = 1ul << 0
		}
		#endregion
	}

	public class idSWFDisplayEntry
	{
		public ushort CharacterID;
		public ushort Depth;
		public ushort ClipDepth;
		public ushort BlendMode;
		public float Ratio;

		public idSWFMatrix Matrix;
		public idSWFColorXForm ColorXForm = idSWFColorXForm.Default;

		/// <summary>
		/// If this entry is a sprite, then this will point to the specific instance of that sprite.
		/// </summary>
		public idSWFSpriteInstance SpriteInstance;

		/// <summary>
		/// If this entry is text, then this will point to the specific instance of the text.
		/// </summary>
		public idSWFTextInstance TextInstance;
	}
}
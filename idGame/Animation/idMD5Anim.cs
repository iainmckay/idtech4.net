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

using idTech4.Geometry;
using idTech4.Math;
using idTech4.Renderer;
using idTech4.Text;

namespace idTech4.Game.Animation
{
	public class idMD5Anim
	{
		#region Properties
		public int FrameCount
		{
			get
			{
				if(_bounds != null)
				{
					return _bounds.Length;
				}

				return 0;
			}
		}

		public int FrameRate
		{
			get
			{
				return _frameRate;
			}
		}

		public int Length
		{
			get
			{
				return _animLength;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}
		#endregion

		#region Members
		private int _frameRate;
		private int _animLength;
		private int _animatedComponentCount;
		private string _name;

		private JointAnimationInfo[] _jointInfo;
		private idBounds[] _bounds;
		private idJointQuaternion[] _baseFrame;
		private float[] _componentFrames;

		private Vector3 _totalDelta;
		#endregion

		#region Constructor
		public idMD5Anim()
		{
			Clear();
		}
		#endregion

		#region Methods
		#region Public
		public void CheckModelHierarchy(idRenderModel model)
		{
			if(_jointInfo.Length != model.JointCount)
			{
				idConsole.Error("Model '{0}' has different # of joints than anim '{1}'", model.Name, this.Name);
			}

			idMD5Joint[] modelJoints = model.Joints;
			int parent = -1;
			int jointCount = _jointInfo.Length;
			int modelJointCount = modelJoints.Length;

			for(int i = 0; i < jointCount; i++)
			{
				int jointIndex = _jointInfo[i].NameIndex;

				if(modelJoints[i].Name != idR.AnimManager.GetJointName(jointIndex))
				{
					idConsole.Error("Model '{0}''s joint names don't match anim '{1}''s", model.Name, this.Name);
				}
				else if(modelJoints[i].Parent != null)
				{
					for(int j = 0; j < modelJointCount; j++)
					{
						if(modelJoints[j] == modelJoints[i].Parent)
						{
							parent = j;
							break;
						}
					}
				}
				else
				{
					parent = -1;
				}

				if(parent != _jointInfo[i].ParentIndex)
				{
					idConsole.Error("Model '{0}' has different joint hierarchy than anim '{1}'", model.Name, this.Name);
				}
			}
		}

		public idBounds GetBounds(int time, int cycleCount)
		{
			FrameBlend frame = ConvertTimeToFrame(time, cycleCount);
			
			idBounds bounds = _bounds[frame.Frame1];
			bounds.AddBounds(_bounds[frame.Frame2]);

			// origin position
			Vector3 offset = _baseFrame[0].Translation;

			if((_jointInfo[0].AnimationBits & (AnimationBits.TranslationX | AnimationBits.TranslationY | AnimationBits.TranslationZ)) != 0)
			{
				int componentOffset1 = _animatedComponentCount * frame.Frame1 + _jointInfo[0].FirstComponent;
				int componentOffset2 = _animatedComponentCount * frame.Frame2 + _jointInfo[0].FirstComponent;

				if((_jointInfo[0].AnimationBits & AnimationBits.TranslationX) == AnimationBits.TranslationX)
				{
					offset.X = _componentFrames[componentOffset1] * frame.FrontLerp + _componentFrames[componentOffset2] * frame.BackLerp;
					componentOffset1++;
					componentOffset2++;
				}

				if((_jointInfo[0].AnimationBits & AnimationBits.TranslationY) == AnimationBits.TranslationY)
				{
					offset.Y = _componentFrames[componentOffset1] * frame.FrontLerp + _componentFrames[componentOffset2] * frame.BackLerp;
					componentOffset1++;
					componentOffset2++;
				}

				if((_jointInfo[0].AnimationBits & AnimationBits.TranslationZ) == AnimationBits.TranslationZ)
				{
					offset.Z = _componentFrames[componentOffset1] * frame.FrontLerp + _componentFrames[componentOffset2] * frame.BackLerp;
				}
			}

			bounds.Min -= offset;
			bounds.Max -= offset;

			return bounds;
		}

		public Vector3 GetOrigin(int time, int cycleCount)
		{
			Vector3 offset = _baseFrame[0].Translation;

			if((_jointInfo[0].AnimationBits & (AnimationBits.TranslationX | AnimationBits.TranslationY | AnimationBits.TranslationZ)) == 0)
			{
				// just use the baseframe		
				return Vector3.Zero;
			}

			FrameBlend frame = ConvertTimeToFrame(time, cycleCount);

			int componentOffset1 = _animatedComponentCount * frame.Frame1 + _jointInfo[0].FirstComponent;
			int componentOffset2 = _animatedComponentCount * frame.Frame2 + _jointInfo[0].FirstComponent;

			if((_jointInfo[0].AnimationBits & AnimationBits.TranslationX) == AnimationBits.TranslationX)
			{
				offset.X = _componentFrames[componentOffset1] * frame.FrontLerp + _componentFrames[componentOffset2] * frame.BackLerp;
				componentOffset1++;
				componentOffset2++;
			}

			if((_jointInfo[0].AnimationBits & AnimationBits.TranslationY) == AnimationBits.TranslationY)
			{
				offset.Y = _componentFrames[componentOffset1] * frame.FrontLerp + _componentFrames[componentOffset2] * frame.BackLerp;
				componentOffset1++;
				componentOffset2++;
			}

			if((_jointInfo[0].AnimationBits & AnimationBits.TranslationZ) == AnimationBits.TranslationZ)
			{
				offset.Z = _componentFrames[componentOffset1] * frame.FrontLerp + _componentFrames[componentOffset2] * frame.BackLerp;
			}

			if(frame.CycleCount > 0)
			{
				offset += _totalDelta * (float) frame.CycleCount;
			}

			return offset;
		}

		public bool LoadAnimation(string fileName)
		{
			idToken token;
			idLexer lexer = new idLexer(LexerOptions.AllowPathNames | LexerOptions.NoStringEscapeCharacters | LexerOptions.NoStringConcatination);

			if(lexer.LoadFile(fileName) == false)
			{
				return false;
			}

			Clear();

			_name = fileName;

			lexer.ExpectTokenString(idRenderModel_MD5.VersionString);
			int version = lexer.ParseInt();

			if(version != idRenderModel_MD5.Version)
			{
				lexer.Error("Invalid version {0}.  Should be version {1}", version, idRenderModel_MD5.Version);
			}

			// skip the commandline
			lexer.ExpectTokenString("commandline");
			lexer.ReadToken();

			// parse num frames
			lexer.ExpectTokenString("numFrames");
			int frameCount = lexer.ParseInt();

			if(frameCount <= 0)
			{
				lexer.Error("Invalid number of frames: {0}", frameCount);
			}

			// parse num joints
			lexer.ExpectTokenString("numJoints");
			int jointCount = lexer.ParseInt();

			if(jointCount <= 0)
			{
				lexer.Error("Invalid number of joints: {0}", jointCount);
			}

			// parse frame rate
			lexer.ExpectTokenString("frameRate");
			_frameRate = lexer.ParseInt();

			if(_frameRate < 0)
			{
				lexer.Error("Invalid frame rate: {0}", _frameRate);
			}

			// parse number of animated components
			lexer.ExpectTokenString("numAnimatedComponents");
			_animatedComponentCount = lexer.ParseInt();

			if((_animatedComponentCount < 0) || (_animatedComponentCount > (jointCount * 6)))
			{
				lexer.Error("Invalid number of animated components: {0}", _animatedComponentCount);
			}

			// parse the hierarchy
			_jointInfo = new JointAnimationInfo[jointCount];

			lexer.ExpectTokenString("hierarchy");
			lexer.ExpectTokenString("{");

			for(int i = 0; i < jointCount; i++)
			{
				token = lexer.ReadToken();

				_jointInfo[i] = new JointAnimationInfo();
				_jointInfo[i].NameIndex = idR.AnimManager.GetJointIndex(token.ToString());

				// parse parent num				
				_jointInfo[i].ParentIndex = lexer.ParseInt();

				if(_jointInfo[i].ParentIndex >= i)
				{
					lexer.Error("Invalid parent num: {0}", _jointInfo[i].ParentIndex);
				}

				if((i != 0) && (_jointInfo[i].ParentIndex < 0))
				{
					lexer.Error("Animations may have only one root joint");
				}

				// parse anim bits
				_jointInfo[i].AnimationBits = (AnimationBits) lexer.ParseInt();

				if(((int) _jointInfo[i].AnimationBits & ~63) != 0)
				{
					lexer.Error("Invalid anim bits: {0}", _jointInfo[i].AnimationBits);
				}

				// parse first component
				_jointInfo[i].FirstComponent = lexer.ParseInt();

				if((_animatedComponentCount > 0) && ((_jointInfo[i].FirstComponent < 0) || (_jointInfo[i].FirstComponent >= _animatedComponentCount)))
				{
					lexer.Error("Invalid first component: {0}", _jointInfo[i].FirstComponent);
				}
			}

			lexer.ExpectTokenString("}");

			// parse bounds
			lexer.ExpectTokenString("bounds");
			lexer.ExpectTokenString("{");

			_bounds = new idBounds[frameCount];

			for(int i = 0; i < frameCount; i++)
			{
				float[] tmp = lexer.Parse1DMatrix(3);
				float[] tmp2 = lexer.Parse1DMatrix(3);

				_bounds[i] = new idBounds(
					new Vector3(tmp[0], tmp[1], tmp[2]),
					new Vector3(tmp2[0], tmp2[1], tmp2[2])
				);
			}

			lexer.ExpectTokenString("}");

			// parse base frame
			_baseFrame = new idJointQuaternion[jointCount];

			lexer.ExpectTokenString("baseframe");
			lexer.ExpectTokenString("{");

			for(int i = 0; i < jointCount; i++)
			{
				float[] tmp = lexer.Parse1DMatrix(3);
				float[] tmp2 = lexer.Parse1DMatrix(3);

				idCompressedQuaternion q = new idCompressedQuaternion(tmp2[0], tmp2[1], tmp2[2]);
				

				_baseFrame[i] = new idJointQuaternion();
				_baseFrame[i].Translation = new Vector3(tmp[0], tmp[1], tmp[2]);
				_baseFrame[i].Quaternion = q.ToQuaternion();
			}

			lexer.ExpectTokenString("}");

			// parse frames
			_componentFrames = new float[_animatedComponentCount * frameCount];
			int frameOffset = 0;

			for(int i = 0; i < frameCount; i++)
			{
				lexer.ExpectTokenString("frame");
				int count = lexer.ParseInt();

				if(count != i)
				{
					lexer.Error("Expected frame number {0}", i);
				}

				lexer.ExpectTokenString("{");

				for(int j = 0; j < _animatedComponentCount; j++, frameOffset++)
				{
					_componentFrames[frameOffset] = lexer.ParseFloat();
				}

				lexer.ExpectTokenString("}");
			}

			// get total move delta
			if(_animatedComponentCount == 0)
			{
				_totalDelta = Vector3.Zero;
			}
			else
			{
				int componentOffset = _jointInfo[0].FirstComponent;

				if((_jointInfo[0].AnimationBits & AnimationBits.TranslationX) == AnimationBits.TranslationX)
				{
					for(int i = 0; i < frameCount; i++)
					{
						_componentFrames[componentOffset + (_animatedComponentCount * i)] -= _baseFrame[0].Translation.X;
					}

					_totalDelta.X = _componentFrames[componentOffset + (_animatedComponentCount * (frameCount - 1))];
					componentOffset++;
				}
				else
				{
					_totalDelta.X = 0;
				}

				if((_jointInfo[0].AnimationBits & AnimationBits.TranslationY) == AnimationBits.TranslationY)
				{
					for(int i = 0; i < frameCount; i++)
					{
						_componentFrames[componentOffset + (_animatedComponentCount * i)] -= _baseFrame[0].Translation.Y;
					}

					_totalDelta.Y = _componentFrames[componentOffset + (_animatedComponentCount * (frameCount - 1))];
					componentOffset++;
				}
				else
				{
					_totalDelta.Y = 0;
				}

				if((_jointInfo[0].AnimationBits & AnimationBits.TranslationZ) == AnimationBits.TranslationZ)
				{
					for(int i = 0; i < frameCount; i++)
					{
						_componentFrames[componentOffset + (_animatedComponentCount * i)] -= _baseFrame[0].Translation.Z;
					}

					_totalDelta.Z = _componentFrames[componentOffset + (_animatedComponentCount * (frameCount - 1))];
				}
				else
				{
					_totalDelta.Z = 0;
				}
			}

			_baseFrame[0].Translation = Vector3.Zero;

			// we don't count last frame because it would cause a 1 frame pause at the end
			_animLength = ((frameCount - 1) * 1000 + _frameRate - 1) / _frameRate;

			// done
			return true;
		}
		#endregion

		#region Private
		private void Clear()
		{
			_frameRate = 24;
			_animLength = 0;
			_name = string.Empty;

			_totalDelta = Vector3.Zero;

			_jointInfo = null;
			_componentFrames = null;
			_bounds = null;		
		}

		private FrameBlend ConvertTimeToFrame(int time, int cycleCount)
		{
			FrameBlend frame = new FrameBlend();
			int frameCount = this.FrameCount;

			if(frameCount <= 1) 
			{
				frame.Frame1 = 0;
				frame.Frame2 = 0;
				frame.BackLerp = 0.0f;
				frame.FrontLerp	= 1.0f;
				frame.CycleCount = 0;
			}
			else if(time <= 0)
			{
				frame.Frame1 = 0;
				frame.Frame2 = 1;
				frame.BackLerp = 0.0f;
				frame.FrontLerp = 1.0f;
				frame.CycleCount = 0;
			}
			else
			{
				int frameTime = time * _frameRate;
				int frameNumber = frameTime / 1000;

				frame.CycleCount = frameNumber / (frameCount - 1);

				if((cycleCount > 0) && (frame.CycleCount >= cycleCount))
				{
					frame.CycleCount = cycleCount - 1;
					frame.Frame1 = frameCount - 1;
					frame.Frame2 = frame.Frame1;
					frame.BackLerp = 0.0f;
					frame.FrontLerp = 1.0f;
				}
				else
				{
					frame.Frame1 = frameNumber % (frameCount - 1);
					frame.Frame2 = frame.Frame1 + 1;
					if(frame.Frame2 >= frameCount)
					{
						frame.Frame2 = 0;
					}

					frame.BackLerp = (frameTime % 1000) * 0.001f;
					frame.FrontLerp = 1.0f - frame.BackLerp;
				}
			}

			return frame;
		}

		#endregion
		#endregion
	}

	public struct FrameBlend
	{
		public int CycleCount; // how many times the anim has wrapped to the begining (0 for clamped anims)
		public int Frame1;
		public int Frame2;
		public float FrontLerp;
		public float BackLerp;
	}
}
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
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Geometry;
using idTech4.Renderer;
using idTech4.Sound;
using idTech4.Text;
using idTech4.Text.Decl;

namespace idTech4.Game.Animation
{
	public class idDeclModel : idDecl
	{
		#region Constants
		public readonly string[] ChannelNames = {
			"all", "torso", "legs", "head", "eyelids"
		};
		#endregion

		#region Properties
		public idJointQuaternion[] DefaultPose
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _model.DefaultPose;
			}
		}

		public idDeclSkin DefaultSkin
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _skin;
			}
		}

		public idRenderModel Model
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _model;
			}
		}

		public Vector3 VisualOffset
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _offset;
			}
		}
		#endregion

		#region Members
		private idDeclSkin _skin;
		private idRenderModel _model;

		private List<idAnim> _anims = new List<idAnim>();

		private Vector3 _offset;
		private JointInfo[] _joints;
		private int[] _jointParents;
		private int[][] _channelJoints;
		#endregion

		#region Constructor
		public idDeclModel()
			: base()
		{

		}
		#endregion

		#region Methods
		#region Public
		public JointInfo FindJoint(int index)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(_model == null)
			{
				return null;
			}

			return _joints[index];
		}

		public JointInfo FindJoint(string name)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(_model == null)
			{
				return null;
			}

			idMD5Joint[] joints = _model.Joints;
			int count = _joints.Length;
				
			for(int i = 0; i < count; i++)
			{
				if(joints[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true)
				{
					return _joints[i];
				}
			}

			return null;
		}

		public idAnim GetAnimation(int index)
		{
			if((index < 1) || (index > _anims.Count))
			{
				return null;
			}

			return _anims[index - 1];
		}

		public void SetupJoints(idJointMatrix[] joints, ref idBounds frameBounds, bool removeOriginOffset)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if((_model == null) || (_model.IsDefault == true))
			{
				joints = null;
				frameBounds.Clear();
			}

			// get the number of joints
			int count = _model.JointCount;

			if(count == 0)
			{
				idConsole.Error("model '{0}' has no joints", _model.Name);
			}
			
			// set up initial pose for model (with no pose, model is just a jumbled mess)
			joints = new idJointMatrix[count];
			idJointQuaternion[] pose = this.DefaultPose;
			
			// convert the joint quaternions to joint matrices
			idHelper.ConvertJointQuaternionsToJointMatrices(joints, pose);

			// check if we offset the model by the origin joint
			if(removeOriginOffset == true)
			{
#if VELOCITY_MOVE
				joints[0].Translation(new Vector3(_offset.X, _offset.Y + pose[0].Translation.Y, _offset.Z + pose[0].Translation.Z));
#else
				joints[0].Translation = _offset;
#endif
			} 
			else 
			{
				joints[0].Translation = pose[0].Translation + _offset;
			}

			// transform the joint hierarchy
			idHelper.TransformJoints(joints, _jointParents, 1, joints.Length - 1);
			
			// get the bounds of the default pose
			frameBounds = _model.GetBounds(null);
		}

		public void Touch()
		{
			if(_model != null)
			{
				idE.RenderModelManager.FindModel(_model.Name);
			}
		}
		#endregion

		#region Private
		private int[] GetJointList(string jointNames)
		{
			if(_model == null)
			{
				return null;
			}

			List<int> joints = new List<int>();
			JointInfo joint, child;
			int count = _model.JointCount;
			bool subtract = false;
			bool getChildren = false;
			string jointName;
			int nameCount = jointNames.Length;

			for(int i = 0; i < nameCount; i++)
			{
				while((i != jointNames.Length) && (jointNames[i] == ' '))
				{
					i++;
				}

				if(i == jointNames.Length)
				{
					break;
				}

				jointName = string.Empty;


				if(jointNames[i] == '-')
				{
					subtract = true;
					i++;
				}
				else
				{
					subtract = false;
				}

				if(jointNames[i] == '*')
				{
					getChildren = true;
					i++;
				}
				else
				{
					getChildren = false;
				}

				while((i != jointNames.Length) && (jointNames[i] != ' '))
				{
					jointName += jointNames[i];
					i++;
				}

				if((joint = FindJoint(jointName)) == null)
				{
					idConsole.Warning("Unknown joint '{0}' in '{1}' for model '{2}'", jointName, jointNames, this.Name);
					continue;
				}

				if(subtract == false)
				{
					if(joints.Contains(joint.Index) == false)
					{
						joints.Add(joint.Index);
					}
				}
				else
				{
					joints.Remove(joint.Index);
				}

				if(getChildren == true)
				{
					// include all joint's children
					int jointOffset = joint.Index + 1;
					child = FindJoint(jointOffset);

					for(i = joint.Index + 1; i < count; i++, jointOffset++)
					{
						// all children of the joint should follow it in the list.
						// once we reach a joint without a parent or with a parent
						// who is earlier in the list than the specified joint, then
						// we've gone through all it's children.
						if(child.ParentIndex < joint.Index)
						{
							break;
						}

						if(subtract == false)
						{
							if(joints.Contains(child.Index) == false)
							{
								joints.Add(child.Index);
							}
							else
							{
								joints.Remove(child.Index);
							}
						}
					}
				}
			}

			return joints.ToArray();
		}

		private bool ParseAnimation(idLexer lexer, int defaultAnimCount)
		{
			List<idMD5Anim> md5anims = new List<idMD5Anim>();
			idMD5Anim md5anim;
			idAnim anim;
			AnimationFlags flags = new AnimationFlags();

			idToken token;
			idToken realName = lexer.ReadToken();

			if(realName == null)
			{
				lexer.Warning("Unexpected end of file");
				MakeDefault();

				return false;
			}

			string alias = realName.ToString();
			int i;
			int count = _anims.Count;

			for(i = 0; i < count; i++)
			{
				if(_anims[i].FullName.Equals(alias, StringComparison.OrdinalIgnoreCase) == true)
				{
					break;
				}
			}

			if((i < count) && (i >= defaultAnimCount))
			{
				lexer.Warning("Duplicate anim '{0}'", realName);
				MakeDefault();

				return false;
			}

			if(i < defaultAnimCount)
			{
				anim = _anims[i];
			}
			else
			{
				// create the alias associated with this animation
				anim = new idAnim();
				_anims.Add(anim);
			}

			// random anims end with a number.  find the numeric suffix of the animation.
			int len = alias.Length;

			for(i = len - 1; i > 0; i--)
			{
				if(Char.IsNumber(alias[i]) == false)
				{
					break;
				}
			}

			// check for zero length name, or a purely numeric name
			if(i <= 0)
			{
				lexer.Warning("Invalid animation name '{0}'", alias);
				MakeDefault();

				return false;
			}

			// remove the numeric suffix
			alias = alias.Substring(0, i + 1);

			// parse the anims from the string
			do
			{
				if((token = lexer.ReadToken()) == null)
				{
					lexer.Warning("Unexpected end of file");
					MakeDefault();

					return false;
				}

				// lookup the animation
				md5anim = idR.AnimManager.GetAnimation(token.ToString());
				
				if(md5anim == null)
				{
					lexer.Warning("Couldn't load anim '{0}'", token);
					return false;
				}

				md5anim.CheckModelHierarchy(_model);

				if(md5anims.Count > 0)
				{
					// make sure it's the same length as the other anims
					if(md5anim.Length != md5anims[0].Length)
					{
						lexer.Warning("Anim '{0}' does not match length of anim '{1}'", md5anim.Name, md5anims[0].Name);
						MakeDefault();

						return false;
					}
				}

				// add it to our list
				md5anims.Add(md5anim);
			}
			while(lexer.CheckTokenString(",") == true);

			if(md5anims.Count == 0)
			{
				lexer.Warning("No animation specified");
				MakeDefault();

				return false;
			}

			anim.SetAnimation(this, realName.ToString(), alias, md5anims.ToArray());
			
			// parse any frame commands or animflags
			if(lexer.CheckTokenString("{") == true)
			{
				while(true)
				{
					if((token = lexer.ReadToken()) == null)
					{
						lexer.Warning("Unexpected end of file");
						MakeDefault();

						return false;
					}

					string tokenValue = token.ToString();

					if(tokenValue == "}")
					{
						break;
					}
					else if(tokenValue == "prevent_idle_override") 
					{
						flags.PreventIdleOverride = true;
					}
					else if(tokenValue == "random_cycle_start") 
					{
						flags.RandomCycleStart = true;
					}
					else if(tokenValue == "ai_no_turn") 
					{
						flags.AINoTurn = true;
					}
					else if(tokenValue == "anim_turn")
					{
						flags.AnimationTurn = true;
					}
					else if(tokenValue == "frame")
					{
						// create a frame command
						int frameIndex;
						string err;

						// make sure we don't have any line breaks while reading the frame command so the error line # will be correct
						if((token = lexer.ReadTokenOnLine()) == null)
						{
							lexer.Warning("Missing frame # after 'frame'");
							MakeDefault();

							return false;
						}
						else if((token.Type == TokenType.Punctuation) && (token.ToString() == "-"))
						{
							lexer.Warning("Invalid frame # after 'frame'");
							MakeDefault();

							return false;
						}
						else if((token.Type != TokenType.Number) || (token.SubType == TokenSubType.Float))
						{
							lexer.Error("expected integer value, found '{0}'", token);
						}

						// get the frame number
						frameIndex = token.ToInt32();

						// put the command on the specified frame of the animation
						if((err = anim.AddFrameCommand(this, frameIndex, lexer, null)) != null)
						{
							lexer.Warning(err.ToString());
							MakeDefault();

							return false;
						}
					}
					else
					{
						lexer.Warning("Unknown command '{0}'", token);
						MakeDefault();

						return false;
					}
				}
			}

			// set the flags
			anim.Flags = flags;

			return true;
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

				return "{ }";
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

				idConsole.Warning("TODO: idDeclModel.MemoryUsage");
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

			int defaultAnimationCount = 0;
			idToken token;
			idToken token2;
			string tokenValue;
			string fileName;
			string extension;
			int count;
			idMD5Joint[] md5Joints;

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
					if((token2 = lexer.ReadToken()) == null)
					{
						lexer.Warning("Unexpected end of file");
						MakeDefault();

						return false;
					}

					_skin = idE.DeclManager.FindSkin(token2.ToString());

					if(_skin == null)
					{
						lexer.Warning("Skin '{0}' not found", token2.ToString());
						MakeDefault();

						return false;
					}
				} 
				else if(tokenValue == "mesh")
				{
					if((token2 = lexer.ReadToken()) == null)
					{
						lexer.Warning("Unexpected end of file");
						MakeDefault();

						return false;
					}

					fileName = token2.ToString();
					extension = Path.GetExtension(fileName);

					if(extension != idRenderModel_MD5.MeshExtension)
					{
						lexer.Warning("Invalid model for MD5 mesh");
						MakeDefault();

						return false;
					}

					_model = idE.RenderModelManager.FindModel(fileName);

					if(_model == null)
					{
						lexer.Warning("Model '{0}' not found", fileName);
						MakeDefault();

						return false;
					}
					else if(_model.IsDefault == true)
					{
						lexer.Warning("Model '{0}' defaulted", fileName);
						MakeDefault();

						return false;
					}

					// get the number of joints
					count = _model.JointCount;

					if(count == 0)
					{
						lexer.Warning("Model '{0}' has no joints", fileName);
					}

					// set up the joint hierarchy
					md5Joints = _model.Joints;

					_joints = new JointInfo[count];
					_jointParents = new int[count];
					_channelJoints = new int[(int) AnimationChannel.Count][];
					_channelJoints[0] = new int[count];

					for(int i = 0; i < count; i++)
					{
						_joints[i] = new JointInfo();
						_joints[i].Channel = AnimationChannel.All;
						_joints[i].Index = i;

						if(md5Joints[i].Parent != null)
						{
							_joints[i].ParentIndex = _model.GetJointIndex(md5Joints[i].Parent);
						}
						else
						{
							_joints[i].ParentIndex = -1;
						}

						_jointParents[i] = _joints[i].ParentIndex;
						_channelJoints[0][i] = i;
					}
				}
				else if(tokenValue == "remove")
				{
					idConsole.Warning("TODO: remove");

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
					if(_model == null)
					{
						lexer.Warning("Must specify mesh before defining anims");
						MakeDefault();

						return false;
					}
					else if(ParseAnimation(lexer, defaultAnimationCount) == false)
					{
						MakeDefault();

						return false;
					}
				} 
				else if(tokenValue == "offset") 
				{
					float[] tmp = lexer.Parse1DMatrix(3);

					if(tmp == null)
					{
						lexer.Warning("Expected vector following 'offset'");
						MakeDefault();
						return false;
					}

					_offset = new Vector3(tmp[0], tmp[1], tmp[2]);
				} 
				else if(tokenValue == "channel") 
				{
					if(_model == null)
					{
						lexer.Warning("Must specify mesh before defining channels");
						MakeDefault();

						return false;
					}

					// set the channel for a group of joints
					if((token2 = lexer.ReadToken()) == null)
					{
						lexer.Warning("Unexpected end of file");
						MakeDefault();

						return false;
					}

					if(lexer.CheckTokenString("(") == false)
					{
						lexer.Warning("Expected { after '{0}'", token2.ToString());
						MakeDefault();

						return false;
					}

					int i;
					int channelCount = (int) AnimationChannel.Count;

					for(i = (int) AnimationChannel.All + 1; i < channelCount; i++)
					{
						if(ChannelNames[i].Equals(token2.ToString(), StringComparison.OrdinalIgnoreCase) == true)
						{
							break;
						}
					}

					if(i >= channelCount)
					{
						lexer.Warning("Unknown channel '{0}'", token2.ToString());
						MakeDefault();

						return false;
					}

					int channel = i;
					StringBuilder jointNames = new StringBuilder();
					string token2Value;

					while(lexer.CheckTokenString(")") == false)
					{
						if((token2 = lexer.ReadToken()) == null)
						{
							lexer.Warning("Unexpected end of file");
							MakeDefault();

							return false;
						}

						token2Value = token2.ToString();
						jointNames.Append(token2Value);

						if((token2Value != "*") && (token2Value != "-"))
						{
							jointNames.Append(" ");
						}
					}

					int[] jointList = GetJointList(jointNames.ToString());
					int jointLength = jointList.Length;

					List<int> channelJoints = new List<int>();
					
					for(count = i = 0; i < jointLength; i++)
					{
						int jointIndex = jointList[i];

						if(_joints[jointIndex].Channel != AnimationChannel.All)
						{
							lexer.Warning("Join '{0}' assigned to multiple channels", _model.GetJointName(jointIndex));
							continue;
						}

						_joints[jointIndex].Channel = (AnimationChannel) channel;
						channelJoints.Add(jointIndex);
					}

					_channelJoints[channel] = channelJoints.ToArray();
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

			idConsole.Warning("TODO: idDeclModel.ClearData");
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

	public enum AnimationChannel
	{
		All = 0,
		Torso = 1,
		Legs = 2,
		Head = 3,
		EyeLids = 4,
		Count
	}

	public struct AnimationFlags
	{
		public bool PreventIdleOverride;
		public bool RandomCycleStart;
		public bool AINoTurn;
		public bool AnimationTurn;
	}

	public enum AnimationFrameCommandType
	{
		ScriptFunction,
		ScriptFunctionObject,
		EventFunction,
		Sound,
		SoundVoice,
		SoundVoice2,
		SoundBody,
		SoundBody2,
		SoundBody3,
		SoundWeapon,
		SoundItem,
		SoundGlobal,
		SoundChatter,
		Skin,
		Trigger,
		TriggerSmokeParticle,
		Melee,
		DirectDamage,
		BeginAttack,
		EndAttack,
		MuzzleFlash,
		CreateMissile,
		LaunchMissile,
		FireMissileAtTarget,
		Footstep,
		LeftFoot,
		RightFoot,
		EnableEyeFocus,
		DisableEyeFocus,
		Fx,
		DisableGravity,
		EnableGravity,
		Jump,
		EnableClip,
		DisableClip,
		EnableWalkIk,
		DisableWalkIk,
		EnableLegIk,
		DisableLegIk,
		RecordDemo,
		AviGame
	}

	public class AnimationFrameLookup
	{
		public int Index;
		public int FirstCommand;
	}

	public class AnimationFrameCommand
	{
		public AnimationFrameCommandType Type;
		public string String;

		public idSoundMaterial SoundMaterial;
		public object Function;
		public idDeclSkin Skin;
		public int Index;
	}

	[Flags]
	public enum AnimationBits
	{
		TranslationX = 1 << 0,
		TranslationY = 1 << 1,
		TranslationZ = 1 << 2,
		QuaternionX = 1 << 3,
		QuaternionY = 1 << 4,
		QuaternionZ = 1 << 5
	}

	public class JointInfo
	{
		public int Index;
		public int ParentIndex;
		public AnimationChannel Channel;
	}

	public class idAnim
	{
		#region Properties
		public int AnimationCount
		{
			get
			{
				return _anims.Length;
			}
		}

		public AnimationFlags Flags
		{
			get
			{
				return _animFlags;
			}
			set
			{
				_animFlags = value;
			}
		}

		public string FullName
		{
			get
			{
				return _realName;
			}
		}

		public int Length
		{
			get
			{
				if((_anims == null) || (_anims.Length == 0))
				{
					return 0;
				}

				return _anims[0].Length;
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
		private idDeclModel _modelDef;
		private idMD5Anim[] _anims;
		private string _name;
		private string _realName;
		private AnimationFlags _animFlags;

		private List<AnimationFrameLookup> _frameLookups = new List<AnimationFrameLookup>();
		private List<AnimationFrameCommand> _frameCommands = new List<AnimationFrameCommand>();
		#endregion

		#region Constructor
		public idAnim()
		{

		}
		#endregion

		#region Methods
		#region Public
		public string AddFrameCommand(idDeclModel modelDef, int frameIndex, idLexer lexer, idDict def)
		{
			// make sure we're within bounds
			if((frameIndex < 1) || (frameIndex > _anims[0].FrameCount))
			{
				return string.Format("Frame {0} out of range", frameIndex);
			}

			// frame numbers are 1 based in .def files, but 0 based internally
			frameIndex--;

			idToken token;
			AnimationFrameCommand frameCommand = new AnimationFrameCommand();

			if((token = lexer.ReadTokenOnLine()) == null)
			{
				return "Unexpected end of line";
			}

			string tokenValue = token.ToString();

			if(tokenValue == "call")
			{
				if((token = lexer.ReadTokenOnLine()) == null)
				{
					return "Unexpected end of line";
				}

				tokenValue = token.ToString();
				frameCommand.Type = AnimationFrameCommandType.ScriptFunction;
				idConsole.Warning("TODO: fc.function = gameLocal.program.FindFunction( token );");

				if(frameCommand.Function == null)
				{
					return string.Format("Function '{0}' not found", tokenValue);
				}
			}
			else if(tokenValue == "object_call")
			{
				if((token = lexer.ReadTokenOnLine()) == null)
				{
					return "Unexpected end of line";
				}

				tokenValue = token.ToString();
				frameCommand.Type = AnimationFrameCommandType.ScriptFunctionObject;
				frameCommand.String = tokenValue;
			}
			else if(tokenValue == "event")
			{
				if((token = lexer.ReadTokenOnLine()) == null)
				{
					return "Unexpected end of line";
				}

				tokenValue = token.ToString();
				frameCommand.Type = AnimationFrameCommandType.EventFunction;

				idConsole.Warning("TODO: idAnim Event");
				/*const idEventDef *ev = idEventDef::FindEvent( token );
				if ( !ev ) {
					return va( "Event '%s' not found", token.c_str() );
				}
				if ( ev->GetNumArgs() != 0 ) {
					return va( "Event '%s' has arguments", token.c_str() );
				}*/

				frameCommand.String = tokenValue;
			} 
			else if((tokenValue == "sound") 
				|| (tokenValue == "sound_voice")
				|| (tokenValue == "sound_voice2")
				|| (tokenValue == "sound_body")
				|| (tokenValue == "sound_body2")
				|| (tokenValue == "sound_body3")
				|| (tokenValue == "sound_weapon")
				|| (tokenValue == "sound_global")
				|| (tokenValue == "sound_item")
				|| (tokenValue == "sound_chatter"))
			{
				if((token = lexer.ReadTokenOnLine()) == null)
				{
					return "Unexpected end of line";
				}

				switch(tokenValue)
				{
					case "sound":
						frameCommand.Type = AnimationFrameCommandType.Sound;
						break;

					case "sound_voice":
						frameCommand.Type = AnimationFrameCommandType.SoundVoice;
						break;

					case "sound_voice2":
						frameCommand.Type = AnimationFrameCommandType.SoundVoice2;
						break;

					case "sound_body":
						frameCommand.Type = AnimationFrameCommandType.SoundBody;
						break;

					case "sound_body2":
						frameCommand.Type = AnimationFrameCommandType.SoundBody2;
						break;

					case "sound_body3":
						frameCommand.Type = AnimationFrameCommandType.SoundBody3;
						break;

					case "sound_weapon":
						frameCommand.Type = AnimationFrameCommandType.SoundWeapon;
						break;

					case "sound_global":
						frameCommand.Type = AnimationFrameCommandType.SoundGlobal;
						break;

					case "sound_item":
						frameCommand.Type = AnimationFrameCommandType.SoundItem;
						break;

					case "sound_chatter":
						frameCommand.Type = AnimationFrameCommandType.SoundChatter;
						break;
				}
				
				tokenValue = token.ToString();

				if(tokenValue.StartsWith("snd_") == true)
				{
					frameCommand.String = tokenValue;
				}
				else
				{
					frameCommand.SoundMaterial = idE.DeclManager.FindSound(tokenValue);

					if(frameCommand.SoundMaterial.State == DeclState.Defaulted)
					{
						idConsole.Warning("Sound '{0}' not found", tokenValue);
					}
				}
			}
			else if(tokenValue == "skin")
			{
				if((token = lexer.ReadTokenOnLine()) == null)
				{
					return "Unexpected end of line";
				}

				tokenValue = token.ToString();
				frameCommand.Type = AnimationFrameCommandType.Skin;

				if(tokenValue == "none")
				{
					frameCommand.Skin = null;
				}
				else
				{
					frameCommand.Skin = idE.DeclManager.FindSkin(tokenValue);

					if(frameCommand.Skin == null)
					{
						return string.Format("Skin '{0}' not found", tokenValue);
					}
				}
			}
			else if(tokenValue == "fx")
			{
				if((token = lexer.ReadTokenOnLine()) == null)
				{
					return "Unexpected end of line";
				}

				tokenValue = token.ToString();
				frameCommand.Type = AnimationFrameCommandType.Fx;

				if(idE.DeclManager.FindType(DeclType.Fx, tokenValue) == null)
				{
					return string.Format("fx '{0}' not found", tokenValue);
				}

				frameCommand.String = tokenValue;
			}
			else if(tokenValue == "trigger")
			{
				if((token = lexer.ReadTokenOnLine()) == null)
				{
					return "Unexpected end of line";
				}
				
				tokenValue = token.ToString();
				frameCommand.Type = AnimationFrameCommandType.Trigger;
				frameCommand.String = tokenValue;
			}
			else if(tokenValue == "triggerSmokeParticle")
			{
				if((token = lexer.ReadTokenOnLine()) == null)
				{
					return "Unexpected end of line";
				}

				tokenValue = token.ToString();
				frameCommand.Type = AnimationFrameCommandType.TriggerSmokeParticle;
				frameCommand.String = tokenValue;
			}
			else if((tokenValue == "melee")
				|| (tokenValue == "direct_damage")
				|| (tokenValue == "attack_begin"))
			{
				if((token = lexer.ReadTokenOnLine()) == null)
				{
					return "Unexpected end of line";
				}

				switch(tokenValue)
				{
					case "melee":
						frameCommand.Type = AnimationFrameCommandType.Melee;
						break;

					case "direct_damage":
						frameCommand.Type = AnimationFrameCommandType.DirectDamage;
						break;

					case "attack_begin":
						frameCommand.Type = AnimationFrameCommandType.BeginAttack;
						break;
				}

				tokenValue = token.ToString();
								
				if(idR.Game.FindEntityDef(tokenValue, false) == null)
				{
					return string.Format("Unknown entityDef '{0}'", tokenValue);
				}

				frameCommand.String = tokenValue;
			}
			else if(tokenValue == "attack_end")
			{
				frameCommand.Type = AnimationFrameCommandType.EndAttack;
			}
			else if(tokenValue == "muzzle_flash")
			{
				if((token = lexer.ReadTokenOnLine()) == null)
				{
					return "Unexpected end of line";
				}

				tokenValue = token.ToString();

				if((tokenValue != string.Empty) && (modelDef.FindJoint(tokenValue) == null))
				{
					return string.Format("Joint '{0}' not found", tokenValue);
				}

				frameCommand.Type = AnimationFrameCommandType.MuzzleFlash;
				frameCommand.String = tokenValue;
			}
			else if((tokenValue == "create_missile")
				|| (tokenValue == "launch_missile"))
			{
				if((token = lexer.ReadTokenOnLine()) == null)
				{
					return "Unexpected end of line";
				}

				switch(tokenValue)
				{
					case "create_missile":
						frameCommand.Type = AnimationFrameCommandType.CreateMissile;
						break;

					case "launch_missile":
						frameCommand.Type = AnimationFrameCommandType.LaunchMissile;
						break;
				}

				tokenValue = token.ToString();
				frameCommand.String = tokenValue;

				if(modelDef.FindJoint(tokenValue) == null)
				{
					return string.Format("Joint '{0}' not found", tokenValue);
				}
			}
			else if(tokenValue == "fire_missile_at_target")
			{
				if((token = lexer.ReadTokenOnLine()) == null)
				{
					return "Unexpected end of line";
				}

				JointInfo jointInfo = modelDef.FindJoint(token.ToString());

				if(jointInfo == null)
				{
					return string.Format("Joint '{0}' not found", token.ToString());
				}

				if((token = lexer.ReadTokenOnLine()) == null)
				{
					return "Unexpected end of line";
				}

				frameCommand.Type = AnimationFrameCommandType.FireMissileAtTarget;
				frameCommand.String = token.ToString();
				frameCommand.Index = jointInfo.Index;
			}
			else if(tokenValue == "footstep")
			{
				frameCommand.Type = AnimationFrameCommandType.Footstep;
			}
			else if(tokenValue == "leftfoot")
			{
				frameCommand.Type = AnimationFrameCommandType.LeftFoot;
			}
			else if(tokenValue == "rightfoot")
			{
				frameCommand.Type = AnimationFrameCommandType.RightFoot;
			}
			else if(tokenValue == "enableEyeFocus")
			{
				frameCommand.Type = AnimationFrameCommandType.EnableEyeFocus;
			}
			else if(tokenValue == "disableEyeFocus")
			{
				frameCommand.Type = AnimationFrameCommandType.DisableEyeFocus;
			}
			else if(tokenValue == "disableGravity")
			{
				frameCommand.Type = AnimationFrameCommandType.DisableGravity;
			}
			else if(tokenValue == "enableGravity")
			{
				frameCommand.Type = AnimationFrameCommandType.EnableGravity;
			}
			else if(tokenValue == "jump")
			{
				frameCommand.Type = AnimationFrameCommandType.Jump;
			}
			else if(tokenValue == "enableClip")
			{
				frameCommand.Type = AnimationFrameCommandType.EnableClip;
			}
			else if(tokenValue == "disableClip")
			{
				frameCommand.Type = AnimationFrameCommandType.DisableClip;
			}
			else if(tokenValue == "enableWalkIK")
			{
				frameCommand.Type = AnimationFrameCommandType.EnableWalkIk;
			}
			else if(tokenValue == "disableWalkIK")
			{
				frameCommand.Type = AnimationFrameCommandType.DisableWalkIk;
			}
			else if(tokenValue == "enableLegIK")
			{
				if((token = lexer.ReadTokenOnLine()) == null)
				{
					return "Unexpected end of file";
				}

				frameCommand.Type = AnimationFrameCommandType.EnableLegIk;
				frameCommand.Index = int.Parse(token.ToString());
			}
			else if(tokenValue == "disableLegIK")
			{
				if((token = lexer.ReadTokenOnLine()) == null)
				{
					return "Unexpected end of file";
				}

				frameCommand.Type = AnimationFrameCommandType.DisableLegIk;
				frameCommand.Index = int.Parse(token.ToString());
			}
			else if(tokenValue == "recordDemo")
			{
				frameCommand.Type = AnimationFrameCommandType.RecordDemo;

				if((token = lexer.ReadTokenOnLine()) != null)
				{
					frameCommand.String = token.ToString();
				}
			}
			else if(tokenValue == "aviGame")
			{
				frameCommand.Type = AnimationFrameCommandType.AviGame;

				if((token = lexer.ReadTokenOnLine()) != null)
				{
					frameCommand.String = token.ToString();
				}
			}
			else
			{
				return string.Format("Unknown command '{0}'", tokenValue);
			}

			// check if we've initialized the frame lookup table
			if(_frameLookups.Count == 0)
			{
				// we haven't, so allocate the table and initialize it

				for(int i = 0; i < _anims[0].FrameCount; i++)
				{
					_frameLookups.Add(new AnimationFrameLookup());
				}
			}

			// calculate the index of the new command
			int index = _frameLookups[frameIndex].FirstCommand + _frameLookups[frameIndex].Index;
			int count = _frameLookups.Count;

			_frameCommands.Insert(index, frameCommand);

			// fix the indices of any later frames to account for the inserted command
			for(int i = frameIndex + 1; i < count; i++)
			{
				_frameLookups[i].FirstCommand++;
			}

			// increase the number of commands on this frame
			_frameLookups[frameIndex].Index++;

			// return with no error
			return null;
		}

		public bool GetBounds(out idBounds bounds, int animNumber, int currentTime, int cycleCount)
		{
			if(_anims[animNumber] == null)
			{
				bounds = idBounds.Zero;
				return false;
			}

			bounds = _anims[animNumber].GetBounds(currentTime, cycleCount);

			return true;
		}

		public bool GetOrigin(out Vector3 offset, int animNumber, int currentTime, int cycleCount)
		{
			if(_anims[animNumber] == null)
			{
				offset = Vector3.Zero;
				return false;
			}

			offset = _anims[animNumber].GetOrigin(currentTime, cycleCount);

			return true;
		}
			
		public void SetAnimation(idDeclModel modelDef, string sourceName, string animName, idMD5Anim[] md5anims)
		{
			_modelDef = modelDef;
			_anims = md5anims;
			
			_realName = sourceName;
			_name = animName;
			_animFlags = new AnimationFlags();
			_frameCommands.Clear();
			_frameLookups.Clear();
		}
		#endregion
		#endregion
	}
	
	public struct JointAnimationInfo
	{
		public int NameIndex;
		public int ParentIndex;
		public AnimationBits AnimationBits;
		public int FirstComponent;
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using idTech4.Geometry;
using idTech4.Renderer;
using idTech4.Text;
using idTech4.Text.Decl;

namespace idTech4.Game.Animation
{
	public class idAnimator
	{
		#region Properties
		public idRenderModel Model
		{
			get
			{
				if(_modelDef != null)
				{
					return _modelDef.Model;
				}

				return null;
			}
		}

		public idDeclModel ModelDefinition
		{
			get
			{
				return _modelDef;
			}
		}
		#endregion

		#region Members
		private idDeclModel _modelDef;
		private idEntity _entity;

		private idAnimBlend[,] _channels = new idAnimBlend[idR.AnimationChannelCount, idR.AnimationCountPerChannel];
	
		/*idList<jointMod_t *>		jointMods;
		int							numJoints;*/
		private idJointMatrix[] _joints;

		private int _lastTransformTime;		// mutable because the value is updated in CreateFrame
		private bool _stoppedAnimatingUpdate;
		private bool _removeOriginOffset;
		private bool _forceUpdate;

		private idBounds _frameBounds;

		private float _afPoseBlendWeight;
		private List<int> _afPoseJoints = new List<int>();
		/*idList<idAFPoseJointMod>	AFPoseJointMods;
		idList<idJointQuat>			AFPoseJointFrame;*/
		private idBounds _afPoseBounds;
		private int _afPoseTime;
		#endregion

		#region Constructor
		public idAnimator(idEntity entity)
		{
			_entity = entity;
			_lastTransformTime = -1;

			idConsole.Warning("TODO: ClearAFPose();");

			for(int i = (int) AnimationChannel.All; i < (int) AnimationChannel.Count; i++)
			{
				for(int j = 0; j < idR.AnimationCountPerChannel; j++)
				{
					_channels[i, j] = new idAnimBlend();
					_channels[i, j].Reset(null);
				}
			}
		}
		#endregion

		#region Methods
		#region Public
		public bool CreateFrame(int currentTime, bool force)
		{
			idConsole.Warning("TODO: idAnimator.CreateFrame");
			return false;
		}

		public void ForceUpdate()
		{
			_lastTransformTime = -1;
			_forceUpdate = true;
		}

		public bool GetBounds(int currentTime, out idBounds bounds)
		{
			bounds = idBounds.Zero;

			if((_modelDef == null) || (_modelDef.Model == null))
			{
				return false;
			}

			int count = 0;

			if(_afPoseJoints.Count > 0)
			{
				bounds = _afPoseBounds;
				count = 1;
			}

			for(int i = (int) AnimationChannel.All; i < (int) AnimationChannel.Count; i++)
			{
				for(int j = 0; j < idR.AnimationCountPerChannel; j++)
				{
					idAnimBlend blend = _channels[i, j];
				
					if(blend.AddBounds(currentTime, ref bounds, _removeOriginOffset) == true)
					{
						count++;
					}
				}
			}

			if(count == 0)
			{
				if(_frameBounds.IsCleared == false)
				{
					bounds = _frameBounds;
					return true;
				}
				else
				{
					return false;
				}
			}

			bounds.Translate(_modelDef.VisualOffset);

			if(idR.CvarSystem.GetBool("g_debugBounds") == true)
			{
				idConsole.Warning("TODO: g_debugBounds");

				/*if ( bounds[1][0] - bounds[0][0] > 2048 || bounds[1][1] - bounds[0][1] > 2048 ) {
					if ( entity ) {
						gameLocal.Warning( "big frameBounds on entity '%s' with model '%s': %f,%f", entity->name.c_str(), modelDef->ModelHandle()->Name(), bounds[1][0] - bounds[0][0], bounds[1][1] - bounds[0][1] );
					} else {
						gameLocal.Warning( "big frameBounds on model '%s': %f,%f", modelDef->ModelHandle()->Name(), bounds[1][0] - bounds[0][0], bounds[1][1] - bounds[0][1] );
					}
				}*/
			}

			_frameBounds = bounds;

			return true;
		}

		public idJointMatrix[] GetJoints()
		{
			return _joints;
		}

		public idRenderModel SetModel(string modelName)
		{
			FreeData();

			// check if we're just clearing the model
			if((modelName == null) || (modelName == string.Empty))
			{
				return null;
			}

			_modelDef = idR.DeclManager.FindType<idDeclModel>(DeclType.ModelDef, modelName, false);

			if(_modelDef == null)
			{
				return null;
			}

			idRenderModel renderModel = _modelDef.Model;

			if(renderModel == null)
			{
				_modelDef = null;
				return null;
			}

			// make sure model hasn't been purged
			_modelDef.Touch();

			_modelDef.SetupJoints(_joints, ref _frameBounds, _removeOriginOffset);
			_modelDef.Model.Reset();

			// set the modelDef on all channels
			for(int i = (int) AnimationChannel.All; i < (int) AnimationChannel.Count; i++)
			{
				for(int j = 0; j < idR.AnimationCountPerChannel; j++)
				{
					_channels[i, j].Reset(_modelDef);
				}
			}

			return _modelDef.Model;
		}
		#endregion

		#region Private
		private void FreeData()
		{
			if(_entity != null)
			{
				idConsole.Warning("TODO: _entity.BecomeInactive(EntityThinkFlags.Animate);");
			}

			for(int i = (int) AnimationChannel.All; i < (int) AnimationChannel.Count; i++)
			{
				for(int j = 0; j < idR.AnimationCountPerChannel; j++)
				{
					_channels[i, j].Reset(null);
				}
			}

			idConsole.Warning("TODOO: jointMods.DeleteContents(true);");

			_joints = null;
			_modelDef = null;

			ForceUpdate();
		}
		#endregion
		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4;
using idTech4.CollisionManager;
using idTech4.Renderer;

namespace idTech4.Game.Physics
{
	// would prefer value semantics for this
	public class ClipSector
	{
		public int Axis; // -1 = leaf node
		public float Distance;
		public ClipSector[] Children = new ClipSector[2];
		public ClipLink Links;
	}

	// again, would prefer value semantics
	public class ClipLink
	{
		public idClipModel Model;
		public ClipSector Sector;
		public ClipLink PreviousInSector;
		public ClipLink NextInSector;
		public ClipLink NextLink;
	}

	public class idClip
	{
		#region Constants
		public const int MaxSectorDepth = 12;
		public const int MaxSectors = ((1 << (MaxSectorDepth + 1)) - 1);
		#endregion

		#region Properties
		public ClipSector[] Sectors
		{
			get
			{
				return _clipSectors;
			}
		}

		public idClipModel DefaultClipModel
		{
			get
			{
				return _defaultClipModel;
			}
		}
		#endregion

		#region Members
		private ClipSector[] _clipSectors;

		private idBounds _worldBounds;
		private idClipModel _defaultClipModel;

		private int _clipSectorCount;
		private int _contentCount;
		private int _motionCount;
		private int _touchCount;
		private int _contactCount;
		private int _translationCount;
		private int _rotationCount;
		private int _renderModelTraceCount;
		#endregion

		#region Constructor
		public idClip()
		{

		}
		#endregion

		#region Methods
		#region Public
		public void Init()
		{
			Vector3 maxSector = Vector3.Zero;

			// clear clip sectors
			_clipSectors = new ClipSector[MaxSectors];
			_clipSectorCount = 0;
			_touchCount = -1;

			// get world map bounds
			int h = idR.CollisionModelManager.LoadModel("worldMap", false);
			_worldBounds = idR.CollisionModelManager.GetModelBounds(h);

			// create world sectors
			CreateClipSectors(0, _worldBounds, ref maxSector);

			Vector3 size = _worldBounds.Max - _worldBounds.Min;

			idConsole.WriteLine("map bounds are ({0})", size);
			idConsole.WriteLine("max clip sector is ({0})", maxSector);

			// initialize a default clip model
			_defaultClipModel = new idClipModel();
			// TODO: _defaultClipModel.LoadModel(new idTraceModel(idBounds.Expand(8)));

			// set counters to zero
			_rotationCount = 0;
			_translationCount = 0;
			_motionCount = 0;
			_renderModelTraceCount = 0;
			_contentCount = 0;
			_contactCount = 0;
		}

		public bool Translation(out TraceResult result, Vector3 start, Vector3 end, idClipModel model, Matrix traceModelAxis, ContentFlags contentMask, idEntity passEntity)
		{
			// TODO
			/*if(TestHugeTranslation(result, model, start, end, traceModelAxis) == true)
			{
				return true;
			}*/

			// TODO
			/*idTraceModel traceModel = TraceModelForClipModel(model);
			idBounds traceBounds = new idBounds();
			TraceResult traceResult;
			float radius = 0;

			if((passEntity == null) || (passEntity.Index != idR.EntityIndexWorld))
			{
				// test world
				_translationCount++;

				// TODO: idR.CollisionModelManager.Translation(out result, start, end, traceModel, traceModelAxis, contentMask, 0, Vector3.Zero, Matrix.Identity);
				result.ContactInformation.EntityIndex = (result.Fraction != 1.0f) ? idR.EntityIndexWorld : idR.EntityIndexNone;

				if(result.Fraction == 0.0f)
				{
					return true; // blocked immediately by the world
				}
			}
			else
			{
				result = new TraceResult();
				result.Fraction = 1.0f;
				result.EndPosition = end;
				result.EndAxis = traceModelAxis;
			}

			if(traceModel == null)
			{
				traceBounds = idBounds.FromPointTranslation(start, result.EndPosition - start);
				radius = 0.0f;
			}
			else
			{
				traceBounds = idBounds.FromBoundsTranslation(traceModel.Bounds, start, traceModelAxis, result.EndPosition - start);
				radius = traceModel.Bounds.GetRadius();
			}

			idClipModel[] clipModelList = GetTraceClipModels(traceBounds, contentMask, passEntity);

			foreach(idClipModel touch in clipModelList)
			{
				if(touch == null)
				{
					continue;
				}

				if(touch.RenderModelHandle != -1)
				{
					_renderModelTraceCount++;
					traceResult = TraceRenderModel(start, end, radius, traceModelAxis, touch);
				}
				else
				{
					_translationCount++;
					// TODO: traceResult = idR.CollisionModelManager.Translation(start, end, traceModel, traceModelAxis, contentMask, touch.Handle, touch.Origin, touch.Axis);
				}

				if(traceResult.Fraction < result.Fraction)
				{
					result = traceResult;
					result.ContactInformation.EntityIndex = touch.Entity.Index;
					result.ContactInformation.ID = touch.ID;

					if(result.Fraction == 0.0f)
					{
						break;
					}
				}
			}

			return (result.Fraction < 1.0f);*/
			result = new TraceResult();
			return false;
		}

		public bool Rotation(out TraceResult result, Vector3 start, idRotation rotation, idClipModel model, Matrix traceModelAxis, ContentFlags contentMask, idEntity passEntity)
		{
			/*idTraceModel traceModel = TraceModelForClipModel(model);
			idBounds traceBounds = new idBounds();
			TraceResult traceResult;

			if((passEntity == null) || (passEntity.Index != idR.EntityIndexWorld))
			{
				// test world
				_rotationCount++;

				// TODO: NEED ENGINE SOURCE idR.CollisionModelManager.Rotation(out result, start, rotation, traceModel, traceModelAxis, contentMask, 0, Vector3.Zero, Matrix.Identity);
				result.ContactInformation.EntityIndex = (result.Fraction != 1.0f) ? idR.EntityIndexWorld : idR.EntityIndexNone;

				if(result.Fraction == 0.0f)
				{
					return true; // blocked immediately by the world
				}
			}
			else
			{
				result = new TraceResult();
				result.Fraction = 1.0f;
				result.EndPosition = start;
				result.EndAxis = traceModelAxis * rotation.ToMatrix();
			}

			if(traceModel == null)
			{
				traceBounds = idBounds.FromPointRotation(start, rotation);
			}
			else
			{
				traceBounds = idBounds.FromBoundsRotation(traceModel.Bounds, start, traceModelAxis, rotation);
			}

			idClipModel[] clipModelList = GetTraceClipModels(traceBounds, contentMask, passEntity);

			foreach(idClipModel touch in clipModelList)
			{
				if(touch == null)
				{
					continue;
				}

				if(touch.RenderModelHandle != -1)
				{
					continue;
				}

				_rotationCount++;
				// TODO: traceResult = idR.CollisionModelManager.Rotation(start, rotation, traceModel, traceModelAxis, contentMask, touch.Handle, touch.Origin, touch.Axis);

				if(traceResult.Fraction < result.Fraction)
				{
					result = traceResult;
					result.ContactInformation.EntityIndex = touch.Entity.Index;
					result.ContactInformation.ID = touch.ID;

					if(result.Fraction == 0.0f)
					{
						break;
					}
				}
			}

			return (result.Fraction < 1.0f);*/
			result = new TraceResult();
			return false;
		}

		public ContentFlags Contents(Vector3 start, idClipModel model, Matrix traceModelAxis, ContentFlags contentMask, idEntity passEntity)
		{
			ContentFlags contents = ContentFlags.None;
			idBounds traceModelBounds = new idBounds();
			
			// TODO
			/*idTraceModel traceModel = TraceModelForClipModel(model);

			if((passEntity == null) || (passEntity.Index != idR.EntityIndexWorld))
			{
				// test world
				_contentCount++;
				// TODO: NEED ENGINE SOURCE contents = idR.CollisionModelManager.Contents(start, traceModel, traceModelAxis, contentMask, 0, Vector3.Zero, Matrix.Identity);
			}
			else
			{
				contents = ContentFlags.None;
			}

			if(traceModel == null)
			{
				traceModelBounds.Min = start;
				traceModelBounds.Max = start;
			}
			else if(traceModelAxis != Matrix.Identity)
			{
				traceModelBounds = idBounds.FromTransformedBounds(traceModel.Bounds, start, traceModelAxis);
			}
			else
			{
				traceModelBounds.Min = traceModel.Bounds.Min + start;
				traceModelBounds.Max = traceModel.Bounds.Max + start;
			}

			idClipModel[] traceModelList = GetTraceClipModels(traceModelBounds, -1, passEntity);

			foreach(idClipModel touch in traceModelList)
			{
				if(touch == null)
				{
					continue;
				}

				// no contents test with render models
				if(touch.RenderModelHandle != -1)
				{
					continue;
				}

				// if the entity does not have any contents we are looking for
				if((touch.Contents & contentMask) == ContentFlags.None)
				{
					continue;
				}

				// if the entity has no new contents flags
				if((touch.Contents & contents) == touch.Contents)
				{
					continue;
				}

				_contentCount++;

				// TODO
				/*if(idR.CollisionModelManager.Contents(start, traceModel, traceModelAxis, contentMask, touch.Handle, touch.Origin, touch.Axis) > 0)
				{
					contents |= (touch.Contents & contentMask);
				}*/
			/*}*/

			return contents;
		}

		public ContentFlags ContentsModel(Vector3 start, idClipModel model, Matrix traceModelAxis, ContentFlags contentMask, int modelHandle, Vector3 modelOrigin, Matrix modelAxis)
		{
			_contentCount++;

			// TODO: NEED ENGINE SOURCE return idR.CollisionModelManager.Contents(start, TraceModelForClipModel(model), traceModelAxis, contentMask, modelHandle, modelOrigin, modelAxis);
			return ContentFlags.None;
		}

		public TraceResult TranslationModel(Vector3 start, Vector3 end, idClipModel model, Matrix traceModelAxis, ContentFlags contentMask, int modelHandle, Vector3 modelOrigin, Matrix modelAxis)
		{
			// TODO: idTraceModel traceModel = TraceModelForClipModel(model);

			_translationCount++;

			// TODO return idR.CollisionModelManager.Translation(start, end, traceModel, traceModelAxis, contentMask, modelHandle, modelOrigin, modelAxis);
			return new TraceResult();
		}

		public TraceResult RotationModel(Vector3 start, idRotation rotation, idClipModel model, Matrix traceModelAxis, ContentFlags contentMask, int modelHandle, Vector3 modelOrigin, Matrix modelAxis)
		{
			// TODO: idTraceModel traceModel = TraceModelForClipModel(model);

			_rotationCount++;

			// TODO: return idR.CollisionModelManager.Rotation(start, rotation, traceModel, traceModelAxis, contentMask, modelHandle, modelOrigin, modelAxis);
			return new TraceResult();
		}
		#endregion

		#region Private
		/// <summary>
		/// Builds a uniformly subdivided tree for the given world size.
		/// </summary>
		/// <param name="depth"></param>
		/// <param name="bounds"></param>
		/// <param name="?"></param>
		/// <returns></returns>
		private ClipSector CreateClipSectors(int depth, idBounds bounds, ref Vector3 maxSector)
		{
			idBounds front, back;

			ClipSector anode = _clipSectors[_clipSectorCount];
			_clipSectorCount++;

			if(depth == idClip.MaxSectorDepth)
			{
				anode.Axis = -1;
				anode.Children[0] = anode.Children[1] = null;

				if((bounds.Max.X - bounds.Min.X) > maxSector.X)
				{
					maxSector.X = bounds.Max.X - bounds.Min.X;
				}

				if((bounds.Max.Y - bounds.Min.Y) > maxSector.Y)
				{
					maxSector.Y = bounds.Max.Y - bounds.Min.Y;
				}

				if((bounds.Max.Z - bounds.Min.Z) > maxSector.Z)
				{
					maxSector.Z = bounds.Max.Z - bounds.Min.Z;
				}

				return anode;
			}

			Vector3 size = bounds.Max - bounds.Min;
			front = bounds;
			back = bounds;

			if((size.X >= size.Y) && (size.X >= size.Z))
			{
				anode.Axis = 0;
				anode.Distance = 0.5f * (bounds.Max.X + bounds.Min.X);
				front.Min.X = back.Max.X = anode.Distance;
			}
			else if((size.Y >= size.X) && (size.Y >= size.Z))
			{
				anode.Axis = 1;
				anode.Distance = 0.5f * (bounds.Max.Y + bounds.Min.Y);
				front.Min.Y = back.Max.Y = anode.Distance;
			}
			else
			{
				anode.Axis = 2;
				anode.Distance = 0.5f * (bounds.Max.Z + bounds.Min.Z);
				front.Min.Z = back.Max.Z = anode.Distance;
			}

			anode.Children[0] = CreateClipSectors(depth + 1, front, ref maxSector);
			anode.Children[1] = CreateClipSectors(depth + 1, back, ref maxSector);

			return anode;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// an ent will be excluded from testing if:
		/// cm->entity == passEntity (don't clip against the pass entity)
		/// cm->entity == passOwner (missiles don't clip with owner)
		/// cm->owner == passEntity (don't interact with your own missiles)
		/// cm->owner == passOwner (don't interact with other missiles from same owner)
		/// </remarks>
		/// <param name="bounds"></param>
		/// <param name="contentMask"></param>
		/// <param name="passEntity"></param>
		/// <returns></returns>
		private idClipModel[] GetTraceClipModels(idBounds bounds, ContentFlags contentMask, idEntity passEntity)
		{
			// TODO
			/*int i, num;
			idClipModel	*cm;
			idEntity *passOwner;

			num = ClipModelsTouchingBounds( bounds, contentMask, clipModelList, MAX_GENTITIES );

			if ( !passEntity ) {
				return num;
			}

			if ( passEntity->GetPhysics()->GetNumClipModels() > 0 ) {
				passOwner = passEntity->GetPhysics()->GetClipModel()->GetOwner();
			} else {
				passOwner = NULL;
			}

			for ( i = 0; i < num; i++ ) {

				cm = clipModelList[i];

				// check if we should ignore this entity
				if ( cm->entity == passEntity ) {
					clipModelList[i] = NULL;			// don't clip against the pass entity
				} else if ( cm->entity == passOwner ) {
					clipModelList[i] = NULL;			// missiles don't clip with their owner
				} else if ( cm->owner ) {
					if ( cm->owner == passEntity ) {
						clipModelList[i] = NULL;		// don't clip against own missiles
					} else if ( cm->owner == passOwner ) {
						clipModelList[i] = NULL;		// don't clip against other missiles from same owner
					}
				}
			}

			return num;*/

			return null;
		}
		#endregion
		#endregion
	}
}
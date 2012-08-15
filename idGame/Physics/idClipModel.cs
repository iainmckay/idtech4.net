using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Collision;
using idTech4.Geometry;
using idTech4.Renderer;

namespace idTech4.Game.Physics
{
	public class idClipModel : IDisposable
	{
		#region Constants
		public readonly float BoxEpsilon = 1.0f;
		#endregion

		#region Properties
		/// <summary>
		/// ID for entities that use multiple clip models.
		/// </summary>
		public int ID
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				return _id;
			}
		}

		/// <summary>
		/// Is this clip model is used for clipping.
		/// </summary>
		public bool Enabled
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				return _enabled;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				_enabled = value;
			}
		}

		/// <summary>
		/// Entity using this clip model.
		/// </summary>
		public idEntity Entity
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				return _entity;
			}
		}

		/// <summary>
		/// Owner of the entity that owns this clip model.
		/// </summary>
		public idEntity Owner
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				return _owner;
			}
		}

		/// <summary>
		/// Origin of clip model.
		/// </summary>
		public Vector3 Origin
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				return _origin;
			}
		}

		/// <summary>
		/// Orientation of clip model.
		/// </summary>
		public Matrix Axis
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				return _axis;
			}
		}

		public idBounds Bounds
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				return _bounds;
			}
		}

		public idBounds AbsoluteBounds
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				return _absBounds;
			}
		}

		/// <summary>
		/// Material for trace models.
		/// </summary>
		public idMaterial Material
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				return _material;
			}
		}

		/// <summary>
		/// All contents ored together.
		/// </summary>
		public ContentFlags Contents
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				return _contents;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				_contents = value;
			}
		}

		/// <summary>
		/// Handle to collision model.
		/// </summary>
		public int CollisionModelHandle
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				return _collisionModelHandle;
			}
		}

		/// <summary>
		/// Trace model used for collision detection.
		/// </summary>
		public int TraceModelIndex
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				return _traceModelIndex;
			}
		}

		/// <summary>
		/// Render model def handle.
		/// </summary>
		public int RenderModelHandle
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				return _renderModelHandle;
			}
		}

		public int Handle
		{
			get
			{
				if(_collisionModelHandle > 0)
				{
					return _collisionModelHandle;
				}
				else if(_traceModelIndex != -1)
				{
					// TODO: return idR.CollisionModelManager.SetupTraceModel(GetCachedTraceModel(_traceModelIndex), _material);
				}
					
				// this happens in multiplayer on the combat models
				idConsole.Warning("idClipModel.Handle: clip model {0} on '{1}' ({2:X}) is not a collision or trace model", _id, _entity.Name, Entity.Index);
				return 0;

			}
		}
		#endregion

		#region Members
		private int _id; // id for entities that use multiple clip models.
		private bool _enabled; // true if this clip model is used for clipping.

		private idEntity _entity; // entity using this clip model.
		private idEntity _owner; // owner of the entity that owns this clip model.

		private Vector3 _origin; // origin of clip model.
		private Matrix _axis; // orientation of clip model.
		private idBounds _bounds;
		private idBounds _absBounds;

		private idMaterial _material; // material for trace models.
		private ContentFlags _contents; // all contents ored together.
		
		private int _collisionModelHandle;	// handle to collision model.

		private int _traceModelIndex; // trace model used for collision detection.
		private int _renderModelHandle;	// render model def handle.

		private ClipLink _clipLinks = new ClipLink(); // links into sectors.

		private int _touchCount;
		#endregion

		#region Constructor
		public idClipModel()
		{
			Init();
		}

		public idClipModel(string name)
			: this()
		{
			throw new NotImplementedException();
			// TODO: LoadModel();
		}

		public idClipModel(idTraceModel traceModel) 
		{
			Init();
			LoadModel(traceModel);
		}
		/*
idClipModel::idClipModel( const int renderModelHandle ) {
	Init();
	contents = CONTENTS_RENDERMODEL;
	LoadModel( renderModelHandle );
}*/

		public idClipModel(idClipModel model)
		{
			_id = model.ID;
			_owner = model.Owner;
			
			_enabled = model.Enabled;
			_entity = model.Entity;

			_origin = model.Origin;
			_axis = model.Axis;
			_bounds = model.Bounds;
			_absBounds = model.AbsoluteBounds;

			_material = model.Material;
			_contents = model.Contents;
			_collisionModelHandle = model.CollisionModelHandle;
			_traceModelIndex = -1;

			// TODO
			/*if ( model->traceModelIndex != -1 ) {
				LoadModel( *GetCachedTraceModel( model->traceModelIndex ) );
			}*/

			_renderModelHandle = model.RenderModelHandle;
			_touchCount = -1;
		}

		~idClipModel()
		{
			Dispose(false);
		}
		#endregion

		#region Methods
		#region Static
		public static int CheckModel(string name)
		{
			idConsole.Warning("TODO: idClipModel.CheckModel");
			return -1;
		//	return idR.CollisionModelManager.LoadModel(name, false);
		}
		#endregion
	
		#region Public
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>Must have been linked with an entity and ID before.</remarks>
		/// <param name="clip"></param>
		public void Link(idClip clip)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idClipModel");
			}

			if(_entity == null)
			{
				return;
			}

			if(_clipLinks != null)
			{
				Unlink(); // unlink from old position.
			}

			if(_bounds.IsCleared == true)
			{
				return;
			}
			
			// set the abs box
			if(_axis != Matrix.Identity)
			{
				// expand for rotation
				_absBounds = idBounds.FromTransformedBounds(_bounds, _origin, _axis);
			}
			else
			{
				// normal
				_absBounds.Min = _bounds.Min + _origin;
				_absBounds.Max = _bounds.Max + _origin;
			}

			// because movement is clipped an epsilon away from an actual edge,
			// we must fully check even when bounding boxes don't quite touch
			_absBounds.Min -= new Vector3(BoxEpsilon, BoxEpsilon, BoxEpsilon);
			_absBounds.Max += new Vector3(BoxEpsilon, BoxEpsilon, BoxEpsilon);

			// TODO: this may not be correct! from what I can gleam it just starts at the start of the array
			LinkSectors(clip.Sectors[0]);
		}

		public void Link(idClip clip, idEntity entity, int newID, Vector3 newOrigin, Matrix newAxis)
		{
			Link(clip, entity, newID, newOrigin, newAxis, -1);
		}

		public void Link(idClip clip, idEntity entity, int newID, Vector3 newOrigin, Matrix newAxis, int renderModelHandle)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idClipModel");
			}

			_entity = entity;
			_id = newID;
			_origin = newOrigin;
			_axis = newAxis;

			if(_renderModelHandle != -1)
			{
				_renderModelHandle = renderModelHandle;

				RenderEntityComponent renderEntity = idR.Game.RenderWorld.GetRenderEntity(renderModelHandle);

				if(renderEntity != null)
				{
					_bounds = renderEntity.Bounds;
				}
			}

			Link(clip);
		}

		public void Unlink()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idClipModel");
			}

			ClipLink link = null;

			while(link != null)
			{
				if(link.PreviousInSector != null)
				{
					link.PreviousInSector.NextInSector = link.NextInSector;
				}
				else
				{
					link.Sector.Links = link.NextInSector;
				}

				if(link.NextInSector != null)
				{
					link.NextInSector.PreviousInSector = link.PreviousInSector;
				}

				link = link.NextLink;
			}
		}

		public void LoadModel(idTraceModel traceModel)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idClipModel");
			}

			_collisionModelHandle = 0;
			_renderModelHandle = -1;

			idConsole.Warning("TODO: idClipModel.LoadModel");
			/*if(_traceModelIndex != -1)
			{
				FreeTraceModel(_traceModelIndex);
			}

			_traceModelIndex = AllocTraceModel(traceModel);
			_bounds = traceModel.Bounds;*/
		}
		#endregion

		#region Private
		private void Init()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idClipModel");
			}

			_enabled = true;

			_contents = ContentFlags.Body;

			_renderModelHandle = -1;
			_traceModelIndex = -1;
			_touchCount = -1;
		}		

		private void LinkSectors(ClipSector node)
		{
			ClipLink link;

			while(node.Axis != -1)
			{
				float min = ((node.Axis == 0) ? _absBounds.Min.X : ((node.Axis == 1) ? _absBounds.Min.Y : _absBounds.Min.Z));
				float max = ((node.Axis == 0) ? _absBounds.Max.X : ((node.Axis == 1) ? _absBounds.Max.Y : _absBounds.Max.Z));

				if(min > node.Distance)
				{
					node = node.Children[0];
				}
				else if(max < node.Distance)
				{
					node = node.Children[1];
				}
				else
				{
					LinkSectors(node.Children[0]);
					node = node.Children[1];
				}
			}

			link = new ClipLink();
			link.Model = this;
			link.Sector = node;
			link.NextInSector = node.Links;
			link.PreviousInSector = null;

			if(node.Links != null)
			{
				node.Links.PreviousInSector = link;
			}

			node.Links = link;
			link.NextLink = _clipLinks;
			_clipLinks = link;
		}
		#endregion
		#endregion

		#region IDisposable implementation
		#region Properties
		public bool Disposed
		{
			get
			{
				return _disposed;
			}
		}
		#endregion

		#region Members
		private bool _disposed;
		#endregion

		#region Methods
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idClipModel");
			}

			if(disposing == true)
			{
				// make sure the clip model is no longer linked
				Unlink();

				idConsole.Warning("TODO: idClipModel.Dispose");
				/*if(_traceModelIndex != -1)
				{
					FreeTraceModel(_traceModelIndex);
				}*/

				_traceModelIndex = -1;
				_disposed = true;
			}
		}
		#endregion
		#endregion
	}
}
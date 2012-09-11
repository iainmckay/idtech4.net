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
		public static readonly float BoxEpsilon = 1.0f;
		#endregion

		#region Properties
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

		public int Handle
		{
			get
			{
				if(_collisionModelHandle > 0)
				{
					return _collisionModelHandle;
				}
				else if(_traceModelCache != null)
				{
					idConsole.Warning("TODO: TODO: return idR.CollisionModelManager.SetupTraceModel(GetCachedTraceModel(_traceModelIndex), _material);");
				}

				// this happens in multiplayer on the combat models
				idConsole.Warning("idClipModel.Handle: clip model {0} on '{1}' ({2:X}) is not a collision or trace model", _id, _entity.Name, Entity.Index);

				return 0;

			}
		}

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

		public bool IsTraceModel
		{
			get
			{
				return (_traceModelCache != null);
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

		/// <summary>
		/// Trace model used for collision detection.
		/// </summary>
		public TraceModelCache TraceModelCache
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idClipModel");
				}

				return _traceModelCache;
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

		private TraceModelCache _traceModelCache; // trace model used for collision detection.
		private int _renderModelHandle;	// render model def handle.

		private ClipLink _clipLinks = new ClipLink(); // links into sectors.

		private int _touchCount;

		private static Dictionary<idTraceModel, WeakReference> _traceModelCacheDict = new Dictionary<idTraceModel, WeakReference>();
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
			_traceModelCache = null;

			if(model.TraceModelCache != null)
			{
				idConsole.Warning("TODO: LoadModel( *GetCachedTraceModel( model->traceModelIndex ) );");
			}

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
		public static CollisionModel CheckModel(string name)
		{
			return idR.CollisionModelManager.LoadModel(name, false);
		}

		private TraceModelCache GetTraceModelCache(idTraceModel model)
		{
			if(_traceModelCacheDict.ContainsKey(model) == true)
			{
				if(_traceModelCacheDict[model].IsAlive == true)
				{
					return (TraceModelCache) _traceModelCacheDict[model].Target;
				}

				_traceModelCacheDict.Remove(model);
			}

			TraceModelCache entry = new TraceModelCache();
			entry.TraceModel = model;
			entry.TraceModel.GetMassProperties(1.0f, out entry.Volume, out entry.CenterOfMass, out entry.InertiaTensor);

			WeakReference weakRef = new WeakReference(entry);
			_traceModelCacheDict.Add(model, weakRef);

			return entry;
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
			_traceModelCache = GetTraceModelCache(traceModel);

			_bounds = traceModel.Bounds;
		}

		/// <remarks>
		/// Unlinks the clip model.
		/// </remarks>
		/// <param name="rotation"></param>
		public void Rotate(idRotation rotation)
		{
			Unlink();

			_origin *= rotation;
			_axis *= rotation.ToMatrix();
		}

		/// <remarks>
		/// Unlinks the clip model.
		/// </remarks>
		/// <param name="translation"></param>
		public void Translate(Vector3 translation)
		{
			Unlink();
			_origin += translation;
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
			_traceModelCache = null;
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

				_traceModelCache = null;
				_disposed = true;
			}
		}
		#endregion
		#endregion
	}

	public class TraceModelCache
	{
		public idTraceModel TraceModel;
		public float Volume;
		public Vector3 CenterOfMass;
		public Matrix InertiaTensor;
	}
}
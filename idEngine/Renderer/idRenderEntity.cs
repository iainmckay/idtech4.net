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

namespace idTech4.Renderer
{
	public class idRenderEntity
	{
		#region Properties
		public idRenderModel CachedDynamicModel
		{
			get
			{
				return _cachedDynamicModel;
			}
			set
			{
				_cachedDynamicModel = value;
			}
		}

		public idRenderModel DynamicModel
		{
			get
			{
				return _dynamicModel;
			}
			set
			{
				_dynamicModel = value;
			}
		}
		
		public int EntityIndex
		{
			get
			{
				return _entityIndex;
			}
			set
			{
				_entityIndex = value;
			}
		}

		public AreaReference EntityReference
		{
			get
			{
				return _entityReference;
			}
			set
			{
				_entityReference = value;
			}
		}

		public Matrix ModelMatrix
		{
			get
			{
				return _modelMatrix;
			}
			set
			{
				_modelMatrix = value;
			}
		}

		public bool NeedsPortalSky
		{
			get
			{
				return _needsPortalSky;
			}
			set
			{
				_needsPortalSky = value;
			}
		}

		public RenderEntityComponent Parameters
		{
			get
			{
				return _parameters;
			}
		}

		public idBounds ReferenceBounds
		{
			get
			{
				return _referenceBounds;
			}
			set
			{
				_referenceBounds = value;
			}
		}

		public int ViewCount
		{
			get
			{
				return _viewCount;
			}
			set
			{
				_viewCount = value;
			}
		}

		public ViewEntity ViewEntity
		{
			get
			{
				return _viewEntity;
			}
			set
			{
				_viewEntity = value;
			}
		}

		public idRenderWorld World
		{
			get
			{
				return _world;
			}
			set
			{
				_world = value;
			}
		}
		#endregion

		#region Members
		private RenderEntityComponent _parameters;
		private Matrix _modelMatrix; // this is just a rearrangement of parms.axis and parms.origin
		private idRenderWorld _world;

		private int _entityIndex; // in world entityDefs

		private int _lastModifiedFrameNum;	// to determine if it is constantly changing,
											// and should go in the dynamic frame memory, or kept
											// in the cached memory

		private bool _archived; // for demo writing

		private idRenderModel _dynamicModel; // if parms.model->IsDynamicModel(), this is the generated data
		private int	_dynamicModelFrameCount; // continuously animating dynamic models will recreate
											 // dynamicModel if this doesn't == tr.viewCount
		private idRenderModel _cachedDynamicModel;

		private idBounds _referenceBounds; // the local bounds used to place entityRefs, either from parms or a model

		// a viewEntity_t is created whenever a idRenderEntityLocal is considered for inclusion
		// in a given view, even if it turns out to not be visible
		private int _viewCount;	// if tr.viewCount == viewCount, viewEntity is valid,
								// but the entity may still be off screen
		private ViewEntity _viewEntity; // in frame temporary memory
		private int _visibleCount;

		// if tr.viewCount == visibleCount, at least one ambient
		// surface has actually been added by R_AddAmbientDrawsurfs
		// note that an entity could still be in the view frustum and not be visible due
		// to portal passing

		/*idRenderModelDecal *	decals;					// chain of decals that have been projected on this model
		idRenderModelOverlay *	overlay;				// blood overlays on animated models*/

		private AreaReference _entityReference;
		/*private idInteraction _firstInteraction; // doubly linked list
		private idInteraction _lastInteraction;*/
		
		private bool _needsPortalSky;
		#endregion

		#region Constructor
		public idRenderEntity()
		{
			_parameters = new RenderEntityComponent();
			_modelMatrix = Matrix.Identity;
			_referenceBounds = idBounds.Zero;
		}
		#endregion
	
			/*virtual void			FreeRenderEntity();
	virtual void			UpdateRenderEntity( const renderEntity_t *re, bool forceUpdate = false );
	virtual void			GetRenderEntity( renderEntity_t *re );
	virtual void			ForceUpdate();
	virtual int				GetIndex();

	// overlays are extra polygons that deform with animating models for blood and damage marks
	virtual void			ProjectOverlay( const idPlane localTextureAxis[2], const idMaterial *material );
	virtual void			RemoveDecals();
		*/
	}
}
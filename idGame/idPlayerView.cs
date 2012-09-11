using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Game.Entities;
using idTech4.Math;
using idTech4.Renderer;

namespace idTech4.Game
{
	public class idPlayerView
	{
		#region Properties
		/// <summary>
		/// Gets the current kick angle.
		/// </summary>
		/// <remarks>
		/// kickVector, a world space direction that the attack should.
		/// </remarks>
		public idAngles AngleOffset
		{
			get
			{
				idAngles angles = new idAngles();

				if(idR.Game.Time < _kickFinishTime)
				{
					float offset = _kickFinishTime - idR.Game.Time;
					angles = _kickAngles * offset * offset * idR.CvarSystem.GetFloat("g_kickAmplitude");

					for(int i = 0; i < 3; i++)
					{
						if(angles.Get(i) > 70.0f)
						{
							angles.Set(i, 70.0f);
						}
						else if(angles.Get(i) < -70.0f)
						{
							angles.Set(i, -70.0f);
						}
					}
				}

				return angles;
			}
		}
		#endregion

		#region Members
		private int _doubleVisionFinishTime; // double vision will be stopped at this time
		private idMaterial _doubleVisionMaterial; // material to take the double vision screen shot

		private int _kickFinishTime; // view kick will be stopped at this time
		private idAngles _kickAngles;
	
		private bool _bfgVisionEnabled;

		private idMaterial _tunnelMaterial;		// health tunnel vision
		private idMaterial _armorMaterial;		// armor damage view effect
		private idMaterial _berserkMaterial;	// berserk effect
		private idMaterial _irGogglesMaterial;	// ir effect
		private idMaterial _bloodSprayMaterial; // blood spray
		private idMaterial _bfgMaterial;		// when targeted with BFG
		private idMaterial _lagoMaterial;		// lagometer drawing
		private float _lastDamageTime;			// accentuate the tunnel effect for a while

		private Vector4 _fadeColor;			// fade color
		private Vector4 _fadeToColor;		// color to fade to
		private Vector4 _fadeFromColor;		// color to fade from
		private float _fadeRate;			// fade rate
		private int _fadeTime;				// fade time

		private idAngles _shakeAngles;		// from the sound sources

		private ScreenBlob[] _screenBlobs;

		private idPlayer _player;
		private idRenderView _view;
		#endregion

		#region Constructor
		public idPlayerView(idPlayer player)
		{
			_player = player;
			_screenBlobs = new ScreenBlob[idR.MaxScreenBlobs];
			_view = new idRenderView();
	
			_doubleVisionMaterial = idR.DeclManager.FindMaterial("_scratch");
			_tunnelMaterial = idR.DeclManager.FindMaterial("textures/decals/tunnel");
			_armorMaterial = idR.DeclManager.FindMaterial("armorViewEffect");
			_berserkMaterial = idR.DeclManager.FindMaterial("textures/decals/berserk");
			_irGogglesMaterial = idR.DeclManager.FindMaterial("textures/decals/irblend");
			_bloodSprayMaterial = idR.DeclManager.FindMaterial("textures/decals/bloodspray");
			_bfgMaterial = idR.DeclManager.FindMaterial("textures/decals/bfgvision");
			_lagoMaterial = idR.DeclManager.FindMaterial(idR.LagometerMaterial, false );

			ClearEffects();
		}
		#endregion

		#region Methods
		#region Public
		public void ClearEffects()
		{		
			_lastDamageTime = (idR.Game.Time - 99999) / 1000;
			_doubleVisionFinishTime = idR.Game.Time - 99999;
			_kickFinishTime = idR.Game.Time - 99999;

			for(int i = 0; i < idR.MaxScreenBlobs; i++)
			{
				_screenBlobs[i].FinishTime = idR.Game.Time;
			}

			_fadeTime = 0;
			_bfgVisionEnabled = false;
		}
		#endregion
		#endregion
	}

	public struct ScreenBlob
	{
		public idMaterial Material;
		public float X;
		public float Y;
		public float Width;
		public float Height;

		public float S1;
		public float T1;
		public float S2;
		public float T2;

		public int FinishTime;
		public int StartFadeTime;
		public float DriftAmount;
	}
}
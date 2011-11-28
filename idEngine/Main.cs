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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace idTech4
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public sealed class Main : Microsoft.Xna.Framework.Game
	{
		#region Properties
		public GameTime Time
		{
			get
			{
				return _gameTime;
			}
		}
		#endregion

		#region Members
		private GameTime _gameTime = new GameTime();
		private GraphicsDeviceManager graphics;
		#endregion

		#region Constructor
		public Main()
			: base()
		{
			idE.Game = this;
			
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}
		#endregion

		#region Methods
		#region Initialization
		private void CreateConsole()
		{
			// don't show it now that we have a splash screen up
			// TODO
			/*if(win32.win_viewlog.GetBool())*/
			{
				idE.SystemConsole.Show();
				idE.SystemConsole.FocusInput();
			}

			idConsole.ClearInputHistory();
		}

		private void InitializeSystem()
		{

		}
		#endregion
		#endregion

		#region Game implementation
		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{			
			// TODO
			/*const HCURSOR hcurSave = ::SetCursor( LoadCursor( 0, IDC_WAIT ) );
			Sys_SetPhysicalWorkMemory( 192 << 20, 1024 << 20 );
			Sys_GetCurrentMemoryStatus( exeLaunchMemoryStats );*/

			/*win32.hInstance = hInstance;
			idStr::Copynz( sys_cmdline, lpCmdLine, sizeof( sys_cmdline ) );
					*/

			// done before Com/Sys_Init since we need this for error output
			CreateConsole();

			idE.System.Init(this.LaunchParameters);
			/*
			#if TEST_FPU_EXCEPTIONS != 0
				common->Printf( Sys_FPU_GetState() );
			#endif

			#ifndef	ID_DEDICATED
				if ( win32.win_notaskkeys.GetInteger() ) {
					DisableTaskKeys( TRUE, FALSE, /*( win32.win_notaskkeys.GetInteger() == 2 )*/
				/* FALSE );
				}
			#endif

			Sys_StartAsyncThread();

			// hide or show the early console as necessary
			if ( win32.win_viewlog.GetInteger() || com_skipRenderer.GetBool() || idAsyncNetwork::serverDedicated.GetInteger() ) {
				Sys_ShowConsole( 1, true );
			} else {
				Sys_ShowConsole( 0, false );
			}

			#ifdef SET_THREAD_AFFINITY 
				// give the main thread an affinity for the first cpu
				SetThreadAffinityMask( GetCurrentThread(), 1 );
			#endif

			::SetCursor( hcurSave );

			// Launch the script debugger
			if ( strstr( lpCmdLine, "+debugger" ) ) {
				// DebuggerClientInit( lpCmdLine );
				return 0;
			}

			::SetFocus( win32.hWnd );

			// main game loop
			while( 1 ) {

				Win_Frame();

				#ifdef DEBUG
				Sys_MemFrame();
				#endif

				// set exceptions, even if some crappy syscall changes them!
				Sys_FPU_EnableExceptions( TEST_FPU_EXCEPTIONS );

				#ifdef ID_ALLOW_TOOLS
				if ( com_editors ) {
					if ( com_editors & EDITOR_GUI ) {
						// GUI editor
						GUIEditorRun();
					} else if ( com_editors & EDITOR_RADIANT ) {
						// Level Editor
						RadiantRun();
					}
					else if (com_editors & EDITOR_MATERIAL ) {
						//BSM Nerve: Add support for the material editor
						MaterialEditorRun();
					}
					else {
						if ( com_editors & EDITOR_LIGHT ) {
							// in-game Light Editor
							LightEditorRun();
						}
						if ( com_editors & EDITOR_SOUND ) {
							// in-game Sound Editor
							SoundEditorRun();
						}
						if ( com_editors & EDITOR_DECL ) {
							// in-game Declaration Browser
							DeclBrowserRun();
						}
						if ( com_editors & EDITOR_AF ) {
							// in-game Articulated Figure Editor
							AFEditorRun();
						}
						if ( com_editors & EDITOR_PARTICLE ) {
							// in-game Particle Editor
							ParticleEditorRun();
						}
						if ( com_editors & EDITOR_SCRIPT ) {
							// in-game Script Editor
							ScriptEditorRun();
						}
						if ( com_editors & EDITOR_PDA ) {
							// in-game PDA Editor
							PDAEditorRun();
						}
					}
				}
				#endif
				
				// run the game
				common->Frame();
			}*/

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// TODO: use this.Content to load your game content here
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			_gameTime = gameTime;
			// TODO: Add your update logic here



			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			// TODO: Add your drawing code here

			base.Draw(gameTime);
		}
		#endregion
	}
}

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
using Microsoft.Xna.Framework;

using idTech4.Services;

namespace idTech4
{
	public class CVars
	{
		public static void Register()
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			#region Common
			cvarSystem.Register("developer",				"0", "developer mode",											CVarFlags.Bool | CVarFlags.System | CVarFlags.NoCheat);
			cvarSystem.Register("logFile",					"0", 0, 2, "1 = buffer log, 2 = flush after each print",		CVarFlags.System | CVarFlags.NoCheat, new ArgCompletion_Integer(0, 2));
			cvarSystem.Register("logFileName",				"qconsole2.log", "name of log file, if empty, qconsole.log will be used", CVarFlags.System | CVarFlags.NoCheat);

#if ID_RETAIL
			cvarSystem.Register("com_allowConsole",			"0", "allow toggling console with the tilde key",				CVarFlags.Bool | CVarFlags.System | CVarFlags.Init);
#else
			cvarSystem.Register("com_allowConsole",			"1", "allow toggling console with the tilde key",				CVarFlags.Bool | CVarFlags.System | CVarFlags.Init);
			cvarSystem.Register("com_printFilter",			"",	"only print lines that contain this, add multiple filters with a ; delimeter", CVarFlags.System);
#endif

			cvarSystem.Register("com_compressDemos",		"1", "Compression scheme for demo files\n0: None    (Fast, large files)\n1: LZW     (Fast to compress, Fast to decompress, medium/small files)\n2: LZSS    (Slow to compress, Fast to decompress, small files)\n3: Huffman (Fast to compress, Slow to decompress, medium files)\nSee also: The 'CompressDemo' command", CVarFlags.System | CVarFlags.Integer | CVarFlags.Archive);
			cvarSystem.Register("com_deltaTimeClamp",		"50", "don't process more than this time in a single frame",	CVarFlags.Integer);
			cvarSystem.Register("com_engineHz",				"60", 10.0f, 1024.0f, "Frames per second the engine runs at",	CVarFlags.Float | CVarFlags.Archive);
			cvarSystem.Register("com_fixedTic",				"0", "run a single game frame per render frame",			CVarFlags.Bool);
			cvarSystem.Register("com_japaneseCensorship",	"0", "Enable Japanese censorship",								CVarFlags.NoCheat);
			cvarSystem.Register("com_journal",				"0", 0, 2, "1 = record journal, 2 = play back journal",			CVarFlags.Init | CVarFlags.System, new ArgCompletion_Integer(0, 2));
			cvarSystem.Register("com_logDemos",				"0", "Write demo.log with debug information in it",				CVarFlags.System | CVarFlags.Bool);
			cvarSystem.Register("com_noSleep",				"0", "don't sleep if the game is running too fast",				CVarFlags.Bool);
			cvarSystem.Register("com_preloadDemos",			"0", "Load the whole demo in to RAM before running it",			CVarFlags.System| CVarFlags.Bool | CVarFlags.Archive);
			cvarSystem.Register("com_productionMode",		"0", "0 - no special behavior, 1 - building a production build, 2 - running a production build", CVarFlags.System | CVarFlags.Bool);
			cvarSystem.Register("com_showFPS",				"0", "show frames rendered per second",							CVarFlags.Bool | CVarFlags.System | CVarFlags.Archive | CVarFlags.NoCheat);
			cvarSystem.Register("com_showMemoryUsage",		"0", "show total and per frame memory usage",					CVarFlags.Bool | CVarFlags.System | CVarFlags.NoCheat);
			cvarSystem.Register("com_sleepGame",			"0", "intentionally add a sleep in the game time",				CVarFlags.System | CVarFlags.Integer);
			cvarSystem.Register("com_sleepDraw",			"0", "intentionally add a sleep in the draw time",				CVarFlags.System | CVarFlags.Integer);
			cvarSystem.Register("com_sleepRender",			"0", "intentionally add a sleep in the render time",			CVarFlags.System | CVarFlags.Integer);
			cvarSystem.Register("com_skipIntroVideos",		"0", "skips intro videos",										CVarFlags.Bool);
			cvarSystem.Register("com_smp",					"1", "run the game and draw code in a separate thread",			CVarFlags.Bool | CVarFlags.System | CVarFlags.NoCheat);
			cvarSystem.Register("com_speeds",				"0", "show engine timings",										CVarFlags.Bool | CVarFlags.System | CVarFlags.NoCheat);			
			cvarSystem.Register("com_timestampPrints",		"0", 0, 2, "print time with each console print, 1 = msec, 2 = sec", CVarFlags.System, new ArgCompletion_Integer(0, 2));
			cvarSystem.Register("com_updateLoadSize",		"0", "update the load size after loading a map",				CVarFlags.Bool | CVarFlags.System | CVarFlags.NoCheat);
			#endregion

			#region Console
			cvarSystem.Register("con_speed",				"3", "speed at which the console moves up and down",					CVarFlags.System);
			cvarSystem.Register("con_notifyTime",			"3", "time messages are displayed onscreen when console is pulled up",	CVarFlags.System);

#if DEBUG
			cvarSystem.Register("con_noPrint",				"0", "print on the console but not onscreen when console is pulled up", CVarFlags.Bool | CVarFlags.System | CVarFlags.NoCheat);
#else
			cvarSystem.Register("con_noPrint",				"1", "print on the console but not onscreen when console is pulled up", CVarFlags.Bool | CVarFlags.System | CVarFlags.NoCheat);
#endif
			#endregion

			#region Decl
			cvarSystem.Register("decl_show", "0", 0, 2, "set to 1 to print parses, 2 to also print references", CVarFlags.System, new ArgCompletion_Integer(0, 2));
			#endregion

			#region Filesystem
			cvarSystem.Register("fs_basepath",					"", "",												CVarFlags.System | CVarFlags.Init);
			cvarSystem.Register("fs_buildresources",			"0", "copy every file touched to a resource file",	CVarFlags.System | CVarFlags.Init | CVarFlags.Bool);
			cvarSystem.Register("fs_copyfiles",					"0", "copy every file touched to fs_savepath",		CVarFlags.System | CVarFlags.Init | CVarFlags.Bool);
			cvarSystem.Register("fs_debug",						"0", 0, 2, "",										CVarFlags.System | CVarFlags.Bool, new ArgCompletion_Integer(0, 2));
			cvarSystem.Register("fs_debugBGL",					"0", "",											CVarFlags.System | CVarFlags.Bool);
			cvarSystem.Register("fs_debugResources",			"0", "",											CVarFlags.System | CVarFlags.Bool);
			cvarSystem.Register("fs_enableBackgroundCaching",	"1", "if 1 allow the 360 to precache game files in the background", CVarFlags.System);
			cvarSystem.Register("fs_enableBGL",					"0", "",											CVarFlags.System | CVarFlags.Bool);
			cvarSystem.Register("fs_game",						"", "mod path",										CVarFlags.System | CVarFlags.Init | CVarFlags.ServerInfo);
			cvarSystem.Register("fs_game_base",					"", "alternate mod path, searched after the main fs_game path, before the basedir", CVarFlags.System | CVarFlags.Init | CVarFlags.ServerInfo);
			cvarSystem.Register("fs_resourceLoadPriority",		"1", "if 1, open requests will be honored from resource files first; if 0, the resource files are checked after normal search paths", CVarFlags.System);
			cvarSystem.Register("fs_savepath",					"", "",												CVarFlags.System | CVarFlags.Init);
			#endregion

			#region Game
			cvarSystem.Register("g_demoMode",			"0", "this is a demo", CVarFlags.Integer);
			cvarSystem.Register("g_useNewGuiCode",		"1", "use optimized device context code, 2 = toggle on/off every frame",	CVarFlags.Game | CVarFlags.Integer);
			cvarSystem.Register("g_useOldPDAStrings",	"0", "read strings from the .pda files rather than from the .lang file", CVarFlags.Bool);
			#endregion

			#region GUI
			cvarSystem.Register("gui_debug",			"0", "",															CVarFlags.Gui | CVarFlags.Bool);
			cvarSystem.Register("gui_edit",				"0", "",															CVarFlags.Gui | CVarFlags.Bool);
			#endregion

			#region Misc.
			cvarSystem.Register("timescale",					"1", 0.001f, 100.0f, "Number of game frames to run per render frame",	CVarFlags.System | CVarFlags.Float);
			#endregion

			#region Network
			cvarSystem.Register("net_migrateHost",				"-1", "Become host of session (0 = party, 1 = game) for testing purposes", CVarFlags.Integer);
			#endregion

			#region Renderer
			cvarSystem.Register("r_clear",						"2",		"force screen clear every frame, 1 = purple, 2 = black, 'r g b' = custom", CVarFlags.Renderer);
			cvarSystem.Register("r_customHeight",				"720",		"custom screen height. set r_vidMode to -1 to activate",	CVarFlags.Renderer | CVarFlags.Archive | CVarFlags.Integer);
			cvarSystem.Register("r_customWidth",				"1280",		"custom screen width. set r_vidMode to -1 to activate",		CVarFlags.Renderer | CVarFlags.Archive | CVarFlags.Integer);
			cvarSystem.Register("r_debugRenderToTexture",		"0",		"",															CVarFlags.Renderer | CVarFlags.Integer);
			cvarSystem.Register("r_displayRefresh",				"0", 0.0f, 240.0f, "optional display refresh rate option for vid mode", CVarFlags.Renderer | CVarFlags.NoCheat | CVarFlags.Integer);
			cvarSystem.Register("r_drawEyeColor",				"0",		"Draw a colored box, red = left eye, blue = right eye, grey = non-stereo", CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_drawFlickerBox",				"0",		"visual test for dropping frames",							CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_fullscreen",					"1",		"0 = windowed, 1 = full screen on monitor 1, 2 = full screen on monitor 2, etc", CVarFlags.Renderer | CVarFlags.Archive | CVarFlags.Integer);
			cvarSystem.Register("r_lightScale",					"3",		"all light intensities are multiplied by this",				CVarFlags.Archive | CVarFlags.Renderer | CVarFlags.Float);
			cvarSystem.Register("r_logFile",					"0",		"number of frames to emit GL logs",							CVarFlags.Renderer | CVarFlags.Integer);
			cvarSystem.Register("r_motionBlur",					"0",		 "1 - 5, log2 of the number of motion blur samples",		CVarFlags.Renderer | CVarFlags.Integer | CVarFlags.Archive);
			cvarSystem.Register("r_multiSamples",				"0",		"number of antialiasing samples",							CVarFlags.Renderer | CVarFlags.Archive | CVarFlags.Integer);
							
			// visual debugging info
			cvarSystem.Register("r_showAddModel",				"0",		"report stats from tr_addModel",							CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showCull",					"0",		"report sphere and box culling stats",						CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showDemo",					"0",		"report reads and writes to the demo file",					CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showDepth",					"0",		"display the contents of the depth buffer and the depth range", CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showDominantTri",			"0",		"draw lines from vertexes to center of dominant triangles", CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showDynamic",				"0",		"report stats on dynamic surface generation",				CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showEdges",					"0",		"draw the sil edges",										CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showIntensity",				"0",		"draw the screen colors based on intensity, red = 0, green = 128, blue = 255", CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showLightCount",				"0", 0, 3,	"1 = colors surfaces based on light count, 2 = also count everything through walls, 3 = also print overdraw", CVarFlags.Renderer | CVarFlags.Integer, new ArgCompletion_Integer(0, 3));
			cvarSystem.Register("r_showLights",					"0", 0, 3,	"1 = just print volumes numbers, highlighting ones covering the view, 2 = also draw planes of each volume, 3 = also draw edges of each volume", CVarFlags.Renderer | CVarFlags.Integer, new ArgCompletion_Integer(0, 3));
			cvarSystem.Register("r_showLightScissors",			"0",		"show light scissor rectangles",							CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showMemory",					"0",		"print frame memory utilization",							CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showNormals",				"0",		"draws wireframe normals",									CVarFlags.Renderer | CVarFlags.Float);
			cvarSystem.Register("r_showOverDraw",				"0", 0, 3,	"1 = geometry overdraw, 2 = light interaction overdraw, 3 = geometry and light interaction overdraw", CVarFlags.Renderer | CVarFlags.Integer, new ArgCompletion_Integer(0, 3));
			cvarSystem.Register("r_showPortals",				"0",		"draw portal outlines in color based on passed / not passed", CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showPrimitives",				"0",		"report drawsurf/index/vertex counts",						CVarFlags.Renderer | CVarFlags.Integer);
			cvarSystem.Register("r_showShadows",				"0", 0, 3,	"1 = visualize the stencil shadow volumes, 2 = draw filled in", CVarFlags.Renderer | CVarFlags.Integer, new ArgCompletion_Integer(0, 3));
			cvarSystem.Register("r_showSilhouette",				"0",		"highlight edges that are casting shadow planes",			CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showSurfaces",				"0",		"report surface/light/shadow counts",						CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showSurfaceInfo",			"0",		"show surface material name under crosshair",				CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showSwapBuffers",			"0",		"Show timings from GL_BlockingSwapBuffers",					CVarFlags.Bool); 
			cvarSystem.Register("r_showTangentSpace",			"0", 0, 3,	"shade triangles by tangent space, 1 = use 1st tangent vector, 2 = use 2nd tangent vector, 3 = use normal vector", CVarFlags.Renderer | CVarFlags.Integer, new ArgCompletion_Integer(0, 3));
			cvarSystem.Register("r_showTexturePolarity",		"0",		"shade triangles by texture area polarity",					CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showTextureVectors",			"0",		"if > 0 draw each triangles texture (tangent) vectors",		CVarFlags.Renderer | CVarFlags.Float);
			cvarSystem.Register("r_showTrace",					"0", 0, 2,	"show the intersection of an eye trace with the world",		CVarFlags.Renderer | CVarFlags.Integer, new ArgCompletion_Integer(0, 3));
			cvarSystem.Register("r_showTris",					"0", 0, 4,	"enables wireframe rendering of the world, 1 = only draw visible ones, 2 = draw all front facing, 3 = draw all, 4 = draw with alpha", CVarFlags.Renderer | CVarFlags.Integer, new ArgCompletion_Integer(0, 4));
			cvarSystem.Register("r_showUnsmoothedTangents",		"0",		"if 1, put all nvidia register combiner programming in display lists", CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showUpdates",				"0",		"report entity and light updates and ref counts",			CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showVertexColor",			"0",		"draws all triangles with the solid vertex color",			CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_showViewEntitys",			"0", 0, 2,	"1 = displays the bounding boxes of all view models, 2 = print index numbers", CVarFlags.Renderer | CVarFlags.Integer, new ArgCompletion_Integer(0, 2));

			cvarSystem.Register("r_skipAmbient",				"0",		"bypasses all non-interaction drawing",						CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipBackEnd",				"0",		"don't draw anything",										CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipBlendLights",			"0",		"skip all blend lights",									CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipBump",					"0",		"uses a flat surface instead of the bump map",				CVarFlags.Renderer | CVarFlags.Archive | CVarFlags.Bool);
			cvarSystem.Register("r_skipCopyTexture",			"0",		"do all rendering, but don't actually copyTexSubImage2D",	CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipDecals",					"0",		"skip decal surfaces",										CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipDeforms",				"0",		"leave all deform materials in their original state",		CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipDiffuse",				"0",		"use black for diffuse",									CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipDynamicInteractions",	"0",		"skip interactions created after level load",				CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipDynamicTextures",		"0",		"don't dynamically create textures",						CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipFogLights",				"0",		"skip all fog lights",										CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipFrontEnd",				"0",		"bypasses all front end work, but 2D gui rendering still draws", CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipGuiShaders",				"0", 0, 3,	"1 = skip all gui elements on surfaces, 2 = skip drawing but still handle events, 3 = draw but skip events", CVarFlags.Renderer | CVarFlags.Integer, new ArgCompletion_Integer(0, 3));
			cvarSystem.Register("r_skipInteractions",			"0",		"skip all light/surface interaction drawing",				CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipNewAmbient",				"0",		"bypasses all vertex/fragment program ambient drawing",		CVarFlags.Renderer | CVarFlags.Archive | CVarFlags.Bool);
			cvarSystem.Register("r_skipOverlays",				"0",		"skip overlay surfaces",									CVarFlags.Renderer | CVarFlags.Bool);			
			cvarSystem.Register("r_skipParticles",				"0", 0, 1,	"1 = skip all particle systems",							CVarFlags.Renderer | CVarFlags.Integer, new ArgCompletion_Integer(0, 1));
			cvarSystem.Register("r_skipPostProcess",			"0",		"skip all post-process renderings",							CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipRender",					"0",		"skip 3D rendering, but pass 2D",							CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipRenderContext",			"0",		"NULL the rendering context during backend 3D rendering",	CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipShaderPasses",			"0",		"",															CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipShadows",				"0",		"disable shadows",											CVarFlags.Renderer | CVarFlags.Archive | CVarFlags.Bool);
			cvarSystem.Register("r_skipSpecular",				"0",		"use black for specular1",									CVarFlags.Renderer | CVarFlags.Archive | CVarFlags.Cheat | CVarFlags.Bool);
			cvarSystem.Register("r_skipStaticInteractions",		"0",		"skip interactions created at level load",					CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipSubviews",				"0",		"1 = don't render any gui elements on surfaces",			CVarFlags.Renderer | CVarFlags.Integer);
			cvarSystem.Register("r_skipSuppress",				"0",		"ignore the per-view suppressions",							CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipTranslucent",			"0",		"skip the translucent interaction rendering",				CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_skipUpdates",				"0",		"1 = don't accept any entity or light updates, making everything static", CVarFlags.Renderer | CVarFlags.Bool);

			cvarSystem.Register("r_swapInterval",				"1",		"0 = tear, 1 = swap-tear where available, 2 = always v-sync", CVarFlags.Renderer | CVarFlags.Archive | CVarFlags.Integer);

			cvarSystem.Register("r_useConstantMaterials",		"1",		"use pre-calculated material registers if possible",		CVarFlags.ReadOnly | CVarFlags.Bool);
			cvarSystem.Register("r_useEntityPortalCulling",		"1", 0, 2,	"0 = none, 1 = cull frustum corners to plane, 2 = exact clip the frustum faces", CVarFlags.Renderer | CVarFlags.Integer, new ArgCompletion_Integer(0, 2));
			cvarSystem.Register("r_useLightAreaCulling",		"1",		"0 = off, 1 = on",											CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_useLightPortalCulling",		"1", 0, 2,	"0 = none, 1 = cull frustum corners to plane, 2 = exact clip the frustum faces", CVarFlags.Renderer | CVarFlags.Integer, new ArgCompletion_Integer(0, 2));
			cvarSystem.Register("r_useLightScissors",			"3", 0, 3,	"0 = no scissor, 1 = non-clipped scissor, 2 = near-clipped scissor, 3 = fully-clipped scissor", CVarFlags.Renderer | CVarFlags.Integer, new ArgCompletion_Integer(0, 3));
			cvarSystem.Register("r_useScissor",					"1",		"scissor clip as portals and lights are processed",			CVarFlags.Renderer | CVarFlags.Bool);
			cvarSystem.Register("r_useStateCaching",			"1",         "avoid redundant state changes",                           CVarFlags.Renderer | CVarFlags.Bool);

			cvarSystem.Register("r_vidMode",					"0",		"fullscreen video mode number",								CVarFlags.Renderer | CVarFlags.Archive | CVarFlags.Integer);
			cvarSystem.Register("r_windowHeight",				"720",		"Non-fullscreen parameter",									CVarFlags.Renderer | CVarFlags.Archive | CVarFlags.Integer);
			cvarSystem.Register("r_windowWidth",				"1280",		"Non-fullscreen parameter",									CVarFlags.Renderer | CVarFlags.Archive | CVarFlags.Integer);
			cvarSystem.Register("r_windowX",					"0",		"Non-fullscreen parameter",									CVarFlags.Renderer | CVarFlags.Archive | CVarFlags.Integer);
			cvarSystem.Register("r_windowY",					"0",		"Non-fullscreen parameter",									CVarFlags.Renderer | CVarFlags.Archive | CVarFlags.Integer);
			#endregion

			#region Resolution Scale
			cvarSystem.Register("rs_display",					"0",		"0 - percentages, 1 - pixels per frame",					CVarFlags.Integer);
			cvarSystem.Register("rs_dropFraction",				"0.11",		"Drop the resolution in increments of this",				CVarFlags.Float);
			cvarSystem.Register("rs_dropMilliseconds",			"15.0",		"Drop the resolution when GPU time exceeds this",			CVarFlags.Float);
			cvarSystem.Register("rs_enable",					"1",		"Enable dynamic resolution scaling, 0 - off, 1 - horz only, 2 - vert only, 3 - both", CVarFlags.Integer);
			cvarSystem.Register("rs_forceFractionX",			"0",		"Force a specific 0.0 to 1.0 horizontal resolution scale",	CVarFlags.Float);
			cvarSystem.Register("rs_forceFractionY",			"0",		"Force a specific 0.0 to 1.0 vertical resolution scale",	CVarFlags.Float);
			cvarSystem.Register("rs_raiseFraction",				"0.06",		"Raise the resolution in increments of this",				CVarFlags.Float);
			cvarSystem.Register("rs_raiseFrames",				"5",		"Require this many frames below rs_raiseMilliseconds",		CVarFlags.Integer);
			cvarSystem.Register("rs_raiseMilliseconds",			"13.0",		"Raise the resolution when GPU time is below this for several frames", CVarFlags.Float);
			cvarSystem.Register("rs_showResolutionChanges",		"0",		"1 = Print whenever the resolution scale changes, 2 = always",	CVarFlags.Integer);
			#endregion

			#region Stereo
			cvarSystem.Register("stereoRender_defaultGuiDepth", "0", "Fraction of separation when not specified",						CVarFlags.Renderer);
			#endregion

			#region System
			cvarSystem.Register("sys_arch",			"",					"", CVarFlags.System | CVarFlags.Init);
			cvarSystem.Register("sys_cpustring",	"detect",			"", CVarFlags.System | CVarFlags.Init);
			cvarSystem.Register("sys_lang",			idLanguage.English, "", CVarFlags.System | CVarFlags.Init/* TODO: sysLanguageNames, idCmdSystem::ArgCompletion_String<sysLanguageNames>*/);
			#endregion

			#region SWF
			cvarSystem.Register("swf_debug",						"0",		"debug swf scripts.  1 shows traces/errors.  2 also shows warnings.  3 also shows disassembly.  4 shows parameters in the disassembly.", CVarFlags.Integer | CVarFlags.Archive);
			cvarSystem.Register("swf_debugInvoke",					"0",		"debug swf functions being called from game.",	CVarFlags.Integer);
			cvarSystem.Register("swf_forceAlpha",					"0", 0.0f, 1.0f, "force an alpha value on all elements, useful to show invisible animating elements", CVarFlags.Float);
			cvarSystem.Register("swf_stopat",						"0",		"stop at a specific frame",						CVarFlags.Float);
			cvarSystem.Register("swf_subtitleEarlyTrans",			"3500",		"early time out to switch the line",			CVarFlags.Integer);
			cvarSystem.Register("swf_subtitleExtraTime",			"3500",		"time after subtitles vo is complete",			CVarFlags.Integer);
			cvarSystem.Register("swf_subtitleLengthGuess",			"10000",	"early time out to switch the line",			CVarFlags.Integer);
		
			cvarSystem.Register("swf_textMaxInputLength",			"104",		"max number of characters that can go into the input line", CVarFlags.Integer);			
			cvarSystem.Register("swf_textParagraphInc",				"1.3",		"scroll speed for text",						CVarFlags.Float);
			cvarSystem.Register("swf_textParagraphSpeed",			"15",		"scroll speed for text",						CVarFlags.Integer);			
			cvarSystem.Register("swf_textRndLetterSpeed",			"8",		"scroll speed for text",						CVarFlags.Integer);
			cvarSystem.Register("swf_textRndLetterDelay",			"100",		"scroll speed for text",						CVarFlags.Integer);
			cvarSystem.Register("swf_textScrollSpeed",				"80",		"scroll speed for text",						CVarFlags.Integer);
			cvarSystem.Register("swf_textStrokeSize",				"1.65f", 0.0f, 2.0f, "size of font glyph stroke",			CVarFlags.Float);
			cvarSystem.Register("swf_textStrokeSizeGlyphSpacer",	"1.5f",		"additional space for spacing glyphs using stroke", CVarFlags.Float);

			cvarSystem.Register("swf_timescale",					"1",		"timescale for swf files",						CVarFlags.Float);
			cvarSystem.Register("swf_titleSafe",					"0.005", 0.0f, 0.075f, "space between UI elements and screen edge", CVarFlags.Float);
			#endregion

			#region Windows
			cvarSystem.Register("win_allowAltTab",				"0", "allow Alt-Tab when fullscreen",					CVarFlags.System | CVarFlags.Bool);
			cvarSystem.Register("win_notaskkeys",				"0", "disable windows task keys",						CVarFlags.System | CVarFlags.Integer);
			cvarSystem.Register("win_username",					"",  "windows user name",								CVarFlags.System | CVarFlags.Init);
			cvarSystem.Register("win_outputEditString",			"1", "",												CVarFlags.System | CVarFlags.Bool);
			cvarSystem.Register("win_viewlog",					"0", "",												CVarFlags.System | CVarFlags.Integer);			
			cvarSystem.Register("win_allowMultipleInstances",	"0", "allow multiple instances running concurrently",	CVarFlags.System | CVarFlags.Bool);
			#endregion
			
			/*new idCVar("in_mouse", "1", CVAR_SYSTEM | CVAR_BOOL, "enable mouse input" );
			static idCVar lcp_showFailures( "lcp_showFailures", "0", CVAR_BOOL, "show LCP solver failures" );

			idCVar com_assertOutOfDebugger( "com_assertOutOfDebugger", "0", CVAR_BOOL, "by default, do not assert while not running under the debugger" );

			static idCVar jobs_longJobMicroSec( "jobs_longJobMicroSec", "10000", CVAR_INTEGER, "print a warning for jobs that take more than this number of microseconds" );
			static idCVar jobs_prioritize( "jobs_prioritize", "1", CVAR_BOOL | CVAR_NOCHEAT, "prioritize job lists" );
			idCVar jobs_numThreads( "jobs_numThreads", NUM_JOB_THREADS, CVAR_INTEGER | CVAR_NOCHEAT, "number of threads used to crunch through jobs", 0, MAX_JOB_THREADS );

			idCVar cm_drawMask(			"cm_drawMask",			"none",		CVAR_GAME,				"collision mask", cm_contentsNameByIndex, idCmdSystem::ArgCompletion_String<cm_contentsNameByIndex> );
idCVar cm_drawColor(		"cm_drawColor",			"1 0 0 .5",	CVAR_GAME,				"color used to draw the collision models" );
idCVar cm_drawFilled(		"cm_drawFilled",		"0",		CVAR_GAME | CVAR_BOOL,	"draw filled polygons" );
idCVar cm_drawInternal(		"cm_drawInternal",		"1",		CVAR_GAME | CVAR_BOOL,	"draw internal edges green" );
idCVar cm_drawNormals(		"cm_drawNormals",		"0",		CVAR_GAME | CVAR_BOOL,	"draw polygon and edge normals" );
idCVar cm_backFaceCull(		"cm_backFaceCull",		"0",		CVAR_GAME | CVAR_BOOL,	"cull back facing polygons" );
idCVar cm_debugCollision(	"cm_debugCollision",	"0",		CVAR_GAME | CVAR_BOOL,	"debug the collision detection" );

			static idCVar cm_testCollision(		"cm_testCollision",		"0",					CVAR_GAME | CVAR_BOOL,		"" );
static idCVar cm_testRotation(		"cm_testRotation",		"1",					CVAR_GAME | CVAR_BOOL,		"" );
static idCVar cm_testModel(			"cm_testModel",			"0",					CVAR_GAME | CVAR_INTEGER,	"" );
static idCVar cm_testTimes(			"cm_testTimes",			"1000",					CVAR_GAME | CVAR_INTEGER,	"" );
static idCVar cm_testRandomMany(	"cm_testRandomMany",	"0",					CVAR_GAME | CVAR_BOOL,		"" );
static idCVar cm_testOrigin(		"cm_testOrigin",		"0 0 0",				CVAR_GAME,					"" );
static idCVar cm_testReset(			"cm_testReset",			"0",					CVAR_GAME | CVAR_BOOL,		"" );
static idCVar cm_testBox(			"cm_testBox",			"-16 -16 0 16 16 64",	CVAR_GAME,					"" );
static idCVar cm_testBoxRotation(	"cm_testBoxRotation",	"0 0 0",				CVAR_GAME,					"" );
static idCVar cm_testWalk(			"cm_testWalk",			"1",					CVAR_GAME | CVAR_BOOL,		"" );
static idCVar cm_testLength(		"cm_testLength",		"1024",					CVAR_GAME | CVAR_FLOAT,		"" );
static idCVar cm_testRadius(		"cm_testRadius",		"64",					CVAR_GAME | CVAR_FLOAT,		"" );
static idCVar cm_testAngle(			"cm_testAngle",			"60",					CVAR_GAME | CVAR_FLOAT,		"" );

			idCVar preLoad_Collision( "preLoad_Collision", "1", CVAR_SYSTEM | CVAR_BOOL, "preload collision beginlevelload" );

			

			idCVar binaryLoadParticles( "binaryLoadParticles", "1", 0, "enable binary load/write of particle decls" );

			

			#ifdef ID_RETAIL
idCVar net_allowCheats( "net_allowCheats", "0", CVAR_BOOL | CVAR_ROM, "Allow cheats in multiplayer" );
#else
idCVar net_allowCheats( "net_allowCheats", "0", CVAR_BOOL | CVAR_NOCHEAT, "Allow cheats in multiplayer" );
#endif

			idCVar com_version( "si_version", version.string, CVAR_SYSTEM|CVAR_ROM|CVAR_SERVERINFO, "engine version" );
idCVar com_forceGenericSIMD( "com_forceGenericSIMD", "0", CVAR_BOOL | CVAR_SYSTEM | CVAR_NOCHEAT, "force generic platform independent SIMD" );

			

idCVar preload_CommonAssets( "preload_CommonAssets", "1", CVAR_SYSTEM | CVAR_BOOL, "preload common assets" );

idCVar net_inviteOnly( "net_inviteOnly", "1", CVAR_BOOL | CVAR_ARCHIVE, "whether or not the private server you create allows friends to join or invite only" );
			
		

			idCVar popupDialog_debug( "popupDialog_debug", "0", CVAR_BOOL | CVAR_ARCHIVE, "display debug spam" );

			idCVar dialog_saveClearLevel( "dialog_saveClearLevel", "1000", CVAR_INTEGER, "Time required to show long message" );

			


idCVar com_aviDemoSamples( "com_aviDemoSamples", "16", CVAR_SYSTEM, "" );
idCVar com_aviDemoWidth( "com_aviDemoWidth", "256", CVAR_SYSTEM, "" );
idCVar com_aviDemoHeight( "com_aviDemoHeight", "256", CVAR_SYSTEM, "" );
idCVar com_skipGameDraw( "com_skipGameDraw", "0", CVAR_SYSTEM | CVAR_BOOL, "" );



idCVar net_drawDebugHud( "net_drawDebugHud", "0", CVAR_SYSTEM | CVAR_INTEGER, "0 = None, 1 = Hud 1, 2 = Hud 2, 3 = Snapshots" );



idCVar com_wipeSeconds( "com_wipeSeconds", "1", CVAR_SYSTEM, "" );
idCVar com_disableAutoSaves( "com_disableAutoSaves", "0", CVAR_SYSTEM | CVAR_BOOL, "" );
idCVar com_disableAllSaves( "com_disableAllSaves", "0", CVAR_SYSTEM | CVAR_BOOL, "" );

			idCVar com_product_lang_ext( "com_product_lang_ext", "1", CVAR_INTEGER | CVAR_SYSTEM | CVAR_ARCHIVE, "Extension to use when creating language files." );

			idCVar net_clientMaxPrediction( "net_clientMaxPrediction", "5000", CVAR_SYSTEM | CVAR_INTEGER | CVAR_NOCHEAT, "maximum number of milliseconds a client can predict ahead of server." );
idCVar net_snapRate( "net_snapRate", "100", CVAR_SYSTEM | CVAR_INTEGER, "How many milliseconds between sending snapshots" );
idCVar net_ucmdRate( "net_ucmdRate", "40", CVAR_SYSTEM | CVAR_INTEGER, "How many milliseconds between sending usercmds" );

idCVar net_debug_snapShotTime( "net_debug_snapShotTime", "0", CVAR_BOOL | CVAR_ARCHIVE, "" );
idCVar com_forceLatestSnap( "com_forceLatestSnap", "0", CVAR_BOOL, "" );

			// Enables effective snap rate: dynamically adjust the client snap rate based on:
//	-client FPS
//	-server FPS (interpolated game time received / interval it was received over)
//  -local buffered time (leave a cushion to absorb spikes, slow down when infront of it, speed up when behind it) ie: net_minBufferedSnapPCT_Static
idCVar net_effectiveSnapRateEnable( "net_effectiveSnapRateEnable", "1", CVAR_BOOL, "Dynamically adjust client snaprate");
idCVar net_effectiveSnapRateDebug( "net_effectiveSnapRateDebug", "0", CVAR_BOOL, "Debug");

			// Min buffered snapshot time to keep as a percentage of the effective snaprate
//	-ie we want to keep 50% of the amount of time difference between last two snaps.
//	-we need to scale this because we may get throttled at the snaprate may change
//  -Acts as a buffer to absorb spikes
idCVar net_minBufferedSnapPCT_Static( "net_minBufferedSnapPCT_Static", "1.0", CVAR_FLOAT, "Min amount of snapshot buffer time we want need to buffer");
idCVar net_maxBufferedSnapMS( "net_maxBufferedSnapMS", "336", CVAR_INTEGER, "Max time to allow for interpolation cushion");
idCVar net_minBufferedSnapWinPCT_Static( "net_minBufferedSnapWinPCT_Static", "1.0", CVAR_FLOAT, "Min amount of snapshot buffer time we want need to buffer");

// Factor at which we catch speed up interpolation if we fall behind our optimal interpolation window
//  -This is a static factor. We may experiment with a dynamic one that would be faster the farther you are from the ideal window
idCVar net_interpolationCatchupRate( "net_interpolationCatchupRate", "1.3", CVAR_FLOAT, "Scale interpolationg rate when we fall behind");
idCVar net_interpolationFallbackRate( "net_interpolationFallbackRate", "0.95", CVAR_FLOAT, "Scale interpolationg rate when we fall behind");
idCVar net_interpolationBaseRate( "net_interpolationBaseRate", "1.0", CVAR_FLOAT, "Scale interpolationg rate when we fall behind");

// Enabled a dynamic ideal snap buffer window: we will scale the distance and size 
idCVar net_optimalDynamic( "net_optimalDynamic", "1", CVAR_BOOL, "How fast to add to our optimal time buffer when we are playing snapshots faster than server is feeding them to us");

// These values are used instead if net_optimalDynamic is 0 (don't scale by actual snap rate/interval)
idCVar net_optimalSnapWindow( "net_optimalSnapWindow", "112", CVAR_FLOAT, "");
idCVar net_optimalSnapTime( "net_optimalSnapTime", "112", CVAR_FLOAT, "How fast to add to our optimal time buffer when we are playing snapshots faster than server is feeding them to us");

// this is at what percentage of being ahead of the interpolation buffer that we start slowing down (we ramp down from 1.0 to 0.0 starting here)
// this is a percentage of the total cushion time.
idCVar net_interpolationSlowdownStart( "net_interpolationSlowdownStart", "0.5", CVAR_FLOAT, "Scale interpolation rate when we fall behind");


// Extrapolation is now disabled
idCVar net_maxExtrapolationInMS( "net_maxExtrapolationInMS", "0", CVAR_INTEGER, "Max time in MS that extrapolation is allowed to occur.");

			
			idCVar memcpyImpl( "memcpyImpl", "0", 0, "Which implementation of memcpy to use for idFile_Memory::Write() [0/1 - standard (1 eliminates branch misprediction), 2 - auto-vectorized]" );

			idCVar sgf_threads( "sgf_threads", "2", CVAR_INTEGER, "0 = all foreground, 1 = background write, 2 = background write + compress" );
idCVar sgf_checksums( "sgf_checksums", "1", CVAR_BOOL, "enable save game file checksums" );
idCVar sgf_testCorruption( "sgf_testCorruption", "-1", CVAR_INTEGER, "test corruption at the 128 kB compressed block" );

// this is supposed to get faster going from -15 to -9, but it gets slower as well as worse compression
idCVar sgf_windowBits( "sgf_windowBits", "-15", CVAR_INTEGER, "zlib window bits" );


			

			idCVar zip_numSeeks( "zip_numSeeks", "0", CVAR_INTEGER, "" );
idCVar zip_skippedSeeks( "zip_skippedSeeks", "0", CVAR_INTEGER, "" );
idCVar zip_seeksForward( "zip_seeksForward", "0", CVAR_INTEGER, "" );
idCVar zip_seeksBackward( "zip_seeksBackward", "0", CVAR_INTEGER, "" );
idCVar zip_avgSeekDistance( "zip_avgSeekDistance", "0", CVAR_INTEGER, "" );

			idCVar joy_mergedThreshold( "joy_mergedThreshold", "1", CVAR_BOOL | CVAR_ARCHIVE, "If the thresholds aren't merged, you drift more off center" );
idCVar joy_newCode( "joy_newCode", "1", CVAR_BOOL | CVAR_ARCHIVE, "Use the new codepath" );
idCVar joy_triggerThreshold( "joy_triggerThreshold", "0.05", CVAR_FLOAT | CVAR_ARCHIVE, "how far the joystick triggers have to be pressed before they register as down" );
idCVar joy_deadZone( "joy_deadZone", "0.2", CVAR_FLOAT | CVAR_ARCHIVE, "specifies how large the dead-zone is on the joystick" );
idCVar joy_range( "joy_range", "1.0", CVAR_FLOAT | CVAR_ARCHIVE, "allow full range to be mapped to a smaller offset" );
idCVar joy_gammaLook( "joy_gammaLook", "1", CVAR_INTEGER | CVAR_ARCHIVE, "use a log curve instead of a power curve for movement" );
idCVar joy_powerScale( "joy_powerScale", "2", CVAR_FLOAT | CVAR_ARCHIVE, "Raise joystick values to this power" );
idCVar joy_pitchSpeed( "joy_pitchSpeed", "100",	CVAR_ARCHIVE | CVAR_FLOAT, "pitch speed when pressing up or down on the joystick", 60, 600 );
idCVar joy_yawSpeed( "joy_yawSpeed", "240",	CVAR_ARCHIVE | CVAR_FLOAT, "pitch speed when pressing left or right on the joystick", 60, 600 );

// these were a bad idea!
idCVar joy_dampenLook( "joy_dampenLook", "1", CVAR_BOOL | CVAR_ARCHIVE, "Do not allow full acceleration on look" );
idCVar joy_deltaPerMSLook( "joy_deltaPerMSLook", "0.003", CVAR_FLOAT | CVAR_ARCHIVE, "Max amount to be added on look per MS" );

idCVar in_mouseSpeed( "in_mouseSpeed", "1",	CVAR_ARCHIVE | CVAR_FLOAT, "speed at which the mouse moves", 0.25f, 4.0f );
idCVar in_alwaysRun( "in_alwaysRun", "1", CVAR_SYSTEM | CVAR_ARCHIVE | CVAR_BOOL, "always run (reverse _speed button) - only in MP" );

idCVar in_useJoystick( "in_useJoystick", "0", CVAR_ARCHIVE | CVAR_BOOL, "enables/disables the gamepad for PC use" );
idCVar in_joystickRumble( "in_joystickRumble", "1", CVAR_SYSTEM | CVAR_ARCHIVE | CVAR_BOOL, "enable joystick rumble" );
idCVar in_invertLook( "in_invertLook", "0", CVAR_ARCHIVE | CVAR_BOOL, "inverts the look controls so the forward looks up (flight controls) - the proper way to play games!" );
idCVar in_mouseInvertLook( "in_mouseInvertLook", "0", CVAR_ARCHIVE | CVAR_BOOL, "inverts the look controls so the forward looks up (flight controls) - the proper way to play games!" );

			idCVar idUsercmdGenLocal::in_yawSpeed( "in_yawspeed", "140", CVAR_SYSTEM | CVAR_ARCHIVE | CVAR_FLOAT, "yaw change speed when holding down _left or _right button" );
idCVar idUsercmdGenLocal::in_pitchSpeed( "in_pitchspeed", "140", CVAR_SYSTEM | CVAR_ARCHIVE | CVAR_FLOAT, "pitch change speed when holding down look _lookUp or _lookDown button" );
idCVar idUsercmdGenLocal::in_angleSpeedKey( "in_anglespeedkey", "1.5", CVAR_SYSTEM | CVAR_ARCHIVE | CVAR_FLOAT, "angle change scale when holding down _speed button" );
idCVar idUsercmdGenLocal::in_toggleRun( "in_toggleRun", "0", CVAR_SYSTEM | CVAR_ARCHIVE | CVAR_BOOL, "pressing _speed button toggles run on/off - only in MP" );
idCVar idUsercmdGenLocal::in_toggleCrouch( "in_toggleCrouch", "0", CVAR_SYSTEM | CVAR_ARCHIVE | CVAR_BOOL, "pressing _movedown button toggles player crouching/standing" );
idCVar idUsercmdGenLocal::in_toggleZoom( "in_toggleZoom", "0", CVAR_SYSTEM | CVAR_ARCHIVE | CVAR_BOOL, "pressing _zoom button toggles zoom on/off" );
idCVar idUsercmdGenLocal::sensitivity( "sensitivity", "5", CVAR_SYSTEM | CVAR_ARCHIVE | CVAR_FLOAT, "mouse view sensitivity" );
idCVar idUsercmdGenLocal::m_pitch( "m_pitch", "0.022", CVAR_SYSTEM | CVAR_ARCHIVE | CVAR_FLOAT, "mouse pitch scale" );
idCVar idUsercmdGenLocal::m_yaw( "m_yaw", "0.022", CVAR_SYSTEM | CVAR_ARCHIVE | CVAR_FLOAT, "mouse yaw scale" );
idCVar idUsercmdGenLocal::m_smooth( "m_smooth", "1", CVAR_SYSTEM | CVAR_ARCHIVE | CVAR_INTEGER, "number of samples blended for mouse viewing", 1, 8, idCmdSystem::ArgCompletion_Integer<1,8> );
idCVar idUsercmdGenLocal::m_showMouseRate( "m_showMouseRate", "0", CVAR_SYSTEM | CVAR_BOOL, "shows mouse movement" );

			idCVar zip_verbosity( "zip_verbosity", "0", CVAR_BOOL, "1 = verbose logging when building zip files" );

			
idCVar stereoRender_warp( "stereoRender_warp", "0", CVAR_RENDERER | CVAR_ARCHIVE | CVAR_BOOL, "use the optical warping renderprog instead of stereoDeGhost" );
idCVar stereoRender_warpStrength( "stereoRender_warpStrength", "1.45", CVAR_RENDERER | CVAR_ARCHIVE | CVAR_FLOAT, "amount of pre-distortion" );

idCVar stereoRender_warpCenterX( "stereoRender_warpCenterX", "0.5", CVAR_RENDERER | CVAR_FLOAT | CVAR_ARCHIVE, "center for left eye, right eye will be 1.0 - this" );
idCVar stereoRender_warpCenterY( "stereoRender_warpCenterY", "0.5", CVAR_RENDERER | CVAR_FLOAT | CVAR_ARCHIVE, "center for both eyes" );
idCVar stereoRender_warpParmZ( "stereoRender_warpParmZ", "0", CVAR_RENDERER | CVAR_FLOAT | CVAR_ARCHIVE, "development parm" );
idCVar stereoRender_warpParmW( "stereoRender_warpParmW", "0", CVAR_RENDERER | CVAR_FLOAT | CVAR_ARCHIVE, "development parm" );
idCVar stereoRender_warpTargetFraction( "stereoRender_warpTargetFraction", "1.0", CVAR_RENDERER | CVAR_FLOAT | CVAR_ARCHIVE, "fraction of half-width the through-lens view covers" );


idCVar r_syncEveryFrame( "r_syncEveryFrame", "1", CVAR_BOOL, "Don't let the GPU buffer execution past swapbuffers" ); 


			idCVar image_highQualityCompression( "image_highQualityCompression", "0", CVAR_BOOL, "Use high quality (slow) compression" );

			idCVar r_showBuffers( "r_showBuffers", "0", CVAR_INTEGER, "" );

			idCVar r_centerX( "r_centerX", "0", CVAR_FLOAT, "projection matrix center adjust" );
idCVar r_centerY( "r_centerY", "0", CVAR_FLOAT, "projection matrix center adjust" );


			

			idCVar preLoad_Images( "preLoad_Images", "1", CVAR_SYSTEM | CVAR_BOOL, "preload images during beginlevelload" );

			idCVar r_forceSoundOpAmplitude( "r_forceSoundOpAmplitude", "0", CVAR_FLOAT, "Don't call into the sound system for amplitudes" );
			idCVar idRenderModelStatic::r_mergeModelSurfaces( "r_mergeModelSurfaces", "1", CVAR_BOOL|CVAR_RENDERER, "combine model surfaces with the same material" );
idCVar idRenderModelStatic::r_slopVertex( "r_slopVertex", "0.01", CVAR_RENDERER, "merge xyz coordinates this far apart" );
idCVar idRenderModelStatic::r_slopTexCoord( "r_slopTexCoord", "0.001", CVAR_RENDERER, "merge texture coordinates this far apart" );
idCVar idRenderModelStatic::r_slopNormal( "r_slopNormal", "0.02", CVAR_RENDERER, "merge normals that dot less than this" );


			idCVar r_useGPUSkinning( "r_useGPUSkinning", "1", CVAR_INTEGER, "animate normals and tangents instead of deriving" );

			idCVar r_binaryLoadRenderModels( "r_binaryLoadRenderModels", "1", 0, "enable binary load/write of render models" );
idCVar preload_MapModels( "preload_MapModels", "1", CVAR_SYSTEM | CVAR_BOOL, "preload models during begin or end levelload" );


			idCVar r_logLevel( "r_logLevel", "2", CVAR_INTEGER, "1 = blocks only, 2 = everything", 1, 2 );
			idCVar r_pix( "r_pix", "0", CVAR_INTEGER, "print GPU/CPU event timing" );

			idCVar r_skipStripDeadCode( "r_skipStripDeadCode", "0", CVAR_BOOL, "Skip stripping dead code" );
idCVar r_useUniformArrays( "r_useUniformArrays", "1", CVAR_BOOL, "" );

			idCVar r_requestStereoPixelFormat( "r_requestStereoPixelFormat", "1", CVAR_RENDERER, "Ask for a stereo GL pixel format on startup" );
idCVar r_debugContext( "r_debugContext", "0", CVAR_RENDERER, "Enable various levels of context debug." );
idCVar r_glDriver( "r_glDriver", "", CVAR_RENDERER, "\"opengl32\", etc." );
idCVar r_skipIntelWorkarounds( "r_skipIntelWorkarounds", "0", CVAR_RENDERER | CVAR_BOOL, "skip workarounds for Intel driver bugs" );


idCVar r_useViewBypass( "r_useViewBypass", "1", CVAR_RENDERER | CVAR_INTEGER, "bypass a frame of latency to the view" );
idCVar r_useLightPortalFlow( "r_useLightPortalFlow", "1", CVAR_RENDERER | CVAR_BOOL, "use a more precise area reference determination" );
idCVar r_singleTriangle( "r_singleTriangle", "0", CVAR_RENDERER | CVAR_BOOL, "only draw a single triangle per primitive" );
idCVar r_checkBounds( "r_checkBounds", "0", CVAR_RENDERER | CVAR_BOOL, "compare all surface bounds with precalculated ones" );

idCVar r_useSilRemap( "r_useSilRemap", "1", CVAR_RENDERER | CVAR_BOOL, "consider verts with the same XYZ, but different ST the same for shadows" );
idCVar r_useNodeCommonChildren( "r_useNodeCommonChildren", "1", CVAR_RENDERER | CVAR_BOOL, "stop pushing reference bounds early when possible" );
idCVar r_useShadowSurfaceScissor( "r_useShadowSurfaceScissor", "1", CVAR_RENDERER | CVAR_BOOL, "scissor shadows by the scissor rect of the interaction surfaces" );
idCVar r_useCachedDynamicModels( "r_useCachedDynamicModels", "1", CVAR_RENDERER | CVAR_BOOL, "cache snapshots of dynamic models" );
idCVar r_useSeamlessCubeMap( "r_useSeamlessCubeMap", "1", CVAR_RENDERER | CVAR_BOOL, "use ARB_seamless_cube_map if available" );
idCVar r_useSRGB( "r_useSRGB", "0", CVAR_RENDERER | CVAR_INTEGER | CVAR_ARCHIVE, "1 = both texture and framebuffer, 2 = framebuffer only, 3 = texture only" );
idCVar r_maxAnisotropicFiltering( "r_maxAnisotropicFiltering", "8", CVAR_RENDERER | CVAR_ARCHIVE | CVAR_INTEGER, "limit aniso filtering" );
idCVar r_useTrilinearFiltering( "r_useTrilinearFiltering", "1", CVAR_RENDERER | CVAR_ARCHIVE | CVAR_BOOL, "Extra quality filtering" );
idCVar r_lodBias( "r_lodBias", "0.5", CVAR_RENDERER | CVAR_ARCHIVE, "image lod bias" );



idCVar r_znear( "r_znear", "3", CVAR_RENDERER | CVAR_FLOAT, "near Z clip plane distance", 0.001f, 200.0f );

idCVar r_ignoreGLErrors( "r_ignoreGLErrors", "1", CVAR_RENDERER | CVAR_BOOL, "ignore GL errors" );*/

/*idCVar r_gamma( "r_gamma", "1.0", CVAR_RENDERER | CVAR_ARCHIVE | CVAR_FLOAT, "changes gamma tables", 0.5f, 3.0f );
idCVar r_brightness( "r_brightness", "1", CVAR_RENDERER | CVAR_ARCHIVE | CVAR_FLOAT, "changes gamma tables", 0.5f, 2.0f );

idCVar r_jitter( "r_jitter", "0", CVAR_RENDERER | CVAR_BOOL, "randomly subpixel jitter the projection matrix" );



idCVar r_offsetFactor( "r_offsetfactor", "0", CVAR_RENDERER | CVAR_FLOAT, "polygon offset parameter" );
idCVar r_offsetUnits( "r_offsetunits", "-600", CVAR_RENDERER | CVAR_FLOAT, "polygon offset parameter" );

idCVar r_shadowPolygonOffset( "r_shadowPolygonOffset", "-1", CVAR_RENDERER | CVAR_FLOAT, "bias value added to depth test for stencil shadow drawing" );
idCVar r_shadowPolygonFactor( "r_shadowPolygonFactor", "0", CVAR_RENDERER | CVAR_FLOAT, "scale value for stencil shadow drawing" );
idCVar r_subviewOnly( "r_subviewOnly", "0", CVAR_RENDERER | CVAR_BOOL, "1 = don't render main view, allowing subviews to be debugged" );
idCVar r_testGamma( "r_testGamma", "0", CVAR_RENDERER | CVAR_FLOAT, "if > 0 draw a grid pattern to test gamma levels", 0, 195 );
idCVar r_testGammaBias( "r_testGammaBias", "0", CVAR_RENDERER | CVAR_FLOAT, "if > 0 draw a grid pattern to test gamma levels" );

idCVar r_flareSize( "r_flareSize", "1", CVAR_RENDERER | CVAR_FLOAT, "scale the flare deforms from the material def" ); 

idCVar r_skipPrelightShadows( "r_skipPrelightShadows", "0", CVAR_RENDERER | CVAR_BOOL, "skip the dmap generated static shadow volumes" );

idCVar r_useLightDepthBounds( "r_useLightDepthBounds", "1", CVAR_RENDERER | CVAR_BOOL, "use depth bounds test on lights to reduce both shadow and interaction fill" );
idCVar r_useShadowDepthBounds( "r_useShadowDepthBounds", "1", CVAR_RENDERER | CVAR_BOOL, "use depth bounds test on individual shadow volumes to reduce shadow fill" );

idCVar r_screenFraction( "r_screenFraction", "100", CVAR_RENDERER | CVAR_INTEGER, "for testing fill rate, the resolution of the entire screen can be changed" );
idCVar r_usePortals( "r_usePortals", "1", CVAR_RENDERER | CVAR_BOOL, " 1 = use portals to perform area culling, otherwise draw everything" );
idCVar r_singleLight( "r_singleLight", "-1", CVAR_RENDERER | CVAR_INTEGER, "suppress all but one light" );
idCVar r_singleEntity( "r_singleEntity", "-1", CVAR_RENDERER | CVAR_INTEGER, "suppress all but one entity" );
idCVar r_singleSurface( "r_singleSurface", "-1", CVAR_RENDERER | CVAR_INTEGER, "suppress all but one surface on each entity" );
idCVar r_singleArea( "r_singleArea", "0", CVAR_RENDERER | CVAR_BOOL, "only draw the portal area the view is actually in" );
idCVar r_orderIndexes( "r_orderIndexes", "1", CVAR_RENDERER | CVAR_BOOL, "perform index reorganization to optimize vertex use" );
idCVar r_lightAllBackFaces( "r_lightAllBackFaces", "0", CVAR_RENDERER | CVAR_BOOL, "light all the back faces, even when they would be shadowed" );


idCVar r_useEntityCallbacks( "r_useEntityCallbacks", "1", CVAR_RENDERER | CVAR_BOOL, "if 0, issue the callback immediately at update time, rather than defering" );

idCVar r_showSkel( "r_showSkel", "0", CVAR_RENDERER | CVAR_INTEGER, "draw the skeleton when model animates, 1 = draw model with skeleton, 2 = draw skeleton only", 0, 2, idCmdSystem::ArgCompletion_Integer<0,2> );
idCVar r_jointNameScale( "r_jointNameScale", "0.02", CVAR_RENDERER | CVAR_FLOAT, "size of joint names when r_showskel is set to 1" );
idCVar r_jointNameOffset( "r_jointNameOffset", "0.5", CVAR_RENDERER | CVAR_FLOAT, "offset of joint names when r_showskel is set to 1" );

idCVar r_debugLineDepthTest( "r_debugLineDepthTest", "0", CVAR_RENDERER | CVAR_ARCHIVE | CVAR_BOOL, "perform depth test on debug lines" );
idCVar r_debugLineWidth( "r_debugLineWidth", "1", CVAR_RENDERER | CVAR_ARCHIVE | CVAR_BOOL, "width of debug lines" );
idCVar r_debugArrowStep( "r_debugArrowStep", "120", CVAR_RENDERER | CVAR_ARCHIVE | CVAR_INTEGER, "step size of arrow cone line rotation in degrees", 0, 120 );
idCVar r_debugPolygonFilled( "r_debugPolygonFilled", "1", CVAR_RENDERER | CVAR_BOOL, "draw a filled polygon" );

idCVar r_materialOverride( "r_materialOverride", "", CVAR_RENDERER, "overrides all materials", idCmdSystem::ArgCompletion_Decl<DECL_MATERIAL> );



idCVar stereoRender_enable( "stereoRender_enable", "0", CVAR_INTEGER | CVAR_ARCHIVE, "1 = side-by-side compressed, 2 = top and bottom compressed, 3 = side-by-side, 4 = 720 frame packed, 5 = interlaced, 6 = OpenGL quad buffer" );
idCVar stereoRender_swapEyes( "stereoRender_swapEyes", "0", CVAR_BOOL | CVAR_ARCHIVE, "reverse eye adjustments" );
idCVar stereoRender_deGhost( "stereoRender_deGhost", "0.05", CVAR_FLOAT | CVAR_ARCHIVE, "subtract from opposite eye to reduce ghosting" );


		idCVar	r_forceScreenWidthCentimeters( "r_forceScreenWidthCentimeters", "0", CVAR_RENDERER | CVAR_ARCHIVE, "Override screen width returned by hardware" );



idCVar r_forceZPassStencilShadows( "r_forceZPassStencilShadows", "0", CVAR_RENDERER | CVAR_BOOL, "force Z-pass rendering for performance testing" );
idCVar r_useStencilShadowPreload( "r_useStencilShadowPreload", "1", CVAR_RENDERER | CVAR_BOOL, "use stencil shadow preload algorithm instead of Z-fail" );

idCVar r_skipInteractionFastPath( "r_skipInteractionFastPath", "1", CVAR_RENDERER | CVAR_BOOL, "" );
idCVar r_useLightStencilSelect( "r_useLightStencilSelect", "0", CVAR_RENDERER | CVAR_BOOL, "use stencil select pass" );

			idCVar r_showCenterOfProjection( "r_showCenterOfProjection", "0", CVAR_RENDERER | CVAR_BOOL, "Draw a cross to show the center of projection" );
idCVar r_showLines( "r_showLines", "0", CVAR_RENDERER | CVAR_INTEGER, "1 = draw alternate horizontal lines, 2 = draw alternate vertical lines" );

			idCVar r_useAreasConnectedForShadowCulling( "r_useAreasConnectedForShadowCulling", "2", CVAR_RENDERER | CVAR_INTEGER, "cull entities cut off by doors" );
idCVar r_useParallelAddLights( "r_useParallelAddLights", "1", CVAR_RENDERER | CVAR_BOOL, "aadd all lights in parallel with jobs" );

			idCVar r_skipStaticShadows( "r_skipStaticShadows", "0", CVAR_RENDERER | CVAR_BOOL, "skip static shadows" );
idCVar r_skipDynamicShadows( "r_skipDynamicShadows", "0", CVAR_RENDERER | CVAR_BOOL, "skip dynamic shadows" );
idCVar r_useParallelAddModels( "r_useParallelAddModels", "1", CVAR_RENDERER | CVAR_BOOL, "add all models in parallel with jobs" );
idCVar r_useParallelAddShadows( "r_useParallelAddShadows", "1", CVAR_RENDERER | CVAR_INTEGER, "0 = off, 1 = threaded", 0, 1 );
idCVar r_useShadowPreciseInsideTest( "r_useShadowPreciseInsideTest", "1", CVAR_RENDERER | CVAR_BOOL, "use a precise and more expensive test to determine whether the view is inside a shadow volume" );
idCVar r_cullDynamicShadowTriangles( "r_cullDynamicShadowTriangles", "1", CVAR_RENDERER | CVAR_BOOL, "cull occluder triangles that are outside the light frustum so they do not contribute to the dynamic shadow volume" );
idCVar r_cullDynamicLightTriangles( "r_cullDynamicLightTriangles", "1", CVAR_RENDERER | CVAR_BOOL, "cull surface triangles that are outside the light frustum so they do not get rendered for interactions" );
idCVar r_forceShadowCaps( "r_forceShadowCaps", "0", CVAR_RENDERER | CVAR_BOOL, "0 = skip rendering shadow caps if view is outside shadow volume, 1 = always render shadow caps" );

			idCVar r_showVertexCache( "r_showVertexCache", "0", CVAR_RENDERER | CVAR_BOOL, "Print stats about the vertex cache every frame" );
idCVar r_showVertexCacheTimings( "r_showVertexCache", "0", CVAR_RENDERER | CVAR_BOOL, "Print stats about the vertex cache every frame" );

			idCVar s_showLevelMeter( "s_showLevelMeter", "0", CVAR_BOOL|CVAR_ARCHIVE, "Show VU meter" );
idCVar s_meterTopTime( "s_meterTopTime", "1000", CVAR_INTEGER|CVAR_ARCHIVE, "How long (in milliseconds) peaks are displayed on the VU meter" );
idCVar s_meterPosition( "s_meterPosition", "100 100 20 200", CVAR_ARCHIVE, "VU meter location (x y w h)" );
idCVar s_device( "s_device", "-1", CVAR_INTEGER|CVAR_ARCHIVE, "Which audio device to use (listDevices to list, -1 for default)" );
idCVar s_showPerfData( "s_showPerfData", "0", CVAR_BOOL, "Show XAudio2 Performance data" );

			idCVar s_skipHardwareSets( "s_skipHardwareSets", "0", CVAR_BOOL, "Do all calculation, but skip XA2 calls" );
idCVar s_debugHardware( "s_debugHardware", "0", CVAR_BOOL, "Print a message any time a hardware voice changes" );

			idCVar s_singleEmitter( "s_singleEmitter", "0", CVAR_INTEGER, "mute all sounds but this emitter" );
idCVar s_showStartSound( "s_showStartSound", "0", CVAR_BOOL, "print a message every time a sound starts/stops" );
idCVar s_useOcclusion( "s_useOcclusion", "1", CVAR_BOOL, "Attenuate sounds based on walls" );
idCVar s_centerFractionVO( "s_centerFractionVO", "0.75", CVAR_FLOAT, "Portion of VO sounds routed to the center channel" );

			idCVar s_noSound( "s_noSound", "0", CVAR_BOOL, "returns NULL for all sounds loaded and does not update the sound rendering" );

#ifdef ID_RETAIL
idCVar s_useCompression( "s_useCompression", "1", CVAR_BOOL, "Use compressed sound files (mp3/xma)" );
idCVar s_playDefaultSound( "s_playDefaultSound", "0", CVAR_BOOL, "play a beep for missing sounds" );
idCVar s_maxSamples( "s_maxSamples", "5", CVAR_INTEGER, "max samples to load per shader" );
#else
idCVar s_useCompression( "s_useCompression", "1", CVAR_BOOL, "Use compressed sound files (mp3/xma)" );
idCVar s_playDefaultSound( "s_playDefaultSound", "1", CVAR_BOOL, "play a beep for missing sounds" );
idCVar s_maxSamples( "s_maxSamples", "5", CVAR_INTEGER, "max samples to load per shader" );
#endif

idCVar preLoad_Samples( "preLoad_Samples", "1", CVAR_SYSTEM | CVAR_BOOL, "preload samples during beginlevelload" );

			idCVar s_lockListener( "s_lockListener", "0", CVAR_BOOL, "lock listener updates" );
idCVar s_constantAmplitude( "s_constantAmplitude", "-1", CVAR_FLOAT, "" );
idCVar s_maxEmitterChannels( "s_maxEmitterChannels", "48", CVAR_INTEGER, "Can be set lower than the absolute max of MAX_HARDWARE_VOICES" );
idCVar s_cushionFadeChannels( "s_cushionFadeChannels", "2", CVAR_INTEGER, "Ramp currentCushionDB so this many emitter channels should be silent" );
idCVar s_cushionFadeRate( "s_cushionFadeRate", "60", CVAR_FLOAT, "DB / second change to currentCushionDB" );
idCVar s_cushionFadeLimit( "s_cushionFadeLimit", "-30", CVAR_FLOAT, "Never cushion fade beyond this level" );
idCVar s_cushionFadeOver( "s_cushionFadeOver", "10", CVAR_FLOAT, "DB above s_cushionFadeLimit to start ramp to silence" );
idCVar s_unpauseFadeInTime( "s_unpauseFadeInTime", "250", CVAR_INTEGER, "When unpausing a sound world, milliseconds to fade sounds in over" );
idCVar s_doorDistanceAdd( "s_doorDistanceAdd", "150", CVAR_FLOAT, "reduce sound volume with this distance when going through a door" );
idCVar s_drawSounds( "s_drawSounds", "0", CVAR_INTEGER, "", 0, 2, idCmdSystem::ArgCompletion_Integer<0,2> );
idCVar s_showVoices( "s_showVoices", "0", CVAR_BOOL, "show active voices" );
idCVar s_volume_dB( "s_volume_dB", "0", CVAR_ARCHIVE | CVAR_FLOAT, "volume in dB" );

			idCVar s_subFraction( "s_subFraction", "0.5", CVAR_ARCHIVE | CVAR_FLOAT, "Amount of each sound to send to the LFE channel" );
		

			


			idCVar swf_debugShowAddress( "swf_debugShowAddress", "0", CVAR_BOOL, "shows addresses along with object types when they are serialized" );

			

			idCVar r_useOpenGL32( "r_useOpenGL32", "1", CVAR_INTEGER, "0 = OpenGL 2.0, 1 = OpenGL 3.2 compatibility profile, 2 = OpenGL 3.2 core profile", 0, 2 );

			idCVar net_socksServer( "net_socksServer", "", CVAR_ARCHIVE, "" );
idCVar net_socksPort( "net_socksPort", "1080", CVAR_ARCHIVE | CVAR_INTEGER, "" );
idCVar net_socksUsername( "net_socksUsername", "", CVAR_ARCHIVE, "" );
idCVar net_socksPassword( "net_socksPassword", "", CVAR_ARCHIVE, "" );

idCVar net_ip( "net_ip", "localhost", 0, "local IP address" );


			idCVar savegame_winInduceDelay( "savegame_winInduceDelay", "0", CVAR_INTEGER, "on windows, this is a delay induced before any file operation occurs" );

			#ifdef _DEBUG
idCVar win_userPersistent( "win_userPersistent", "1", CVAR_BOOL, "debugging cvar for profile persistence status" );
idCVar win_userOnline( "win_userOnline", "1", CVAR_BOOL, "debugging cvar for profile online status" );
idCVar win_isInParty( "win_isInParty", "0", CVAR_BOOL, "debugging cvar for platform party status" );
idCVar win_partyCount( "win_partyCount", "0", CVAR_INTEGER, "debugginc var for platform party count" );
#endif

			idCVar net_maxRate( "net_maxRate", "50", CVAR_INTEGER, "max send rate in kilobytes per second" );

idCVar net_showReliableCompression( "net_showReliableCompression", "0", CVAR_BOOL, "Show reliable compression ratio." );

idCVar net_verifyReliableQueue( "net_verifyReliableQueue", "2", CVAR_INTEGER, "0: warn only, 1: error, 2: fixup, 3: fixup and verbose, 4: force test" );

			idCVar net_verboseSnapshot( "net_verboseSnapshot", "0", CVAR_INTEGER|CVAR_NOCHEAT, "Verbose snapshot code to help debug snapshot problems. Greater the number greater the spam" );
idCVar net_verboseSnapshotCompression( "net_verboseSnapshotCompression", "0", CVAR_INTEGER|CVAR_NOCHEAT, "Verbose snapshot code to help debug snapshot problems. Greater the number greater the spam" );
idCVar net_verboseSnapshotReport( "net_verboseSnapshotReport", "0", CVAR_INTEGER|CVAR_NOCHEAT, "Verbose snapshot code to help debug snapshot problems. Greater the number greater the spam" );

idCVar net_ssTemplateDebug( "net_ssTemplateDebug", "0", CVAR_BOOL, "Debug snapshot template states" );
idCVar net_ssTemplateDebug_len( "net_ssTemplateDebug_len", "32", CVAR_INTEGER, "Offset to start template state debugging" );
idCVar net_ssTemplateDebug_start( "net_ssTemplateDebug_start", "0", CVAR_INTEGER, "length of template state to print in debugging" );

			idCVar net_optimalSnapDeltaSize( "net_optimalSnapDeltaSize", "1000", CVAR_INTEGER, "Optimal size of snapshot delta msgs." );
idCVar net_debugBaseStates( "net_debugBaseStates", "0", CVAR_BOOL, "Log out base state information" );
idCVar net_skipClientDeltaAppend( "net_skipClientDeltaAppend", "0", CVAR_BOOL, "Simulate delta receive buffer overflowing" );

			idCVar net_checkVersion( "net_checkVersion", "0", CVAR_INTEGER, "Check for matching version when clients connect. 0: normal rules, 1: force check, otherwise no check (pass always)" );
idCVar net_peerTimeoutInSeconds( "net_peerTimeoutInSeconds", "30", CVAR_INTEGER, "If the host hasn't received a response from a peer in this amount of time (in seconds), the peer will be disconnected." );
idCVar net_peerTimeoutInSeconds_Lobby( "net_peerTimeoutInSeconds_Lobby", "20", CVAR_INTEGER, "If the host hasn't received a response from a peer in this amount of time (in seconds), the peer will be disconnected." );

// NOTE - The snapshot exchange does the bandwidth challenge
idCVar net_bw_challenge_enable( "net_bw_challenge_enable", "0", CVAR_BOOL, "Enable pre game bandwidth challenge for throttling snap rate" ); 

idCVar net_bw_test_interval( "net_bw_test_interval", "33", CVAR_INTEGER, "MS - how often to send packets in bandwidth test" );
idCVar net_bw_test_numPackets( "net_bw_test_numPackets", "30", CVAR_INTEGER, "Number of bandwidth challenge packets to send" );
idCVar net_bw_test_packetSizeBytes( "net_bw_test_packetSizeBytes", "1024", CVAR_INTEGER, "Size of each packet to send out" );
idCVar net_bw_test_timeout( "net_bw_test_timeout", "500", CVAR_INTEGER, "MS after receiving a bw test packet that client will time out" );
idCVar net_bw_test_host_timeout( "net_bw_test_host_timeout", "3000", CVAR_INTEGER, "How long host will wait in MS to hear bw results from peers" );

idCVar net_bw_test_throttle_rate_pct( "net_bw_test_throttle_rate_pct", "0.80", CVAR_FLOAT, "Min rate % a peer must match in bandwidth challenge before being throttled. 1.0=perfect, 0.0=received nothing" );
idCVar net_bw_test_throttle_byte_pct( "net_bw_test_throttle_byte_pct", "0.80", CVAR_FLOAT, "Min byte % a peer must match in bandwidth challenge before being throttled. 1.0=perfect (received everything) 0.0=Received nothing" );
idCVar net_bw_test_throttle_seq_pct( "net_bw_test_throttle_seq_pct", "0.80", CVAR_FLOAT, "Min sequence % a peer must match in bandwidth test before being throttled. 1.0=perfect. This score will be more adversely affected by packet loss than byte %" );

idCVar net_ignoreConnects( "net_ignoreConnects", "0", CVAR_INTEGER, "Test as if no one can connect to me. 0 = off, 1 = ignore with no reply, 2 = send goodbye" );

idCVar net_skipGoodbye( "net_skipGoodbye", "0", CVAR_BOOL, "" ); 

			idCVar net_debughud3_bps_max( "net_debughud3_bps_max", "5120.0f", CVAR_FLOAT, "Highest factor of server base snapRate that a client can be throttled" );

			idCVar net_migration_debug( "net_migration_debug", "0", CVAR_BOOL, "debug" ); 
idCVar net_migration_disable( "net_migration_disable", "0", CVAR_BOOL, "debug" ); 
idCVar net_migration_forcePeerAsHost( "net_migration_forcePeerAsHost", "-1", CVAR_INTEGER, "When set to >-1, it forces that peer number to be the new host during migration" );

			idCVar net_snapshot_send_warntime( "net_snapshot_send_warntime", "500", CVAR_INTEGER, "Print warning messages if we take longer than this to send a client a snapshot." );

idCVar net_queueSnapAcks( "net_queueSnapAcks", "1", CVAR_BOOL, "" );

idCVar net_peer_throttle_mode( "net_peer_throttle_mode", "0", CVAR_INTEGER, "= 0 off, 1 = enable fixed, 2 = absolute, 3 = both" );

idCVar net_peer_throttle_minSnapSeq( "net_peer_throttle_minSnapSeq", "150", CVAR_INTEGER, "Minumum number of snapshot exchanges before throttling can be triggered" );

idCVar net_peer_throttle_bps_peer_threshold_pct( "net_peer_throttle_bps_peer_threshold_pct", "0.60", CVAR_FLOAT, "Min reported incoming bps % of sent from host that a peer must maintain before throttling kicks in" );
idCVar net_peer_throttle_bps_host_threshold( "net_peer_throttle_bps_host_threshold", "1024", CVAR_FLOAT, "Min outgoing bps of host for bps based throttling to be considered" );

idCVar net_peer_throttle_bps_decay( "net_peer_throttle_bps_decay", "0.25f", CVAR_FLOAT, "If peer exceeds this number of queued snap deltas, then throttle his effective snap rate" );
idCVar net_peer_throttle_bps_duration( "net_peer_throttle_bps_duration", "3000", CVAR_INTEGER, "If peer exceeds this number of queued snap deltas, then throttle his effective snap rate" );

idCVar net_peer_throttle_maxSnapRate( "net_peer_throttle_maxSnapRate", "4", CVAR_INTEGER, "Highest factor of server base snapRate that a client can be throttled" );

idCVar net_snap_bw_test_throttle_max_scale( "net_snap_bw_test_throttle_max_scale", "0.80", CVAR_FLOAT, "When clamping bandwidth to reported values, scale reported value by this" );

idCVar net_snap_redundant_resend_in_ms( "net_snap_redundant_resend_in_ms", "800", CVAR_INTEGER, "Delay between redundantly sending snaps during initial snap exchange" );
idCVar net_min_ping_in_ms( "net_min_ping_in_ms", "1500", CVAR_INTEGER, "Ping has to be higher than this before we consider throttling to recover" );
idCVar net_pingIncPercentBeforeRecover( "net_pingIncPercentBeforeRecover", "1.3", CVAR_FLOAT, "Percentage change increase of ping before we try to recover" );
idCVar net_maxFailedPingRecoveries( "net_maxFailedPingRecoveries", "10", CVAR_INTEGER, "Max failed ping recoveries before we stop trying" );
idCVar net_pingRecoveryThrottleTimeInSeconds( "net_pingRecoveryThrottleTimeInSeconds", "3", CVAR_INTEGER, "Throttle snaps for this amount of time in seconds to recover from ping spike" );

idCVar net_peer_timeout_loading( "net_peer_timeout_loading", "90000", CVAR_INTEGER, "time in MS to disconnect clients during loading - production only" );

			idCVar net_forceDropSnap( "net_forceDropSnap", "0", CVAR_BOOL, "wait on snaps" );

			
			idCVar profile_verbose( "profile_verbose", "0", CVAR_BOOL, "Turns on debug spam for profiles" );
			idCVar saveGame_verbose( "saveGame_verbose", "0", CVAR_BOOL | CVAR_ARCHIVE, "debug spam" );
idCVar saveGame_checksum( "saveGame_checksum", "1", CVAR_BOOL, "data integrity check" );
idCVar saveGame_enable( "saveGame_enable", "1", CVAR_BOOL, "are savegames enabled" );

			idCVar ui_skinIndex( "ui_skinIndex", "0", CVAR_ARCHIVE, "Selected skin index" );
idCVar ui_autoSwitch( "ui_autoSwitch", "1", CVAR_ARCHIVE | CVAR_BOOL, "auto switch weapon" );
idCVar ui_autoReload( "ui_autoReload", "1", CVAR_ARCHIVE | CVAR_BOOL, "auto reload weapon" );

idCVar net_maxSearchResults( "net_maxSearchResults", "25", CVAR_INTEGER, "Max results that are allowed to be returned in a search request" );
idCVar net_maxSearchResultsToTry( "net_maxSearchResultsToTry", "5", CVAR_INTEGER, "Max results to try before giving up." );		// At 15 second timeouts per, 1 min 15 worth of connecting attempt time

idCVar net_LobbyCoalesceTimeInSeconds( "net_LobbyCoalesceTimeInSeconds", "30", CVAR_INTEGER, "Time in seconds when a lobby will try to coalesce with another lobby when there is only one user." );
idCVar net_LobbyRandomCoalesceTimeInSeconds( "net_LobbyRandomCoalesceTimeInSeconds", "3", CVAR_INTEGER, "Random time to add to net_LobbyCoalesceTimeInSeconds" );

idCVar net_useGameStateLobby( "net_useGameStateLobby", "0", CVAR_BOOL, "" );
//idCVar net_useGameStateLobby( "net_useGameStateLobby", "1", CVAR_BOOL, "" );

#if !defined( ID_RETAIL ) || defined( ID_RETAIL_INTERNAL )
idCVar net_ignoreTitleStorage( "net_ignoreTitleStorage", "0", CVAR_BOOL, "Ignore title storage" );
#endif

idCVar net_maxLoadResourcesTimeInSeconds( "net_maxLoadResourcesTimeInSeconds", "0", CVAR_INTEGER, "How long, in seconds, clients have to load resources. Used for loose asset builds." );

extern idCVar net_debugBaseStates;

idCVar net_testPartyMemberConnectFail( "net_testPartyMemberConnectFail", "-1", CVAR_INTEGER, "Force this party member index to fail to connect to games." );

//FIXME: this could use a better name.
idCVar net_offlineTransitionThreshold( "net_offlineTransitionThreshold", "1000", CVAR_INTEGER, "Time, in milliseconds, to wait before kicking back to the main menu when a profile losses backend connection during an online game");

idCVar net_port( "net_port", "27015", CVAR_INTEGER, "host port number" ); // Port to host when using dedicated servers, port to broadcast on when looking for a dedicated server to connect to
idCVar net_headlessServer( "net_headlessServer", "0", CVAR_BOOL, "toggle to automatically host a game and allow peer[0] to control menus" );


			idCVar net_connectTimeoutInSeconds( "net_connectTimeoutInSeconds", "15", CVAR_INTEGER, "timeout (in seconds) while connecting" );

			idCVar net_verbose( "net_verbose", "0", CVAR_BOOL, "Print a bunch of message about the network session" );
idCVar net_verboseResource( "net_verboseResource", "0", CVAR_BOOL, "Prints a bunch of message about network resources" );
idCVar net_verboseReliable( "net_verboseReliable", "0", CVAR_BOOL, "Prints the more spammy messages about reliable network msgs" );
idCVar si_splitscreen( "si_splitscreen", "0", CVAR_INTEGER, "force splitscreen" );

idCVar net_forceLatency( "net_forceLatency", "0", CVAR_INTEGER, "Simulate network latency (milliseconds round trip time - applied equally on the receive and on the send)" );
idCVar net_forceDrop( "net_forceDrop", "0", CVAR_INTEGER, "Percentage chance of simulated network packet loss" );
idCVar net_forceUpstream( "net_forceUpstream", "0", CVAR_FLOAT, "Force a maximum upstream in kB/s (256kbps <-> 32kB/s)" ); // I would much rather deal in kbps but most of the code is written in bytes ..
idCVar net_forceUpstreamQueue( "net_forceUpstreamQueue", "64", CVAR_INTEGER, "How much data is queued when enforcing upstream (in kB)" );
idCVar net_verboseSimulatedTraffic( "net_verboseSimulatedTraffic", "0", CVAR_BOOL, "Print some stats about simulated traffic (net_force* cvars)" );

			idCVar com_deviceZeroOverride( "com_deviceZeroOverride", "-1", CVAR_INTEGER, "change input routing for device 0 to poll a different device" );
idCVar mp_bot_input_override( "mp_bot_input_override", "-1", CVAR_INTEGER, "Override local input routing for bot control" );

			idCVar savegame_error( "savegame_error", "0", CVAR_INTEGER, "Combination of bits that will simulate and error, see 'savegamePrintErrors'.  0 = no error" );
			idCVar com_requireNonProductionSignIn( "com_requireNonProductionSignIn", "1", CVAR_BOOL|CVAR_ARCHIVE, "If true, will require sign in, even on non production builds." );

			idCVar bearTurretAngle( "bearTurretAngle", "0", CVAR_FLOAT, "" );
idCVar bearTurretForce( "bearTurretForce", "200", CVAR_FLOAT, "" );

			
idCVar hud_titlesafe( "hud_titlesafe", "0.0", CVAR_GUI | CVAR_FLOAT, "fraction of the screen to leave around hud for titlesafe area" );

			idCVar ai_think( "ai_think", "1", CVAR_BOOL, "for testing.." );

			idCVar binaryLoadAnim( "binaryLoadAnim", "1", 0, "enable binary load/write of idMD5Anim" );

			static idCVar		r_showSkel( "r_showSkel", "0", CVAR_RENDERER | CVAR_INTEGER, "", 0, 2, idCmdSystem::ArgCompletion_Integer<0,2> );*/
		}
	}
}
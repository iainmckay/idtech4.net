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
using idTech4.Renderer;
using idTech4.Sound;
using idTech4.Text;
using idTech4.Text.Decls;

namespace idTech4.Services
{
	/// <summary>
	/// All "small text" data types, like materials, sound shaders, fx files,
	/// entity defs, etc. are managed uniformly, allowing reloading, purging,
	/// listing, printing, etc. All "large text" data types that never have more
	/// than one declaration in a given file, like maps, models, AAS files, etc.
	/// are not handled here.
	/// </summary>
	/// <remarks>
	/// A decl will never, ever go away once it is created. The manager is
	/// guaranteed to always return the same decl pointer for a decl type/name
	/// combination. The index of a decl in the per type list also stays the
	/// same throughout the lifetime of the engine. Although the pointer to
	/// a decl always stays the same, one should never maintain pointers to
	/// data inside decls. The data stored in a decl is not garranteed to stay
	/// the same for more than one engine frame.
	/// <p/>
	/// The decl indexes of explicitely defined decls are guarenteed to be
	/// consistent based on the parsed decl files. However, the indexes of
	/// implicit decls may be different based on the order in which levels
	/// are loaded.
	/// <p/>
	/// The decl namespaces are separate for each type. Comments for decls go
	/// above the text definition to keep them associated with the proper decl.
	/// <p/>
	/// During decl parsing, errors should never be issued, only warnings
	/// followed by a call to MakeDefault().
	/// </remarks>
	public interface IDeclManager
	{
		#region Properties
		idDeclFile ImplicitDeclFile { get; }
		int MediaPrintIndent { get; set; }
		#endregion

		#region Decl
		idDecl DeclByIndex(DeclType type, int index, bool forceParse = true);
		#endregion

		#region Find
		/// <summary>
		/// Finds the decl with the given name and type; returning a default version if requested.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="makeDefault"> If true, a default decl of appropriate type will be created.</param>
		/// <returns>Decl with the given name or a default decl of the requested type if not found or null if makeDefault is false.</returns>
		idDecl FindType(DeclType type, string name, bool makeDefault = true);

		/// <summary>
		/// Finds the decl with the given name and type; returning a default version if requested.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="makeDefault"> If true, a default decl of appropriate type will be created.</param>
		/// <returns>Decl with the given name or a default decl of the requested type if not found or null if makeDefault is false.</returns>
		T FindType<T>(DeclType type, string name, bool makeDefault = true) where T : idDecl;
		idMaterial FindMaterial(string name, bool makeDefault = true);
		idDeclSkin FindSkin(string name, bool makeDefault = true);
		idSoundMaterial FindSound(string name, bool makeDefault = true);

		/// <summary>
		/// This finds or creates the decl, but does not cause a parse.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="makeDefault"></param>
		/// <returns></returns>
		idDecl FindTypeWithoutParsing(DeclType type, string name, bool makeDefault = true);
		DeclType GetDeclTypeFromName(string name);
		#endregion

		#region Initialization
		#region Properties
		bool IsInitialized { get; }
		#endregion

		#region Methods
		void Initialize();
		#endregion
		#endregion

		#region Misc.
		void MediaPrint(string format, params object[] args);
		#endregion

		#region Registration
		/// Registers a new folder with decl files.
		/// </summary>
		/// <param name="folder"></param>
		/// <param name="extension"></param>
		/// <param name="defaultType"></param>
		void RegisterDeclFolder(string folder, string extension, DeclType defaultType);

		/// <summary>
		/// Registers a new decl type.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="allocator"></param>
		void RegisterDeclType(string name, DeclType type, idDeclAllocatorBase allocator);
		#endregion
	}

	public enum DeclType
	{
		Unknown,
		Table,
		Material,
		Skin,
		Sound,
		EntityDef,
		ModelDef,
		Fx,
		Particle,
		ArticulatedFigure,
		Pda,
		Video,
		Audio,
		Email,
		ModelExport,
		MapDef
	}

	public enum DeclState
	{
		Unparsed,
		/// <summary>Set if a parse failed due to an error, or the lack of any source.</summary>
		Defaulted,
		Parsed
	}

	public abstract class idDeclAllocatorBase
	{
		public abstract idDecl Create();
	}

	public sealed class idDeclAllocator<T> : idDeclAllocatorBase where T : idDecl, new()
	{
		public override idDecl Create()
		{
			return (idDecl) new T();
		}
	}
}
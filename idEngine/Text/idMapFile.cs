using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace idTech4.Text
{
	/// <summary>
	/// Reads or writes the contents of .map files into a standard internal
	/// format, which can then be moved into private formats for collision
	/// detection, map processing, or editor use.
	/// <para/>
	/// No validation (duplicate planes, null area brushes, etc) is performed.
	/// There are no limits to the number of any of the elements in maps.
	/// The order of entities, brushes, and sides is maintained.
	/// </summary>
	public class idMapFile
	{
	}
}
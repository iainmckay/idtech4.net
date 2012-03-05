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

using idTech4.Text;
using idTech4.Renderer;

namespace idTech4.UI
{
	public sealed class idGuiScript
	{
		#region Constants
		private static readonly GuiCommand[] CommandList = new GuiCommand[] {
			new GuiCommand("set", Script_Set, 2, 999),
			new GuiCommand("setFocus", Script_SetFocus, 1, 1),
			new GuiCommand("endGame", Script_EndGame, 0, 0),
			new GuiCommand("resetTime", Script_ResetTime, 0, 2),
			new GuiCommand("showCursor", Script_ShowCursor, 1, 1),
			new GuiCommand("resetCinematics", Script_ResetCinematics, 0, 2),
			new GuiCommand("transition", Script_Transition, 4, 6),
			new GuiCommand("localSound", Script_LocalSound, 1, 1),
			new GuiCommand("runScript", Script_RunScript, 1, 1),
			new GuiCommand("evalRegs", Script_EvaluateRegisters, 0, 0)
		};
		#endregion

		#region Properties
		public int ConditionRegister
		{
			get
			{
				return _conditionRegister;
			}
			set
			{
				_conditionRegister = value;
			}
		}

		public idGuiScriptList ElseList
		{
			get
			{
				return _elseList;
			}
		}

		public idGuiScriptList IfList
		{
			get
			{
				return _ifList;
			}
		}
		#endregion

		#region Members
		private int _conditionRegister;

		private GuiCommandHandler _handler;

		private List<idWinGuiScript> _parameters = new List<idWinGuiScript>();

		private idGuiScriptList _ifList = new idGuiScriptList();
		private idGuiScriptList _elseList = new idGuiScriptList();
		#endregion

		#region Constructor
		public idGuiScript()
		{
			_conditionRegister = -1;
		}
		#endregion

		#region Methods
		#region Public
		public void FixupParameters(idWindow window)
		{
			if(_handler == Script_Set)
			{
				bool precacheBackground = false;
				bool precacheSounds = false;

				idWinString str = (idWinString) _parameters[0].Variable;
				idWindowVariable dest = window.GetVariableByName(str.ToString(), true);

				if(dest != null)
				{
					_parameters[0].Variable = dest;
					_parameters[0].Owner = false;

					if(dest is idWinBackground)
					{
						precacheBackground = true;
					}
				}
				else if(str.ToString().ToLower() == "cmd")
				{
					precacheSounds = true;
				}

				int parameterCount = _parameters.Count;

				for(int i = 1; i < parameterCount; i++)
				{
					str = (idWinString) _parameters[i].Variable;
					string strValue = str.ToString();

					if(strValue.StartsWith("gui::", StringComparison.InvariantCultureIgnoreCase) == true)
					{
						//  always use a string here, no point using a float if it is one
						//  FIXME: This creates duplicate variables, while not technically a problem since they
						//  are all bound to the same guiDict, it does consume extra memory and is generally a bad thing
						idWinString defVar = new idWinString(null);
						defVar.Init(strValue, window);

						window.AddDefinedVariable(defVar);

						_parameters[i].Variable = defVar;
						_parameters[i].Owner = false;

						//dest = win->GetWinVarByName(*str, true);
						//if (dest) {
						//	delete parms[i].var;
						//	parms[i].var = dest;
						//	parms[i].own = false;
						//}
						// 
					}
					else if(strValue.StartsWith("$") == true)
					{
						// 
						//  dont include the $ when asking for variable
						dest = window.UserInterface.Desktop.GetVariableByName(strValue.Substring(1), true);
						// 					
						if(dest != null)
						{
							_parameters[i].Variable = dest;
							_parameters[i].Owner = false;
						}
					}
					else if(strValue.StartsWith("#str_") == true)
					{
						str.Set(idE.Language.Get(strValue));
					}
					else if(precacheBackground == true)
					{
						idE.DeclManager.FindMaterial(strValue).Sort = (float) MaterialSort.Gui;
					}
					else if(precacheSounds == true)
					{
						idConsole.WriteLine("TODO: PrecacheSounds");
						// Search for "play <...>"
						/*idToken token;
						idParser parser( LEXFL_NOSTRINGCONCAT | LEXFL_ALLOWMULTICHARLITERALS | LEXFL_ALLOWBACKSLASHSTRINGCONCAT );
						parser.LoadMemory(str->c_str(), str->Length(), "command");

						while ( parser.ReadToken(&token) ) {
							if ( token.Icmp("play") == 0 ) {
								if ( parser.ReadToken(&token) && ( token != "" ) ) {
									declManager->FindSound( token.c_str() );
								}
							}
						}*/
					}
				}
			}
			else if(_handler == Script_Transition)
			{
				if(_parameters.Count < 4)
				{
					idConsole.Warning("Window {0} in gui {1} has a bad transition definition", window.Name, window.UserInterface.SourceFile);
				}

				idWinString str = (idWinString) _parameters[0].Variable;

				// 
				DrawWindow destOwner = null;
				idWindowVariable dest = window.GetVariableByName(str.ToString(), true, ref destOwner);
				// 

				if(dest != null)
				{
					_parameters[0].Variable = dest;
					_parameters[0].Owner = false;
				}
				else
				{
					idConsole.Warning("Window {0} in gui {1}: a transition does not have a valid destination var {2}", window.Name, window.UserInterface.SourceFile, str);
				}

				// 
				//  support variables as parameters		
				for(int c = 1; c < 3; c++)
				{
					str = (idWinString) _parameters[c].Variable;
					idWinVector4 v4 = new idWinVector4(null);

					_parameters[c].Variable = v4;
					_parameters[c].Owner = true;

					DrawWindow owner = null;

					if(str.ToString().StartsWith("$") == true)
					{
						dest = window.GetVariableByName(str.ToString().Substring(1), true, ref owner);
					}
					else
					{
						dest = null;
					}

					if(dest != null)
					{
						idWindow ownerParent;
						idWindow destParent;

						if(owner != null)
						{
							ownerParent = (owner.Simple != null) ? owner.Simple.Parent : owner.Window.Parent;
							destParent = (destOwner.Simple != null) ? destOwner.Simple.Parent : destOwner.Window.Parent;

							// if its the rectangle they are referencing then adjust it 
							if((ownerParent != null) && (destParent != null) && (dest == ((owner.Simple != null) ? owner.Simple.GetVariableByName("rect") : owner.Window.GetVariableByName("rect"))))
							{
								Rectangle rect = ((idWinRectangle) dest).Data;
								ownerParent.ClientToScreen(ref rect);
								destParent.ScreenToClient(ref rect);

								v4.Set(dest.ToString());
							}
							else
							{
								v4.Set(dest.ToString());
							}
						}
						else
						{
							v4.Set(dest.ToString());
						}
					}
					else
					{
						v4.Set(str.ToString());
					}
				}
			}
			else
			{
				int c = _parameters.Count;

				for(int i = 0; i < c; i++)
				{
					_parameters[i].Variable.Init(_parameters[i].Variable.ToString(), window);
				}
			}
		}

		public bool Parse(idScriptParser parser)
		{
			// first token should be function call
			// then a potentially variable set of parms
			// ended with a ;
			idToken token;
			GuiCommand cmd = new GuiCommand();

			if((token = parser.ReadToken()) == null)
			{
				parser.Error("Unexpected end of file");
				return false;
			}

			_handler = null;

			string tokenLower = token.ToString().ToLower();

			foreach(GuiCommand tmp in CommandList)
			{
				if(tmp.Name.ToLower() == tokenLower)
				{
					_handler = tmp.Handler;
					cmd = tmp;
					break;
				}
			}

			if(_handler == null)
			{
				parser.Error("Unknown script call {0}", token.ToString());
			}

			// now read parms til ;
			// all parms are read as idWinStr's but will be fixed up later 
			// to be proper types
			while(true)
			{
				if((token = parser.ReadToken()) == null)
				{
					parser.Error("Unexpected end of file");
					return false;
				}

				tokenLower = token.ToString().ToLower();

				if(tokenLower == ";")
				{
					break;
				}
				else if(tokenLower == "}")
				{
					parser.UnreadToken(token);
					break;
				}

				idWinString str = new idWinString(string.Empty);
				str.Set(token.ToString());

				_parameters.Add(new idWinGuiScript(true, str));
			}

			// 
			//  verify min/max params
			if((_handler != null) && ((_parameters.Count < cmd.MinParameterCount) || (_parameters.Count > cmd.MaxParameterCount)))
			{
				parser.Error("incorrect number of parameters for script {0}", cmd.Name);
			}
			// 

			return true;
		}
		#endregion

		#region Commands
		private static void Script_EndGame(idWindow window, List<idWinGuiScript> source)
		{
			idConsole.WriteLine("TODO: Script_EndGame");
		}

		private static void Script_EvaluateRegisters(idWindow window, List<idWinGuiScript> source)
		{
			idConsole.WriteLine("TODO: Script_EvaluateRegisters");
		}

		private static void Script_LocalSound(idWindow window, List<idWinGuiScript> source)
		{
			idConsole.WriteLine("TODO: Script_LocalSound");
		}

		private static void Script_ResetCinematics(idWindow window, List<idWinGuiScript> source)
		{
			idConsole.WriteLine("TODO: Script_ResetCinematics");
		}

		private static void Script_ResetTime(idWindow window, List<idWinGuiScript> source)
		{
			idConsole.WriteLine("TODO: Script_ResetTime");
		}

		private static void Script_RunScript(idWindow window, List<idWinGuiScript> source)
		{
			idConsole.WriteLine("TODO: Script_RunScript");
		}

		private static void Script_Set(idWindow window, List<idWinGuiScript> source)
		{
			idConsole.WriteLine("TODO: Script_Set");
		}

		private static void Script_SetFocus(idWindow window, List<idWinGuiScript> source)
		{
			idConsole.WriteLine("TODO: Script_SetFocus");
		}

		private static void Script_ShowCursor(idWindow window, List<idWinGuiScript> source)
		{
			idConsole.WriteLine("TODO: Script_ShowCursor");
		}

		private static void Script_Transition(idWindow window, List<idWinGuiScript> source)
		{
			idConsole.WriteLine("TODO: Script_Transition");
		}
		#endregion
		#endregion
	}

	public sealed class idGuiScriptList
	{
		#region Members
		private List<idGuiScript> _list = new List<idGuiScript>();
		#endregion

		#region Constructor
		public idGuiScriptList()
		{

		}
		#endregion

		#region Methods
		public void Append(idGuiScript script)
		{
			_list.Add(script);
		}

		public void Execute(idWindow window)
		{
			idConsole.WriteLine("TODO: Execute");

			/*foreach(idGuiScript script in _list)
			{
				if(script.ConditionRegister >= 0)
				{
					if(window.HasOperations == true)
					{
						float f= window.EvalulateRegisters(script.ConditionRegister);

						if(f > 0)
						{							
							if (gs->ifList) {
								win->RunScriptList(gs->ifList);
							}
						} else if (gs->elseList) {
							win->RunScriptList(gs->elseList);
						}
					}
				}

				script.Execute(window);
			}*/
		}

		public void FixupParameters(idWindow window)
		{
			int c = _list.Count;

			for(int i = 0; i < c; i++)
			{
				idGuiScript script = _list[i];
				script.FixupParameters(window);

				if(script.IfList != null)
				{
					script.IfList.FixupParameters(window);
				}

				if(script.ElseList != null)
				{
					script.ElseList.FixupParameters(window);
				}
			}
		}
		#endregion
	}

	internal delegate void GuiCommandHandler(idWindow window, List<idWinGuiScript> source);

	internal struct GuiCommand
	{
		public string Name;
		public GuiCommandHandler Handler;
		public int MinParameterCount;
		public int MaxParameterCount;

		public GuiCommand(string name, GuiCommandHandler handler, int minParameterCount, int maxParameterCount)
		{
			Name = name;
			Handler = handler;
			MinParameterCount = minParameterCount;
			MaxParameterCount = maxParameterCount;
		}
	}

	internal sealed class idWinGuiScript
	{
		#region Properties
		public bool Owner
		{
			get
			{
				return _owner;
			}
			set
			{
				_owner = value;
			}
		}

		public idWindowVariable Variable
		{
			get
			{
				return _variable;
			}
			set
			{
				_variable = value;
			}
		}
		#endregion

		#region Members
		private bool _owner;
		private idWindowVariable _variable;
		#endregion

		#region Constructor
		public idWinGuiScript(bool owner, idWindowVariable var)
		{
			_owner = owner;
			_variable = var;
		}
		#endregion
	}
}
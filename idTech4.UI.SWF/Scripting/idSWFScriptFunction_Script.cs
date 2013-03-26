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
using System;
using System.Collections.Generic;
using System.Diagnostics;

using idTech4.Services;

namespace idTech4.UI.SWF.Scripting
{
	/// <summary>
	/// A script function that's implemented in action script.
	/// </summary>
	public class idSWFScriptFunction_Script : idSWFScriptFunction
	{
		#region Properties
		public byte[] Data
		{
			set
			{
				_bitStream.Data = value;
			}
		}
		#endregion

		#region Members
		private List<idSWFScriptVariable> _registers;
		private List<idSWFScriptObject> _scope = new List<idSWFScriptObject>();
		private idSWFConstantPool _constants   = new idSWFConstantPool();
		private idSWFSpriteInstance _defaultSprite;		// some actions have an implicit sprite they work off of (e.g. Action_GotoFrame outside of object scope)

		private idSWFBitStream _bitStream = new idSWFBitStream();

		private ushort _flags;

		private int _callStackLevel;
		#endregion

		#region Constructor
		public idSWFScriptFunction_Script(List<idSWFScriptObject> scope, idSWFSpriteInstance defaultSprite)
		{
			_registers     = new List<idSWFScriptVariable>(4);
			_defaultSprite = defaultSprite;

			SetScope(scope);			
		}
		#endregion

		#region Misc.
		private idSWFScriptVariable Run(idSWFScriptObject scriptObject, idSWFStack stack, idSWFBitStream bitStream)
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			_callStackLevel = -1;

			idSWFSpriteInstance sprite        = scriptObject.Sprite;
			idSWFSpriteInstance currentTarget = sprite;

			if(currentTarget == null)
			{
				sprite = currentTarget = _defaultSprite;
			}

			_callStackLevel++;

			while(bitStream.Position < bitStream.Length) 
			{
				idSWFAction code    = (idSWFAction) bitStream.ReadByte();
				ushort recordLength = 0;

				if((int) code >= 0x80)
				{
					recordLength = bitStream.ReadUInt16();
				}

				if(cvarSystem.GetInt("swf_debug") >= 3)
				{
					// stack[0] is always 0 so don't read it
					if(cvarSystem.GetInt("swf_debug") >= 4)
					{
						for(int i = stack.Count - 1; i >= 0; i--)
						{
							idLog.WriteLine("   {0}: {1} ({2})", (char) (64 + stack.Count - i), stack[i].ToString(), stack[i].TypeOf());
						}

						for(int i = 0; i < _registers.Count; i++)
						{
							if(_registers[i].IsUndefined == false)
							{
								idLog.WriteLine(" R{0}: {1} ({2})", i, _registers[i].ToString(), _registers[i].TypeOf());
							}
						}
					}

					idLog.WriteLine("SWF{0}: code Action_{1}", _callStackLevel, code);
				}

				switch(code)
				{
					case idSWFAction.Return:
						_callStackLevel--;
						return stack.A;

					case idSWFAction.End:
						_callStackLevel--;
						return new idSWFScriptVariable();

			/*case Action_NextFrame:
				if ( verify( currentTarget != NULL ) ) {
					currentTarget->NextFrame();
				} else if ( swf_debug.GetInteger() > 0 ) {
					idLib::Printf( "SWF: no target movie clip for nextFrame\n" );
				}
				break;*/

					case idSWFAction.PrevFrame:
						if(currentTarget != null)
						{
							currentTarget.PreviousFrame();
						}
						else if(cvarSystem.GetInt("swf_debug") > 0)
						{
							idLog.Warning("SWF: no target movie clip for prevFrame");
						}
						break;
			/*case Action_Play:
				if ( verify( currentTarget != NULL ) ) {
					currentTarget->Play();
				} else if ( swf_debug.GetInteger() > 0 ) {
					idLib::Printf( "SWF: no target movie clip for play\n" );
				}
				break;*/

					case idSWFAction.Stop:
						if(currentTarget != null)
						{
							currentTarget.Stop();
						}
						else if(cvarSystem.GetInt("swf_debug") > 0)
						{
							idLog.WriteLine("SWF: no target movie clip for stop");
						}
						break;

						/*
			case Action_ToggleQuality: break;
			case Action_StopSounds: break;
			case Action_GotoFrame: {
				assert( recordLength == 2 );
				int frameNum = bitstream.ReadU16() + 1;
				if ( verify( currentTarget != NULL ) ) {
					currentTarget->RunTo( frameNum );
				} else if ( swf_debug.GetInteger() > 0 ) {
					idLib::Printf( "SWF: no target movie clip for runTo %d\n", frameNum );
				}
				break;
			}
			case Action_SetTarget: {
				const char * targetName = (const char *)bitstream.ReadData( recordLength );
				if ( verify( thisSprite != NULL ) ) {
					currentTarget = thisSprite->ResolveTarget( targetName );
				} else if ( swf_debug.GetInteger() > 0 ) {
					idLib::Printf( "SWF: no target movie clip for setTarget %s\n", targetName );
				}
				break;
			}
			case Action_GoToLabel: {
				const char * targetName = (const char *)bitstream.ReadData( recordLength );
				if ( verify( currentTarget != NULL ) ) {
					currentTarget->RunTo( currentTarget->FindFrame( targetName ) );
				} else if ( swf_debug.GetInteger() > 0 ) {
					idLib::Printf( "SWF: no target movie clip for runTo %s\n", targetName );
				}
				break;
			}*/

					case idSWFAction.Push:
						idSWFBitStream pushStream = new idSWFBitStream(bitStream.ReadData(recordLength));

						while(pushStream.Position < pushStream.Length)
						{
							byte type = pushStream.ReadByte();

							switch(type)
							{
								case 0: stack.Alloc().Set(pushStream.ReadString()); break;
								case 1: throw new NotImplementedException(); //stack.Alloc().Set(pushStream.ReadSingle()); break;
								case 2: stack.Alloc().SetNull(); break;
								case 3: stack.Alloc().SetUndefined(); break;
								case 4: stack.Add((idSWFScriptVariable) _registers[pushStream.ReadByte()].Clone()); break;
								case 5: stack.Alloc().Set(pushStream.ReadByte() != 0); break;
								case 6: stack.Alloc().Set((float) pushStream.ReadDouble()); break;
								case 7: stack.Alloc().Set(pushStream.ReadInt32()); break;
								case 8: stack.Alloc().Set(_constants.Get(pushStream.ReadByte())); break;
								case 9: stack.Alloc().Set(_constants.Get(pushStream.ReadUInt16())); break;
							}
						}
						break;

					case idSWFAction.Pop:
						stack.Pop(1);
						break;

					case idSWFAction.Add:
						stack.B.Set(stack.B.ToFloat() + stack.A.ToFloat());
						stack.Pop(1);
						break;

					case idSWFAction.Subtract:
						stack.B.Set(stack.B.ToFloat() - stack.A.ToFloat());
						stack.Pop(1);
						break;

					case idSWFAction.Multiply:
						stack.B.Set(stack.B.ToFloat() * stack.A.ToFloat());
						stack.Pop(1);
						break;

					case idSWFAction.Divide:
						stack.B.Set(stack.B.ToFloat() / stack.A.ToFloat());
						stack.Pop(1);
						break;

					case idSWFAction.Equals:
						stack.B.Set(stack.B.ToFloat() == stack.A.ToFloat());
						stack.Pop(1);
						break;

					case idSWFAction.Less:
						stack.B.Set(stack.B.ToFloat() < stack.A.ToFloat());
						stack.Pop(1);
						break;

					case idSWFAction.And:
						stack.B.Set((stack.B.ToFloat() > 0) && (stack.A.ToFloat() > 0));
						stack.Pop(1);
						break;

					case idSWFAction.Or:
						stack.B.Set((stack.B.ToFloat() > 0) || (stack.A.ToFloat() > 0));
						stack.Pop(1);
						break;

					case idSWFAction.Not:
						stack.A.Set(!stack.A.ToBool());
						break;

			/*case Action_StringEquals:
				stack.B().SetBool( stack.B().ToString() == stack.A().ToString() );
				stack.Pop( 1 );
				break;
			case Action_StringLength:
				stack.A().SetInteger( stack.A().ToString().Length() );
				break;
			case Action_StringAdd:
				stack.B().SetString( stack.B().ToString() + stack.A().ToString() );
				stack.Pop( 1 );
				break;
			case Action_StringExtract:
				stack.C().SetString( stack.C().ToString().Mid( stack.B().ToInteger(), stack.A().ToInteger() ) );
				stack.Pop( 2 );
				break;
			case Action_StringLess:
				stack.B().SetBool( stack.B().ToString() < stack.A().ToString() );
				stack.Pop( 1 );
				break;
			case Action_StringGreater:
				stack.B().SetBool( stack.B().ToString() > stack.A().ToString() );
				stack.Pop( 1 );
				break;
			case Action_ToInteger:
				stack.A().SetInteger( stack.A().ToInteger() );
				break;
			case Action_CharToAscii:
				stack.A().SetInteger( stack.A().ToString()[0] );
				break;
			case Action_AsciiToChar:
				stack.A().SetString( va( "%c", stack.A().ToInteger() ) );
				break;
			case Action_Jump:
				bitstream.Seek( bitstream.ReadS16() );
				break;
			case Action_If: {
				int16 offset = bitstream.ReadS16();
				if ( stack.A().ToBool() ) {
					bitstream.Seek( offset );
				}
				stack.Pop( 1 );
				break;
			}*/

					case idSWFAction.GetVariable:
						string variableName = stack.A.ToString();

						for(int i = _scope.Count - 1; i >= 0; i--)
						{
							stack.A.Set(_scope[i].Get(variableName).ToString());

							if(stack.A.IsUndefined == false)
							{
								break;
							}
						}

						if((stack.A.IsUndefined == true) && (cvarSystem.GetInt("swf_debug") > 1))
						{
							idLog.WriteLine("SWF: unknown variable {0}", variableName);
						}
						break;
			/*}
			case Action_SetVariable: {
				idStr variableName = stack.B().ToString();
				bool found = false;
				for ( int i = scope.Num() - 1; i >= 0; i-- ) {
					if ( scope[i]->HasProperty( variableName ) ) {
						scope[i]->Set( variableName, stack.A() );
						found = true;
						break;
					}
				}
				if ( !found ) {
					thisObject->Set( variableName, stack.A() );
				}
				stack.Pop( 2 );
				break;
			}
			case Action_GotoFrame2: {

				uint32 frameNum = 0;
				uint8 flags = bitstream.ReadU8();
				if ( flags & 2 ) {
					frameNum += bitstream.ReadU16();
				}

				if ( verify( thisSprite != NULL ) ) {
					if ( stack.A().IsString() ) {
						frameNum += thisSprite->FindFrame( stack.A().ToString() );
					} else {
						frameNum += (uint32)stack.A().ToInteger();
					}
					if ( ( flags & 1 ) != 0 ){
						thisSprite->Play();
					} else {
						thisSprite->Stop();
					}
					thisSprite->RunTo( frameNum );
				} else if ( swf_debug.GetInteger() > 0 ) {
					if ( ( flags & 1 ) != 0 ){
						idLib::Printf( "SWF: no target movie clip for gotoAndPlay\n" );
					} else {
						idLib::Printf( "SWF: no target movie clip for gotoAndStop\n" );
					}
				}
				stack.Pop( 1 );
				break;
			}
			case Action_GetProperty: {
				if ( verify( thisSprite != NULL ) ) {
					idSWFSpriteInstance * target = thisSprite->ResolveTarget( stack.B().ToString() );
					stack.B() = target->scriptObject->Get( GetPropertyName( stack.A().ToInteger() ) );
				} else if ( swf_debug.GetInteger() > 0 ) {
					idLib::Printf( "SWF: no target movie clip for getProperty\n" );
				}
				stack.Pop( 1 );
				break;
			}
			case Action_SetProperty: {
				if ( verify( thisSprite != NULL ) ) {
					idSWFSpriteInstance * target = thisSprite->ResolveTarget( stack.C().ToString() );
					target->scriptObject->Set( GetPropertyName( stack.B().ToInteger() ), stack.A() );
				} else if ( swf_debug.GetInteger() > 0 ) {
					idLib::Printf( "SWF: no target movie clip for setProperty\n" );
				}
				stack.Pop( 3 );
				break;
			}
			case Action_Trace:
				idLib::PrintfIf( swf_debug.GetInteger() > 0, "SWF Trace: %s\n", stack.A().ToString().c_str() );
				stack.Pop( 1 );
				break;
			case Action_GetTime:
				stack.Alloc().SetInteger( Sys_Milliseconds() );
				break;
			case Action_RandomNumber:
				assert( thisSprite && thisSprite->sprite && thisSprite->sprite->GetSWF() );
				stack.A().SetInteger( thisSprite->sprite->GetSWF()->GetRandom().RandomInt( stack.A().ToInteger() ) );
				break;*/

					case idSWFAction.CallFunction:
						string functionName = stack.A.ToString();

						idSWFScriptVariable function = null;
						idSWFScriptObject obj        = null;

						for(int i = _scope.Count - 1; i >= 0; i--)
						{
							function = _scope[i].Get(functionName);

							if(function.IsUndefined == false)
							{
								obj = _scope[i];
								break;
							}
						}

						stack.Pop(1);

						idSWFParameterList parms = new idSWFParameterList(stack.A.ToInt32());

						stack.Pop(1);

						for(int i = 0; i < parms.Count; i++)
						{
							parms[i] = (idSWFScriptVariable) stack.A.Clone();
							stack.Pop(1);
						}

						if(function.IsFunction == true)
						{
							stack.Add((idSWFScriptVariable) function.Function.Invoke(obj, parms).Clone());
						}
						else
						{
							if(cvarSystem.GetInt("swf_debug") > 0)
							{
								idLog.WriteLine("SWF: unknown function {0}", functionName);
								stack.Alloc().SetUndefined();
							}
						}
						break;
			
			/*case Action_CallMethod: {
				idStr functionName = stack.A().ToString();
				// If the top stack is undefined but there is an object, it's calling the constructor
				if ( functionName.IsEmpty() || stack.A().IsUndefined() || stack.A().IsNULL() ) {
					functionName = "__constructor__";
				}
				idSWFScriptObject * object = NULL;
				idSWFScriptVar function;
				if ( stack.B().IsObject() ) {
					object = stack.B().GetObject();
					function = object->Get( functionName );
					if ( !function.IsFunction() ) {
						idLib::PrintfIf( swf_debug.GetInteger() > 1, "SWF: unknown method %s on %s\n", functionName.c_str(), object->DefaultValue( true ).ToString().c_str() );
					}
				} else {
					idLib::PrintfIf( swf_debug.GetInteger() > 1, "SWF: NULL object for method %s\n", functionName.c_str() );
				}

				stack.Pop( 2 );

				idSWFParmList parms;
				parms.SetNum( stack.A().ToInteger() );
				stack.Pop( 1 );
				for ( int i = 0; i < parms.Num(); i++ ) {
					parms[i] = stack.A();
					stack.Pop( 1 );
				}

				if ( function.IsFunction() ) {
					stack.Alloc() = function.GetFunction()->Call( object, parms );
				} else {
					stack.Alloc().SetUndefined();
				}
				break;
			}
			case Action_ConstantPool: {
				constants.Clear();
				uint16 numConstants = bitstream.ReadU16();
				for ( int i = 0; i < numConstants; i++ ) {
					constants.Append( idSWFScriptString::Alloc( bitstream.ReadString() ) );
				}
				break;
			}
			case Action_DefineFunction: {
				idStr functionName = bitstream.ReadString();

				idSWFScriptFunction_Script * newFunction = idSWFScriptFunction_Script::Alloc();
				newFunction->SetScope( scope );
				newFunction->SetConstants( constants );
				newFunction->SetDefaultSprite( defaultSprite );

				uint16 numParms = bitstream.ReadU16();
				newFunction->AllocParameters( numParms );
				for ( int i = 0; i < numParms; i++ ) {
					newFunction->SetParameter( i, 0, bitstream.ReadString() );
				}
				uint16 codeSize = bitstream.ReadU16();
				newFunction->SetData( bitstream.ReadData( codeSize ), codeSize );

				if ( functionName.IsEmpty() ) {
					stack.Alloc().SetFunction( newFunction );
				} else {
					thisObject->Set( functionName, idSWFScriptVar( newFunction ) );
				}
				newFunction->Release();
				break;
			}
			case Action_DefineFunction2: {
				idStr functionName = bitstream.ReadString();

				idSWFScriptFunction_Script * newFunction = idSWFScriptFunction_Script::Alloc();
				newFunction->SetScope( scope );
				newFunction->SetConstants( constants );
				newFunction->SetDefaultSprite( defaultSprite );

				uint16 numParms = bitstream.ReadU16();

				// The number of registers is from 0 to 255, although valid values are 1 to 256. 
				// There must always be at least one register for DefineFunction2, to hold "this" or "super" when required.
				uint8 numRegs = bitstream.ReadU8() + 1; 

				// Note that SWF byte-ordering causes the flag bits to be reversed per-byte
				// from how the swf_file_format_spec_v10.pdf document describes the ordering in ActionDefineFunction2.
				// PreloadThisFlag is byte 0, not 7, PreloadGlobalFlag is 8, not 15.  
				uint16 flags = bitstream.ReadU16();

				newFunction->AllocParameters( numParms );
				newFunction->AllocRegisters( numRegs );
				newFunction->SetFlags( flags );

				for ( int i = 0; i < numParms; i++ ) {
					uint8 reg = bitstream.ReadU8();
					const char * name = bitstream.ReadString();
					if ( reg >= numRegs ) {
						idLib::Warning( "SWF: Parameter %s in function %s bound to out of range register %d", name, functionName.c_str(), reg );
						reg = 0;
					}
					newFunction->SetParameter( i, reg, name );
				}

				uint16 codeSize = bitstream.ReadU16();
				newFunction->SetData( bitstream.ReadData( codeSize ), codeSize );

				if ( functionName.IsEmpty() ) {
					stack.Alloc().SetFunction( newFunction );
				} else {
					thisObject->Set( functionName, idSWFScriptVar( newFunction ) );
				}
				newFunction->Release();
				break;
			}
			case Action_Enumerate: {
				idStr variableName = stack.A().ToString();
				for ( int i = scope.Num() - 1; i >= 0; i-- ) {
					stack.A() = scope[i]->Get( variableName );
					if ( !stack.A().IsUndefined() ) {
						break;
					}
				}
				if ( !stack.A().IsObject() ) {
					stack.A().SetNULL();
				} else {
					idSWFScriptObject * object = stack.A().GetObject();
					object->AddRef();
					stack.A().SetNULL();
					for ( int i = 0; i < object->NumVariables(); i++ ) {
						stack.Alloc().SetString( object->EnumVariable( i ) );
					}
					object->Release();
				}
				break;
			}
			case Action_Enumerate2: {
				if ( !stack.A().IsObject() ) {
					stack.A().SetNULL();
				} else {
					idSWFScriptObject * object = stack.A().GetObject();
					object->AddRef();
					stack.A().SetNULL();
					for ( int i = 0; i < object->NumVariables(); i++ ) {
						stack.Alloc().SetString( object->EnumVariable( i ) );
					}
					object->Release();
				}
				break;
			}
			case Action_Equals2: {
				stack.B().SetBool( stack.A().AbstractEquals( stack.B() ) );
				stack.Pop( 1 );
				break;
			}
			case Action_StrictEquals: {
				stack.B().SetBool( stack.A().StrictEquals( stack.B() ) );
				stack.Pop( 1 );
				break;
			}
			case Action_GetMember: {
				if ( ( stack.B().IsUndefined() || stack.B().IsNULL() ) && swf_debug.GetInteger() > 1 ) {
					idLib::Printf( "SWF: tried to get member %s on an invalid object in sprite '%s'\n", stack.A().ToString().c_str(), thisSprite != NULL ? thisSprite->GetName() : "" );
				}
				if ( stack.B().IsObject() ) {
					idSWFScriptObject * object = stack.B().GetObject();
					if ( stack.A().IsNumeric() ) {
						stack.B() = object->Get( stack.A().ToInteger() );
					} else {
						stack.B() = object->Get( stack.A().ToString() );
					}
					if ( stack.B().IsUndefined() && swf_debug.GetInteger() > 1 ) {
						idLib::Printf( "SWF: unknown member %s\n", stack.A().ToString().c_str() );
					}
				} else if ( stack.B().IsString() ) {
					idStr propertyName = stack.A().ToString();
					if ( propertyName.Cmp( "length" ) == 0 ) {
						stack.B().SetInteger( stack.B().ToString().Length() );
					} else if ( propertyName.Cmp( "value" ) == 0 ) {
						// Do nothing
					} else {
						stack.B().SetUndefined();
					}
				} else if ( stack.B().IsFunction() ) {
					idStr propertyName = stack.A().ToString();
					if ( propertyName.Cmp( "prototype" ) == 0 ) {
						// if this is a function, it's a class definition function, and it just wants the prototype object
						// create it if it hasn't been already, and return it
						idSWFScriptFunction * sfs = stack.B().GetFunction();
						idSWFScriptObject * object = sfs->GetPrototype();

						if ( object == NULL ) {
							object = idSWFScriptObject::Alloc();
							// Set the __proto__ to the main Object prototype
							idSWFScriptVar baseObjConstructor = scope[0]->Get( "Object" );
							idSWFScriptFunction *baseObj = baseObjConstructor.GetFunction();
							object->Set( "__proto__", baseObj->GetPrototype() );
							sfs->SetPrototype( object );
						}

						stack.B() = idSWFScriptVar( object );
					} else {
						stack.B().SetUndefined();
					}
				} else {
					stack.B().SetUndefined();
				}
				stack.Pop( 1 );
				break;
			}*/

					case idSWFAction.SetMember:
						if(stack.C.IsObject == true)
						{
							idSWFScriptObject tmp = stack.C.Object;

							if(stack.B.IsNumeric == true)
							{
								tmp.Set(stack.B.ToInt32(), stack.A);
							}
							else
							{
								tmp.Set(stack.B.ToString(), stack.A);
							}
						}

						stack.Pop(3);
						break;
			
			/*case Action_InitArray: {
				idSWFScriptObject * object = idSWFScriptObject::Alloc();
				object->MakeArray();

				int numElements = stack.A().ToInteger();
				stack.Pop( 1 );

				for ( int i = 0; i < numElements; i++ ) {
					object->Set( i, stack.A() );
					stack.Pop( 1 );
				}

				stack.Alloc().SetObject( object );

				object->Release();
				break;
			}
			case Action_InitObject: {
				idSWFScriptObject * object = idSWFScriptObject::Alloc();

				int numElements = stack.A().ToInteger();
				stack.Pop( 1 );

				for ( int i = 0; i < numElements; i++ ) {
					object->Set( stack.B().ToString(), stack.A() );
					stack.Pop( 2 );
				}

				stack.Alloc().SetObject( object );

				object->Release();
				break;
			}
			case Action_NewObject: {
				idSWFScriptObject * object = idSWFScriptObject::Alloc();

				idStr functionName = stack.A().ToString();
				stack.Pop( 1 );

				if ( functionName.Cmp( "Array" ) == 0 ) {
					object->MakeArray();

					int numElements = stack.A().ToInteger();
					stack.Pop( 1 );

					for ( int i = 0; i < numElements; i++ ) {
						object->Set( i, stack.A() );
						stack.Pop( 1 );
					}

					idSWFScriptVar baseObjConstructor = scope[0]->Get( "Object" );
					idSWFScriptFunction *baseObj = baseObjConstructor.GetFunction();
					object->Set( "__proto__", baseObj->GetPrototype() );
					// object prototype is not set here because it will be auto created from Object later
				} else {
					idSWFParmList parms;
					parms.SetNum( stack.A().ToInteger() );
					stack.Pop( 1 );
					for ( int i = 0; i < parms.Num(); i++ ) {
						parms[i] = stack.A();
						stack.Pop( 1 );
					}

					idSWFScriptVar objdef = scope[0]->Get( functionName );
					if ( objdef.IsFunction() ) {
						idSWFScriptFunction * constructorFunction = objdef.GetFunction();
						object->Set( "__proto__", constructorFunction->GetPrototype() );
						object->SetPrototype( constructorFunction->GetPrototype() );
						constructorFunction->Call( object, parms );
					} else {
						idLib::Warning( "SWF: Unknown class definition %s", functionName.c_str() );
					}
				}

				stack.Alloc().SetObject( object );

				object->Release();
				break;
			}
			case Action_Extends: {
				idSWFScriptFunction * superclassConstructorFunction = stack.A().GetFunction();
				idSWFScriptFunction *subclassConstructorFunction = stack.B().GetFunction();
				stack.Pop( 2 );

				idSWFScriptObject * scriptObject = idSWFScriptObject::Alloc();
				scriptObject->SetPrototype( superclassConstructorFunction->GetPrototype() );
				scriptObject->Set( "__proto__", idSWFScriptVar( superclassConstructorFunction->GetPrototype() ) );
				scriptObject->Set( "__constructor__", idSWFScriptVar( superclassConstructorFunction ) );

				subclassConstructorFunction->SetPrototype( scriptObject );

				scriptObject->Release();
				break;
			}
			case Action_TargetPath: {
				if ( !stack.A().IsObject() ) {
					stack.A().SetUndefined();
				} else {
					idSWFScriptObject * object = stack.A().GetObject();
					if ( object->GetSprite() == NULL ) {
						stack.A().SetUndefined();
					} else {
						idStr dotName = object->GetSprite()->name.c_str();
						for ( idSWFSpriteInstance * target = object->GetSprite()->parent; target != NULL; target = target->parent ) {
							dotName = target->name + "." + dotName;
						}
						stack.A().SetString( dotName );
					}
				}
				break;
			}
			case Action_With: {
				int withSize = bitstream.ReadU16();
				idSWFBitStream bitstream2( bitstream.ReadData( withSize ), withSize, false );
				if ( stack.A().IsObject() ) {
					idSWFScriptObject * withObject = stack.A().GetObject();
					withObject->AddRef();
					stack.Pop( 1 );
					scope.Append( withObject );
					Run( thisObject, stack, bitstream2 );
					scope.SetNum( scope.Num() - 1 );
					withObject->Release();
				} else {
					if ( swf_debug.GetInteger() > 0 ) {
						idLib::Printf( "SWF: with() invalid object specified\n" );
					}
					stack.Pop( 1 );
				}
				break;
			}
			case Action_ToNumber:
				stack.A().SetFloat( stack.A().ToFloat() );
				break;
			case Action_ToString:
				stack.A().SetString( stack.A().ToString() );
				break;
			case Action_TypeOf:
				stack.A().SetString( stack.A().TypeOf() );
				break;
			case Action_Add2: {
				if ( stack.A().IsString() || stack.B().IsString() ) {
					stack.B().SetString( stack.B().ToString() + stack.A().ToString() );
				} else {
					stack.B().SetFloat( stack.B().ToFloat() + stack.A().ToFloat() );
				}
				stack.Pop( 1 );
				break;
			}
			case Action_Less2: {
				if ( stack.A().IsString() && stack.B().IsString() ) {
					stack.B().SetBool( stack.B().ToString() < stack.A().ToString() );
				} else {
					stack.B().SetBool( stack.B().ToFloat() < stack.A().ToFloat() );
				}
				stack.Pop( 1 );
				break;
			}
			case Action_Greater: {
				if ( stack.A().IsString() && stack.B().IsString() ) {
					stack.B().SetBool( stack.B().ToString() > stack.A().ToString() );
				} else {
					stack.B().SetBool( stack.B().ToFloat() > stack.A().ToFloat() );
				}
				stack.Pop( 1 );
				break;
			}
			case Action_Modulo: {
				int32 a = stack.A().ToInteger();
				int32 b = stack.B().ToInteger();
				if ( a == 0 ) {
					stack.B().SetUndefined();
				} else {
					stack.B().SetInteger( b % a );
				}
				stack.Pop( 1 );
				break;
			}
			case Action_BitAnd:
				stack.B().SetInteger( stack.B().ToInteger() & stack.A().ToInteger() );
				stack.Pop( 1 );
				break;
			case Action_BitLShift:
				stack.B().SetInteger( stack.B().ToInteger() << stack.A().ToInteger() );
				stack.Pop( 1 );
				break;
			case Action_BitOr:
				stack.B().SetInteger( stack.B().ToInteger() | stack.A().ToInteger() );
				stack.Pop( 1 );
				break;
			case Action_BitRShift:
				stack.B().SetInteger( stack.B().ToInteger() >> stack.A().ToInteger() );
				stack.Pop( 1 );
				break;
			case Action_BitURShift:
				stack.B().SetInteger( (uint32)stack.B().ToInteger() >> stack.A().ToInteger() );
				stack.Pop( 1 );
				break;
			case Action_BitXor:
				stack.B().SetInteger( stack.B().ToInteger() ^ stack.A().ToInteger() );
				stack.Pop( 1 );
				break;
			case Action_Decrement:
				stack.A().SetFloat( stack.A().ToFloat() - 1.0f );
				break;
			case Action_Increment:
				stack.A().SetFloat( stack.A().ToFloat() + 1.0f );
				break;
			case Action_PushDuplicate: {
				idSWFScriptVar dup = stack.A();
				stack.Alloc() = dup;
				break;
			}
			case Action_StackSwap: {
				idSWFScriptVar temp = stack.A();
				stack.A() = stack.B();
				stack.A() = temp;
				break;
			}
			case Action_StoreRegister: {
				uint8 registerNumber = bitstream.ReadU8();
				registers[ registerNumber ] = stack.A();
				break;
			}
			case Action_DefineLocal: {
				scope[scope.Num()-1]->Set( stack.B().ToString(), stack.A() );
				stack.Pop( 2 );
				break;
			}
			case Action_DefineLocal2: {
				scope[scope.Num()-1]->Set( stack.A().ToString(), idSWFScriptVar() );
				stack.Pop( 1 );
				break;
			}
			case Action_Delete: {
				if ( swf_debug.GetInteger() > 0 ) {
					idLib::Printf( "SWF: Delete ignored\n" );
				}
				// We no longer support deleting variables because the performance cost of updating the hash tables is not worth it
				stack.Pop( 2 );
				break;
			}
			case Action_Delete2: {
				if ( swf_debug.GetInteger() > 0 ) {
					idLib::Printf( "SWF: Delete2 ignored\n" );
				}
				// We no longer support deleting variables because the performance cost of updating the hash tables is not worth it
				stack.Pop( 1 );
				break;
			}
			// These are functions we just don't support because we never really needed to
			case Action_CloneSprite:
			case Action_RemoveSprite:
			case Action_Call:
			case Action_SetTarget2:
			case Action_NewMethod:*/
			
					default:
						idLog.Warning("SWF: Unhandled Action {0}", code);

						// We have to abort here because the rest of the script is basically meaningless now
						Debug.Assert(false);

						_callStackLevel--;

						return new idSWFScriptVariable();
				}
			}

			_callStackLevel--;

			return new idSWFScriptVariable();
		}

		private void SetScope(List<idSWFScriptObject> newScope) 
		{
			Debug.Assert(_scope.Count == 0);

			_scope.Clear();
			_scope.AddRange(newScope);
		}
		#endregion

		#region idSWFScriptFunction implementation
		public override idSWFScriptVariable Invoke(idSWFScriptObject scriptObj, idSWFParameterList parms)
		{
			if(_bitStream.HasData == false)
			{
				throw new InvalidOperationException("No data loaded.");
			}

			_bitStream.Rewind();

			// We assume scope[0] is the global scope
			Debug.Assert(_scope.Count > 0);

			if(scriptObj == null)
			{
				scriptObj = _scope[0];
			}

			idSWFScriptObject locals = new idSWFScriptObject();
			idSWFStack stack         = new idSWFStack(parms.Count + 1);
			
			for(int i = 0; i < parms.Count; i++)
			{
				stack[parms.Count - i - 1] = (idSWFScriptVariable) parms[i].Clone();

				// unfortunately at this point we don't have the function name anymore, so our warning messages aren't very detailed
				if(i < _parameters.Count)
				{
					if((_parameters[i].Register > 0) && (_parameters[i].Register < _registers.Count))
					{
						_registers[_parameters[i].Register] = parms[i];
					}
				
					locals.Set(_parameters[i].Name, parms[i]);
				}
			}

			// set any additional parameters to undefined
			for(int i = parms.Count; i < _parameters.Count; i++)
			{
				if((_parameters[i].Register > 0) && (_parameters[i].Register < _registers.Count))
				{
					_registers[_parameters[i].Register].SetUndefined();
				}

				locals.Set(_parameters[i].Name, new idSWFScriptVariable());
			}

			stack.A.Set(parms.Count);

			int preloadReg = 1;

			if((_flags & (1ul << 0)) != 0)
			{
				// load "this" into a register
				_registers[preloadReg].Set(scriptObj);
				preloadReg++;
			}

			if((_flags & (1ul << 1)) != 0)
			{
				// create "this"
				locals.Set("this", new idSWFScriptVariable(scriptObj));
			}

			if((_flags & (1ul << 2)) != 0)
			{
				idSWFScriptObject arguments = new idSWFScriptObject();

				// load "arguments" into a register
				arguments.MakeArray();

				int elementCount = parms.Count;

				for(int i = 0; i < elementCount; i++)
				{
					arguments.Set(i, parms[i]);
				}

				_registers[preloadReg].Set(arguments);
				preloadReg++;				
			}

			if((_flags & (1ul << 3)) != 0)
			{
				idSWFScriptObject arguments = new idSWFScriptObject();

				// create "arguments"
				arguments.MakeArray();

				int elementCount = parms.Count;

				for(int i = 0; i < elementCount; i++)
				{
					arguments.Set(i, parms[i]);
				}

				locals.Set("arguments", new idSWFScriptVariable(arguments));
			}

			if((_flags & (1ul << 4)) != 0)
			{
				// load "super" into a register
				_registers[preloadReg].Set(scriptObj.Prototype);
				preloadReg++;
			}

			if((_flags & (1ul << 5)) != 0)
			{
				// create "super"
				locals.Set("super", new idSWFScriptVariable(scriptObj.Prototype));
			}

			if((_flags & (1ul << 6)) != 0)
			{
				// preload _root
				_registers[preloadReg] = (idSWFScriptVariable) _scope[0].Get("_root").Clone();
				preloadReg++;
			}

			if((_flags & (1ul << 7)) != 0)
			{
				// preload _parent
				if((scriptObj.Sprite != null) && (scriptObj.Sprite.Parent != null))
				{
					_registers[preloadReg].Set(scriptObj.Sprite.Parent.ScriptObject);
				}
				else
				{
					_registers[preloadReg].SetNull();
				}
				
				preloadReg++;
			}

			if((_flags & (1ul << 8)) != 0)
			{
				// load "_global" into a register
				_registers[preloadReg].Set(_scope[0]);
				preloadReg++;
			}

			int scopeSize = _scope.Count;
			_scope.Add(locals);

			idSWFScriptVariable retVal = Run(scriptObj, stack, _bitStream);

			Debug.Assert(_scope.Count == (scopeSize + 1));

			_scope.RemoveRange(scopeSize, _scope.Count - scopeSize);

			return retVal;
		}
		#endregion
	}
}
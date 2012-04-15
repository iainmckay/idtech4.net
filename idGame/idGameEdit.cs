using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.UI;

namespace idTech4.Game
{
	public class idGameEdit : idGameEdit
	{
		#region Constructor
		public idGameEdit()
		{
			idR.GameEdit = this;
		}
		#endregion

		#region Methods
		public void ParseSpawnArgsToRenderEntity(idDict args, /*idRenderEntity*/ object renderEntity)
		{
			//renderEntity.Clear();
			
			string temp = args.GetString("model");

			//TODO
			/*modelDef = NULL;
			if ( temp[0] != '\0' ) {
				modelDef = static_cast<const idDeclModelDef *>( declManager->FindType( DECL_MODELDEF, temp, false ) );
				if ( modelDef ) {
					renderEntity->hModel = modelDef->ModelHandle();
				}
				if ( !renderEntity->hModel ) {
					renderEntity->hModel = renderModelManager->FindModel( temp );
				}
			}
			if ( renderEntity->hModel ) {
				renderEntity->bounds = renderEntity->hModel->Bounds( renderEntity );
			} else*/
			{
				// TODO: renderEntity.Bounds = new idBounds();
			}

			temp = args.GetString("skin");

			if(temp != null)
			{
				//renderEntity.CustomSkin = idR.DeclManager.FindSkin(temp);
			}
			else if(1 == 0 /* modelDef != null*/)
			{
				// TODO
				//renderEntity->customSkin = modelDef->GetDefaultSkin();
			}

			temp = args.GetString("shader");

			if(temp != null)
			{
				//renderEntity.CustomShader = idR.DeclManager.FindMaterial(temp);
			}

			//renderEntity.Origin = args.GetVector3("origin", Vector3.Zero);

			// get the rotation matrix in either full form, or single angle form
			//renderEntity.Axis = args.GetMatrix("rotation", "1 0 0 0 1 0 0 0 1");

			/*if(renderEntity.Axis == Matrix.Identity)
			{
				float angle = args.GetFloat("angle");

				if(angle != 0.0f)
				{
					renderEntity.Axis = Matrix.CreateRotationY(angle); // TODO: this might fuck things up, upside down models and stuff
				}
				else
				{
					renderEntity.Axis = Matrix.Identity;
				}
			}*/

			// TODO
			//renderEntity.ReferencedSound = null;

			// get shader parms
			Vector3 color = args.GetVector3("_color", new Vector3(1, 1, 1));

			/*float[] shaderParms = renderEntity.ShaderParms;*/

			/*shaderParms[(int) ShaderParameter.Red] = color.X;
			shaderParms[(int) ShaderParameter.Green] = color.Y;
			shaderParms[(int) ShaderParameter.Blue] = color.Z;*/

			/*shaderParms[3] = args.GetFloat("shaderParm3", 1);
			shaderParms[4] = args.GetFloat("shaderParm4", 0);
			shaderParms[5] = args.GetFloat("shaderParm5", 0);
			shaderParms[6] = args.GetFloat("shaderParm6", 0);
			shaderParms[7] = args.GetFloat("shaderParm7", 0);
			shaderParms[8] = args.GetFloat("shaderParm8", 0);
			shaderParms[9] = args.GetFloat("shaderParm9", 0);
			shaderParms[10] = args.GetFloat("shaderParm10", 0);
			shaderParms[11] = args.GetFloat("shaderParm11", 0);

			renderEntity.ShaderParms = shaderParms;*/

			// check noDynamicInteractions flag
			/*renderEntity.NoDynamicInteractions = args.GetBool("noDynamicInteractions");

			// check noshadows flag
			renderEntity.NoShadow = args.GetBool("noshadows");

			// check noselfshadows flag
			renderEntity.NoSelfShadow = args.GetBool("noselfshadows");*/

			// TODO
			// init any guis, including entity-specific states
			/*for(int i = 0; i < renderEntity.Length; i++)
			{
				temp = args.GetString(i == 0 ? "gui" : string.Format("gui{0}", i + 1));

				if(temp != null)
				{
					renderEntity.Gui[i] = AddRenderGui(temp, args);
				}
			}*/
		}

		private idUserInterface AddRenderGui(string name, idDict args)
		{
			// TODO
			/*idKeyValue kv = args.MatchPrefix("gui_parm", null);
			idUserInterface gui = idR.UIManager.FindInterface(name, true, (kv != null));

			UpdateGuiParams(gui, args);

			return gui;*/
			return null;
		}

		private void UpdateGuiParams(idUserInterface gui, idDict args)
		{
			// TODO
			/*if((gui == null) || (args == null))
			{
				return;
			}

			idKeyValue kv = args.MatchPrefix("gui_parm", null);

			while(kv != null)
			{
				gui.State.Set(kv.Key, kv.Value);
				kv = args.MatchPrefix("gui_parm", kv);
			}

			gui.SetState("noninteractive", args.GetBool("gui_noninteractive"));
			gui.StateChanged(idR.Game.Time);*/
		}
		#endregion
	}
}

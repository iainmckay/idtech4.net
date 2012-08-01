using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Game.Animation;
using idTech4.Renderer;
using idTech4.Text;
using idTech4.Text.Decl;
using idTech4.UI;

namespace idTech4.Game
{
	public class idGameEdit : idBaseGameEdit
	{
		#region Constructor
		public idGameEdit()
		{
			
		}
		#endregion

		#region Methods
		#region Private
		private idUserInterface AddRenderGui(string name, idDict args)
		{
			idUserInterface gui = idR.UIManager.FindInterface(name, true, args.ContainsKey("gui_parm"));

			UpdateGuiParams(gui, args);

			return gui;
		}

		private void UpdateGuiParams(idUserInterface gui, idDict args)
		{
			if((gui == null) || (args == null))
			{
				return;
			}

			foreach(KeyValuePair<string, string> kvp in args.MatchPrefix("gui_parm"))
			{
				gui.State.Set(kvp.Key, kvp.Value);
			}

			gui.State.Set("noninteractive", args.GetBool("gui_noninteractive"));
			gui.StateChanged(idR.Game.Time);
		}
		#endregion
		#endregion

		#region idBaseGameEdit implementation
		#region Methods
		public override idRenderEntity ParseSpawnArgsToRenderEntity(idDict args)
		{
			idRenderEntity renderEntity = new idRenderEntity();
			idDeclModel modelDef = null;
			
			string temp = args.GetString("model");

			if(temp != string.Empty)
			{
				modelDef = idE.DeclManager.FindType<idDeclModel>(DeclType.ModelDef, temp, false);

				if(modelDef != null)
				{
					renderEntity.Model = modelDef.Model;
				}

				if(renderEntity.Model == null)
				{
					renderEntity.Model = idE.RenderModelManager.FindModel(temp);
				}
			}

			if(renderEntity.Model != null)
			{
				renderEntity.Bounds = renderEntity.Model.GetBounds(renderEntity);
			}
			else
			{
				renderEntity.Bounds = new idBounds();
			}

			temp = args.GetString("skin");

			if(temp != null)
			{
				renderEntity.CustomSkin = idR.DeclManager.FindSkin(temp);
			}
			else if(modelDef != null)
			{
				renderEntity.CustomSkin = modelDef.DefaultSkin;
			}

			temp = args.GetString("shader");

			if(temp != null)
			{
				renderEntity.CustomMaterial = idR.DeclManager.FindMaterial(temp);
			}

			renderEntity.Origin = args.GetVector3("origin", Vector3.Zero);

			// get the rotation matrix in either full form, or single angle form
			renderEntity.Axis = args.GetMatrix("rotation", "1 0 0 0 1 0 0 0 1");

			if(renderEntity.Axis == Matrix.Identity)
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
			}

			// TODO
			//renderEntity.ReferencedSound = null;

			// get shader parms
			Vector3 color = args.GetVector3("_color", new Vector3(1, 1, 1));

			float[] materialParms = renderEntity.MaterialParameters;

			materialParms[(int) MaterialParameter.Red] = color.X;
			materialParms[(int) MaterialParameter.Green] = color.Y;
			materialParms[(int) MaterialParameter.Blue] = color.Z;

			materialParms[3] = args.GetFloat("shaderParm3", 1);
			materialParms[4] = args.GetFloat("shaderParm4", 0);
			materialParms[5] = args.GetFloat("shaderParm5", 0);
			materialParms[6] = args.GetFloat("shaderParm6", 0);
			materialParms[7] = args.GetFloat("shaderParm7", 0);
			materialParms[8] = args.GetFloat("shaderParm8", 0);
			materialParms[9] = args.GetFloat("shaderParm9", 0);
			materialParms[10] = args.GetFloat("shaderParm10", 0);
			materialParms[11] = args.GetFloat("shaderParm11", 0);

			renderEntity.MaterialParameters = materialParms;

			// check noDynamicInteractions flag
			renderEntity.NoDynamicInteractions = args.GetBool("noDynamicInteractions");

			// check noshadows flag
			renderEntity.NoShadow = args.GetBool("noshadows");

			// check noselfshadows flag
			renderEntity.NoSelfShadow = args.GetBool("noselfshadows");

			// TODO
			// init any guis, including entity-specific states
			for(int i = 0; i < renderEntity.Gui.Length; i++)
			{
				temp = args.GetString(i == 0 ? "gui" : string.Format("gui{0}", i + 1));

				if(temp != null)
				{
					renderEntity.Gui[i] = AddRenderGui(temp, args);
				}
			}

			return renderEntity;
		}		
		#endregion
		#endregion
	}
}
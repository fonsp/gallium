using System.Collections.Generic;
using GraphicsLibrary;
using GraphicsLibrary.Collision;
using GraphicsLibrary.Core;
using GraphicsLibrary.Hud;
using GraphicsLibrary.Input;
using GraphicsLibrary.Voxel;
using OpenTK;
using OpenTK.Graphics;

namespace Gallium.Program
{
	public partial class Game
	{

		public override void InitGame()
		{
			#region Program arguments

			//TODO: Program arguments

			#endregion
			#region Entities

			

			skybox.mesh.material.textureName = "skybox";
			skybox.isLit = false;
			//skybox.mesh.material.baseColor = new Color4(.4f, .4f, .4f, 1.0f);
			
			testHueScale.mesh.material.textureName = "huescale";
			testHueScale.position = new Vector3(100, 0, 100);
			

			hudDebug = new HudDebug("hudDebug");
			crosshair.width = 32f;
			crosshair.height = 32f;

			HudBase.Instance.Add(crosshair);
			HudBase.Instance.Add(hudDebug);
			HudBase.Instance.Add(ActionTrigger.textField);

			RootNode.Instance.Add(worldNode);
			RootNode.Instance.Add(monster);
			RootNode.Instance.Add(skybox);

			#endregion

			Camera.Instance.position = new Vector3(0, 80, 0);
			Camera.Instance.ZNear = .01f;
			//Camera.Instance.ZFar = 100f;

			RenderWindow.Instance.Title = "Gallium";
			InputManager.CursorLockState = CursorLockState.Centered;
			InputManager.HideCursor();
		}
	}
}
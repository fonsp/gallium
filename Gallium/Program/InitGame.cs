using System.Collections.Generic;
using Gallium.Blocks;
using GraphicsLibrary;
using GraphicsLibrary.Collision;
using GraphicsLibrary.Core;
using GraphicsLibrary.Hud;
using GraphicsLibrary.Input;
using OpenTK;
using OpenTK.Graphics;

namespace Gallium.Program
{
	public partial class Game
	{
		private Cannon cannon = new Cannon("cannon");
		private List<CollisionAABB> mapCollision = new List<CollisionAABB>();
		private Node ship = new Node("ship");
		private ParticleField starfield;

		public override void InitGame()
		{
			#region Program arguments

			//TODO: Program arguments

			#endregion
			#region Entities

			skybox.mesh.material.textureName = "skybox";
			skybox.isLit = false;
			skybox.mesh.material.baseColor = new Color4(.4f, .4f, .4f, 1.0f);

			cannon.position = new Vector3(100f, 0f, 100f);

			Camera.Instance.friction = new Vector3((float)config.GetDouble("playerFriction"), 1, (float)config.GetDouble("playerFriction"));

			monster.mesh.material.textureName = "huescale";

			ship.Add(new Block("blockA") {			position = new Vector3(-25f, 0f, 0f) });
			ship.Add(new SupportBlock("blockB") {	position = new Vector3(25f, 0f, 0f) });
			ship.Add(new SupportBlock("blockC") {	position = new Vector3(-75f, 0f, 0f) });
			ship.Add(new SupportBlock("blockD") {	position = new Vector3(75f, 0f, 0f) });
			ship.Add(new SupportBlock("blockE") {	position = new Vector3(-25f, 50f, 0f) });
			ship.Add(new SupportBlock("blockF") {	position = new Vector3(25f, 50f, 0f) });


			/*Debug.WriteLine(Quaternion.FromAxisAngle(Vector3.UnitY, 1f * (float)Math.PI / 2.0f).ToString());
			Debug.WriteLine(Quaternion.FromAxisAngle(Vector3.UnitY, 2f * (float)Math.PI / 2.0f).ToString());
			Debug.WriteLine(Quaternion.FromAxisAngle(Vector3.UnitY, 3f * (float)Math.PI / 2.0f).ToString());
			Debug.WriteLine(Quaternion.FromAxisAngle(Vector3.UnitX, (float)Math.PI / 2.0f).ToString());
			Debug.WriteLine(Quaternion.FromAxisAngle(Vector3.UnitX, (float)Math.PI / -2.0f).ToString());
			Debug.WriteLine(Quaternion.Identity.ToString());*/

			//ship.rotationalVelocity = Quaternion.FromAxisAngle(new Vector3(0f, 1f, 0f), 0.314159265358979f);

			testHueScale.mesh.material.textureName = "huescale";
			testHueScale.position = new Vector3(100, 0, 100);

			starfield = new ParticleField(128, "starfieldemitter", new Material("star", Color4.White));
			starfield.RandomizeParticles(5f, 10f);

			hudDebug = new HudDebug("hudDebug");
			crosshair.width = 32f;
			crosshair.height = 32f;

			HudBase.Instance.Add(crosshair);
			HudBase.Instance.Add(hudDebug);
			HudBase.Instance.Add(ActionTrigger.textField);

			RootNode.Instance.Add(monster);
			RootNode.Instance.Add(skybox);
			RootNode.Instance.Add(cannon);
			RootNode.Instance.Add(testHueScale);
			RootNode.Instance.Add(ship);
			RootNode.Instance.Add(starfield);

			#endregion

			Camera.Instance.position = new Vector3(0, 250, 0);

			RenderWindow.Instance.Title = "Gallium";
			InputManager.CursorLockState = CursorLockState.Centered;
			InputManager.HideCursor();
		}
	}
}
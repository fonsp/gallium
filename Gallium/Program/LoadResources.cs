using System.IO;
using GraphicsLibrary.Content;
using GraphicsLibrary.Core;
using GraphicsLibrary.Hud;
using GraphicsLibrary.Voxel;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Gallium.Program
{
	public partial class Game
	{
		private Entity skybox = new Entity("skybox");
		private Entity monster = new Entity("monster");
		private Entity testHueScale = new Entity("testHueScale");
        private Entity screen = new Entity("screen");
		private WorldNode worldNode = new WorldNode("world");
		private HudDebug hudDebug;
		private HudImage crosshair = new HudImage("crosshair", "crosshair0");

		public override void LoadResources()
		{
			config = new Config("Game.ini");

			TextureManager.AddTexture("monsterTexture", @"Content/textures/monsterText1.png");
			TextureManager.AddTexture("skybox", @"Content/textures/skyboxblue.png");
			TextureManager.AddTexture("font0", @"Content/textures/font0.png");
			TextureManager.AddTexture("font1", @"Content/textures/font1.png");
			TextureManager.AddTexture("font2", @"Content/textures/font2.png");
			TextureManager.AddTexture("crosshair0", @"Content/textures/crosshair0.png");
			TextureManager.AddTexture("white", @"Content/textures/white.png");
			TextureManager.AddTexture("huescale", @"Content/textures/huescale.png");
			TextureManager.AddTexture("map0a", @"Content/textures/map0/darkBrick.png", TextureMinFilter.Linear, TextureMagFilter.Linear);
			TextureManager.AddTexture("map0b", @"Content/textures/map0/rockWall.png", TextureMinFilter.Linear, TextureMagFilter.Linear);
			TextureManager.AddTexture("map0c", @"Content/textures/map0/crate.png", TextureMinFilter.Linear, TextureMagFilter.Linear);
			TextureManager.AddTexture("map0d", @"Content/textures/map0/metal.png", TextureMinFilter.Linear, TextureMagFilter.Linear);
			TextureManager.AddTexture("map0e", @"Content/textures/floor0.png", TextureMinFilter.Linear, TextureMagFilter.Linear);
			TextureManager.AddTexture("playerTexture", @"Content/textures/player.png");
			TextureManager.AddTexture("testimage", @"Content/textures/testimage.png");
			TextureManager.AddTexture("star", @"Content/textures/star.png");
            TextureManager.AddTexture("screen", @"Content/textures/testimage.png");

			testHueScale.mesh = ObjConverter.ConvertObjToMesh(File.ReadAllText(@"Content/models/monsterUVd.obj"), new Vector3(101, -19, 205));
			monster.mesh = ObjConverter.ConvertObjToMesh(File.ReadAllText(@"Content/models/monsterUVd.obj"), new Vector3(101, -19, 205));
			monster.scale = new Vector3(1f, 1f, 1f);
			skybox.mesh = ObjConverter.ConvertObjToMesh(File.ReadAllText(@"Content/models/sphere.obj"));
			//skybox.scale = new Vector3(10000f, 10000f, 10000f);
			for(int i = 0; i < skybox.mesh.polygonList.Count; i++)
			{
				var verts = skybox.mesh.polygonList[i].vertices;
				for (int j = 0; j < verts.Length; j++)
				{
					verts[j].pos *= -1000f;
					/*float temp = verts[j].pos.Y;
					verts[j].pos.Y = verts[j].pos.X;
					verts[j].pos.X = -temp;*/
				}
			}

			skybox.mesh.material.baseColor = Color4.BlanchedAlmond;
			//skybox.scale = new Vector3(-1f, -1f, -1f);
            screen.mesh = ObjConverter.ConvertObjToMesh(File.ReadAllText(@"Content/models/slide.obj"));

			//mapCollision.AddRange(ObjConverter.ConvertObjToAABBarray(File.ReadAllText(@"Content/models/map1/collision.obj")));
		}
	}
}
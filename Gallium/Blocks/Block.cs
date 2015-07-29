using System;
using GraphicsLibrary.Core;
using OpenTK;

namespace Gallium.Blocks
{
	public class Block:Entity
	{
		public bool blockProperty;
		private Vector3 _facing = new Vector3(0f, 0f, -1f);
		private const float s = 0.70710678118654752440084436210485f;
		public Vector3 facing
		{
			get
			{
				return _facing;
			}
			set
			{
				_facing = value;
				if(value.X > .5f)
					orientation = new Quaternion(0f, s, 0f, s);
				if(value.X < -.5f)
					orientation = new Quaternion(0f, s, 0f, -s);
				if(value.Y > .5f)
					orientation = new Quaternion(-s, 0f, 0f, s);
				if(value.Y < -.5f)
					orientation = new Quaternion(s, 0f, 0f, s);
				if(value.Z > .5f)
					orientation = new Quaternion(0f, 1f, 0f, 0f);
				if(value.Z < -.5f)
					orientation = new Quaternion(0f, 0f, 0f, 1f);
			}
		}

		public Block(string name)
			: base(name)
		{
			mesh = Mesh.DuplicateFrom(blockMesh);
			mass = 95.3f;
			inertia = CalculateSphericalInertia(mass, 1.33f); //TODO: precalc
		}

		public static float CalculateSphericalInertia(float mass, float density)
		{
			return 0.66666667f * mass * (float)Math.Pow(0.75 * mass / (density * Math.PI), 0.6666666667);
		}

		public static Mesh blockMesh;
	}

	public class SupportBlock:Block
	{
		public SupportBlock(string name)
			: base(name)
		{
			mesh = Mesh.DuplicateFrom(blockMesh);
			mass = 40.0f;
			inertia = CalculateSphericalInertia(mass, 1.33f); //TODO: precalc
		}

		new public static Mesh blockMesh;
	}
}

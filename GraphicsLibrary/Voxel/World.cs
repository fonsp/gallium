using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsLibrary.Voxel
{
	public struct IntVector
	{
		public int x, y;
		public IntVector(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public static IntVector operator +(IntVector a, IntVector b)
		{
			return new IntVector(a.x + b.x, a.y + b.y);
		}

		public static IntVector operator -(IntVector a, IntVector b)
		{
			return new IntVector(a.x - b.x, a.y - b.y);
		}

		public static IntVector operator *(IntVector a, IntVector b)
		{
			return new IntVector(a.x * b.x - a.y * b.y, a.x*b.y + a.y*b.x);
		}

		public override string ToString()
		{
			return "(" + x + "," + y + ")";
		}
	}

	public class World
	{
		public Dictionary<IntVector, Chunk> generatedChunks = new Dictionary<IntVector, Chunk>();


		private Chunk GenerateChunkAt(IntVector d)
		{
			Chunk c = GenerateTempChunkAt(d);
			AddChunk(c);
			return c;
		}

		public static Chunk GenerateTempChunkAt(IntVector d)
		{
			Chunk c = new Chunk(d.x, d.y);
			c.GenerateData();
			return c;
		}

		public void AddChunk(Chunk c)
		{
			IntVector d = new IntVector(c.xx, c.yy);
			lock(generatedChunks)
			{
				if(!generatedChunks.ContainsKey(d))
				{
					generatedChunks.Add(d, c);
				}
			}
		}

		public Chunk GetChunk(IntVector d)
		{
			if(generatedChunks.ContainsKey(d))
			{
				return generatedChunks[d];
			}
			return GenerateChunkAt(d);
		}

		public Chunk GetChunk(int x, int y)
		{
			return GetChunk(new IntVector(x, y));
		}

		public byte GetBlock(int ix, int iy, int iz)
		{
			if(iy < 0 || iy > 255)
			{
				return 0;
			}
			int xx = (ix < 0) ? ((ix - 15) / 16) : (ix / 16);
			int yy = (iz < 0) ? ((iz - 15) / 16) : (iz / 16);

			return GetChunk(xx, yy).Get(ix - 16 * xx, iy, iz - 16 * yy);
		}

		public byte GetBlock(float x, float y, float z)
		{
			return GetBlock((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z));
		}

		public byte GetBlock(Vector3 position)
		{
			return GetBlock(position.X, position.Y, position.Z);
		}

	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphicsLibrary.Core;
using OpenTK;
using OpenTK.Graphics.OpenGL;




namespace GraphicsLibrary.Voxel
{
	public class Chunk
	{
		public int xx, yy;
		public byte[] cdata;
		private static Random random = new Random();

		public Chunk(int xx, int yy)
		{
			this.xx = xx;
			this.yy = yy;
			cdata = new byte[16*128*16];
		}

		public void GenerateData()
		{
			for(int x = 0; x < 16; x++)
			{
				for(int z = 0; z < 16; z++)
				{
					for(int y = 0; y < 128; y++)
					{
						cdata[x + 16*z + 256*y] = BlockFunction(x + 16 * xx, y, z + 16 * yy);
					}
				}
			}
		}

		private byte BlockFunction(int x, int y, int z)
		{
			/*Random rnd = new Random();
			return (byte)Math.Max(0, rnd.Next(y / -16, 8));*/

			float value = .5f * Simplex.simplex_noise(1, x / 100f, y / 200f, z / 100f) + 
                .5f * Simplex.simplex_noise(1, x/20f, y/20f, z/20f) +
                .015f * (y - 60);
			if(value < 0.9) { return 1; }
			if(value < 1.1) { return 2; }

			return 0;
			return (byte)(y / 16);
			return (byte)Math.Max(0, random.Next(y / -16 - 16, 8));
		}

		public byte Get(int x, int y, int z)
		{
			return cdata[x + 16 * z + 256 * y];
		}

		public void Set(int x, int y, int z, byte value)
		{
			cdata[x + 16 * z + 256 * y] = value;
		}

		private static int[][][] textureIndices = new int[256][][];
		static Chunk()
		{
			for(int index = 0; index < 256; index++)
			{
				int bti = index + (index / 16);
				textureIndices[index] = new int[2][];
				textureIndices[index][0] = new int[] { 17 + bti, 01 + bti, 0 + bti };
				textureIndices[index][1] = new int[] { 17 + bti, 18 + bti, 1 + bti };
			}
		}

		private int[] GetTextureIndex(int block, int dir, int AorB)
		{
			switch(block)
			{
				case 2:
					if(dir == 0) { return textureIndices[3][AorB]; }
					if(dir == 1) { return textureIndices[0][AorB]; }
					return textureIndices[2][AorB];
					break;
			}
			return textureIndices[block][AorB];
		}

		public Mesh GenerateMesh()
		{
			Mesh output = new Mesh();
			UpdateMesh(output);
			return output;
		}

		public void UpdateMesh(Mesh mesh)
		{
			mesh.polygonList = new List<Polygon>();
			List<Vertex> vOuput = new List<Vertex>();


			int vindex = 0;
			int nindex = 0;
			int tindex = 0;

			List<Vector3> vertices = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<Vector2> textCoords = new List<Vector2>();

			List<Face> faces = new List<Face>();

			normals.Add(Vector3.UnitX);
			normals.Add(Vector3.UnitY);
			normals.Add(Vector3.UnitZ);
			normals.Add(-Vector3.UnitX);
			normals.Add(-Vector3.UnitY);
			normals.Add(-Vector3.UnitZ);
			nindex = 6;


			for(int y = 0; y < 17; y++)
			{
				for(int x = 0; x < 17; x++)
				{
					textCoords.Add(new Vector2(x / 16f, y / 16f));
				}
			}
			tindex = 17 * 17;

			for(int x = 0; x < 16; x++)
			{
				for(int y = 0; y < 128; y++)
				{
					for(int z = 0; z < 16; z++)
					{
						byte currentBlock = cdata[x + 16 * z + 256 * y];
						if(currentBlock != 0)
						{
							Vector3 corner = new Vector3(x, y, z);
							vertices.Add(corner);
							vertices.Add(corner + new Vector3(1, 0, 0));
							vertices.Add(corner + new Vector3(1, 0, 1));
							vertices.Add(corner + new Vector3(0, 0, 1));
							vertices.Add(corner + new Vector3(0, 1, 0));
							vertices.Add(corner + new Vector3(1, 1, 0));
							vertices.Add(corner + new Vector3(1, 1, 1));
							vertices.Add(corner + new Vector3(0, 1, 1));

							//int bti = currentBlock + (currentBlock / 16);

							//int[] texA = new int[] { 17 + bti, 1 + bti, 18 + bti };
							//int[] texB = new int[] { 17 + bti, 0 + bti, 01 + bti };
							

							if(GetOutsideRange(x - 1, y, z) == 0)
							{
								faces.Add(new Face(new int[] { vindex + 0, vindex + 7, vindex + 4 }, GetTextureIndex(currentBlock, 0, 0), new int[] { 3, 3, 3 }));
								faces.Add(new Face(new int[] { vindex + 0, vindex + 3, vindex + 7 }, GetTextureIndex(currentBlock, 0, 1), new int[] { 3, 3, 3 }));
							}
							if(GetOutsideRange(x, y, z + 1) == 0)
							{
								faces.Add(new Face(new int[] { vindex + 3, vindex + 6, vindex + 7 }, GetTextureIndex(currentBlock, 0, 0), new int[] { 2, 2, 2 }));
								faces.Add(new Face(new int[] { vindex + 3, vindex + 2, vindex + 6 }, GetTextureIndex(currentBlock, 0, 1), new int[] { 2, 2, 2 }));
							}
							if(GetOutsideRange(x + 1, y, z) == 0)
							{

								faces.Add(new Face(new int[] { vindex + 2, vindex + 5, vindex + 6 }, GetTextureIndex(currentBlock, 0, 0), new int[] { 0, 0, 0 }));
								faces.Add(new Face(new int[] { vindex + 2, vindex + 1, vindex + 5 }, GetTextureIndex(currentBlock, 0, 1), new int[] { 0, 0, 0 }));
							}
							if(GetOutsideRange(x, y, z - 1) == 0)
							{

								faces.Add(new Face(new int[] { vindex + 1, vindex + 4, vindex + 5 }, GetTextureIndex(currentBlock, 0, 0), new int[] { 5, 5, 5 }));
								faces.Add(new Face(new int[] { vindex + 1, vindex + 0, vindex + 4 }, GetTextureIndex(currentBlock, 0, 1), new int[] { 5, 5, 5 }));
							}
							if(GetOutsideRange(x, y + 1, z) == 0)
							{

								faces.Add(new Face(new int[] { vindex + 4, vindex + 6, vindex + 5 }, GetTextureIndex(currentBlock, 1, 0), new int[] { 1, 1, 1 }));
								faces.Add(new Face(new int[] { vindex + 4, vindex + 7, vindex + 6 }, GetTextureIndex(currentBlock, 1, 1), new int[] { 1, 1, 1 }));
							}
							if(GetOutsideRange(x, y - 1, z) == 0)
							{
								faces.Add(new Face(new int[] { vindex + 0, vindex + 2, vindex + 3 }, GetTextureIndex(currentBlock, 2, 0), new int[] { 4, 4, 4 }));
								faces.Add(new Face(new int[] { vindex + 0, vindex + 1, vindex + 2 }, GetTextureIndex(currentBlock, 2, 1), new int[] { 4, 4, 4 }));
							}

							vindex += 8;
						}
					}
				}
			}

			foreach(Face f in faces)
			{
				Vertex[] vertexArr = new Vertex[f.vIndices.Length];

				if(f.vIndices.Length == 3)
				{
					for(int i = 0; i < 3; i++)
					{
						vertexArr[i] = new Vertex(vertices[f.vIndices[i]], normals[f.vnIndices[i]], textCoords[f.vtIndices[i]]);
					}
				}
				else
				{
					Debugger.Break();
				}
				vOuput.AddRange(vertexArr);
				mesh.polygonList.Add(new Polygon(vertexArr));
			}
			mesh.vertexArray = vOuput.ToArray();
			mesh.vertexArrayLength = mesh.vertexArray.Length;
			//Debug.WriteLine("Obj conversion complete: " + faces.Count + " faces were converted.");
			faces = null;
			vertices = null;
			vOuput = null;
			//GC.Collect();
		}

		private byte GetOutsideRange(int x, int y, int z)
		{
			if(x < 0 || x > 15 || y < 0 || y > 127 || z < 0 || z > 15)
			{
				//return 0;
				return 1;
			}
			return cdata[x + 16 * z + 256 * y];
		}
	}
}

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
		public byte[,,] cdata;

		public Chunk(int xx, int yy)
		{
			this.xx = xx;
			this.yy = yy;
			cdata = new byte[16, 256, 16];


		}

		public void GenerateData()
		{
			for(int y = 0; y < 256; y++)
			{
				for(int x = 0; x < 16; x++)
				{
				
					for(int z = 0; z < 16; z++)
					{
						cdata[x, y, z] = BlockFunction(x + 16 * xx, y, z + 16 * yy);
					}
				}
			}
		}

		private byte BlockFunction(int x, int y, int z)
		{
			Random rnd = new Random();
			return (byte)Math.Max(0, rnd.Next(y / -16, 8));
		}

		public byte Get(int x, int y, int z)
		{
			return cdata[x, y, z];
		}

		public void Set(int x, int y, int z, byte value)
		{
			cdata[x, y, z] = value;
		}

		public Mesh GenerateMesh()
		{
			Mesh output = new Mesh();
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
			tindex = 17*17;

			for(int x = 0; x < 16; x++)
			{
				for(int y = 0; y < 256; y++)
				{
					for(int z = 0; z < 16; z++)
					{
						byte currentBlock = cdata[x, y, z];
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

							int[] vertexIndices;
							int[] normalIndices = new int[3];
							int[] textureIndices = new int[3];

							int bti = currentBlock;// + (currentBlock % 16);

							//int[] texA = new int[] { 17 + bti, 1 + bti, 18 + bti };
							//int[] texB = new int[] { 17 + bti, 0 + bti, 01 + bti };

							int[] texA = new int[] { 17 + bti, 01 + bti, 0 + bti };
							int[] texB = new int[] { 17 + bti, 18 + bti, 1 + bti };

							if(GetLocalCData(x-1, y, z) == 0)
							{
								faces.Add(new Face(new int[] { vindex + 0, vindex + 7, vindex + 4 }, texA, new int[] { 3, 3, 3 }));
								faces.Add(new Face(new int[] { vindex + 0, vindex + 3, vindex + 7 }, texB, new int[] { 3, 3, 3 }));
							}
							if(GetLocalCData(x, y, z+1) == 0)
							{
								faces.Add(new Face(new int[] { vindex + 3, vindex + 6, vindex + 7 }, texA, new int[] { 2, 2, 2 }));
								faces.Add(new Face(new int[] { vindex + 3, vindex + 2, vindex + 6 }, texB, new int[] { 2, 2, 2 }));
							}
							if(GetLocalCData(x+1, y, z) == 0)
							{

								faces.Add(new Face(new int[] { vindex + 2, vindex + 5, vindex + 6 }, texA, new int[] { 0, 0, 0 }));
								faces.Add(new Face(new int[] { vindex + 2, vindex + 1, vindex + 5 }, texB, new int[] { 0, 0, 0 }));
							}
							if(GetLocalCData(x, y, z-1) == 0)
							{

								faces.Add(new Face(new int[] { vindex + 1, vindex + 4, vindex + 5 }, texA, new int[] { 5, 5, 5 }));
								faces.Add(new Face(new int[] { vindex + 1, vindex + 0, vindex + 4 }, texB, new int[] { 5, 5, 5 }));
							}
							if(GetLocalCData(x, y+1, z) == 0)
							{

								faces.Add(new Face(new int[] { vindex + 4, vindex + 6, vindex + 5 }, texA, new int[] { 1, 1, 1 }));
								faces.Add(new Face(new int[] { vindex + 4, vindex + 7, vindex + 6 }, texB, new int[] { 1, 1, 1 }));
							}
							if(GetLocalCData(x, y-1, z) == 0)
							{
								faces.Add(new Face(new int[] { vindex + 0, vindex + 2, vindex + 3 }, texA, new int[] { 4, 4, 4 }));
								faces.Add(new Face(new int[] { vindex + 0, vindex + 1, vindex + 2 }, texB, new int[] { 4, 4, 4 }));
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
				output.polygonList.Add(new Polygon(vertexArr));
			}
			output.vertexArray = vOuput.ToArray();
			output.vertexArrayLength = output.vertexArray.Length;
			Debug.WriteLine("Obj conversion complete: " + faces.Count + " faces were converted.");
			return output;
		}

		private byte GetLocalCData(int x, int y, int z)
		{
			if(x < 0 || x > 15 || y < 0 || y > 255 || z < 0 || z > 15)
			{
				return 0;
			}
			return cdata[x, y, z];
		}
	}
}

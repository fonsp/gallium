using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace GraphicsLibrary.Core
{
	public class Mesh
	{
		public List<Polygon> polygonList = new List<Polygon>();
		public Material material = new Material();
		public Shader shader;
		//VBO
		public Vertex[] vertexArray;
		public int vertexArrayLength;
		public bool useVBO = false;
		public bool hasVBO = false;
		public uint[] VBOids = new uint[2];

		/*public override string ToString()
		{
			string output = polygonList.Count + "\n";
			foreach(Polygon p in polygonList)
			{
				//output += p.vertices[3].pos + "\n";
				foreach(Vertex v in p.vertices)
				{
					output += v.pos + "\n";
				}
			}
			return output;
		}*/

		public void GenerateVBO()
		{
			if(!useVBO)
			{
				Debug.WriteLine("WARNING: VBO generation failed: Mesh is using immediate mode");
				return;
			}
			if(hasVBO)
			{
				Debug.WriteLine("WARNING: VBO generation failed: VBO already exists");
				return;
			}
			if(vertexArray == null)
			{
				Debug.WriteLine("WARNING: VBO generation failed: vertexArray is null");
				return;
			}

			int stride = BlittableValueType.StrideOf(new Vertex[1]);



			//GL.GenBuffers(1, out VBOids[0]); // TODO: Slowwww
			VBOids[0] = GetNewBuffer();

			GL.BindBuffer(BufferTarget.ArrayBuffer, VBOids[0]);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexArray.Length * stride), vertexArray, BufferUsageHint.StaticDraw);


			VBOids[1] = GetNewBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOids[1]);
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(vertexArray.Length * sizeof(uint)), IntPtr.Zero, BufferUsageHint.StaticDraw);
			GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, (IntPtr)(vertexArray.Length * sizeof(uint)), RenderWindow.Instance.elementBase);
			hasVBO = true;
			vertexArray = null;
			polygonList = null;
			//GC.Collect();
			//GC.WaitForPendingFinalizers();
			//Debug.WriteLine("VBO generation complete");
		}

		public static int bufferCollectionSize = 255;
		private static List<uint> bufferCollection = new List<uint>();

		private static uint GetNewBuffer()
		{

			if(bufferCollection.Count == 0)
			{
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();

				uint[] tempBuffer = new uint[bufferCollectionSize];
				GL.GenBuffers(bufferCollectionSize, tempBuffer);
				bufferCollection.AddRange(tempBuffer);

				stopwatch.Stop();
				Console.WriteLine("Buffers generated in {0}ms", stopwatch.ElapsedMilliseconds);
			}

			uint output = bufferCollection[0];
			bufferCollection.RemoveAt(0);
			return output;
		}

		public void UpdateVBO()
		{
			if(!useVBO)
			{
				Debug.WriteLine("WARNING: VBO update failed: Mesh is using immediate mode");
				return;
			}
			if(!hasVBO)
			{
				Debug.WriteLine("WARNING: VBO update failed: no VBO exists");
				return;
			}
			if(vertexArray == null)
			{
				Debug.WriteLine("WARNING: VBO update failed: vertexArray is null");
				return;
			}

			int stride = BlittableValueType.StrideOf(new Vertex[1]);


			GL.BindBuffer(BufferTarget.ArrayBuffer, VBOids[0]);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexArray.Length * stride), vertexArray, BufferUsageHint.StaticDraw);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOids[1]);
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(vertexArray.Length * sizeof(uint)), IntPtr.Zero, BufferUsageHint.StaticDraw);
			GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, (IntPtr)(vertexArray.Length * sizeof(uint)), RenderWindow.Instance.elementBase);

			hasVBO = true;
			vertexArray = null;
			polygonList = null;
			GC.Collect();
			GC.WaitForPendingFinalizers();
			//Debug.WriteLine("VBO generation complete");
		}

		public void RemoveVBO()
		{
			if(!useVBO)
			{
				Debug.WriteLine("WARNING: VBO removal failed: Mesh is using immediate mode");
				return;
			}
			if(!hasVBO)
			{
				Debug.WriteLine("WARNING: VBO removal failed: VBO does not exist");
				return;
			}

			GL.DeleteBuffers(2, VBOids);
		}

		public static Mesh DuplicateFrom(Mesh template)
		{
			Mesh mesh = new Mesh();
			mesh.vertexArray = template.vertexArray;
			mesh.polygonList = template.polygonList;
			mesh.hasVBO = template.hasVBO;
			mesh.useVBO = template.useVBO;
			mesh.VBOids = template.VBOids;
			return mesh;
		}
	}
}
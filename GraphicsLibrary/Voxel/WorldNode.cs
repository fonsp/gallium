using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphicsLibrary.Core;
using System.Threading;
using GraphicsLibrary.Timing;
using System.Diagnostics;

namespace GraphicsLibrary.Voxel
{
	public class WorldNode:Node
	{
		public World world;
		public int hardRadius = 1;
		public int softRadius = 3;
		public float viewRadius = 48f;

		Dictionary<IntVector, Entity> generatedEntities = new Dictionary<IntVector, Entity>();
		Dictionary<IntVector, Thread> constructionThreads = new Dictionary<IntVector, Thread>();
		Dictionary<IntVector, ConstructionThreadParameters> constructionThreadParameters = new Dictionary<IntVector, ConstructionThreadParameters>();
		Dictionary<IntVector, Entity> needUpdate = new Dictionary<IntVector, Entity>();

		public WorldNode(string name) : base(name)
		{
			world = new World();
		}

		int time = 0;

		public override void Update(float timeSinceLastUpdate)
		{
			time++;
			IntVector d = new IntVector((int)Math.Floor(Camera.Instance.position.X / 16), (int)Math.Floor(Camera.Instance.position.Z / 16));

			
			IntVector i = new IntVector(0, 1);

			for(int dist = 0; dist <= softRadius; dist++)
			{
				IntVector dd = d + new IntVector(dist, dist);
				IntVector dir = new IntVector(-1, 0);
				for(int a = 0; a < ((dist == 0) ? 1 : 4); a++)
				{
					for(int b = 0; b < Math.Max(2*dist, 1); b++)
					{
						//Console.WriteLine("NOW AT {0}", dd);
						if((!generatedEntities.ContainsKey(dd)) && (!constructionThreads.ContainsKey(dd)))
						{
							//Console.WriteLine("ConstrThr created at {0} on {1}", dd, time);
							ConstructionThreadParameters para = new ConstructionThreadParameters(dd);
							Thread t = new Thread(() => ASyncChunkGenerate(para));
							constructionThreadParameters.Add(dd, para);
							constructionThreads.Add(dd, t);
							t.Start();
						}
						if(dist <= hardRadius)
						{
							Thread t = null;
							lock(constructionThreads)
							{
								if(constructionThreads.ContainsKey(dd))
								{
									t = constructionThreads[dd];
								}

								if(t != null)
								{
									Console.WriteLine("Hard radius reached at {0}.", dd);
									//t.Start();
									t.Join();
								}
							}
							
							//GenerateEntityIfNeeded(dd);
						}
							
						
						dd += dir;
					}
					dir *= i;
				}
			}

			// TODO: Make this dynamic.
			Thread.Sleep(10);

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			bool donesomething = false;
			lock(constructionThreadParameters)
			{
				List<IntVector> toRemove = new List<IntVector>();
				foreach(KeyValuePair<IntVector, ConstructionThreadParameters> pair in constructionThreadParameters)
				{
					if(!donesomething)
					{ 
						ConstructionThreadParameters para = pair.Value;
						IntVector dd = pair.Key;
						if(para.done)
						{
							donesomething = true;
							if(!constructionThreads.ContainsKey(dd))
							{
								throw new Exception();
							}
							Thread t = constructionThreads[dd];
							t.Abort();
							para.entity.mesh.GenerateVBO();
							Add(para.entity);
							world.AddChunk(para.chunk);
							generatedEntities.Add(dd, para.entity);
							toRemove.Add(dd);
						}
					}
				}
				foreach(IntVector dd in toRemove)
				{
					constructionThreads.Remove(dd);
					constructionThreadParameters.Remove(dd);
				}
			}
			if(donesomething)
			{
				stopwatch.Stop();
				Console.WriteLine("Chunk completion: {0}ms", stopwatch.ElapsedMilliseconds);
			}

			float camx = Camera.Instance.position.X - 8f;
			float camz = Camera.Instance.position.Z - 8f;
			float distSquared = viewRadius * viewRadius;

			foreach(KeyValuePair<string, Node> pair in children)
			{
				Node child = pair.Value;
				child.isVisible = ((camx - child.derivedPosition.X)* (camx - child.derivedPosition.X) + (camz - child.derivedPosition.Z)* (camz - child.derivedPosition.Z) < distSquared);
			}
		}

		/*public void GenerateEntityIfNeeded(IntVector d)
		{
			Chunk c = world.GetChunk(d);
			if(!generatedEntities.ContainsKey(d))
			{
				Entity cent = new Entity("C." + d.x + "." + d.y);
				cent.mesh = c.GenerateMesh();
				cent.mesh.useVBO = true;
				cent.mesh.GenerateVBO();
				cent.position = new OpenTK.Vector3(16 * d.x, 0, 16 * d.y);
				lock(children)
				{
					Add(cent);
				}
				lock(generatedEntities)
				{
					generatedEntities.Add(d, cent);
				}
			}
		}*/

		private static void ASyncChunkGenerate(ConstructionThreadParameters para)
		{
			//Console.WriteLine("Thread STARTED at {0}", para.d);
			para.chunk = World.GenerateTempChunkAt(para.d);
			para.entity = new Entity("C." + para.d.x + "." + para.d.y);
			para.entity.position = new OpenTK.Vector3(16 * para.d.x, 0, 16 * para.d.y);
			para.entity.ignoreDrawDistance = false;
			para.entity.mesh = para.chunk.GenerateMesh();
			para.entity.mesh.useVBO = true;
			para.entity.mesh.material.textureName = "terrain";
			//cent.mesh.GenerateVBO(); ////FIXIXXXXX
			para.done = true;
			//Console.WriteLine("Thread COMPLETED at {0}", para.d);
		}
	}

	public class ConstructionThreadParameters
	{
		public bool done = false;
		public IntVector d;
		public Chunk chunk;
		public Entity entity;

		public ConstructionThreadParameters(IntVector d)
		{
			this.d = d;
		}

		public override string ToString()
		{
			return d + "" + done;
		}
	}
}

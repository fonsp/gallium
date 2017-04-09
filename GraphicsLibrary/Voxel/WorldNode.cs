using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphicsLibrary.Core;
using System.Threading;
using GraphicsLibrary.Timing;
using System.Diagnostics;
using OpenTK;

namespace GraphicsLibrary.Voxel
{
	public class WorldNode:Node
	{
		public World world;
		public int hardRadius = 2;
		public int softRadius = 15;
		public float viewRadius = 48f;

		Dictionary<IntVector, Entity> generatedEntities = new Dictionary<IntVector, Entity>();
		Dictionary<IntVector, Thread> constructionThreads = new Dictionary<IntVector, Thread>();
		Dictionary<IntVector, ConstructionThreadParameters> constructionThreadParameters = new Dictionary<IntVector, ConstructionThreadParameters>();
		List<IntVector> needUpdate = new List<IntVector>();
		List<Thread> threadsToStart = new List<Thread>();

		public WorldNode(string name) : base(name)
		{
			world = new World();
		}

		int numberOfThreadsStarted = 0;
		int currentNumberOfStartedThreads = 0;
		float donesomethingtimer = 0f;
		public const float completionDelay = 0.02f;

		public override void Update(float timeSinceLastUpdate)
		{
			numberOfThreadsStarted = 0;
			IntVector d = new IntVector((int)Math.Floor(Camera.Instance.position.X / 16), (int)Math.Floor(Camera.Instance.position.Z / 16));

			Stopwatch stopwatch = new Stopwatch();
			

			IntVector i = new IntVector(0, 1);
			List<IntVector> hardLimitsToComplete = new List<IntVector>();

			for(int dist = 0; dist <= softRadius; dist++)
			{
				IntVector dd = d + new IntVector(dist, dist);
				IntVector dir = new IntVector(-1, 0);
				for(int a = 0; a < ((dist == 0) ? 1 : 4); a++)
				{
					for(int b = 0; b < Math.Max(2*dist, 1); b++)
					{
						if(needUpdate.Contains(dd))
						{
							ChunkUpdate(dd);
							needUpdate.Remove(dd);
							donesomethingtimer = completionDelay;
						}

						//Console.WriteLine("NOW AT {0}", dd);
						if((!generatedEntities.ContainsKey(dd)) && (!constructionThreads.ContainsKey(dd)))
						{
							
							//Console.WriteLine("ConstrThr created at {0} on {1}", dd, time);
							ConstructionThreadParameters para = new ConstructionThreadParameters(dd);
							Thread t = new Thread(() => ASyncChunkGenerate(para));
							constructionThreadParameters.Add(dd, para);
							constructionThreads.Add(dd, t);


							//t.Start();
							threadsToStart.Add(t);

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

									if(threadsToStart.Contains(t))
									{
										threadsToStart.Remove(t);
										t.Start();
									}

									t.Join();
									hardLimitsToComplete.Add(dd);
								}
							}
							
							//GenerateEntityIfNeeded(dd);
						}
							
						
						dd += dir;
					}
					dir *= i;
				}
			}

			while(numberOfThreadsStarted < 2 && threadsToStart.Count != 0)
			{
				stopwatch.Start();

				threadsToStart[0].Start();
				threadsToStart.RemoveAt(0);
				numberOfThreadsStarted++;
				currentNumberOfStartedThreads++;

				stopwatch.Stop(); if(stopwatch.ElapsedMilliseconds >= 0) { Console.WriteLine("{0}, {1}", constructionThreads.Count, stopwatch.ElapsedMilliseconds); }
			}
			
			
			// TODO: Make this dynamic.
			if(constructionThreads.Count != 0 && timeSinceLastUpdate < .03333f)
			{
				Thread.Sleep(10);
			}

			donesomethingtimer -= timeSinceLastUpdate;
			if(donesomethingtimer <= 0f)
			{
				donesomethingtimer = 0f;
			}

			

			int count = 0;

			lock(constructionThreadParameters)
			{
				List<IntVector> toRemove = new List<IntVector>();
				foreach(KeyValuePair<IntVector, ConstructionThreadParameters> pair in constructionThreadParameters)
				{
					ConstructionThreadParameters para = pair.Value;
					IntVector dd = pair.Key;
					if(hardLimitsToComplete.Contains(dd) || donesomethingtimer <= 0f)
					{
						if(para.done)
						{
							

							if(!constructionThreads.ContainsKey(dd))
							{
								throw new Exception();
							}
							Thread t = constructionThreads[dd];

							donesomethingtimer += completionDelay;
							
							para.entity.mesh.GenerateVBO();
							
							Add(para.entity);
							world.AddChunk(para.chunk);
							generatedEntities.Add(dd, para.entity);
							toRemove.Add(dd);

							count++;

							currentNumberOfStartedThreads--;
						}
					}
				}
				foreach(IntVector dd in toRemove)
				{
					constructionThreads.Remove(dd);
					constructionThreadParameters.Remove(dd);
				}
			}
			if(donesomethingtimer == completionDelay)
			{
				
				//Console.WriteLine("Chunk completion: {0} chunk in {1}ms", count, stopwatch.ElapsedMilliseconds);
			}

			float camx = Camera.Instance.position.X - 8f;
			float camz = Camera.Instance.position.Z - 8f;
			float radiusSquared = viewRadius * viewRadius;

			foreach(KeyValuePair<string, Node> pair in children)
			{
				Node child = pair.Value;
				float chunkDistSquared = (camx - child.derivedPosition.X) * (camx - child.derivedPosition.X) + (camz - child.derivedPosition.Z) * (camz - child.derivedPosition.Z);
				//child.isVisible = (chunkDistSquared < radiusSquared);
				
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

		private void ChunkUpdate(IntVector d)
		{
			// TOOD: ASync?
			Chunk chunk = world.GetTempChunk(d);
			Entity entity = generatedEntities[d];
			chunk.UpdateMesh(entity.mesh);
			//entity.mesh.useVBO = true;
			//entity.mesh.material.textureName = "terrain";
			entity.mesh.UpdateVBO();
		}

		public byte GetBlock(int ix, int iy, int iz)
		{
			return world.GetBlock(ix, iy, iz);
		}

		public byte GetBlock(float x, float y, float z)
		{
			return world.GetBlock(x, y, z);
		}

		public byte GetBlock(Vector3 position)
		{
			return world.GetBlock(position);
		}

		public void SetBlock(int ix, int iy, int iz, byte value)
		{
			IntVector d = world.SetBlock(ix, iy, iz, value);
			if(!needUpdate.Contains(d))
			{
				needUpdate.Add(d);
			}
		}

		public void SetBlock(float x, float y, float z, byte value)
		{
			SetBlock((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z), value);
		}

		public void SetBlock(Vector3 position, byte value)
		{
			SetBlock(position.X, position.Y, position.Z, value);
		}

		public void FinishAllThreadwork()
		{
			foreach(KeyValuePair<IntVector, Thread> pair in constructionThreads)
			{
				pair.Value.Join();
			}
			while(constructionThreads.Count != 0)
			{
				Update(.1f);
			}
		}

		public void UnloadChunk(IntVector d)
		{
			if(constructionThreads.ContainsKey(d))
			{
				constructionThreads[d].Abort();
				constructionThreads.Remove(d);
				constructionThreadParameters.Remove(d);
			}
			if(needUpdate.Contains(d))
			{
				needUpdate.Remove(d);
			}


			Entity entity = generatedEntities[d];
			entity.mesh.RemoveVBO();
			RemoveChild(entity.name);
			entity = null;
			generatedEntities.Remove(d);

			Chunk chunk = world.GetChunk(d);
			ChunkLoader.SaveChunk(chunk);
			world.RemoveChunk(d);
			chunk = null;



			GC.Collect();
		}

		public void UnloadAll()
		{
			FinishAllThreadwork();
			while(generatedEntities.Count != 0)
			{
				UnloadChunk(generatedEntities.First().Key);
			}
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
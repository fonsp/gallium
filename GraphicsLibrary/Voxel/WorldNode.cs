﻿using System;
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
        public int hardRadius = 3;
		public int softRadius = 10;
        public int unloadBorder = 2;
		public float viewRadius = 48f;

		Dictionary<IntVector, Entity> generatedEntities = new Dictionary<IntVector, Entity>();
		Dictionary<IntVector, Task> constructionTasks = new Dictionary<IntVector, Task>();
		Dictionary<IntVector, ConstructionTaskParameters> constructionTaskParameters = new Dictionary<IntVector, ConstructionTaskParameters>();
		List<IntVector> needUpdate = new List<IntVector>();
		List<Task> tasksToStart = new List<Task>();

        private Thread chunkSavingThread;

		public WorldNode(string name) : base(name)
		{
			world = new World();
		}

		int numberOfTasksStarted = 0;
		int currentNumberOfStartedTasks = 0;
		float donesomethingtimer = 0f;

		// TODO: This could probably be dynamic.
		public const float completionDelay = 0.02f;

		public override void Update(float timeSinceLastUpdate)
		{
			numberOfTasksStarted = 0;
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
						if((!generatedEntities.ContainsKey(dd)) && (!constructionTasks.ContainsKey(dd)))
						{
							
							//Console.WriteLine("ConstrThr created at {0} on {1}", dd, time);
							ConstructionTaskParameters para = new ConstructionTaskParameters(dd);
							Task t = new Task(() => ASyncChunkGenerate(para));
                            //t.Name = "CT" + dd;
							constructionTaskParameters.Add(dd, para);
							constructionTasks.Add(dd, t);


							//t.Start();
							tasksToStart.Add(t);

						}
						if(dist <= hardRadius)
						{
							Task t = null;
							lock(constructionTasks)
							{
								if(constructionTasks.ContainsKey(dd))
								{
									t = constructionTasks[dd];
								}

								if(t != null)
								{
									Console.WriteLine("Hard radius reached at {0}.", dd);
									//t.Start();

									if(tasksToStart.Contains(t))
									{
										tasksToStart.Remove(t);
										t.Start();
									}

									t.Wait();
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


			// TODO: 1? Should be more. Some task starts cost ~20ms for some reason, while others cost < 2ms.
			while(numberOfTasksStarted < 1 && tasksToStart.Count != 0)
			{
				stopwatch.Start();

				tasksToStart[0].Start();
				numberOfTasksStarted++;
				currentNumberOfStartedTasks++;

				stopwatch.Stop();
				Console.WriteLine(tasksToStart[0].Status.ToString());
				tasksToStart.RemoveAt(0);

				if(stopwatch.ElapsedMilliseconds >= 0) { Console.WriteLine("{0}, {1}", constructionTasks.Count, stopwatch.ElapsedMilliseconds); }
			}
			
			
			// TODO: If running on a single thread this might be necessary.
			if(constructionTasks.Count != 0 && timeSinceLastUpdate < .03333f)
			{
				//Thread.Sleep(10);
			}

			donesomethingtimer -= timeSinceLastUpdate;
			if(donesomethingtimer <= 0f)
			{
				donesomethingtimer = 0f;
			}

			

			int count = 0;

			lock(constructionTaskParameters)
			{
				List<IntVector> toRemove = new List<IntVector>();
				foreach(KeyValuePair<IntVector, ConstructionTaskParameters> pair in constructionTaskParameters)
				{
					ConstructionTaskParameters para = pair.Value;
					IntVector dd = pair.Key;
					if(hardLimitsToComplete.Contains(dd) || donesomethingtimer <= 0f)
					{
						if(para.done)
						{
							

							if(!constructionTasks.ContainsKey(dd))
							{
								throw new Exception();
							}
							Task t = constructionTasks[dd];

							donesomethingtimer += completionDelay;
							
							para.entity.mesh.GenerateVBO();

                            if (hardLimitsToComplete.Contains(dd))
                            {
                                para.entity.materialAge = para.entity.materialLifetime;
                            }

							Add(para.entity);
                            
							world.AddChunk(para.chunk);
							generatedEntities.Add(dd, para.entity);
							toRemove.Add(dd);

							count++;

							currentNumberOfStartedTasks--;
						}
					}
				}
				foreach(IntVector dd in toRemove)
				{
					constructionTasks.Remove(dd);
					constructionTaskParameters.Remove(dd);
				}
			}
			if(donesomethingtimer == completionDelay)
			{
				
				//Console.WriteLine("Chunk completion: {0} chunk in {1}ms", count, stopwatch.ElapsedMilliseconds);
			}

			float camx = Camera.Instance.position.X - 8f;
			float camz = Camera.Instance.position.Z - 8f;
			float radiusSquared = viewRadius * viewRadius;

			Vector3 dirUp, dirRight, dirFront;
			Camera.Instance.GetDirections(out dirUp, out dirRight, out dirFront);




            List<IntVector> toUnload = new List<IntVector>();
			foreach(KeyValuePair<IntVector, Entity> pair in generatedEntities)
			{
				if(Math.Abs(pair.Key.x - d.x) >= softRadius + unloadBorder || Math.Abs(pair.Key.y - d.y) >= softRadius + unloadBorder)
                {
                    toUnload.Add(pair.Key);
                }
				
                //float chunkDistSquared = (camx - child.derivedPosition.X) * (camx - child.derivedPosition.X) + (camz - child.derivedPosition.Z) * (camz - child.derivedPosition.Z);
				//child.isVisible = (chunkDistSquared < radiusSquared);
				
			}

            toUnload.ForEach(x => UnloadChunk(x));

            Hud.HudDebug.taskNum = constructionTasks.Count();
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

		private static void ASyncChunkGenerate(ConstructionTaskParameters para)
		{
            //Console.WriteLine("Task STARTED at {0}", para.d);

            //TODO: CHECK IF CHUNK IS NOT IN TOSAVE LIST

            para.chunk = World.GenerateTempChunkAt(para.d);
			para.entity = new Entity("C." + para.d.x + "." + para.d.y);
			para.entity.position = new Vector3(16 * para.d.x, 0, 16 * para.d.y);
			para.entity.ignoreDrawDistance = false;
			para.entity.occlude = true;
			para.entity.occlusionOffset = new Vector3(8f, 64f, 8f);
            para.entity.occlusionRadius = 65f;

			para.entity.mesh = para.chunk.GenerateMesh();

			para.entity.mesh.useVBO = true;
			para.entity.mesh.material.textureName = "terrain";
            para.entity.mesh.material.AddTransitionColor(new OpenTK.Graphics.Color4(1f, 1f, 1f, 0f), 0f);
            para.entity.materialLifetime = .2f;
            //para.entity.mesh.material.AddTransitionColor(new OpenTK.Graphics.Color4(1f, 1f, 1f, 1f), 1f);
            para.entity.mesh.material.enableColorTransitions = true;
            //cent.mesh.GenerateVBO(); ////TODO: FIXIXXXXX
            para.done = true;
			//Console.WriteLine("Task COMPLETED at {0}", para.d);
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

		public void FinishAllTaskwork()
		{
			foreach(KeyValuePair<IntVector, Task> pair in constructionTasks)
			{
				pair.Value.Wait();
			}
			while(constructionTasks.Count != 0)
			{
				Update(.1f);
			}
		}

		public void UnloadChunk(IntVector d)
		{
			if(constructionTasks.ContainsKey(d))
			{
				// TODO: Should cancel instead of await: task should be cancelable.
				constructionTasks[d].Wait();
				constructionTasks.Remove(d);
				constructionTaskParameters.Remove(d);
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
			//ChunkLoader.SaveChunk(chunk);
			//ChunkLoader.SaveChunkASync(chunk);
			//Console.WriteLine("Chunk save requested " + d);

			// TODO: Nullify?
			//chunk = null;



			GC.Collect();
		}

		public void UnloadAll()
		{
			FinishAllTaskwork();
			while(generatedEntities.Count != 0)
			{
				UnloadChunk(generatedEntities.First().Key);
			}
		}
	}

	public class ConstructionTaskParameters
	{
		public bool done = false;
		public IntVector d;
		public Chunk chunk;
		public Entity entity;

		public ConstructionTaskParameters(IntVector d)
		{
			this.d = d;
		}

		public override string ToString()
		{
			return d + "" + done;
		}
	}
}
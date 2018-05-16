using System;
using System.Diagnostics;
using System.Management;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using GraphicsLibrary.Core;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace GraphicsLibrary.Hud
{
	public class HudDebug:HudElement
	{
		public Dictionary<string, HudDebugField> fields = new Dictionary<string, HudDebugField>();

		public HudGraph fpsGraph = new HudGraph("fpsGraph");
		public HudGraph xGraph = new HudGraph("xGraph");
		public HudGraph yGraph = new HudGraph("yGraph");
		public HudGraph zGraph = new HudGraph("zGraph");

        private HudElement cpuGraphContainer = new HudElement("cpuGraphContainer");
        private List<HudGraph> cpuGraphs = new List<HudGraph>();

        private List<PerformanceCounter> cpuCounters = new List<PerformanceCounter>();
        private int cores = 0;
        private int processors = 0;

        public readonly int memory;

		public HudDebug(string name)
			: base(name)
		{
			NewField("fps", 0, AlignMode.Left, "", " fps");
			NewField("position", 1, AlignMode.Left, "Camera: (", ")");

			NewField("version", 3, AlignMode.Left, "Gallium v" + Assembly.GetExecutingAssembly().GetName().Version + "; github.com/fons-", "");
			NewField("cpuVendor", 4, AlignMode.Left, "CPU: ", "");
			foreach(ManagementObject service in new ManagementObjectSearcher("select * from Win32_Processor").Get())
			{
				fields["cpuVendor"].value = service["Name"].ToString();
			}
			NewField("ram", 5, AlignMode.Left, "RAM: ", " MB");


			NewField("gpuVendor", 6, AlignMode.Left, "GPU: ", "");
			fields["gpuVendor"].value = GL.GetString(StringName.Vendor) + "; " + GL.GetString(StringName.Renderer) + "; " + GL.GetString(StringName.Version);
			NewField("display", 7, AlignMode.Left, "Display: ", "");
			NewField("window", 8, AlignMode.Left, "Window: ", "");
			NewField("vsync", 9, AlignMode.Left, "VSync: ", "");

            fpsGraph.position.Y = 11 * 14;
			Add(fpsGraph);
			xGraph.position.Y = 12 * 14 + 128;
			xGraph.color = Color4.Red;
			Add(xGraph);
			yGraph.position.Y = 12 * 14 + 128;
			yGraph.backgroundColor = Color4.Transparent;
			yGraph.color = Color4.Green;
			Add(yGraph);
			zGraph.position.Y = 12 * 14 + 128;
			zGraph.backgroundColor = Color4.Transparent;
			zGraph.color = Color4.Blue;
			Add(zGraph);

            NewField("tw", 0, AlignMode.Right, "World time: ", " s");
            NewField("occludecount", 2, AlignMode.Right, "Occluded: ", "");
            NewField("tasknum", 3, AlignMode.Right, "Working tasks: ", "");

            cpuGraphContainer.position.Y = 5 * 14;
            Add(cpuGraphContainer);

            foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                cores += int.Parse(item["NumberOfCores"].ToString());
            }
            processors = Environment.ProcessorCount;
            /*foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_LogicalMemoryConfiguration").Get())
            {
                memory += int.Parse(item["TotalPageFileSpace"].ToString());
            }*/

            for(int i = 0; i < processors; i++)
            {
                cpuCounters.Add(new PerformanceCounter("Processor", "% Processor Time", i.ToString()));
                HudGraph graph = new HudGraph("CPUgraph" + i);
                graph.height = 64;
                graph.position.Y = 80 * i;
                cpuGraphs.Add(graph);
                cpuGraphContainer.Add(graph);
            }



            Console.WriteLine(cores + " cores");
            Console.WriteLine(processors+ " processors");
            Console.WriteLine(memory);
		}

		/// <summary>
		/// Adds a new field.
		/// </summary>
		/// <param name="fieldName">Field identifier</param>
		/// <param name="lineOffset">Number of lines below the top edge of the screen</param>
		/// <param name="align">Left/Right align</param>
		/// <param name="prefix">Displayed text will be: [prefix][value][suffix]</param>
		/// <param name="suffix">Displayed text will be: [prefix][value][suffix]</param>
		private void NewField(string fieldName, int lineOffset, AlignMode align, string prefix, string suffix)
		{
			if(fields.ContainsKey(fieldName))
			{
				throw new ArgumentException("Name already exists");
			}
			fields.Add(fieldName, new HudDebugField(fieldName, lineOffset, align));
			fields[fieldName].prefix = prefix;
			fields[fieldName].suffix = suffix;
			Add(fields[fieldName]);
		}

		/// <summary>
		/// Set the value of the specified field.
		/// </summary>
		/// <param name="fieldName">Name of the field</param>
		/// <param name="value">New value</param>
		public void SetValue(string fieldName, string value)
		{
			fields[fieldName].value = value;
		}

		/// <summary>
		/// Gets the value of the specified field.
		/// </summary>
		/// <param name="fieldName">Name of the field</param>
		/// <returns>The value of the specified field</returns>
		public string GetValue(string fieldName)
		{
			return fields[fieldName].value;
		}

		public int fps = 0;
		private int fCount = 0;
		private float fTime = 0f;

        public static int occlusionStatOccluded, occlusionStatTotal, taskNum;
        private PerformanceCounter pCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

		public override void Update(float timeSinceLastUpdate)
		{
			base.Update(timeSinceLastUpdate);
            cpuGraphContainer.position.X = RenderWindow.Instance.Width - 256;

			fCount++;
			if(fTime >= 0.5f)
			{
				fTime -= 0.5f;
				fps = fCount * 2;
				fpsGraph.value = (byte)((fps * 256) / 120);
				fCount = 0;

                for (int i = 0; i < processors; i++)
                {
                    cpuGraphs[i].value = (byte)(cpuCounters[i].NextValue() * 256 / 100);
                }

                //Console.WriteLine((from pc in cpuCounters select pc.NextValue()).Average());
            }
			fTime += timeSinceLastUpdate;

			fpsGraph.value = (timeSinceLastUpdate == 0) ? (byte)255 : (byte)Math.Min(255, (1/timeSinceLastUpdate) * 256 / 120);

			fields["fps"].value = fps.ToString("D");
			fields["position"].value = Camera.Instance.derivedPosition.X.ToString("F1") + ", " +
									   Camera.Instance.derivedPosition.Y.ToString("F1") + ", " +
									   Camera.Instance.derivedPosition.Z.ToString("F1");

			fields["ram"].value = ((float)Process.GetCurrentProcess().PrivateMemorySize64 / 1024f / 1024f).ToString("F2");
			fields["display"].value = RenderWindow.Instance.Width.ToString("D") + "x" + RenderWindow.Instance.Height.ToString("D");
			fields["window"].value = RenderWindow.Instance.WindowState + ", " + RenderWindow.Instance.WindowBorder;
			fields["vsync"].value = RenderWindow.Instance.VSync.ToString();
			
			fields["tw"].value = RenderWindow.Instance.worldTime.ToString("F2");

            fields["occludecount"].value = occlusionStatOccluded + "/" + occlusionStatTotal + " (" + 100*occlusionStatOccluded/Math.Max(1,occlusionStatTotal) + "%)";
            occlusionStatOccluded = occlusionStatTotal = 0;
            fields["tasknum"].value = taskNum.ToString();

			xGraph.value = (byte)((256 * Camera.Instance.position.X) / 16);
			yGraph.value = (byte)((256 * Camera.Instance.position.Y) / 16);
			zGraph.value = (byte)((256 * Camera.Instance.position.Z) / 16);

            
		}
	}
}
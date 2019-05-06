//#define FULLSCREEN

using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using GraphicsLibrary.Input;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using GraphicsLibrary.Timing;
using GraphicsLibrary.Content;
using GraphicsLibrary.Core;
using GraphicsLibrary.Hud;

namespace GraphicsLibrary
{
	public class RenderWindow : GameWindow
	{
		#region SingleTon
		public GraphicsProgram program;

		private static RenderWindow instance;
		/// <summary>
		/// The main render window.
		/// </summary>
		public static RenderWindow Instance
		{
			get { return instance ?? (instance = new RenderWindow()); }
		}
		#endregion
		/// <summary>
		/// Exit the application when the 'Esc' key is pressed.
		/// </summary>
		public bool escapeOnEscape = true;
        private readonly GameTimer updateSw = new GameTimer();
		public HudConsole hudConsole = new HudConsole("mainHudConsole", 8);
		/// <summary>
		/// World time
		/// </summary>
		public float worldTime = 0f;
		/// <summary>
		/// Observer time
		/// </summary>
		public float localTime = 0f;
		/// <summary>
		/// The speed of light.
		/// </summary>
		public float c = 29979245800f;
		/// <summary>
		/// Camera velocity, relative to the world
		/// </summary>
		public float v = 0f;
		/// <summary>
		/// v/c
		/// </summary>
		public float b = 0f;
		/// <summary>
		/// The Lorentz factor
		/// </summary>
		public float lf = 1f;

		public bool enableRelativity = false;
		/// <summary>
		/// Enable the Doppler effect
		/// </summary>
		public bool enableDoppler = false;
		/// <summary>
		/// Enable relativistic brightness
		/// </summary>
		public bool enableRelBrightness = false;
		/// <summary>
		/// Enable relativistic aberration
		/// </summary>
		public bool enableRelAberration = false;
		/// <summary>
		/// Smoothed velocity of the camera
		/// </summary>
		public Vector3 smoothedVelocity = Vector3.Zero;
		public float smoothFactor = 4000f;
		protected double timeSinceLastUpdate = 0;
		/// <summary>
		/// Time multiplier (not related to physics)
		/// </summary>
		public double timeMultiplier = 1.0;
		public int amountOfRenderPasses = 2;
		public float drawDistance = 300f;

		public float fogStart = 40f;
		public float fogEnd = 80f;
		public Color4 fogColor = Color4.White;
        public Vector3 lightDir = new Vector3(2.2354324f, 2, 1.01243f);

		/// <summary>
		/// Default geometry shader
		/// </summary>
		public Shader defaultShader = Shader.diffuseShader;
		public uint[] elementBase = new uint[1000000];
        public uint fboHandle, colorTexture, depthTexture, depthRenderbuffer, ditherTexture, shadowTexture, shadowBuffer;
        public uint fboHandle2, colorTexture2, depthTexture2, depthRenderbuffer2, ditherTexture2, shadowTexture2, shadowBuffer2;
        public int shadowTextureSize = 2048;
        public float shadowWidth = 300f, shadowHeight = 100f;
        public bool drawShadows = true, enableAmbientOcclusion = true, enableTextures = true;
        public Color4 lightColor = Color4.FromHsv(new Vector4(48f/360f, .47f, 1f, 1f)), darkColor  = Color4.FromHsv(new Vector4(192f/360f, .47f, 1f, 1f));
        public float lightStrength = 1.9f, darkStrength = .2f;

        public bool initialized = false;
		private float focalDistance;

		public RenderWindow(string windowName, int width, int height)
			: base(width, height, GraphicsMode.Default, windowName
#if FULLSCREEN
			, GameWindowFlags.Fullscreen
#endif
)
		{
			VSync = VSyncMode.Off;

		}
		public RenderWindow(string windowName)
			: this(windowName, 880, 600)
		{ }

		public RenderWindow()
			: this("Default render window")
		{ }

		protected override void OnLoad(EventArgs e)
		{
			#region General
			Debug.WriteLine("Initializing OpenGL..");

			WindowBorder = WindowBorder.Resizable;
			try
			{
				GL.ClearColor(Color.Black);
				GL.ShadeModel(ShadingModel.Smooth);
				GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
				//GL.Enable(EnableCap.ColorMaterial);
				GL.Enable(EnableCap.DepthTest);
				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
				GL.Enable(EnableCap.CullFace);
				GL.CullFace(CullFaceMode.Back);

				for(uint i = 0; i < elementBase.Length; i++)
				{
					elementBase[i] = i;
				}
			}
			catch(Exception exception)
			{
				Debug.WriteLine("WARNING: OpenGL could not be initialized: " + exception.Message + " @ " + exception.Source, Color4.Red);
				Exit();
			}

			#endregion
			#region Texture loading
			Debug.WriteLine("Initializing textures..");

			try
			{
				TextureManager.AddTexture("default", @"Content/textures/defaultTexture.png", TextureMinFilter.Linear, TextureMagFilter.Nearest);
				TextureManager.AddTexture("terrain", @"Content/textures/terrain.png", TextureMinFilter.Linear, TextureMagFilter.Nearest);


				GL.BindTexture(TextureTarget.ProxyTexture2D, TextureManager.GetTexture("default"));

				GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, Color4.Black);
				GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, new float[] { 50.0f });
			}
			catch(Exception exception)
			{
				Debug.WriteLine("WARNING: default textures could not be initialized: " + exception.Message + " @ " + exception.Source, Color4.Red);
				Exit();
			}

			#endregion
			#region Lighting init
			Debug.WriteLine("Initializing lighting..");

			try
			{
				GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { darkStrength * darkColor.R, darkStrength * darkColor.G, darkStrength * darkColor.B, 1.0f });
				GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { lightStrength * lightColor.R, lightStrength * lightColor.G, lightStrength * lightColor.B, 1.0f });
                //GL.Light(LightName.Light0, LightParameter.Position, Vector4.Normalize(new Vector4(.2f, .9f, .5f, 0.0f)));
                GL.Light(LightName.Light0, LightParameter.Position, Vector4.Normalize(new Vector4(2f, 2f, 1f, 0.0f)));

                GL.Enable(EnableCap.Lighting);
				GL.Enable(EnableCap.Light0);
			}
			catch(Exception exception)
			{
				Debug.WriteLine("WARNING: lighting could not be initialized: " + exception.Message + " @ " + exception.Source, Color4.Red);
				Exit();
			}

			#endregion
			#region Default shaders init
			Debug.WriteLine("Initializing default shaders..");
			//TODO: useless?

			try
			{
				Shader asdf = Shader.diffuseShaderCompiled;
				asdf = Shader.unlitShaderCompiled;
				asdf = null;
			}
			catch(Exception exception)
			{
				Debug.WriteLine("WARNING: default shader could not be initialized: " + exception.Message + " @ " + exception.Source, Color4.Red);
				Exit();
			}

			#endregion
			#region FBO init
			Debug.WriteLine("Initializing FBO..");

			//Debug.WriteLine("FBO size: {" + Width + ", " + Height + "}");
			try
			{
				// Create Color Texture
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.GenTextures(1, out colorTexture);
				GL.BindTexture(TextureTarget.Texture2D, colorTexture);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
				GL.BindTexture(TextureTarget.Texture2D, 0);

                /*GL.ActiveTexture(TextureUnit.Texture5);
                GL.GenTextures(1, out colorTexture2);
                GL.BindTexture(TextureTarget.Texture2D, colorTexture2);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, shadowSize, shadowSize, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                GL.BindTexture(TextureTarget.Texture2D, 0);*/

                // Create depth Texture
                GL.ActiveTexture(TextureUnit.Texture1);
				GL.GenTextures(1, out depthTexture);
				GL.BindTexture(TextureTarget.Texture2D, depthTexture);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.None);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
				GL.BindTexture(TextureTarget.Texture2D, 0);

                GL.ActiveTexture(TextureUnit.Texture6);
                GL.GenTextures(1, out depthTexture2);
                GL.BindTexture(TextureTarget.Texture2D, depthTexture2);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.None);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, shadowTextureSize, shadowTextureSize, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                //GL.BindTexture(TextureTarget.Texture2D, 0);

                GL.ActiveTexture(TextureUnit.Texture0);

                //TODO: test for GL Error here (might be unsupported format)

                

				// Create an FBO and attach the textures
				GL.Ext.GenFramebuffers(1, out fboHandle);
				GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fboHandle);
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, colorTexture, 0);
				GL.ActiveTexture(TextureUnit.Texture1);
				GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTexture, 0);
				GL.ActiveTexture(TextureUnit.Texture0);

                GL.Ext.GenFramebuffers(1, out fboHandle2);
                GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fboHandle2);
                GL.ActiveTexture(TextureUnit.Texture5);
                //GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, colorTexture2, 0);
                GL.ActiveTexture(TextureUnit.Texture6);
                GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTexture2, 0);
                GL.ActiveTexture(TextureUnit.Texture5);

                // Dither texture sample
                GL.ActiveTexture(TextureUnit.Texture1);
				GL.GenTextures(1, out ditherTexture);
				GL.BindTexture(TextureTarget.Texture2D, ditherTexture);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
				byte[] imageData = new byte[] { 208, 112, 240, 80, 48, 176, 16, 144, 224, 64, 192, 96, 0, 128, 32, 160 };
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, 4, 4, 0, PixelFormat.Red, PixelType.UnsignedByte, imageData);

				/*GL.ActiveTexture(TextureUnit.Texture5);
                
                GL.Ext.GenFramebuffers(1, out shadowBuffer);
                GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, shadowBuffer);
                
                GL.GenTextures(1, out shadowTexture);
                GL.BindTexture(TextureTarget.Texture2D, shadowTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, 1024, shadowSize, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, shadowTexture, 0);
                GL.DrawBuffer(DrawBufferMode.None);
                GL.ReadBuffer(ReadBufferMode.None);*/

                GL.ActiveTexture(TextureUnit.Texture0);

                if (!CheckFboStatus())
				{
					Debug.WriteLine("WARNING: FBO initialization failure", Color4.Red);
					Exit();
				}
			}
			catch(Exception exception)
			{
				Debug.WriteLine("WARNING: FBO could not be initialized: " + exception.Message + " @ " + exception.Source, Color4.Red);
				Exit();
			}

			#endregion
			#region Camera init
			Debug.WriteLine("Initializing camera..");

			Camera.Instance.Fov = 120f;
			Camera.Instance.width = Width;
			Camera.Instance.height = Height;
			OnResize(new EventArgs());

			#endregion
			#region Custom resources
			Debug.WriteLine("Loading custom resources..");

			/*try
			{*/
			program.LoadResources();
			/*}
			catch (Exception exception)
			{
				Debug.WriteLine("WARNING: custom resources could not be loaded: " + exception.Message + " @ " + exception.Source, Color4.Red);
			}*/

			Debug.WriteLine("{0} textures were loaded", TextureManager.numberOfTextures);

			#endregion
			#region Hud Console

			HudBase.Instance.Add(hudConsole);
			hudConsole.enabled = false;
			hudConsole.isVisible = false;
			hudConsole.FontTextureName = "font2";
			hudConsole.NumberOfLines = 30;
			hudConsole.DebugInput += ConsoleInputReceived;
			KeyPress += hudConsole.HandleKeyPress;
			KeyPress += HandleKeyPress;
			KeyDown += hudConsole.HandleKeyDown;

			#endregion
			#region Game init

			Debug.WriteLine("Initializing game..");

			program.InitGame();
			updateSw.GetElapsedTimeInSeconds();

			#endregion

			initialized = true;

			Debug.WriteLine("Loading complete");
		}

		protected override void OnUnload(EventArgs e)
		{
            initialized = false;
			Debug.WriteLine("Unloading textures..");

			try
			{
				TextureManager.ClearTextureCache();
			}
			catch(Exception exception)
			{
				Debug.WriteLine("WARNING: Failed to unload textures: " + exception.Message + " @ " + exception.Source, Color4.Red);
				throw;
			}

			Debug.WriteLine("Unloading resources complete");
		}

		protected override void OnResize(EventArgs e)
		{
			GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientSize.Width, ClientSize.Height);

			Camera.Instance.width = Width;
			Camera.Instance.height = Height;

			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, colorTexture);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
			GL.BindTexture(TextureTarget.Texture2D, 0);

			GL.ActiveTexture(TextureUnit.Texture1);
			GL.BindTexture(TextureTarget.Texture2D, depthTexture);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
			GL.BindTexture(TextureTarget.Texture2D, 0);

			GL.ActiveTexture(TextureUnit.Texture0);

			UpdateViewport();

			hudConsole.position = new Vector2(0, ClientRectangle.Height - hudConsole.height);

			program.Resize(ClientRectangle);
			Debug.WriteLine("Window and FBO resized to {0}", ClientRectangle);
		}

		public void UpdateViewport()
		{
			Matrix4 proj = Camera.Instance.projection;
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix(ref proj);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //lightDir.X = (float)Math.Sin(worldTime / 10);
            //lightDir.Z = (float)Math.Cos(worldTime / 10);

            InputManager.enabled = !hudConsole.enabled;
			InputManager.UpdateToggleStates();

			enableRelativity = InputManager.IsKeyDown(Key.Q);
			if(InputManager.IsKeyDown(Key.Escape) && escapeOnEscape/* && InputManager.IsKeyDown(Key.Pause)*/)
			{
				Exit();
			}
#if DEBUG
			if(InputManager.IsKeyDown(Key.Pause) && InputManager.IsKeyUp(Key.Escape))
			{
				System.Diagnostics.Debugger.Break();
			}
#endif
			if(enableRelativity)
			{
				b = Camera.Instance.velocity.Length / c;
				lf = 1f / (float)Math.Sqrt(1.001 - b * b);

				timeSinceLastUpdate = e.Time * timeMultiplier;
				localTime += (float)timeSinceLastUpdate;
				program.Update((float)timeSinceLastUpdate);
				timeSinceLastUpdate *= lf;
				worldTime += (float)timeSinceLastUpdate;
			}
			else
			{
				b = 0f;
				lf = 1f;

				timeSinceLastUpdate = e.Time * timeMultiplier;
				localTime += (float)timeSinceLastUpdate;
				program.Update((float)timeSinceLastUpdate);
				worldTime += (float)timeSinceLastUpdate;
			}

			RootNode.Instance.UpdateNode((float)timeSinceLastUpdate);
			if(Camera.Instance.parent == null)
			{
				Camera.Instance.UpdateNode((float)timeSinceLastUpdate);
			}
			HudBase.Instance.Update((float)(e.Time * timeMultiplier));
			Node.directionsComputed = false;
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
            Vector3 normalizedLightDir = lightDir.Normalized();
            GL.Light(LightName.Light0, LightParameter.Position, new Vector4(normalizedLightDir.X, normalizedLightDir.Y, normalizedLightDir.Z, 0.0f) / 2f);

            #region Shader updates

            float renderTime = (float)e.Time; //TODO e.Time accuracy

			Shader.diffuseShaderCompiled.SetUniform("worldTime", worldTime);
			Shader.unlitShaderCompiled.SetUniform("worldTime", worldTime);
			Shader.particleShaderCompiled.SetUniform("worldTime", worldTime);
			Shader.wireframeShaderCompiled.SetUniform("worldTime", worldTime);
			Shader.collisionShaderCompiled.SetUniform("worldTime", worldTime);
			Shader.hudShaderCompiled.SetUniform("worldTime", worldTime);
			Shader.fboShaderCompiled.SetUniform("worldTime", worldTime);
			Shader.blurShaderCompiled.SetUniform("worldTime", worldTime);
			Shader.crtShaderCompiled.SetUniform("worldTime", worldTime);
			Shader.ditherShaderCompiled.SetUniform("worldTime", worldTime);

			if(enableRelativity)
			{
				Shader.diffuseShaderCompiled.SetUniform("effects", enableDoppler, enableRelBrightness, enableRelAberration);
				Shader.unlitShaderCompiled.SetUniform("effects", enableDoppler, enableRelBrightness, enableRelAberration);
				Shader.particleShaderCompiled.SetUniform("effects", enableDoppler, enableRelBrightness, enableRelAberration);
				Shader.wireframeShaderCompiled.SetUniform("effects", enableDoppler, enableRelBrightness, enableRelAberration);
				Shader.collisionShaderCompiled.SetUniform("effects", enableDoppler, enableRelBrightness, enableRelAberration);
			}
			else
			{
				Shader.diffuseShaderCompiled.SetUniform("effects", false, false, false);
				Shader.unlitShaderCompiled.SetUniform("effects", false, false, false);
				Shader.particleShaderCompiled.SetUniform("effects", false, false, false);
				Shader.wireframeShaderCompiled.SetUniform("effects", false, false, false);
				Shader.collisionShaderCompiled.SetUniform("effects", false, false, false);
			}

			Vector3 velocityDelta = Camera.Instance.velocity - smoothedVelocity;
			smoothedVelocity += velocityDelta - Vector3.Divide(velocityDelta, (float)Math.Pow(smoothFactor, renderTime)); //TODO: time dilation

			if(enableRelativity)
			{
				v = smoothedVelocity.Length;
				b = v / c;
				lf = 1f / (float)Math.Sqrt(1.0 - b * b);
			}
			else
			{
				v = 0f;
				b = 0f;
				lf = 1f;
			}


			Shader.diffuseShaderCompiled.SetUniform("bW", b);
			Shader.unlitShaderCompiled.SetUniform("bW", b);
			//Shader.particleShaderCompiled.SetUniform("bW", b);
			//Shader.wireframeShaderCompiled.SetUniform("bW", b);
			//Shader.collisionShaderCompiled.SetUniform("bW", b);

			Vector3 vDir = smoothedVelocity.Normalized();

			Shader.diffuseShaderCompiled.SetUniform("vdirW", vDir);
			Shader.unlitShaderCompiled.SetUniform("vdirW", vDir);
			//Shader.particleShaderCompiled.SetUniform("vdirW", vDir);
			//Shader.wireframeShaderCompiled.SetUniform("vdirW", vDir);
			//Shader.collisionShaderCompiled.SetUniform("vdirW", vDir);

			Shader.diffuseShaderCompiled.SetUniform("cpos", Camera.Instance.position);
			Shader.unlitShaderCompiled.SetUniform("cpos", Camera.Instance.position);
			//Shader.particleShaderCompiled.SetUniform("cpos", Camera.Instance.position);
			//Shader.wireframeShaderCompiled.SetUniform("cpos", Camera.Instance.position);
			//Shader.collisionShaderCompiled.SetUniform("cpos", Camera.Instance.position);

			Matrix4 cRot = Matrix4.CreateFromQuaternion(Camera.Instance.derivedOrientation);

			Shader.diffuseShaderCompiled.SetUniform("crot", cRot);
			Shader.unlitShaderCompiled.SetUniform("crot", cRot);
			//Shader.particleShaderCompiled.SetUniform("crot", cRot);
			//Shader.wireframeShaderCompiled.SetUniform("crot", cRot);
			//Shader.collisionShaderCompiled.SetUniform("crot", cRot);

			Shader.diffuseShaderCompiled.SetUniform("fogStart", fogStart);
			Shader.diffuseShaderCompiled.SetUniform("fogEnd", fogEnd);
			Shader.diffuseShaderCompiled.SetUniform("fogColor", fogColor);
            Shader.diffuseShaderCompiled.SetUniform("enableTextures", enableTextures);


            #endregion
            #region 3D
            #region Shadow pass

            Shader.shadowShaderCompiled.SetUniform("tex", 0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureManager.GetTexture("terrain"));
            GL.ActiveTexture(TextureUnit.Texture6);
            GL.BindTexture(TextureTarget.Texture2D, depthTexture2);
            GL.ActiveTexture(TextureUnit.Texture0);

            Shader.diffuseShaderCompiled.SetUniform("shadowTex", 6);


            //GL.ActiveTexture(TextureUnit.Texture5);
            //GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientSize.Width, ClientSize.Height);

            //Matrix4 proj = Matrix4.CreateOrthographic(200f, 200f, -300f, 300f) * Matrix4.LookAt(new Vector3(0, 4, 1), Vector3.Zero, Vector3.UnitY);
            Matrix4 maa = new Matrix4(
                new Vector4(1f, 0f, 0f, 0f),
                new Vector4(-normalizedLightDir.X / normalizedLightDir.Y, 1f, -normalizedLightDir.Z / normalizedLightDir.Y, 0f),
                new Vector4(0f, 0f, 1f, 0f),
                new Vector4(0f, 0f, 0f, 1f));
            Matrix4 proj = Matrix4.CreateTranslation(-Camera.Instance.derivedPosition) * maa * Matrix4.LookAt(Vector3.UnitY, Vector3.Zero, Vector3.UnitZ) *  Matrix4.CreateOrthographic(shadowWidth, shadowWidth, -shadowHeight, shadowHeight);
            Shader.diffuseShaderCompiled.SetUniform("shadowProj", proj);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref proj);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Ext.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, depthRenderbuffer2);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fboHandle2);
            GL.DrawBuffer((DrawBufferMode)FramebufferAttachment.ColorAttachment0Ext);
            //GL.PushAttrib(AttribMask.ViewportBit);

            GL.Viewport(0, 0, shadowTextureSize, shadowTextureSize);

            GL.DrawBuffer(DrawBufferMode.None);

            GL.Clear(ClearBufferMask.DepthBufferBit);

            /*for (int i = 0; i < amountOfRenderPasses; i++)
            {
                RootNode.Instance.StartRender(i);
            }*/
            if (drawShadows)
            {
                RootNode.Instance.StartRender(-1);
            }

            // GL.PopAttrib();
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0); // return to visible framebuffer
            GL.DrawBuffer(DrawBufferMode.Back);
            GL.Ext.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, 0);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);

            GL.Enable(EnableCap.FramebufferSrgb);


            /*GL.ActiveTexture(TextureUnit.Texture5);
            GL.CopyTexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 0, 0, shadowSize, shadowSize);

            GL.ActiveTexture(TextureUnit.Texture0);*/

            TextureManager.mTexCache["screen"] = (int)depthTexture2;
            //Shader.unlitShaderCompiled.SetUniform("tex", 6);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureManager.GetTexture("terrain"));
            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, colorTexture2);
            GL.ActiveTexture(TextureUnit.Texture6);
            GL.BindTexture(TextureTarget.Texture2D, depthTexture2);
            GL.ActiveTexture(TextureUnit.Texture0);

            #endregion

            #region First FBO pass
            //GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientSize.Width, ClientSize.Height);
            UpdateViewport();

			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();

			// Enable rendering to FBO
			GL.Ext.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, depthRenderbuffer);
			GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, fboHandle);
			GL.DrawBuffer((DrawBufferMode)FramebufferAttachment.ColorAttachment0Ext);
			//GL.PushAttrib(AttribMask.ViewportBit);
			GL.Viewport(0, 0, Width, Height);

			// Clear previous frame
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			// Geometry render passes
			for(int i = 0; i < amountOfRenderPasses; i++)
			{
				RootNode.Instance.StartRender(i);
			}

			for(int i = 0; i < amountOfRenderPasses; i++)
			{
				if(Camera.Instance.parent == null)
				{
					Camera.Instance.StartRender(i);
				}
			}

			// Focal depth
			float[] pixelData = new float[1];
			GL.ReadPixels(Width / 2, Height / 2, 1, 1, PixelFormat.DepthComponent, PixelType.Float, pixelData);
			focalDistance = pixelData[0];

			// Restore render settings
			//GL.PopAttrib();
			GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0); // return to visible framebuffer
			GL.DrawBuffer(DrawBufferMode.Back);
			GL.Ext.BindRenderbuffer(RenderbufferTarget.RenderbufferExt, 0);
			GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);

            GL.Disable(EnableCap.FramebufferSrgb);

            #endregion
            #region FBO render to back buffer & HUD
            GL.Color4(Color4.White);

			// 2D rendering settings
			GL.DepthMask(false);
			GL.Disable(EnableCap.DepthTest);
			GL.Disable(EnableCap.Lighting);
			GL.Disable(EnableCap.CullFace);

			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(0, ClientRectangle.Width, ClientRectangle.Height, 0, -1, 10);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();

			// GFX sader selection
			if(InputManager.IsKeyToggled(Key.Number3))
			{
				if(InputManager.IsKeyToggled(Key.BackSpace))
				{
					Shader.ssaoShaderCompiled.Enable();
					Shader.ssaoShaderCompiled.SetUniform("tex", 0);
					Shader.ssaoShaderCompiled.SetUniform("depthTex", 1);
					Shader.ssaoShaderCompiled.SetUniform("focalDist", focalDistance);
				}
				else
				{
					Shader.blurShaderCompiled.Enable();
					Shader.blurShaderCompiled.SetUniform("tex", 0);
					Shader.blurShaderCompiled.SetUniform("depthTex", 1);
					Shader.blurShaderCompiled.SetUniform("focalDist", focalDistance);
				}
				GL.ActiveTexture(TextureUnit.Texture1);
				GL.BindTexture(TextureTarget.Texture2D, depthTexture);
			}
			else
			{
				if(InputManager.IsKeyToggled(Key.Number4))
				{
					GL.ActiveTexture(TextureUnit.Texture1);
					GL.BindTexture(TextureTarget.Texture2D, ditherTexture);

					Shader.ditherShaderCompiled.Enable();
					Shader.ditherShaderCompiled.SetUniform("tex", 0);
					Shader.ditherShaderCompiled.SetUniform("ditherTex", 1);
				}
				else
				{
					GL.ActiveTexture(TextureUnit.Texture1);
					GL.BindTexture(TextureTarget.Texture2D, depthTexture);

					Shader.fboShaderCompiled.Enable();
					Shader.fboShaderCompiled.SetUniform("tex", 0);
					Shader.fboShaderCompiled.SetUniform("depthTex", 1);
                    Shader.fboShaderCompiled.SetUniform("dim", new Vector2(Width, Height));
                    Shader.fboShaderCompiled.SetUniform("enableAO", enableAmbientOcclusion);
                }
            }

			// Render FBO quad
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, colorTexture);

			GL.Begin(PrimitiveType.Quads);
			GL.Color4(Color4.White);
			GL.TexCoord2(0, 1); GL.Vertex2(0, 0);
			GL.TexCoord2(0, 0); GL.Vertex2(0, Height);
			GL.TexCoord2(1, 0); GL.Vertex2(Width, Height);
			GL.TexCoord2(1, 1); GL.Vertex2(Width, 0);
			GL.End();

			Shader.hudShaderCompiled.Enable();
			Shader.hudShaderCompiled.SetUniform("tex", 0);

			// HUD render pass
			HudBase.Instance.StartRender();

			// Return to 3D rendering settings
			GL.Color3(Color.White);
			GL.DepthMask(true);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Lighting);
			GL.Enable(EnableCap.CullFace);
			#endregion
			#endregion

			// Display the back buffer
			SwapBuffers();
		}

		private void ConsoleInputReceived(object sender, HudConsoleInputEventArgs e)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			if(e.InputArray.Length > 0 && !String.IsNullOrEmpty(e.InputArray[0]))
			{
				switch(e.InputArray[0].ToLower())
				{
					case "exit":
					case "stop":
					case "close":
						Exit();
						break;
					case "set":
						if(e.InputArray.Length > 1 && !String.IsNullOrEmpty(e.InputArray[1]))
						{
							try
							{
								switch(e.InputArray[1].ToLower())
								{
									case "timemult":
										if(e.InputArray.Length > 2 && !String.IsNullOrEmpty(e.InputArray[2]))
										{
											timeMultiplier = Convert.ToDouble(e.InputArray[2]);
											Debug.WriteLine("timeMult was set to " + timeMultiplier, Color4.Orange);
										}
										break;
									case "c":
										if(e.InputArray.Length > 2 && !String.IsNullOrEmpty(e.InputArray[2]))
										{
											c = (float)Convert.ToDouble(e.InputArray[2]);
											Debug.WriteLine("c was set to " + c, Color4.Orange);
										}
										break;
									case "vsync":
										if(e.InputArray.Length > 2 && !String.IsNullOrEmpty(e.InputArray[2]))
										{
											try
											{
												VSync = (VSyncMode)Enum.Parse(typeof(VSyncMode), e.InputArray[2], true);
											}
											catch(Exception exception)
											{
											}
											Debug.WriteLine("VSync was set to " + VSync, Color4.Orange);
										}
										break;
									case "fov":
										if(e.InputArray.Length > 2 && !String.IsNullOrEmpty(e.InputArray[2]))
										{
											Camera.Instance.Fov = Math.Max(10f, Math.Min(170f, (float)Convert.ToDouble(e.InputArray[2])));
											Debug.WriteLine("fov was set to " + Camera.Instance.Fov, Color4.Orange);
										}
										break;
									default:
										if(!program.ExecuteCommand(e.Input))
										{
											Debug.WriteLine("Usage: set [timeMult|walkSpeed|VSync|mouse|c|fov|showSkybox|showSwitches] [value]", Color4.LightBlue);
										}
										break;
								}
							}
							catch(Exception exception)
							{
							}
						}
						else
						{
							if(!program.ExecuteCommand(e.Input))
							{
								Debug.WriteLine("Usage: set [timeMult|walkSpeed|VSync|mouse|c|fov|showSkybox|showSwitches] [value]", Color4.LightBlue);
							}
						}
						break;
					case "reset":
						if(e.InputArray.Length > 1 && !String.IsNullOrEmpty(e.InputArray[1]))
						{
							switch(e.InputArray[1].ToLower())
							{
								case "timemult":
									timeMultiplier = 1.0;
									Debug.WriteLine("timeMult was reset to " + timeMultiplier, Color4.Orange);
									break;
								case "c":
									c = 29979245800f;
									Debug.WriteLine("c was reset to " + timeMultiplier, Color4.Orange);
									break;
								case "vsync":
									VSync = VSyncMode.On;
									Debug.WriteLine("walkSpeed was reset to " + VSync, Color4.Orange);
									break;
								case "fov":
									Camera.Instance.Fov = 90f;
									Debug.WriteLine("fov was reset to " + Camera.Instance.Fov, Color4.Orange);
									break;
								default:
									if(!program.ExecuteCommand(e.Input))
									{
										Debug.WriteLine("Usage: reset [timeMult|walkSpeed|VSync|mouse|c|fov|showSkybox|showSwitches]", Color4.LightBlue);
									}
									break;
							}
						}
						else
						{
							if(!program.ExecuteCommand(e.Input))
							{
								Debug.WriteLine("Usage: reset [timeMult|walkSpeed|VSync|mouse|c|fov|showSkybox|showSwitches]", Color4.LightBlue);
							}
						}
						break;
					case "get":
						if(e.InputArray.Length > 1 && !String.IsNullOrEmpty(e.InputArray[1]))
						{
							switch(e.InputArray[1].ToLower())
							{
								case "timemult":
									Debug.WriteLine("timeMult = " + timeMultiplier, Color4.Orange);
									break;
								case "c":
									Debug.WriteLine("c = " + c, Color4.Orange);
									break;
								case "vsync":
									Debug.WriteLine("VSync = " + VSync, Color4.Orange);
									break;
								case "fov":
									Debug.WriteLine("fov = " + Camera.Instance.Fov, Color4.Orange);
									break;
								default:
									if(!program.ExecuteCommand(e.Input))
									{
										Debug.WriteLine("Usage: get [timeMult|walkSpeed|VSync|mouse|c|fov|showSkybox|showSwitches]", Color4.LightBlue);
									}
									break;
							}
						}
						else
						{
							if(!program.ExecuteCommand(e.Input))
							{
								Debug.WriteLine("Usage: get [timeMult|walkSpeed|VSync|mouse|c|fov|showSkybox|showSwitches]", Color4.LightBlue);
							}
						}
						break;
					case "tp":
						if(e.InputArray.Length > 3 && !String.IsNullOrEmpty(e.InputArray[1]) && !String.IsNullOrEmpty(e.InputArray[2]) &&
							!String.IsNullOrEmpty(e.InputArray[3]))
						{
							if(e.InputArray[1][0] == '#')
							{
								if(e.InputArray[1].Length > 1)
								{
									Camera.Instance.position.X += (float)Convert.ToDouble(e.InputArray[1].Substring(1, e.InputArray[1].Length - 1));
								}
							}
							else
							{
								Camera.Instance.position.X = (float)Convert.ToDouble(e.InputArray[1]);
							}
							if(e.InputArray[2][0] == '#')
							{
								if(e.InputArray[2].Length > 1)
								{
									Camera.Instance.position.Y += (float)Convert.ToDouble(e.InputArray[2].Substring(1, e.InputArray[2].Length - 1));
								}
							}
							else
							{
								Camera.Instance.position.Y = (float)Convert.ToDouble(e.InputArray[2]);
							}
							if(e.InputArray[3][0] == '#')
							{
								if(e.InputArray[3].Length > 1)
								{
									Camera.Instance.position.Z += (float)Convert.ToDouble(e.InputArray[3].Substring(1, e.InputArray[3].Length - 1));
								}
							}
							else
							{
								Camera.Instance.position.Z = (float)Convert.ToDouble(e.InputArray[3]);
							}
						}
						else
						{
							Debug.WriteLine("Usage: tp [x] [y] [z]", Color4.LightBlue);
						}
						break;
					case "clear":
						hudConsole.ClearScreen();
						break;
					case "reload":
						InputManager.ClearToggleStates();
						worldTime = 0f;
						localTime = 0f;
						c = 29979245800f;
						v = 0f;
						b = 0f;
						lf = 1f;
						enableDoppler = true;
						enableRelBrightness = true;
						enableRelAberration = true;
						smoothedVelocity = Vector3.Zero;
						smoothFactor = 4000f;
						timeMultiplier = 1.0;
						Camera.Instance.position = new Vector3(0, 80, 0);
						break;
					case "list":
					case "help":
						Debug.WriteLine("Available commands: stop set get reset reload list clear", Color4.LightBlue);
						break;
					default:
						if(!program.ExecuteCommand(e.Input))
						{
							Debug.WriteLine("Invalid command. Type 'list' for a list of commands", Color4.Red);
						}
						break;

				}
			}
		}

		private void HandleKeyPress(object sender, KeyPressEventArgs e)
		{
			if(e.KeyChar == '`' || e.KeyChar == '~' || e.KeyChar == '	' || e.KeyChar == '/') //Tab and '/' to support non-European keyboards
			{
				if(hudConsole.enabled)
				{
					hudConsole.enabled = false;
					hudConsole.isVisible = false;
				}
				else
				{
					hudConsole.enabled = true;
					hudConsole.isVisible = true;
					if(hudConsole.input.Length > 0)
					{
						hudConsole.input = hudConsole.input.Remove(hudConsole.input.Length - 1);
					}
				}
			}
		}

		private bool CheckFboStatus()
		{
			// Taken from the OpenTK documentation
			switch(GL.Ext.CheckFramebufferStatus(FramebufferTarget.FramebufferExt))
			{
				case FramebufferErrorCode.FramebufferCompleteExt:
					{
						Debug.WriteLine("FBO: The framebuffer is complete and valid for rendering.");
						return true;
					}
				case FramebufferErrorCode.FramebufferIncompleteAttachmentExt:
					{
						Debug.WriteLine("ERROR: failed to create FBO: One or more attachment points are not framebuffer attachment complete. This could mean there’s no texture attached or the format isn’t renderable. For color textures this means the base format must be RGB or RGBA and for depth textures it must be a DEPTH_COMPONENT format. Other causes of this error are that the width or height is zero or the z-offset is out of range in case of render to volume.");
						break;
					}
				case FramebufferErrorCode.FramebufferIncompleteMissingAttachmentExt:
					{
						Debug.WriteLine("ERROR: failed to create FBO: There are no attachments.");
						break;
					}
				case FramebufferErrorCode.FramebufferIncompleteDimensionsExt:
					{
						Debug.WriteLine("ERROR: failed to create FBO: Attachments are of different size. All attachments must have the same width and height.");
						break;
					}
				case FramebufferErrorCode.FramebufferIncompleteFormatsExt:
					{
						Debug.WriteLine("ERROR: failed to create FBO: The color attachments have different format. All color attachments must have the same format.");
						break;
					}
				case FramebufferErrorCode.FramebufferIncompleteDrawBufferExt:
					{
						Debug.WriteLine("ERROR: failed to create FBO: An attachment point referenced by GL.DrawBuffers() doesn’t have an attachment.");
						break;
					}
				case FramebufferErrorCode.FramebufferIncompleteReadBufferExt:
					{
						Debug.WriteLine("ERROR: failed to create FBO: The attachment point referenced by GL.ReadBuffers() doesn’t have an attachment.");
						break;
					}
				case FramebufferErrorCode.FramebufferUnsupportedExt:
					{
						Debug.WriteLine("ERROR: failed to create FBO: This particular FBO configuration is not supported by the implementation.");
						break;
					}
				default:
					{
						Debug.WriteLine("ERROR: failed to create FBO: Status unknown. (yes, this is really bad.)");
						break;
					}
			}
			return false;
		}
	}
}
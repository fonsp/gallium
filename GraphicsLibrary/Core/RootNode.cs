namespace GraphicsLibrary.Core
{
	public class RootNode:Node
	{
		#region SingleTon
		private static RootNode instance;
		/// <summary>
		/// The primary node in Gallium.
		/// </summary>
		public static RootNode Instance
		{
			get { return instance ?? (instance = new RootNode()); }
		}
		#endregion

		public RootNode()
			: base("root")
		{
		}
	}
}
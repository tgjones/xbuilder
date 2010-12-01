using Microsoft.Xna.Framework;

namespace XBuilder.Util
{
	public static class ConvertUtility
	{
		public static Color ToXnaColor(System.Drawing.Color c)
		{
			return new Color(c.R, c.G, c.B);
		}
	}
}
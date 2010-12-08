using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XBuilder.Xna
{
	internal static class VertexElementExtractor
	{
		public static Vector3[] GetVertexElement(ModelMeshPart meshPart, VertexElementUsage usage)
		{
			VertexDeclaration vd = meshPart.VertexBuffer.VertexDeclaration;
			VertexElement[] elements = vd.GetVertexElements();

			Func<VertexElement, bool> elementPredicate = ve => ve.VertexElementUsage == usage && ve.VertexElementFormat == VertexElementFormat.Vector3;
			if (!elements.Any(elementPredicate))
				return null;

			VertexElement element = elements.First(elementPredicate);

			Vector3[] vertexData = new Vector3[meshPart.NumVertices];
			meshPart.VertexBuffer.GetData((meshPart.VertexOffset * vd.VertexStride) + element.Offset,
				vertexData, 0, vertexData.Length, vd.VertexStride);

			return vertexData;
		}
	}
}
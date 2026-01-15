using XYEngine.Resources;

namespace XYEngine.Utils;

public static class MeshFactory
{
	public static Mesh CreateQuad(Vector2 size, Vector2? origin = null, Region? uvs = null)
	{
		var mesh = new Mesh();
		var originValue = origin ?? Vector2.zero;
		mesh.vertices =
		[
			new Vector2(0, size.y) - originValue,
			size - originValue,
			new Vector2(size.x, 0) - originValue,
			-originValue
		];
		
		var meshUv = uvs ?? new Region(Vector2.zero, Vector2.one);
		mesh.uvs =
		[
			meshUv.position00, new Vector2(meshUv.position11.x, meshUv.position00.y), meshUv.position11,
			new Vector2(meshUv.position00.x, meshUv.position11.y)
		];
		mesh.indices = [0, 3, 1, 3, 2, 1];
		
		return mesh;
	}
	
	public static Mesh CreateQuads((Rect vertices, Region uvs)[] quads)
	{
		var mesh = new Mesh();
		mesh.vertices = new Vector2[quads.Length * 4];
		mesh.uvs = new Vector2[mesh.vertices.Length];
		mesh.indices = new uint[quads.Length * 6];
		
		for (uint i = 0; i < quads.Length; i++)
		{
			var quad = quads[i];
			var pos00 = quad.vertices.position;
			var pos11 = quad.vertices.position + quad.vertices.size;
			var iV = i * 4;
			
			mesh.vertices[iV] = new Vector2(pos00.x, pos11.y);
			mesh.vertices[iV + 1] = pos11;
			mesh.vertices[iV + 2] = new Vector2(pos11.x, pos00.y);
			mesh.vertices[iV + 3] = pos00;
			
			mesh.uvs[iV] = quad.uvs.position00;
			mesh.uvs[iV + 1] = new Vector2(quad.uvs.position11.x, quad.uvs.position00.y);
			mesh.uvs[iV + 2] = quad.uvs.position11;
			mesh.uvs[iV + 3] = new Vector2(quad.uvs.position00.x, quad.uvs.position11.y);
			
			var iI = i * 6;
			mesh.indices[iI] = iV;
			mesh.indices[iI + 1] = iV + 3;
			mesh.indices[iI + 2] = iV + 1;
			mesh.indices[iI + 3] = iV + 3;
			mesh.indices[iI + 4] = iV + 2;
			mesh.indices[iI + 5] = iV + 1;
		}
		
		return mesh;
	}
	
	public static Mesh SetQuadPosition(this Mesh mesh, Rect rect, int index = 0)
	{
		var iV = index * 4;
		var pos00 = rect.position;
		var pos11 = rect.position + rect.size;
		
		mesh.vertices[iV] = new Vector2(pos00.x, pos11.y);
		mesh.vertices[iV + 1] = pos11;
		mesh.vertices[iV + 2] = new Vector2(pos11.x, pos00.y);
		mesh.vertices[iV + 3] = pos00;
		
		return mesh;
	}
	
	
	public static Mesh SetQuadUV(this Mesh mesh, Region uvs, int index = 0)
	{
		var iV = index * 4;
		mesh.uvs[iV] = uvs.position00;
		mesh.uvs[iV + 1] = new Vector2(uvs.position11.x, uvs.position00.y);
		mesh.uvs[iV + 2] = uvs.position11;
		mesh.uvs[iV + 3] = new Vector2(uvs.position00.x, uvs.position11.y);
		
		return mesh;
	}
}
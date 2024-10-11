namespace XYEngine.Graphics;

public class MeshQuad : Mesh
{
    private const int N_VERTICES = 4;
    private const int N_INDICES = 6;
    private const int N_UV = 4;
    
    public int nQuads { get; private set; }

    public MeshQuad(int nQuads = 1)
    {
        this.nQuads = nQuads;
        vertices = new Vector2[nQuads * N_VERTICES];
        indices = new uint[nQuads * N_INDICES];
        uv = new Vector3[nQuads * N_UV];

        // Comme pour le moment, il n'y a pas de gestion des indices, ça s'initialise ici :
        for (var i = 0; i < nQuads; i++)
        {
            var vuIndex = (uint)i * 4;
            var iIndex = i * N_INDICES;
            indices[iIndex] = vuIndex;
            indices[iIndex + 1] = vuIndex + 1;
            indices[iIndex + 2] = vuIndex + 3;
            indices[iIndex + 3] = vuIndex + 1;
            indices[iIndex + 4] = vuIndex + 2;
            indices[iIndex + 5] = vuIndex + 3;
        }
    }

    public MeshQuad SetQuad(int index, Vector2 position, Vector2 size, Vector2 uv00, Vector2 uv11, Vector2? pivot = null, int target = 0)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, nQuads);

        position -= (pivot ?? Vector2.zero) * size;
        var vIndex = index * N_VERTICES;
        vertices[vIndex] = position;
        vertices[vIndex + 1] = new Vector2(position.x, position.y + size.y );
        vertices[vIndex + 2] = position + size;
        vertices[vIndex + 3] = new Vector2(position.x + size.x, position.y);
        
        var tIndex = index * N_UV;
        uv[tIndex] = new Vector3(uv00.x, uv00.y, target);
        uv[tIndex + 1] = new Vector3(uv00.x, uv11.y, target);
        uv[tIndex + 2] = new Vector3(uv11.x, uv11.y, target);
        uv[tIndex + 3] = new Vector3(uv11.x, uv00.y, target);
        return this;
    }

    public MeshQuad SetQuadVertices(int index, Vector2 position, Vector2 size)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, nQuads);

        var vIndex = index * N_VERTICES;
        vertices[vIndex] = position;
        vertices[vIndex + 1] = new Vector2(position.x, position.y + size.y );
        vertices[vIndex + 2] = position + size;
        vertices[vIndex + 3] = new Vector2(position.x + size.x, position.y);
        return this;
    }

    public MeshQuad SetQuadUv(int index, Vector2 uv00, Vector2 uv11, int target = 0)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, nQuads);

        index *= N_UV;
        uv[index] = new Vector3(uv00.x, uv00.y, target);
        uv[index + 1] = new Vector3(uv00.x, uv11.y, target);
        uv[index + 2] = new Vector3(uv11.x, uv11.y, target);
        uv[index + 3] = new Vector3(uv11.x, uv00.y, target);
        return this;
    }
}
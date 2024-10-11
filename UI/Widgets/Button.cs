using XYEngine.Graphics;

namespace XYEngine.UI.Widgets;

public sealed class Button : UIElement
{
    public readonly Label label;
    private readonly MeshQuad meshQuad;
    private readonly StretchTexture stretch;

    public string text { get => label.text; set => label.text = value; }
    public event Action clicked;

    public override Vector2Int size { get => base.size; set => SetSize(value); }

    public Button(StretchTexture stretchTexture)
    {
        label = new Label { pivot = new Vector2(0.5F), anchors = new Vector2(0.5F) };
        AddChild(label);

        stretch = stretchTexture;
        scaleWithSize = false;
        meshQuad = new MeshQuad(9);
        for (var i = 0; i < 9; i++)
            meshQuad.SetQuad(i, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one);

        var corner00 = stretchTexture.corner00;
        var corner11 = stretchTexture.corner11;
        meshQuad.SetQuadUv(0, corner00.position00, corner00.position11);
        meshQuad.SetQuadUv(1, new Vector2(corner00.position11.x, corner00.position00.y), new Vector2(corner11.position00.x, corner00.position11.y));
        meshQuad.SetQuadUv(2, new Vector2(corner11.position00.x, corner00.position00.y), new Vector2(corner11.position11.x, corner00.position11.y));
        meshQuad.SetQuadUv(3, new Vector2(corner00.position00.x, corner00.position11.y), new Vector2(corner00.position11.x, corner11.position00.y));
        meshQuad.SetQuadUv(4, corner00.position11, corner11.position00);
        meshQuad.SetQuadUv(5, new Vector2(corner11.position00.x, corner00.position11.y), new Vector2(corner11.position11.x, corner11.position00.y));
        meshQuad.SetQuadUv(6, new Vector2(corner00.position00.x, corner11.position00.y), new Vector2(corner00.position11.x, corner11.position11.y));
        meshQuad.SetQuadUv(7, new Vector2(corner00.position11.x, corner11.position00.y), new Vector2(corner11.position00.x, corner11.position11.y));
        meshQuad.SetQuadUv(8, corner11.position00, corner11.position11);

        render.texture = stretchTexture.texture;
        render.mesh = meshQuad;
    }

    private void SetSize(Vector2Int size)
    {
        var centerSize = size - stretch.cornerSize00 - stretch.cornerSize11;
        meshQuad.SetQuadVertices(0, Vector2.zero, stretch.cornerSize00);
        meshQuad.SetQuadVertices(1, new Vector2(stretch.cornerSize00.x, 0), new Vector2(centerSize.x, stretch.cornerSize00.y));
        meshQuad.SetQuadVertices(2, new Vector2(stretch.cornerSize00.x + centerSize.x, 0), new Vector2(stretch.cornerSize11.x, stretch.cornerSize00.y));
        meshQuad.SetQuadVertices(3, new Vector2(0, stretch.cornerSize00.y), new Vector2(stretch.cornerSize00.x, centerSize.y));
        meshQuad.SetQuadVertices(4, stretch.cornerSize00, centerSize);
        meshQuad.SetQuadVertices(5, new Vector2(stretch.cornerSize00.x + centerSize.x, stretch.cornerSize00.y), new Vector2(stretch.cornerSize11.x, centerSize.y));
        meshQuad.SetQuadVertices(6, new Vector2(0, stretch.cornerSize00.y + centerSize.y), new Vector2(stretch.cornerSize00.x, stretch.cornerSize11.y));
        meshQuad.SetQuadVertices(7, new Vector2(stretch.cornerSize00.x, stretch.cornerSize00.y + centerSize.y), new Vector2(centerSize.x, stretch.cornerSize11.y));
        meshQuad.SetQuadVertices(8, stretch.cornerSize00 + centerSize, stretch.cornerSize11);
        meshQuad.Apply();
        
        base.size = size;
    }

    private void MouseEnterOrExit(bool enter) => tint = enter ? Color.grey : Color.white;

    protected override void OnAdded()
    {
        UIEvent.Register(this, UIEvent.Type.MouseClick, () => clicked?.Invoke());
        UIEvent.Register(this, UIEvent.Type.MouseEnter, () => MouseEnterOrExit(true));
        UIEvent.Register(this, UIEvent.Type.MouseExit, () => MouseEnterOrExit(false));
    }

    protected override void OnRemoved()
    {
        UIEvent.UnRegisterAllEvents(this);
    }
}
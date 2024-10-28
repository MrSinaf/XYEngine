using XYEngine.Graphics;

namespace XYEngine.UI.Widgets;

public sealed class Button : Panel
{
    public readonly Label label;

    public string text { get => label.text; set => label.text = value; }
    public event Action clicked;

    public Button(StretchTexture stretchTexture) : base(stretchTexture)
    {
        label = new Label { pivotAndAnchors = new Vector2(0.5F) };
        AddChild(label);
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
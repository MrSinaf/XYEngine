using XYEngine.Graphics;

namespace XYEngine.UI;

public sealed class RootElement: UIElement
{
    internal RootElement() {}

    protected internal override void Draw(Shader shader)
    {
        UnmarkAsNeedingUpdate();
        base.Draw(shader);
    }
}
using Silk.NET.OpenGL;
using Shader = XYEngine.Graphics.Shader;
using static XYEngine.GameWindow;

namespace XYEngine.UI.Widgets;

/// <summary>
/// Permet de masquer le contenu débordant de celui-ci.
/// <remarks>Il ne supporte pas pour le moment le fait qu'un <see cref="Mask"/> soit descendant d'un <see cref="Mask"/>.</remarks>
/// </summary>
public class Mask : UIElement
{
    public bool masked = true;

    // TODO: Ajouter le fait de pouvoir supporter plusieurs mask s'enchainant.
    protected internal override void Draw(Shader shader)
    {
        if (!active)
            return;
        
        if (masked)
        {
            var scissorSize = Canvas.ScaleToCanvas(scaledSize);
            var scissorPosition = Canvas.ScaleToCanvas(realPosition);
        
            gl.Scissor(scissorPosition.x, scissorPosition.y, (uint)scissorSize.x, (uint)scissorSize.y);
            gl.Enable(GLEnum.ScissorTest);
            base.Draw(shader);
            gl.Disable(GLEnum.ScissorTest);
        }
        else
            base.Draw(shader);
    }
}
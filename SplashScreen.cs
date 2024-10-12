using XYEngine.Animations;
using XYEngine.Graphics;
using XYEngine.UI.Widgets;

namespace XYEngine;

internal class SplashScreen(Action onFinish) : Scene
{
    private readonly Animation animation = new ();

    protected override void Start()
    {
#if DEBUG
        onFinish();
        return;
#endif
        
        canvas.size = new Vector2Int(80, 45);
        var texture = Resources.LoadTextureSheet("splashScreen");
        var image = new Image(texture)
        {
            anchorMin = new Vector2(0.5F),
            anchorMax = new Vector2(0.5F),
            pivot = new Vector2(0.5F),
            size = texture.data.frameSize
        };
        canvas.root.AddChild(image);

        Keyframe<Rect>[] uvKeys =
        [
            new (0, texture.GetUV(0)), new (10, texture.GetUV(1)), new (11, texture.GetUV(2)), new (12, texture.GetUV(3)), new (13, texture.GetUV(4)),
            new (14, texture.GetUV(5)), new (24, texture.GetUV(6)), new (25, texture.GetUV(7)), new (35, texture.GetUV(7))
        ];
        animation.AddTrack(new AnimationTrackProperty<Rect>(uvKeys, value => (image.render.mesh as MeshQuad)!
                                                                             .SetQuadUv(0, value.position00, value.position11).Apply()));
        animation.onFinish += onFinish;
        animation.Play();
    }

    protected override void Render() => animation.Update();
}
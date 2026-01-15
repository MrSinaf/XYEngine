using XYEngine.UI;
using XYEngine.UI.Widgets;

namespace XYEngine.Scenes;

public class EmptyScene : Scene
{
	protected override void BuildUI(RootElement root) => root.AddChild(new Label("Empty Scene")
	{
		pivotAndAnchors = new Vector2(0.5F)
	});
}
using XYEngine.Resources;

namespace XYEngine.Debugs.Windows;

public class DebugAbout : IDebugWindow, IDebugMainBar
{
	private const float LOGO_WIDTH = 200f;
	private const int WINDOW_SIZE = 300;
	
	public int priority => int.MaxValue;
	public bool visible { get; private set; }
	
	public void OnDebugWindowRender(string name)
	{
		var isOpen = visible;
		if (ImGui.Begin(name, ref isOpen, ImGuiWindowFlags.NoResize))
		{
			ImGui.SetWindowSize(new Vector2(WINDOW_SIZE));
			
			{ // Affichage du logo :
				ImGui.SetCursorPosX((WINDOW_SIZE - LOGO_WIDTH) * 0.5f);
				ImGui.Image((IntPtr)Vault.GetEmbeddedAsset<Texture2D>("xyengine.png").gTexture.handle,
							new Vector2(LOGO_WIDTH));
			}
			
			{ // Infos sur le moteur :
				ImGui.Text("Made by");
				ImGui.SameLine();
				ImGui.TextColored(new Vector4(0.5f, 0.8f, 0.3f, 1.0f), "MrSinaf (Mickaël Dancoisne)");
				
				ImGui.SetCursorPosX(16);
				ImGui.Text($"v{XY.version}");
			}
			
			{ // Titre du jeu :
				ImGui.Dummy(new Vector2(0, 10));
				var calcTextSize = ImGui.CalcTextSize($"Game is {XY.gameName}");
				ImGui.SetCursorPosX((WINDOW_SIZE - calcTextSize.x) * 0.5f);
				
				ImGui.Text("Game is");
				ImGui.SameLine();
				ImGui.TextColored(new Vector4(0.5f, 0.8f, 0.3f, 1.0f), XY.gameName);
			}
			
			ImGui.End();
		}
		visible = isOpen;
	}
	
	public void OnDebugMainBarRender()
	{
		if (ImGui.MenuItem("About"))
			visible = !visible;
	}
}
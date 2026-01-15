using System.Reflection;

namespace XYEngine.Debugs.Windows;

public class DebugVault : IDebugWindow, IDebugMainBar
{
	private static Dictionary<string, AssetReference> assets;
	public int priority => 0;
	
	public bool visible { get; set; }
	internal (string name, AssetReference reference) selectedAsset;
	
	private bool listing;
	private int padding = 10;
	private float thumbnailSize = 96;
	
	public DebugVault()
	{
		var cacheField = typeof(Vault).GetField("assets", BindingFlags.NonPublic | BindingFlags.Static);
		assets = cacheField?.GetValue(null) as Dictionary<string, AssetReference>;
	}
	
	public void OnDebugWindowRender(string name)
	{
		var isOpen = visible;
		if (ImGui.Begin(name, ref isOpen, ImGuiWindowFlags.MenuBar))
		{
			ShowHeader();
			
			if (selectedAsset.name != null)
			{
				ImGui.Columns(2);
				if (listing)
					ShowListing();
				else
					ShowGrid();
				ImGui.NextColumn();
				ShowSelectedAsset();
				ImGui.Columns();
			}
			else if (listing)
				ShowListing();
			else
				ShowGrid();
			
			ImGui.End();
		}
		visible = isOpen;
	}
	
	private void ShowHeader()
	{
		if (ImGui.BeginMenuBar())
		{
			if (ImGui.BeginMenu("Options"))
			{
				ImGui.Checkbox("listing", ref listing);
				ImGui.SliderInt("Padding", ref padding, 10, 50);
				ImGui.SliderFloat("Thumbnail Size", ref thumbnailSize, 32, 256);
				ImGui.EndMenu();
			}
			ImGui.EndMenuBar();
		}
	}
	
	private void ShowListing()
	{
		if (ImGui.BeginChild("Listing"))
		{
			foreach (var (name, assetRef) in assets)
			{
				var cursorStart = ImGui.GetCursorPos();
				{
					var isSelected = selectedAsset.name == name;
					if (ImGui.Selectable($"##Selectable_{name}", isSelected, ImGuiSelectableFlags.None, 
										 new Vector2(0, 32)))
						selectedAsset = (name, assetRef);
				}
				var cursorEnd = ImGui.GetCursorPos();
				
				ImGui.SetCursorPos(cursorStart);
				ImGui.BeginGroup();
				ImGui.PushID(name);
				{
					ImGui.Button($"##{name}_thumb", new Vector2(32));
					ImGui.SameLine();
					
					ImGui.BeginGroup();
					{
						ImGui.Text(name);
						ImGui.TextColored(new Vector4(1, 1, 1, 0.5F), $" {assetRef.asset.GetType().Name}");
					}
					ImGui.EndGroup();
				}
				ImGui.PopID();
				ImGui.EndGroup();
				
				ImGui.SetCursorPos(cursorEnd);
				ImGui.Spacing();
			}
			ImGui.EndChild();
		}
	}
	
	private void ShowGrid()
	{
		if (ImGui.BeginChild("Grid"))
		{
			var cellSize = thumbnailSize + padding;
			var panelWidth = ImGui.GetContentRegionAvail().x;
			var columnCount = (int)(panelWidth / cellSize);
			if (columnCount < 1) columnCount = 1;
			
			ImGui.Columns(columnCount, "GridColumns", false);
			
			foreach (var (name, assetRef) in assets)
			{
				ImGui.PushID(name);
				{
					ImGui.BeginGroup();
					{
						ImGui.Button($"##{name}_thumb", new Vector2(thumbnailSize, thumbnailSize));
						
						var displayName = name;
						if (ImGui.CalcTextSize(displayName).x > thumbnailSize)
						{
							for (var i = name.Length; i > 0; i--)
							{
								var tryName = name[..i] + "...";
								if (ImGui.CalcTextSize(tryName).x <= thumbnailSize)
								{
									displayName = tryName;
									break;
								}
							}
						}
						
						ImGui.Text(displayName);
					}
					ImGui.EndGroup();
					
					if (ImGui.IsItemHovered())
					{
						ImGui.BeginTooltip();
						ImGui.Text(name);
						ImGui.EndTooltip();
					}
					
					if (ImGui.IsItemClicked())
						selectedAsset = (name, assetRef);
				}
				ImGui.PopID();
				ImGui.NextColumn();
			}
			ImGui.Columns();
			ImGui.EndChild();
		}
	}
	
	private void ShowSelectedAsset()
	{
		if (ImGui.BeginChild("SelectedAsset"))
		{
			var asset = selectedAsset.reference.asset;
			
			ImGui.TextColored(new Vector4(0, 1, 0, 1), selectedAsset.name);
			ImGui.TextColored(new Vector4(1, 1, 1, 0.3F), $"  {asset.GetType().Name}");
			
			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();
			
			XYDebug.ShowObjectProperties(asset);
			
			ImGui.EndChild();
		}
	}
	
	public void OnDebugMainBarRender()
	{
		if (ImGui.BeginMenu("Windows"))
		{
			if (ImGui.MenuItem("Vault"))
				visible = !visible;
			
			ImGui.EndMenu();
		}
	}
}
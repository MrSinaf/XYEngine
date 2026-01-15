using System.Reflection;

namespace XYEngine.Debugs.Windows;

internal class DebugAssets : DebugWindow
{
	public override string name => "Assets";
	public override Vector2 size => new (650, 300);
	public override ImGuiWindowFlags flags => ImGuiWindowFlags.None;
	
	private static (string path, IAsset asset) selectedAsset;
	private static Dictionary<string, AssetReference> cache;
	
	public DebugAssets()
	{
		DebugHotReload.Init();
		
		var cacheField = typeof(Vault).GetField("assets", BindingFlags.NonPublic | BindingFlags.Static);
		cache = cacheField?.GetValue(null) as Dictionary<string, AssetReference>;
	}
	
	public override void Render()
	{
		ImGui.Columns(2, "AssetsColumns");
		if (!notFirstDraw)
			ImGui.SetColumnWidth(0, 200);
		
		ImGui.BeginChild("Path");
		if (ImGui.Button(DebugHotReload.actif ? "Stop HotReload" : "Start HotReload (BETA)"))
			DebugHotReload.actif = !DebugHotReload.actif;
		
		DrawAssetsPath(cache);
		ImGui.EndChild();
		
		ImGui.NextColumn();
		
		ImGui.BeginChild("Inspector");
		DrawAssetInspector();
		ImGui.EndChild();
	}
	
	private static void DrawAssetInspector()
	{
		if (selectedAsset.asset == null)
			return;
		
		ImGui.TextColored(new Vector4(0, 1, 0, 0.9F), "Inspector");
		
		if (ImGui.Button("HotReload"))
			Vault.ReloadResource(selectedAsset.path, Path.Combine(DebugHotReload.assetsSourcePath, selectedAsset.path));
		
		ImGui.TextColored(new Vector4(1, 1, 1, 0.3F), $"  {selectedAsset.asset.GetType().Name}");
		ImGui.Separator();
		ImGui.Spacing();
		
		if (selectedAsset.asset is IDebugProperty asset)
			asset.OnDebugPropertyRender();
		else
		{
			var properties = selectedAsset.asset.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (var property in properties)
			{
				if (!property.CanRead || property.GetMethod.GetParameters().Length > 0)
					continue;
				
				ImGui.AlignTextToFramePadding();
				XYDebug.ShowValue(property.Name, property.GetValue(selectedAsset.asset).ToString());
			}
		}
	}
	
	private static void DrawAssetsPath(Dictionary<string, AssetReference> assetCache)
	{
		ImGui.TextColored(new Vector4(0, 1, 0, 0.9F), "Cache");
		ImGui.TextColored(new Vector4(1, 1, 1, 0.3F), $"  Count: {assetCache?.Count ?? 0}");
		ImGui.Separator();
		ImGui.Spacing();
		
		if (assetCache == null || assetCache.Count == 0)
		{
			ImGui.Text("Cache is empty.");
			return;
		}
		
		const string separator = "/";
		var root = new Dictionary<string, object>();
		foreach (var value in assetCache)
		{
			var parts = value.Key.Split(separator);
			var current = root;
			for (var i = 0; i < parts.Length; i++)
			{
				if (i == parts.Length - 1)
					current[parts[i]] = value;
				else
				{
					if (!current.ContainsKey(parts[i]))
						current[parts[i]] = new Dictionary<string, object>();
					current = current[parts[i]] as Dictionary<string, object>;
				}
			}
		}
		RenderTree(root, "");
		
		void RenderTree(Dictionary<string, object> tree, string parentPath)
		{
			foreach (var node in tree)
				switch (node.Value)
				{
					case Dictionary<string, object> subTree:
					{
						if (ImGui.TreeNode(node.Key))
						{
							RenderTree(subTree, $"{parentPath}{node.Key}/");
							ImGui.TreePop();
						}
						break;
					}
					case KeyValuePair<string, AssetReference> asset:
					{
						if (ImGui.Selectable(asset.Key[parentPath.Length..], selectedAsset.path == asset.Key))
							selectedAsset = (asset.Value.resource.path, asset.Value.asset);
						break;
					}
				}
		}
	}
}
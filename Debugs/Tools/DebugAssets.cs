using System.Numerics;
using System.Reflection;
using ImGuiNET;
using XYEngine.Utils;

namespace XYEngine.Debugs;

internal static class DebugAssets
{
	public static bool showAssets;
	
	private static (string path, IAsset asset) selectedAsset;
	private static bool cacheEmbeddedTab;
	
	public static void Render()
	{
		if (showAssets)
			DrawAssetsWindow();
	}
	
	private static void DrawAssetsWindow()
	{
		var cacheField = typeof(AssetManager).GetField("cache", BindingFlags.NonPublic | BindingFlags.Static);
		var embeddedCacheField = typeof(AssetManager).GetField("embeddedCache", BindingFlags.NonPublic | BindingFlags.Static);
		
		ImGui.Begin("Assets", ref showAssets);
		ImGui.Columns(2, "AssetsColumns");
		
		ImGui.BeginChild("Tabs");
		if (ImGui.BeginTabBar("CacheTabs"))
		{
			
			if (ImGui.BeginTabItem("Cache"))
			{
				cacheEmbeddedTab = false;
				ImGui.EndTabItem();
			}
			
			if (ImGui.BeginTabItem("Embedded Cache"))
			{
				cacheEmbeddedTab = true;
				ImGui.EndTabItem();
			}
			
			ImGui.EndTabBar();
		}
		
		DrawAssetList((cacheEmbeddedTab ? embeddedCacheField : cacheField)?.GetValue(null) as Dictionary<string, AssetReference>);
		ImGui.EndChild();
		
		ImGui.NextColumn();
		
		// Colonne de droite : Inspecteur de l'actif sélectionné
		ImGui.BeginChild("Properties");
		DrawAssetInspector();
		ImGui.EndChild();
		ImGui.End();
		
		void DrawAssetList(Dictionary<string, AssetReference> assetCache)
		{
			ImGui.TextColored(new Vector4(1, 1, 0, 1), $"Count: {assetCache?.Count ?? 0}");
			ImGui.Spacing();
			
			if (assetCache == null || assetCache.Count == 0)
			{
				ImGui.Text("Aucune donnée dans le cache.");
				return;
			}
			
			if (cacheEmbeddedTab)
			{
				foreach (var (key, value) in assetCache)
				{
					if (ImGui.Selectable(key, selectedAsset.path == key))
						selectedAsset = (key, value.asset);
				}
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
			ImGui.TreePop();
		}
		
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
							selectedAsset = (asset.Key, asset.Value.asset);
						break;
					}
				}
		}
		
		void DrawAssetInspector()
		{
			if (selectedAsset.asset == null)
				return;
			
			ImGui.TextColored(new Vector4(0, 1, 0, 0.9F), "Asset Inspector");
			ImGui.PushStyleColor(ImGuiCol.Text, 0xFF4A4B50);
			ImGui.Text(selectedAsset.asset.GetType().Name);
			ImGui.PopStyleColor();
			if (!cacheEmbeddedTab && XYDebug.state == DebugState.Full && ImGui.SmallButton("Hot Reload"))
				Utility.JustDoIt(() => AssetManager.ReloadAsset(selectedAsset.path, Path.Combine(DebugHotReload.assetsSourcePath, selectedAsset.path)));
			ImGui.Separator();
			ImGui.Spacing();
			
			var type = selectedAsset.asset.GetType();
			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			
			foreach (var property in properties)
			{
				if (!property.CanRead || !property.CanWrite || property.GetMethod.GetParameters().Length > 0)
					continue;
				
				ImGui.AlignTextToFramePadding();
				ImGui.Text($"{property.Name} {property.GetValue(selectedAsset.asset)}");
			}
		}
	}
}
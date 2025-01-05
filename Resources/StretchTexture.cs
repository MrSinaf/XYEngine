using System.Text.Json;

namespace XYEngine.Resources;

public class StretchTexture : IAsset
{
	public Texture2D texture { get; private set; }
	public RectInt area { get; private set; }
	public RegionInt corner { get; private set; }
	public int scale { get; private set; } = 1;
	
	public Region cornerUV00 { get; private set; }
	public Region cornerUV11 { get; private set; }
	
	public StretchTexture() { }
	
	public StretchTexture(Texture2D texture, RegionInt corner, int scale = 1) : this(texture, corner, new RectInt(Vector2Int.zero, texture.size), scale) { }
	
	public StretchTexture(Texture2D texture, RegionInt corner, RectInt area, int scale = 1)
	{
		this.texture = texture;
		this.corner = corner;
		this.area = area;
		this.scale = scale;
		
		UpdateUV();
	}
	
	private void UpdateUV()
	{
		cornerUV00 = new Region(area.position * texture.texel, (area.position + corner.position00) * texture.texel);
		cornerUV11 = new Region((area.position + area.size - corner.position11) * texture.texel, (area.position + area.size) * texture.texel);
	}
	
	
	void IAsset.Load(AssetProperty property)
	{
		if (property.extension == ".uxy")
		{
			var json = JsonDocument.Parse(property.stream);
			if (json.RootElement.GetProperty("type").GetString() == nameof(StretchTexture))
			{
				var data = json.RootElement.GetProperty("data");
				texture = AssetManager.LoadAsset<Texture2D>(data.GetProperty(nameof(texture)).GetString(), acceptGet: true);
				corner = data.GetProperty(nameof(corner)).Deserialize<RegionInt>();
				area = new RectInt(Vector2Int.zero, texture.size);
				
				if (data.TryGetProperty(nameof(scale), out var element))
					scale = element.GetInt32();
				
				UpdateUV();
			}
		}
	}
	
	void IAsset.UnLoad() => texture = null;
}
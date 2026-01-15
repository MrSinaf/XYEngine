using Silk.NET.OpenGL;
using XYEngine.Debugs;
using XYEngine.Rendering;
using static XYEngine.Graphics;

namespace XYEngine.Resources;

public class RenderTexture : Texture, IDebugProperty
{
	public Vector2Int size { get; private set; }
	public uint width { get; set; }
	public uint height { get; set; }
	public Vector2 texel { get; private set; }
	
	private readonly uint frameBufferHandle;
	private readonly uint rbHandle;
	
	public RenderTexture(uint width, uint height, Texture2DConfig config = null)
	{
		this.width = width;
		this.height = height;
		size = new Vector2Int((int)width, (int)height);
		texel = Vector2.one / size;
		
		gTexture = new GTexture();
		gTexture.SetImage2D(this.width, this.height, new Color[width * height]);
		config ??= Texture2D.internalConfig;
		SetFilter((TextureMin)config.filter, config.filter);
		SetWrap(config.wrap);
		
		frameBufferHandle = gl.GenFramebuffer();
		gl.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferHandle);
		gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
								TextureTarget.Texture2D, gTexture.handle, 0);
		
		rbHandle = gl.GenRenderbuffer();
		gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbHandle);
		gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, GLEnum.DepthComponent, this.width, this.height);
		gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
								   RenderbufferTarget.Renderbuffer, rbHandle);
		
		if (gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
			throw new Exception("Framebuffer incomplete!");
		
		gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
	}
	
	public void Bind()
	{
		gl.BindFramebuffer(FramebufferTarget.Framebuffer, frameBufferHandle);
		gl.Viewport(0, 0, width, height);
		gl.ClearColor(0, 0, 0, 0);
		gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
	}
	
	public void Unbind()
	{
		gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
		gl.Viewport(0, 0, (uint)resolution.x, (uint)resolution.y);
	}
	
	public void Dispose() => MainThreadQueue.EnqueueRenderer(() =>
	{
		gl.DeleteFramebuffer(frameBufferHandle);
		gl.DeleteRenderbuffer(rbHandle);
		gTexture.Dispose();
	});
	
	public void OnDebugPropertyRender()
	{
		XYDebug.ShowValue("Size", $"{size.x}x{size.y}");
		ImGui.Spacing();
		var ratio = (float)height / width;
		var availableSpace = ImGui.GetContentRegionAvail().x;
		var drawList = ImGui.GetWindowDrawList();
		drawList.AddRectFilled(ImGui.GetCursorScreenPos(),
							   ImGui.GetCursorScreenPos() + new Vector2(availableSpace, availableSpace * ratio),
							   ImGui.GetColorU32(new Vector4(0, 0, 0, 1)));
		ImGui.Image((IntPtr)gTexture.handle, new Vector2(availableSpace, availableSpace * ratio), new Vector2(0, 1),
					new Vector2(1, 0));
	}
}
using Silk.NET.OpenGL;

namespace XYEngine.Rendering;

public enum TextureWrap
{
	Repeat = GLEnum.Repeat, ClampToEdge = GLEnum.ClampToEdge, MirroredRepeat = GLEnum.MirroredRepeat,
	ClampToBorder = GLEnum.ClampToBorder
}
using Silk.NET.OpenGL;

namespace XYEngine.Rendering;

public enum TextureMin
{
	Nearest = GLEnum.Nearest, Linear = GLEnum.Linear, NearestMipmapNearest = GLEnum.NearestMipmapNearest,
	LinearMipmapNearest = GLEnum.LinearMipmapNearest, NearestMipmapLinear = GLEnum.NearestMipmapLinear,
	LinearMipmapLinear = GLEnum.NearestMipmapLinear
}
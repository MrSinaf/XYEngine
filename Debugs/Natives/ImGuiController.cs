using System.Drawing;
using System.Runtime.InteropServices;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using XYEngine.Inputs;
using XYEngine.Rendering;
using XYEngine.Resources;
using Key = Silk.NET.Input.Key;
using MouseButton = Silk.NET.Input.MouseButton;
using Shader = XYEngine.Resources.Shader;

namespace XYEngine.Debugs;

public class ImGuiController
{
	private readonly GL gl;
	private readonly IView view;
	
	private Material material;
	private Texture2D fontTexture;
	private bool frameBegun;
	
	private uint vboHandle;
	private uint elementsHandle;
	private uint vertexArrayObject;
	
	public ImGuiController()
	{
		gl = Graphics.gl;
		view = GameWindow.GetWindow();
		
		ImGui.SetCurrentContext(ImGui.CreateContext());
		ImGui.StyleColorsDark();
		
		var io = ImGui.GetIO();
		io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
		
		CreateDeviceResources();
		SetPerFrameImGuiData(1f / 60f);
		
		ImGui.NewFrame();
		frameBegun = true;
		
		var keyboard = Input.context.Keyboards[0];
		keyboard.KeyDown += OnKeyDown;
		keyboard.KeyUp += OnKeyUp;
		Input.charDown += OnKeyChar;
	}
	
	internal void Render()
	{
		if (frameBegun)
		{
			frameBegun = false;
			ImGui.Render();
			RenderImDrawData(ImGui.GetDrawData());
		}
	}
	
	public void Update(float deltaSeconds)
	{
		if (frameBegun)
			ImGui.Render();
		
		SetPerFrameImGuiData(deltaSeconds);
		UpdateImGuiInput();
		frameBegun = true;
		ImGui.NewFrame();
	}
	
	private unsafe void RenderImDrawData(ImDrawDataPtr drawDataPtr)
	{
		var framebufferWidth = (int)(drawDataPtr.DisplaySize.x * drawDataPtr.FramebufferScale.x);
		var framebufferHeight = (int)(drawDataPtr.DisplaySize.y * drawDataPtr.FramebufferScale.y);
		if (framebufferWidth <= 0 || framebufferHeight <= 0)
			return;
		
		gl.GetInteger(GLEnum.ActiveTexture, out var lastActiveTexture);
		gl.ActiveTexture(GLEnum.Texture0);
		
		gl.GetInteger(GLEnum.CurrentProgram, out var lastProgram);
		gl.GetInteger(GLEnum.TextureBinding2D, out var lastTexture);
		
		gl.GetInteger(GLEnum.SamplerBinding, out var lastSampler);
		
		gl.GetInteger(GLEnum.ArrayBufferBinding, out var lastArrayBuffer);
		gl.GetInteger(GLEnum.VertexArrayBinding, out var lastVertexArrayObject);
		
		Span<int> lastPolygonMode = stackalloc int[2];
		gl.GetInteger(GLEnum.PolygonMode, lastPolygonMode);
		
		Span<int> lastScissorBox = stackalloc int[4];
		gl.GetInteger(GLEnum.ScissorBox, lastScissorBox);
		
		gl.GetInteger(GLEnum.BlendSrcRgb, out var lastBlendSrcRgb);
		gl.GetInteger(GLEnum.BlendDstRgb, out var lastBlendDstRgb);
		
		gl.GetInteger(GLEnum.BlendSrcAlpha, out var lastBlendSrcAlpha);
		gl.GetInteger(GLEnum.BlendDstAlpha, out var lastBlendDstAlpha);
		
		gl.GetInteger(GLEnum.BlendEquationRgb, out var lastBlendEquationRgb);
		gl.GetInteger(GLEnum.BlendEquationAlpha, out var lastBlendEquationAlpha);
		
		var lastEnableBlend = gl.IsEnabled(GLEnum.Blend);
		var lastEnableCullFace = gl.IsEnabled(GLEnum.CullFace);
		var lastEnableDepthTest = gl.IsEnabled(GLEnum.DepthTest);
		var lastEnableStencilTest = gl.IsEnabled(GLEnum.StencilTest);
		var lastEnableScissorTest = gl.IsEnabled(GLEnum.ScissorTest);
		
		SetupRenderState(drawDataPtr);
		
		var clipOff = drawDataPtr.DisplayPos;
		var clipScale = drawDataPtr.FramebufferScale;
		
		for (var n = 0; n < drawDataPtr.CmdListsCount; n++)
		{
			var cmdListPtr = drawDataPtr.CmdLists[n];
			
			gl.BufferData(GLEnum.ArrayBuffer, (nuint)(cmdListPtr.VtxBuffer.Size * sizeof(ImDrawVert)),
						  (void*)cmdListPtr.VtxBuffer.Data, GLEnum.StreamDraw);
			gl.BufferData(GLEnum.ElementArrayBuffer, (nuint)(cmdListPtr.IdxBuffer.Size * sizeof(ushort)),
						  (void*)cmdListPtr.IdxBuffer.Data, GLEnum.StreamDraw);
			
			for (var cmdI = 0; cmdI < cmdListPtr.CmdBuffer.Size; cmdI++)
			{
				var cmdPtr = cmdListPtr.CmdBuffer[cmdI];
				
				if (cmdPtr.UserCallback != IntPtr.Zero)
					throw new NotSupportedException();
				
				var clipRect = new Vector4
				{
					x = (cmdPtr.ClipRect.x - clipOff.x) * clipScale.x,
					y = (cmdPtr.ClipRect.y - clipOff.y) * clipScale.y,
					z = (cmdPtr.ClipRect.z - clipOff.x) * clipScale.x,
					w = (cmdPtr.ClipRect.w - clipOff.y) * clipScale.y
				};
				
				if (clipRect.x < framebufferWidth && clipRect.y < framebufferHeight &&
					clipRect is { z: >= 0.0f, w: >= 0.0f })
				{
					gl.Scissor((int)clipRect.x, (int)(framebufferHeight - clipRect.w), (uint)(clipRect.z - clipRect.x),
							   (uint)(clipRect.w - clipRect.y));
					gl.BindTexture(GLEnum.Texture2D, (uint)cmdPtr.TextureId);
					gl.DrawElementsBaseVertex(GLEnum.Triangles, cmdPtr.ElemCount, GLEnum.UnsignedShort,
											  (void*)(cmdPtr.IdxOffset * sizeof(ushort)), (int)cmdPtr.VtxOffset);
				}
			}
		}
		
		gl.DeleteVertexArray(vertexArrayObject);
		vertexArrayObject = 0;
		
		gl.UseProgram((uint)lastProgram);
		gl.BindTexture(GLEnum.Texture2D, (uint)lastTexture);
		
		gl.BindSampler(0, (uint)lastSampler);
		
		gl.ActiveTexture((GLEnum)lastActiveTexture);
		
		gl.BindVertexArray((uint)lastVertexArrayObject);
		
		gl.BindBuffer(GLEnum.ArrayBuffer, (uint)lastArrayBuffer);
		gl.BlendEquationSeparate((GLEnum)lastBlendEquationRgb, (GLEnum)lastBlendEquationAlpha);
		gl.BlendFuncSeparate((GLEnum)lastBlendSrcRgb, (GLEnum)lastBlendDstRgb, (GLEnum)lastBlendSrcAlpha,
							 (GLEnum)lastBlendDstAlpha);
		
		var lastEnablePrimitiveRestart = gl.IsEnabled(GLEnum.PrimitiveRestart);
		
		if (lastEnableBlend)
			gl.Enable(GLEnum.Blend);
		else
			gl.Disable(GLEnum.Blend);
		
		if (lastEnableCullFace)
			gl.Enable(GLEnum.CullFace);
		else
			gl.Disable(GLEnum.CullFace);
		
		if (lastEnableDepthTest)
			gl.Enable(GLEnum.DepthTest);
		else
			gl.Disable(GLEnum.DepthTest);
		
		if (lastEnableStencilTest)
			gl.Enable(GLEnum.StencilTest);
		else
			gl.Disable(GLEnum.StencilTest);
		
		if (lastEnableScissorTest)
			gl.Enable(GLEnum.ScissorTest);
		else
			gl.Disable(GLEnum.ScissorTest);
		
		if (lastEnablePrimitiveRestart)
			gl.Enable(GLEnum.PrimitiveRestart);
		else
			gl.Disable(GLEnum.PrimitiveRestart);
		
		gl.PolygonMode(GLEnum.FrontAndBack, (GLEnum)lastPolygonMode[0]);
		gl.Scissor(lastScissorBox[0], lastScissorBox[1], (uint)lastScissorBox[2], (uint)lastScissorBox[3]);
	}
	
	private unsafe void SetupRenderState(ImDrawDataPtr drawDataPtr)
	{
		gl.Enable(GLEnum.Blend);
		gl.BlendEquation(GLEnum.FuncAdd);
		gl.BlendFuncSeparate(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha, GLEnum.One, GLEnum.OneMinusSrcAlpha);
		gl.Disable(GLEnum.CullFace);
		gl.Disable(GLEnum.DepthTest);
		gl.Disable(GLEnum.StencilTest);
		gl.Enable(GLEnum.ScissorTest);
		gl.Disable(GLEnum.PrimitiveRestart);
		gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
		
		var left = drawDataPtr.DisplayPos.x;
		var right = drawDataPtr.DisplayPos.x + drawDataPtr.DisplaySize.x;
		var top = drawDataPtr.DisplayPos.y;
		var bottom = drawDataPtr.DisplayPos.y + drawDataPtr.DisplaySize.y;
		
		Span<float> orthoProjection =
		[
			2.0f / (right - left), 0.0f, 0.0f, 0.0f,
			0.0f, 2.0f / (top - bottom), 0.0f, 0.0f,
			0.0f, 0.0f, -1.0f, 0.0f,
			(right + left) / (left - right), (top + bottom) / (bottom - top), 0.0f, 1.0f
		];
		
		material.shader.gProgram.SetUniform("Texture", 0);
		material.shader.gProgram.GetUniformLocation("ProjMtx", out var locationProjMtx);
		gl.UniformMatrix4(locationProjMtx, 1, false, orthoProjection);
		
		gl.BindSampler(0, 0);
		
		vertexArrayObject = gl.GenVertexArray();
		gl.BindVertexArray(vertexArrayObject);
		gl.BindBuffer(GLEnum.ArrayBuffer, vboHandle);
		gl.BindBuffer(GLEnum.ElementArrayBuffer, elementsHandle);
		
		var attribLocationVtxPos = gl.GetAttribLocation(material.shader.gProgram.handle, "Position");
		var attribLocationVtxUV = gl.GetAttribLocation(material.shader.gProgram.handle, "UV");
		var attribLocationVtxColor = gl.GetAttribLocation(material.shader.gProgram.handle, "Color");
		
		gl.EnableVertexAttribArray((uint)attribLocationVtxPos);
		gl.EnableVertexAttribArray((uint)attribLocationVtxUV);
		gl.EnableVertexAttribArray((uint)attribLocationVtxColor);
		gl.VertexAttribPointer((uint)attribLocationVtxPos, 2, GLEnum.Float, false, (uint)sizeof(ImDrawVert), (void*)0);
		gl.VertexAttribPointer((uint)attribLocationVtxUV, 2, GLEnum.Float, false, (uint)sizeof(ImDrawVert), (void*)8);
		gl.VertexAttribPointer((uint)attribLocationVtxColor, 4, GLEnum.UnsignedByte, true, (uint)sizeof(ImDrawVert),
							   (void*)16);
	}
	
	private static void OnKeyDown(IKeyboard keyboard, Key keycode, int scancode) => OnKeyEvent(keycode, scancode, true);
	private static void OnKeyUp(IKeyboard keyboard, Key keycode, int scancode) => OnKeyEvent(keycode, scancode, false);
	private static void OnKeyChar(char c) => ImGui.GetIO().AddInputCharacter(c);
	
	private static void OnKeyEvent(Key keyCode, int scancode, bool down)
	{
		var io = ImGui.GetIO();
		var imGuiKey = TranslateInputKeyToImGuiKey(keyCode);
		io.AddKeyEvent(imGuiKey, down);
		io.SetKeyEventNativeData(imGuiKey, (int)keyCode, scancode);
	}
	
	private static void UpdateImGuiInput()
	{
		var io = ImGui.GetIO();
		
		var mouseState = Input.context.Mice[0];
		
		io.MouseDown[0] = mouseState.IsButtonPressed(MouseButton.Left);
		io.MouseDown[1] = mouseState.IsButtonPressed(MouseButton.Right);
		io.MouseDown[2] = mouseState.IsButtonPressed(MouseButton.Middle);
		
		var point = new Point((int)mouseState.Position.X, (int)mouseState.Position.Y);
		io.MousePos = new Vector2(point.X, point.Y);
		
		var wheel = mouseState.ScrollWheels[0];
		io.MouseWheel = wheel.Y;
		io.MouseWheelH = wheel.X;
		
		var keyboard = Input.context.Keyboards[0];
		io.KeyCtrl = keyboard.IsKeyPressed(Key.ControlLeft) || keyboard.IsKeyPressed(Key.ControlRight);
		io.KeyAlt = keyboard.IsKeyPressed(Key.AltLeft) || keyboard.IsKeyPressed(Key.AltRight);
		io.KeyShift = keyboard.IsKeyPressed(Key.ShiftLeft) || keyboard.IsKeyPressed(Key.ShiftRight);
		io.KeySuper = keyboard.IsKeyPressed(Key.SuperLeft) || keyboard.IsKeyPressed(Key.SuperRight);
	}
	
	private void CreateDeviceResources()
	{
		const string vertexSource =
			"""
			layout (location = 0) in vec2 Position;
			layout (location = 1) in vec2 UV;
			layout (location = 2) in vec4 Color;
			uniform mat4 ProjMtx;
			out vec2 Frag_UV;
			out vec4 Frag_Color;
			void main()
			{
			    Frag_UV = UV;
			    Frag_Color = Color;
			    gl_Position = ProjMtx * vec4(Position.xy,0,1);
			}
			""";
		
		const string fragmentSource =
			"""
			in vec2 Frag_UV;
			in vec4 Frag_Color;
			uniform sampler2D Texture;
			layout (location = 0) out vec4 Out_Color;
			void main()
			{
			    Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
			}
			""";
		
		material = new Material(new Shader(vertexSource, fragmentSource));
		
		vboHandle = gl.GenBuffer();
		elementsHandle = gl.GenBuffer();
		
		var io = ImGui.GetIO();
		io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out var width, out var height, out var bytesPerPixel);
		
		var data = new byte[width * height * bytesPerPixel];
		Marshal.Copy(pixels, data, 0, width * height * bytesPerPixel);
		
		var colors = new Color[width * height];
		for (var i = 0; i < width * height; i++)
		{
			var baseIndex = i * bytesPerPixel;
			colors[i] = new Color(data[baseIndex], data[baseIndex + 1], data[baseIndex + 2], data[baseIndex + 3]);
		}
		
		fontTexture = new Texture2D(width, height, colors);
		fontTexture.SetFilter(TextureMin.Linear, TextureMag.Linear);
		
		MainThreadQueue.EnqueueRenderer(() => io.Fonts.SetTexID((IntPtr)fontTexture.gTexture.handle));
	}
	
	private void SetPerFrameImGuiData(float deltaSeconds)
	{
		var io = ImGui.GetIO();
		var resolution = Graphics.resolution;
		io.DisplaySize = resolution;
		
		if (resolution is { x: > 0, y: > 0 })
			io.DisplayFramebufferScale = new Vector2((float)view.FramebufferSize.X / resolution.x,
													 (float)view.FramebufferSize.Y / resolution.y);
		
		io.DeltaTime = deltaSeconds;
	}
	
	private static ImGuiKey TranslateInputKeyToImGuiKey(Key key) => key switch
	{
		Key.Tab            => ImGuiKey.Tab,
		Key.Left           => ImGuiKey.LeftArrow,
		Key.Right          => ImGuiKey.RightArrow,
		Key.Up             => ImGuiKey.UpArrow,
		Key.Down           => ImGuiKey.DownArrow,
		Key.PageUp         => ImGuiKey.PageUp,
		Key.PageDown       => ImGuiKey.PageDown,
		Key.Home           => ImGuiKey.Home,
		Key.End            => ImGuiKey.End,
		Key.Insert         => ImGuiKey.Insert,
		Key.Delete         => ImGuiKey.Delete,
		Key.Backspace      => ImGuiKey.Backspace,
		Key.Space          => ImGuiKey.Space,
		Key.Enter          => ImGuiKey.Enter,
		Key.Escape         => ImGuiKey.Escape,
		Key.Apostrophe     => ImGuiKey.Apostrophe,
		Key.Comma          => ImGuiKey.Comma,
		Key.Minus          => ImGuiKey.Minus,
		Key.Period         => ImGuiKey.Period,
		Key.Slash          => ImGuiKey.Slash,
		Key.Semicolon      => ImGuiKey.Semicolon,
		Key.Equal          => ImGuiKey.Equal,
		Key.LeftBracket    => ImGuiKey.LeftBracket,
		Key.BackSlash      => ImGuiKey.Backslash,
		Key.RightBracket   => ImGuiKey.RightBracket,
		Key.GraveAccent    => ImGuiKey.GraveAccent,
		Key.CapsLock       => ImGuiKey.CapsLock,
		Key.ScrollLock     => ImGuiKey.ScrollLock,
		Key.NumLock        => ImGuiKey.NumLock,
		Key.PrintScreen    => ImGuiKey.PrintScreen,
		Key.Pause          => ImGuiKey.Pause,
		Key.Keypad0        => ImGuiKey.Keypad0,
		Key.Keypad1        => ImGuiKey.Keypad1,
		Key.Keypad2        => ImGuiKey.Keypad2,
		Key.Keypad3        => ImGuiKey.Keypad3,
		Key.Keypad4        => ImGuiKey.Keypad4,
		Key.Keypad5        => ImGuiKey.Keypad5,
		Key.Keypad6        => ImGuiKey.Keypad6,
		Key.Keypad7        => ImGuiKey.Keypad7,
		Key.Keypad8        => ImGuiKey.Keypad8,
		Key.Keypad9        => ImGuiKey.Keypad9,
		Key.KeypadDecimal  => ImGuiKey.KeypadDecimal,
		Key.KeypadDivide   => ImGuiKey.KeypadDivide,
		Key.KeypadMultiply => ImGuiKey.KeypadMultiply,
		Key.KeypadSubtract => ImGuiKey.KeypadSubtract,
		Key.KeypadAdd      => ImGuiKey.KeypadAdd,
		Key.KeypadEnter    => ImGuiKey.KeypadEnter,
		Key.KeypadEqual    => ImGuiKey.KeypadEqual,
		Key.ShiftLeft      => ImGuiKey.LeftShift,
		Key.ControlLeft    => ImGuiKey.LeftCtrl,
		Key.AltLeft        => ImGuiKey.LeftAlt,
		Key.SuperLeft      => ImGuiKey.LeftSuper,
		Key.ShiftRight     => ImGuiKey.RightShift,
		Key.ControlRight   => ImGuiKey.RightCtrl,
		Key.AltRight       => ImGuiKey.RightAlt,
		Key.SuperRight     => ImGuiKey.RightSuper,
		Key.Menu           => ImGuiKey.Menu,
		Key.Number0        => ImGuiKey._0,
		Key.Number1        => ImGuiKey._1,
		Key.Number2        => ImGuiKey._2,
		Key.Number3        => ImGuiKey._3,
		Key.Number4        => ImGuiKey._4,
		Key.Number5        => ImGuiKey._5,
		Key.Number6        => ImGuiKey._6,
		Key.Number7        => ImGuiKey._7,
		Key.Number8        => ImGuiKey._8,
		Key.Number9        => ImGuiKey._9,
		Key.A              => ImGuiKey.A,
		Key.B              => ImGuiKey.B,
		Key.C              => ImGuiKey.C,
		Key.D              => ImGuiKey.D,
		Key.E              => ImGuiKey.E,
		Key.F              => ImGuiKey.F,
		Key.G              => ImGuiKey.G,
		Key.H              => ImGuiKey.H,
		Key.I              => ImGuiKey.I,
		Key.J              => ImGuiKey.J,
		Key.K              => ImGuiKey.K,
		Key.L              => ImGuiKey.L,
		Key.M              => ImGuiKey.M,
		Key.N              => ImGuiKey.N,
		Key.O              => ImGuiKey.O,
		Key.P              => ImGuiKey.P,
		Key.Q              => ImGuiKey.Q,
		Key.R              => ImGuiKey.R,
		Key.S              => ImGuiKey.S,
		Key.T              => ImGuiKey.T,
		Key.U              => ImGuiKey.U,
		Key.V              => ImGuiKey.V,
		Key.W              => ImGuiKey.W,
		Key.X              => ImGuiKey.X,
		Key.Y              => ImGuiKey.Y,
		Key.Z              => ImGuiKey.Z,
		Key.F1             => ImGuiKey.F1,
		Key.F2             => ImGuiKey.F2,
		Key.F3             => ImGuiKey.F3,
		Key.F4             => ImGuiKey.F4,
		Key.F5             => ImGuiKey.F5,
		Key.F6             => ImGuiKey.F6,
		Key.F7             => ImGuiKey.F7,
		Key.F8             => ImGuiKey.F8,
		Key.F9             => ImGuiKey.F9,
		Key.F10            => ImGuiKey.F10,
		Key.F11            => ImGuiKey.F11,
		Key.F12            => ImGuiKey.F12,
		Key.F13            => ImGuiKey.F13,
		Key.F14            => ImGuiKey.F14,
		Key.F15            => ImGuiKey.F15,
		Key.F16            => ImGuiKey.F16,
		Key.F17            => ImGuiKey.F17,
		Key.F18            => ImGuiKey.F18,
		Key.F19            => ImGuiKey.F19,
		Key.F20            => ImGuiKey.F20,
		Key.F21            => ImGuiKey.F21,
		Key.F22            => ImGuiKey.F22,
		Key.F23            => ImGuiKey.F23,
		Key.F24            => ImGuiKey.F24,
		Key.Unknown        => ImGuiKey.None,
		Key.World1         => ImGuiKey.None,
		Key.World2         => ImGuiKey.None,
		Key.F25            => ImGuiKey.None,
		_                  => throw new NotSupportedException()
	};
}
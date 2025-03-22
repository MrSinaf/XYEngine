using XYEngine.Inputs;
using XYEngine.Resources;
using XYEngine.Utils;

namespace XYEngine.UI.Widgets;

public class TextInput : UIElement
{
	public readonly Label valueLabel;
	public readonly Label placeholderLabel;
	public readonly UIElement caret;
	public readonly UIElement selection;
	
	private CancellationTokenSource cancellationBlinks = new ();
	private CancellationTokenSource cancellationHandle;
	
	public event Action<string> onValueChanged;
	private bool mousePressed;
	
	public int cursorPosition
	{
		get;
		private set
		{
			field = value;
			caret.visible = true;
			caret.position = new Vector2Int(valueLabel.font.CalculTextSize(this.value[..field]), 0);
			selectionEnd = field;
		}
	}
	
	public int selectionEnd
	{
		get;
		private set
		{
			field = value;
			selection.position = caret.position;
			selection.size = new Vector2Int(valueLabel.font.CalculTextSize(this.value[..field]) - caret.position.x, caret.size.y);
		}
	}
	
	public bool focus
	{
		get;
		set
		{
			if (field == value)
				return;
			
			field = value;
			if (field)
			{
				Input.keyDown += OnKeyDown;
				Input.keyUp += OnKeyUp;
				Input.clickUp += OnClickUp;
				Input.mouseMove += OnMouseMove;
				Input.charDown += OnCharDown;
				
				cancellationBlinks = new CancellationTokenSource();
				BlinksCaret(cancellationBlinks.Token);
			}
			else
			{
				cancellationHandle?.Cancel();
				cancellationBlinks?.Cancel();
				caret.visible = false;
				
				Input.keyDown -= OnKeyDown;
				Input.keyUp -= OnKeyUp;
				Input.clickUp -= OnClickUp;
				Input.mouseMove -= OnMouseMove;
				Input.charDown -= OnCharDown;
			}
		}
	}
	
	public string value { get => valueLabel.text; set => onValueChanged(valueLabel.text = value); }
	
	public string placeholder { get => placeholderLabel.text; set => placeholderLabel.text = value; }
	
	public TextInput(string placeholder = "", string value = "", string prefab = null)
	{
		base.AddChild(placeholderLabel = new Label(placeholder));
		base.AddChild(valueLabel = new Label(value));
		valueLabel.AddChild(caret = new UIElement());
		valueLabel.AddChild(selection = new UIElement());
		
		UIPrefab.Apply(this, prefab);
		caret.visible = false;
		
		Input.clickDown += OnClickDown;
		onValueChanged = _ => UpdateDisplayText();
	}
	
	protected override void OnDestroy()
	{
		Input.clickDown -= OnClickDown;
		focus = false;
	}
	
	private void OnClickDown(MouseButton button)
	{
		if (button != MouseButton.Left || !(focus = ContainsPoint(Input.mousePosition)))
			return;
		
		mousePressed = true;
		cursorPosition = valueLabel.font.GetCharInPositionText(value, Input.mousePosition.x - valueLabel.realPosition.x);
		selectionEnd = cursorPosition;
	}
	
	private void OnClickUp(MouseButton button)
	{
		if (button == MouseButton.Left)
			mousePressed = false;
	}
	
	private void OnMouseMove(Vector2 position)
	{
		if (!mousePressed)
			return;
		
		selectionEnd = valueLabel.font.GetCharInPositionText(value, Input.mousePosition.x - valueLabel.realPosition.x);
	}
	
	private void OnCharDown(char c)
	{
		value = value.Insert(cursorPosition, c.ToString());
		cursorPosition++;
	}
	
	private void OnKeyDown(Key key)
	{
		switch (key)
		{
			case Key.Backspace:
				cancellationHandle?.Cancel();
				cancellationHandle = new CancellationTokenSource();
				BackspaceKeyHoldAsync(cancellationHandle.Token, () =>
				{
					if (cursorPosition == selectionEnd)
					{
						value = value.Remove(cursorPosition - 1, 1);
						if (cursorPosition > 0)
							cursorPosition--;
					}
					else
					{
						var min = int.Min(cursorPosition, selectionEnd);
						value = value.Remove(min, int.Max(cursorPosition, selectionEnd) - min);
						cursorPosition = min;
					}
				}, null);
				break;
			case Key.Delete:
				if (cursorPosition < value.Length)
					value = value.Remove(cursorPosition, 1);
				break;
			case Key.Enter:
				// value += '\n';
				break;
			case Key.Left:
				cancellationHandle?.Cancel();
				cancellationHandle = new CancellationTokenSource();
				BackspaceKeyHoldAsync(cancellationHandle.Token, () => cursorPosition--, () =>
				{
					if (cursorPosition < 0)
						cursorPosition = 0;
				});
				break;
			case Key.Right:
				cancellationHandle?.Cancel();
				cancellationHandle = new CancellationTokenSource();
				BackspaceKeyHoldAsync(cancellationHandle.Token, () => cursorPosition++, () =>
				{
					if (cursorPosition > value.Length - 1)
						cursorPosition = value.Length;
				});
				break;
		}
		
		selectionEnd = cursorPosition;
	}
	
	private void OnKeyUp(Key key)
	{
		cancellationHandle?.Cancel();
		cancellationHandle = null;
	}
	
	private void UpdateDisplayText()
	{
		if (string.IsNullOrEmpty(value))
		{
			valueLabel.visible = false;
			placeholderLabel.visible = true;
		}
		else
		{
			valueLabel.visible = true;
			placeholderLabel.visible = false;
		}
	}
	
	private async void BlinksCaret(CancellationToken token)
	{
		try
		{
			while (!token.IsCancellationRequested)
			{
				await Task.Delay(500, token);
				caret.visible = true;
				await Task.Delay(500, token);
				caret.visible = false;
			}
		}
		catch { }
	}
	
	private async void BackspaceKeyHoldAsync(CancellationToken token, Action action, Action final)
	{
		try
		{
			action();
			await Task.Delay(TimeSpan.FromSeconds(0.35f), token);
			
			while (!token.IsCancellationRequested)
			{
				action();
				await Task.Delay(TimeSpan.FromSeconds(0.022f), token);
			}
		}
		catch { }
		finally
		{
			final?.Invoke();
		}
	}
	
	[IsDefaultPrefab]
	public static void DefaultPrefab(TextInput e)
	{
		e.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		e.material = new Material(Shader.GetDefaultUI(), ("mainTex", AssetManager.GetEmbeddedAsset<Texture2D>("textures.white_pixel.png")));
		e.size = new Vector2Int(200, 30);
		e.tint = new Color(100, 30, 45);
		
		e.placeholderLabel.tint = Color.grey;
		e.placeholderLabel.pivotAndAnchors = new Vector2(0, 0.5F);
		
		e.valueLabel.tint = Color.white;
		e.valueLabel.pivotAndAnchors = new Vector2(0, 0.5F);
		
		e.caret.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		e.caret.material = new Material(Shader.GetDefaultUI(), ("mainTex", AssetManager.GetEmbeddedAsset<Texture2D>("textures.white_pixel.png")));
		e.caret.size = new Vector2Int(2, 0);
		e.caret.anchorMin = Vector2.zero;
		e.caret.anchorMax = Vector2.top;
		
		e.selection.mesh = MeshFactory.CreateQuad(Vector2.one).Apply();
		e.selection.material = new Material(Shader.GetDefaultUI(), ("mainTex", AssetManager.GetEmbeddedAsset<Texture2D>("textures.white_pixel.png")));
		e.selection.anchorMin = Vector2.zero;
		e.selection.anchorMax = Vector2.top;
		e.selection.opacity = 0.2F;
	}
}
﻿using Silk.NET.Maths;
using XYEngine.Graphics;

namespace XYEngine.UI;

public class UIElement
{
    public readonly Render render = new ();
    public UIElement parent { get; private set; }

    protected bool scaleWithSize { get; init; } = true;
    protected Vector2Int realPosition { get; private set; }

    private readonly List<UIElement> children = [];
    private bool needingUpdate;
    private bool needingUpdateScaledSize;

    private Matrix4X4<float> _matrix = Matrix4X4<float>.Identity;
    private Vector2Int _position;
    private Vector2Int _size;
    private Vector2 _scale = Vector2.one;
    private Vector2Int _scaledSize;
    private int _rotation;
    private Color _tint = Color.white;
    private float _alpha = 1;

    private RectInt _padding;

    private Vector2 _pivot;
    private Vector2 _anchorMin;
    private Vector2 _anchorMax;
    private bool _active = true;
    private bool _parentActive = true;

    /// <summary>
    /// Donne un tableau des enfants.
    /// </summary>
    public UIElement[] childrenArray => children.ToArray();

    /// <summary>
    /// Donne le nombre d'enfants.
    /// </summary>
    public int nChild => children.Count;

    #region GetSet Public

    public virtual bool active
    {
        get => _active && _parentActive;
        set
        {
            _active = value;
            foreach (var child in children)
                child.parentActive = value;
        }
    }

    private bool parentActive
    {
        set
        {
            _parentActive = value;
            foreach (var child in children)
                child.parentActive = value;
        }
    }

    public virtual Vector2Int position
    {
        get => _position;
        set
        {
            _position = value;
            MarkAsNeedingUpdate();
        }
    }

    public virtual RectInt padding
    {
        get => _padding;
        set
        {
            _padding = value;
            MarkAsNeedingUpdate();
        }
    }

    public virtual Vector2Int size
    {
        get => _size;
        set
        {
            if (_size == value)
                return;

            _size = value;
            needingUpdateScaledSize = true;
            MarkAsNeedingUpdate();
        }
    }

    public virtual Vector2 scale
    {
        get => _scale;
        set
        {
            if (_scale == value)
                return;

            _scale = value;
            needingUpdateScaledSize = true;
            MarkAsNeedingUpdate();
        }
    }

    public virtual int rotation
    {
        get => _rotation;
        set
        {
            if (_rotation == value)
                return;

            _rotation = value;
            MarkAsNeedingUpdate();
        }
    }

    public virtual Color tint
    {
        get => _tint;
        set
        {
            if (_tint == value)
                return;

            _tint = value;
            MarkAsNeedingUpdate();
        }
    }

    public virtual float alpha
    {
        get => _alpha;
        set
        {
            _alpha = value;
            MarkAsNeedingUpdate();
        }
    }

    public virtual Vector2 pivot
    {
        get => _pivot;
        set
        {
            if (_pivot == value)
                return;

            _pivot = value;
            MarkAsNeedingUpdate();
        }
    }

    public Vector2 pivotAndAnchors
    {
        set
        {
            pivot = value;
            anchorMin = value;
            anchorMax = value;
        }
    }

    public Vector2 anchors
    {
        set
        {
            anchorMin = value;
            anchorMax = value;
        }
    }

    public virtual Vector2 anchorMin
    {
        get => _anchorMin;
        set
        {
            if (_anchorMin == value)
                return;

            _anchorMin = value;
            MarkAsNeedingUpdate();
        }
    }

    public virtual Vector2 anchorMax
    {
        get => _anchorMax;
        set
        {
            if (_anchorMax == value)
                return;

            _anchorMax = value;
            MarkAsNeedingUpdate();
        }
    }

    public virtual Matrix4X4<float> matrix
    {
        get
        {
            if (needingUpdate)
                CalculeMatrix();

            return _matrix;
        }
    }

    public virtual Vector2Int scaledSize
    {
        get
        {
            if (needingUpdateScaledSize)
                _scaledSize = (size * scale).ToVector2Int();

            return _scaledSize;
        }
    }

    #endregion

    /// <summary>
    /// Ajoute un <see cref="UIElement"/> à ses enfants.
    /// </summary>
    /// <param name="element">Enfant à ajouter.</param>
    public virtual void AddChild(UIElement element)
    {
        element.parent = this;
        element.parentActive = active;
        element.MarkAsNeedingUpdate();
        element.OnAdded();

        children.Add(element);
    }

    /// <summary>
    /// Retire un <see cref="UIElement"/> de ses enfants.
    /// </summary>
    /// <param name="element">Enfant à rendre orphelin.</param>
    public virtual void RemoveChild(UIElement element)
    {
        element.parent = null;
        element.OnRemoved();
        children.Remove(element);
    }

    /// <summary>
    /// Appel <see cref="RemoveChild"/> depuis son parent pour le rendre orphelin.
    /// </summary>
    public void SetOrphan()
    {
        parent?.RemoveChild(this);
    }

    /// <summary>
    /// Permet de détruire le <see cref="UIElement"/>.
    /// </summary>
    /// <remarks>Tous ses enfants seront détruits avec lui.</remarks>
    public void Destroy()
    {
        SetOrphan();

        while (children.Count > 0)
            children[0].Destroy();

        render.Dispose();
    }

    /// <summary>
    /// Déplace l'enfant vers l'avant.
    /// </summary>
    /// <exception cref="NullReferenceException">Si le <see cref="parent"/> est null.</exception>
    public void MoveInFront()
    {
        if (parent == null)
            throw new NullReferenceException("Aucun parent n'a été assigné !");

        parent.children.Remove(this);
        parent.children.Add(this);
    }

    /// <summary>
    /// Déplace l'enfant vers l'arrière.
    /// </summary>
    /// <exception cref="NullReferenceException">Si le <see cref="parent"/> est null.</exception>
    public void MoveInBack()
    {
        if (parent == null)
            throw new NullReferenceException("Aucun parent n'a été assigné !");

        parent.children.Remove(this);
        parent.children.Insert(0, this);
    }

    /// <summary>
    /// Récupère le premier enfant trouvé ayant le type demandé.
    /// </summary>
    /// <param name="recursive">True, s'il faut également effectuer les recherches dans les <see cref="children"/>.</param>
    /// <typeparam name="T">Type d'élément à trouver.</typeparam>
    /// <returns>Renvoi le <see cref="UIElement"/> typé trouvé, null si l'élément n'a pas été trouvé.</returns>
    public T GetChild<T>(bool recursive = false) where T : UIElement
    {
        foreach (var element in children)
        {
            if (element.GetType() == typeof(T))
                return element as T;
        }

        if (recursive)
        {
            if (children.Count == 0)
                return null;

            foreach (var element in children)
                return element.GetChild<T>(true);
        }

        return null;
    }

    /// <summary>
    /// Récupère tous les enfants trouvés ayant le type demandé.
    /// </summary>
    /// <param name="recursive">True, s'il faut également effectuer les recherches dans les <see cref="children"/>.</param>
    /// <typeparam name="T">Type d'élément à trouver.</typeparam>
    /// <returns>Renvoi les <see cref="UIElement"/> typés trouvés.</returns>
    public T[] GetChildren<T>(bool recursive = false) where T : UIElement
    {
        var list = new List<T>();
        foreach (var element in children)
        {
            if (element.GetType() == typeof(T))
                list.Add(element as T);
        }

        if (recursive)
        {
            if (children.Count == 0)
                return null;

            foreach (var element in children)
                list.AddRange(element.GetChildren<T>(true));
        }

        return list.ToArray();
    }

    /// <summary>
    /// Permet de savoir si un point précis se trouve sur l'élément.
    /// </summary>
    /// <param name="point">Position à vérifier.</param>
    /// <returns>True, si le point se trouve à l'intérieur de l'élément, sinon false.</returns>
    public bool ContainsPoint(Vector2Int point)
    {
        point -= realPosition;
        return point >= Vector2Int.zero && point <= _scaledSize;
    }

    /// <summary>
    /// Marque, lui et ses enfants, comme ayant besoin d'une mise à jour.
    /// </summary>
    public void MarkAsNeedingUpdate()
    {
        needingUpdate = true;
        foreach (var child in children)
            child.MarkAsNeedingUpdate();
    }

    /// <summary>
    /// Marque comme n'ayant pas besoin de mise à jour.
    /// </summary>
    public void UnmarkAsNeedingUpdate()
    {
        if (!needingUpdate)
            return;

        needingUpdate = false;
    }

    /// <summary>
    /// Permet d'effectuer une action quand cet élément a été utilisé comme paramètre dans <see cref="AddChild"/>.
    /// </summary>
    protected virtual void OnAdded() { }


    /// <summary>
    /// Permet d'effectuer une action quand cet élément a été utilisé comme paramètre dans <see cref="RemoveChild"/> ou indirectement en faisant <see cref="SetOrphan"/>.
    /// </summary>
    protected virtual void OnRemoved() { }

    protected internal virtual void Draw(Shader shader)
    {
        if (!active)
            return;

        shader.Use();
        shader.SetUniform("model", matrix);
        shader.SetUniform("tintColor", tint);
        shader.SetUniform("alpha", alpha);
        render.Draw(shader);

        foreach (var child in children)
            child.Draw(shader);
    }

    private void CalculeMatrix()
    {
        needingUpdate = false;
        var scaledPivotSize = (pivot * scaledSize).ToVector2Int();

        var calculatePosition = Vector2Int.zero;
        if (anchorMin != anchorMax)
        {
            var anchorSize = new Vector2(MathF.Abs(anchorMin.x - anchorMax.x), MathF.Abs(anchorMin.y - anchorMax.y)) * parent.size;

            if (anchorSize.x == 0) anchorSize.x = scaledSize.x;
            else
            {
                anchorSize.x -= padding.position00.x + padding.position11.x;
                calculatePosition.x += padding.position00.x;
                scaledPivotSize.x = 0;
            }

            if (anchorSize.y == 0) anchorSize.y = scaledSize.y;
            else
            {
                anchorSize.y -= padding.position00.y + padding.position11.y;
                calculatePosition.y += padding.position00.y;
                scaledPivotSize.y = 0;
            }

            size = anchorSize.ToVector2Int();
            UnmarkAsNeedingUpdate();
        }

        realPosition = calculatePosition += position + parent.realPosition - scaledPivotSize + (parent.scaledSize * anchorMin).ToVector2Int();
        _scaledSize = (size * scale).ToVector2Int(NumberOperation.Ceiling);
        var matrixScale = scaleWithSize ? _scaledSize : scale;
        _matrix = Matrix4X4.CreateScale(matrixScale.x, matrixScale.y, 0F) *
                  Matrix4X4.CreateRotationZ(float.DegreesToRadians(rotation)) *
                  Matrix4X4.CreateTranslation(calculatePosition.x, calculatePosition.y, 0F);
    }

    public static UIElement CreateContainer() => new () { anchorMin = Vector2.zero, anchorMax = Vector2.one };
}
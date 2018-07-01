using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class for page items and available spaces.
/// Basically a position and size.
/// </summary>
public abstract class PageSpaceTaker
{
    private Vector2 _origin;
    private Vector2 _size;

    public Vector2 origin { get { return _origin; } }
    public Vector2 size { get { return _size; } }

    public PageSpaceTaker(Vector2 origin, Vector2 size)
    {
        _origin = origin;
        _size = size;
    }

    public PageSpaceTaker(Vector2 size)
    {
        _size = size;
    }

    public void SetOrigin(Vector2 origin)
    {
        _origin = origin;
    }
}

/// <summary>
/// Item that can be placed and displayed on the page.
/// </summary>
public class PageItem : PageSpaceTaker
{
    // Associated game object representing the item.
    public GameObject game_object { get; private set; } 

    public PageItem(GameObject go, Vector2 origin, Vector2 size) : base(origin, size)
    {
        game_object = go;
        game_object.transform.position = origin;
    }

    public PageItem(GameObject go, Vector2 size) : base(size)
    {
        game_object = go;
        game_object.transform.position = origin;
    }

    /// <summary>
    /// Redefine the origin position of the item.
    /// </summary>
    /// <param name="origin"></param>
    public new void SetOrigin(Vector2 origin)
    {
        base.SetOrigin(origin);
        ((RectTransform)game_object.transform).offsetMin = new Vector2(0, 0);
        ((RectTransform)game_object.transform).offsetMax = new Vector2(size.x, size.y);
        ((RectTransform)game_object.transform).anchorMin = Vector2.up;
        ((RectTransform)game_object.transform).anchorMax = Vector2.up;
        ((RectTransform)game_object.transform).anchoredPosition = new Vector2(origin.x, -origin.y);
        ((RectTransform)game_object.transform).localScale = Vector3.one;
    }
}

/// <summary>
/// Represents a free space on page.
/// A free space is a whole rectangle empty of any item or item parts.
/// </summary>
public class AvailableSpace : PageSpaceTaker
{
    public AvailableSpace(Vector2 origin, Vector2 size) : base(origin, size) { }

    /// <summary>
    /// Checks if a given PageSpaceTaker object can fit into the AvailableSpace.
    /// </summary>
    /// <param name="item">The PageSpaceTaker to fit in.</param>
    /// <returns>True if given PageSpaceTaker can fit, False otherwise.</returns>
    public bool CanFit(PageSpaceTaker item)
    {
        return item.size.x <= size.x && item.size.y <= size.y;
    }

    /// <summary>
    /// Checks if a given PageSpaceTaker is contained into the AvailableSpace.
    /// </summary>
    /// <param name="space_taker">PageSpaceTaker to check.</param>
    /// <returns>True if the given parameter is contained into current AvailableSpace.</returns>
    public bool Contains(PageSpaceTaker space_taker)
    {
        if (space_taker.origin.x >= origin.x && space_taker.origin.x + space_taker.size.x <= origin.x + size.x)
            if (space_taker.origin.y >= origin.y && space_taker.origin.y + space_taker.size.y <= origin.y + size.y)
                return true;
        return false;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageLogic : MonoBehaviour
{
    private List<PageItem> _page_items; // list of items currently placed.
    private List<AvailableSpace> _available_spaces; // list of available space definitions.

    [SerializeField]
    private int width; // Page max width.
    [SerializeField]
    private int height;// Page max height.

    private void Awake()
    {
        // Set page desired size.
        ((RectTransform)transform).offsetMin = new Vector2(0, 0);
        ((RectTransform)transform).offsetMax = new Vector2(width, height);
        ((RectTransform)transform).anchoredPosition = new Vector2(0, 0);

        // Init lists.
        _page_items = new List<PageItem>();
        _available_spaces = new List<AvailableSpace>();
        // Sets the whole page as an available space.
        _available_spaces.Add(new AvailableSpace(Vector2.zero, new Vector2(width, height)));
    }

    /// <summary>
    /// Asks Page Logic to create and try to place a new PageItem based on given PageItemProps.
    /// </summary>
    /// <seealso cref="PageItemProps"/>
    /// <param name="props">Properties (basically size) of the new desired item.</param>
    public void AddItem(PageItemProps props)
    {
        // Create the new game object, name it and configure its size, properties, and position.
        GameObject go = new GameObject(props.size.x + "x" + props.size.y);
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(transform);
        Image im = go.AddComponent<Image>();
        im.color = new Color(Random.value, Random.value, Random.value);
        ((RectTransform)go.transform).pivot = Vector2.up;
        ((RectTransform)go.transform).offsetMin = Vector2.zero;
        ((RectTransform)go.transform).offsetMax = new Vector2(props.size.x, props.size.y);
        ((RectTransform)go.transform).anchorMin = Vector2.up;
        ((RectTransform)go.transform).anchorMax = Vector2.up;
        AddItem(new PageItem(go, new Vector2(props.size.x, props.size.y)));
    }

    /// <summary>
    /// Asks Page Logic to try to place a given PageItem.
    /// </summary>
    /// <param name="new_page_item">New page item to place on page.</param>
    public bool AddItem(PageItem new_page_item)
    {
        bool position_found = false;
        // Try to find an AvailableSpace large enough for the new PageItem to fit in it.
        foreach ( AvailableSpace space in _available_spaces )
        {
            if ( space.CanFit(new_page_item) == true )
            {
                // Big enough space has been found.
                // Set PageItem's origin to found AvailableSpace's origin
                new_page_item.SetOrigin(space.origin);
                // Add the freshly placed PageItem to the list of placed items.
                _page_items.Add(new_page_item);
                position_found = true;
                break;
            }
        }

        if ( position_found == true )
        {
            // If item has been placed, recompute available spaces.
            ComputeAvailableSpaces();
        }
        return position_found;
    }

    /// <summary>
    /// Fill _available_spaces variable with a list of AvailableSpace.
    /// Every AvailableSpace object represents a free rectangle space available on the page.
    /// </summary>
    private void ComputeAvailableSpaces()
    {
        // Empty the list.
        _available_spaces.Clear();
        List<AvailableSpace> potential_spaces = new List<AvailableSpace>();
        // Check every position from left to right and bottom to top.
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                // From given origin (x,y) try to grow rectangles and save them into potential_spaces list.
                potential_spaces.AddRange(GrowFromOrigin(new Vector2(x, y)));
            }
        }
        // Merge found potential rectangles to keep only the biggests.
        _available_spaces = MergeAvailableSpaces(potential_spaces);
        // Reorder the list to have bottom left spaces first.
        OrderAvailableSpaces();
    }

    /// <summary>
    /// Grow available rectangles from given origin.
    /// Returns a list of found rectangles.
    /// An available rectangle is a possible rectangle on page that doesn't contain any item or item part.
    /// </summary>
    /// <param name="origin">Origin from where to grow possible rectangles.</param>
    /// <seealso cref="AvailableSpace"/>
    /// <returns>A list of available spaces.</returns>
    private List<AvailableSpace> GrowFromOrigin(Vector2 origin)
    {
        List<AvailableSpace> potential_spaces = new List<AvailableSpace>();
        // Define max x as page width.
        int max_x = width;
        int y = (int)origin.y;
        // While y is less than page height AND not item has been on the way.
        while (y < height && PlaceAvailable(new Vector2(origin.x, y)) == true)
        {
            // Reset x to origin.x 
            int x = (int)origin.x;
            // Loop until a placed item is met.
            while (x < max_x && PlaceAvailable(new Vector2(x, y)) == true)
                x = x + 1;
            // Update max_x as the rectangle won't be allowed to go further.
            max_x = x;
            // Create a new AvailableSpace based on that data.
            AvailableSpace new_space = new AvailableSpace(origin, new Vector2(x, y + 1) - origin);
            if (new_space.size.x > 0 && new_space.size.y > 0)
            {
                // Add found rectangle as a potential space.
                potential_spaces.Add(new_space);
            }
            y = y + 1;
        }
        return potential_spaces;
    }

    /// <summary>
    /// Reorders _available_spaces list to get bottom left spaces first.
    /// </summary>
    private void OrderAvailableSpaces()
    {
        // Simple bubble sort.
        for ( int i = 1; i < _available_spaces.Count; ++i )
        {
            AvailableSpace left = _available_spaces[i - 1];
            AvailableSpace right = _available_spaces[i];
            if ( left.origin.y > right.origin.y )
            {
                _available_spaces[i - 1] = right;
                _available_spaces[i] = left;
                i = 0;
            }
        }
    }

    /// <summary>
    /// Get a list of potential spaces and merge them to keep only the biggests rectangles.
    /// </summary>
    /// <param name="potential_spaces">Exhaustive list of potential space rectangles.</param>
    /// <returns></returns>
    private List<AvailableSpace> MergeAvailableSpaces(List<AvailableSpace> potential_spaces)
    {
        for ( int i = 0; i < potential_spaces.Count; ++i)
        {
            foreach ( AvailableSpace space in potential_spaces )
            {
                // If potential_spaces[i] is contained into space...
                if ( potential_spaces[i] != space && space.Contains(potential_spaces[i]) == true )
                {
                    // Remove it from the list and restart loops at the same index.
                    potential_spaces.RemoveAt(i);
                    i -= 1;
                    break;
                }
            }
        }
        return potential_spaces;
    }

    /// <summary>
    /// Check if the given position is empty or used by an item.
    /// </summary>
    /// <param name="position">Position to check.</param>
    /// <returns>True if postion is empty, False otherwise.</returns>
    private bool PlaceAvailable(Vector2 position)
    {
        // Crawl every item on the page and find if it contains the given position.
        foreach( PageItem item in _page_items )
        {
            if ( position.x >= item.origin.x && position.x < item.origin.x + item.size.x )
                if ( position.y >= item.origin.y && position.y < item.origin.y + item.size.y )
                    return false;
        }
        return true;
    }

}

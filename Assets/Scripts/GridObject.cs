using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject
{
    public Vector2 gridPosition;
    public Color objectColor;

    public GridObject(Vector2 _gridPosition, Color _objectColor)
    {
        gridPosition = _gridPosition;
        objectColor = _objectColor;
    }
}

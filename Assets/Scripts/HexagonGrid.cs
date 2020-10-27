using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class HexagonGrid : MonoBehaviour
{
    [Header("GRID VALUES, MUST SET")]
    public int gridWidth;

    public int gridHeight;

    public float verticalOffset, horizontalOffset;

    public GameObject GridObject;

    public Vector3 GridStartPosition;
    public Vector3 GridObjectRotation;



    [Header("COLOR LIST, EACH HEXAGON WILL GET A RANDOM COLOR FROM THIS")]
    public List<Color> colors;

    public static HexagonGrid Instance;


    private Dictionary<Vector2, HexagonObject> gridObjectDictionary;

    private int gravityCheckCounter = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateGrid();

        
    }

    void CreateGrid()
    {
        gridObjectDictionary = new Dictionary<Vector2, HexagonObject>();

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 instantiatePosition;
                //for the odd numbered columns, the starting point will be higher
                if (x % 2 == 0)
                {
                    instantiatePosition = GridStartPosition + new Vector3(x * horizontalOffset, y * verticalOffset, 0);
                }
                else
                {
                    instantiatePosition = GridStartPosition + new Vector3(x * horizontalOffset, y * verticalOffset + verticalOffset * 0.5f, 0);
                }
                CreateSingleObject(x, y, instantiatePosition);
            }
        }
    }

    void CreateSingleObject(int gridX, int gridY, Vector3 instantiatePosition)
    {
        var gridObject = ObjectPooler.Instance.GetObject(ObjectTypes.Hexagon);
        gridObject.transform.position = instantiatePosition;
        gridObject.transform.rotation = Quaternion.Euler(GridObjectRotation);
        gridObject.transform.SetParent(transform);

        Color randomColor = Color.black;
        ///var gridObject = Instantiate(GridObject, instantiatePosition, Quaternion.Euler(GridObjectRotation), transform);
        Renderer rendererObject = gridObject.GetComponentInChildren<Renderer>();
        if (rendererObject != null)
        {
            randomColor = colors[Random.Range(0, colors.Count)];
            rendererObject.material.color = randomColor;
        }
        else
        {
            Debug.LogError("No Renderer on object :" + gridObject.name);
        }

        GridObject tempGridObject = new GridObject(new Vector2(gridX, gridY), randomColor);
        HexagonObject tempObject = new HexagonObject(tempGridObject, gridObject);

        if (gridObjectDictionary.ContainsKey(tempGridObject.gridPosition))
            return;
        else
            gridObjectDictionary.Add(tempGridObject.gridPosition, tempObject);
    }

    Vector3 GetWorldPosition(HexagonObject hexagonObject)
    {
        var gridPos = hexagonObject.objectValues.gridPosition;
        Vector3 returnPos;
        if (gridPos.x % 2 == 0)
        {
            returnPos = GridStartPosition + new Vector3(gridPos.x * horizontalOffset, gridPos.y * verticalOffset, 0);
        }
        else
        {
            returnPos = GridStartPosition + new Vector3(gridPos.x * horizontalOffset, gridPos.y * verticalOffset + verticalOffset * 0.5f, 0);
        }
        return returnPos;
    }

    Vector3 GetWorldPosition(Vector2 gridPos)
    {
        Vector3 returnPos;
        if (gridPos.x % 2 == 0)
        {
            returnPos = GridStartPosition + new Vector3(gridPos.x * horizontalOffset, gridPos.y * verticalOffset, 0);
        }
        else
        {
            returnPos = GridStartPosition + new Vector3(gridPos.x * horizontalOffset, gridPos.y * verticalOffset + verticalOffset * 0.5f, 0);
        }
        return returnPos;
    }

    void FakeGravity()
    {

        for (int i = 0; i < gridWidth; i++)
        {
            var tempList = gridObjectDictionary.Values.Where(x => x.objectValues.gridPosition.x == i).ToList();

            tempList.Sort((obj1, obj2) => obj1.objectValues.gridPosition.y.CompareTo(obj2.objectValues.gridPosition.y));
            gridObjectDictionary.Remove(tempList[0].objectValues.gridPosition);
            tempList[0].objectValues.gridPosition = new Vector2(i, 0);
            gridObjectDictionary.Add(tempList[0].objectValues.gridPosition, tempList[0]);
            tempList[0].gameObject.transform.position = GetWorldPosition(tempList[0]);

            for (int y = 1; y < tempList.Count; y++)
            {
                gridObjectDictionary.Remove(tempList[y].objectValues.gridPosition);
                tempList[y].objectValues.gridPosition = new Vector2(i, y);
                gridObjectDictionary.Add(tempList[y].objectValues.gridPosition, tempList[y]);
                gravityCheckCounter++;
                StartCoroutine(FakeGravityCoroutine(tempList[y].gameObject.transform, GetWorldPosition(tempList[y]), 0.3f, tempList[y].objectValues.gridPosition));
            }

            StartCoroutine(CreateHexagonRoutine(0.2f, i, (int)tempList.Last().objectValues.gridPosition.y, gridHeight - tempList.Count));
        }
    }

    public void RotateObjects(List<HexagonObject> closestObjectsList, bool moveLeft, Action callback = null)
    {

        closestObjectsList.Sort((x, y) => x.objectValues.gridPosition.y.CompareTo(y.objectValues.gridPosition.y));
        closestObjectsList.Sort((x, y) => x.objectValues.gridPosition.x.CompareTo(y.objectValues.gridPosition.x));
        List<Vector2> positionList = closestObjectsList.Select(x => x.objectValues.gridPosition).ToList();

        if (moveLeft)
        {
            for (int i = 0; i < positionList.Count; i++)
            {
                var currentObj = closestObjectsList[i];
                var newGridPos = positionList[(i + 1) % positionList.Count];
                currentObj.objectValues.gridPosition = newGridPos;
                // item.gameObject.transform.position = GetWorldPosition(item);
                gridObjectDictionary[currentObj.objectValues.gridPosition] = currentObj;
            }
        }
        else
        {
            for (int i = positionList.Count - 1; i >= 0; i--)
            {
                var currentObj = closestObjectsList[i];
                Vector2 newGridPos;
                if (i == 0)
                {
                    newGridPos = positionList[positionList.Count - 1];
                }
                else
                {
                    newGridPos = positionList[(i - 1) % positionList.Count];
                }

                currentObj.objectValues.gridPosition = newGridPos;
                // item.gameObject.transform.position = GetWorldPosition(item);
                gridObjectDictionary[currentObj.objectValues.gridPosition] = currentObj;
            }

        }


        StartCoroutine(RotationCoroutine(closestObjectsList, 0.3f));
    }


    public List<HexagonObject> CheckNeighbours(HexagonObject objectToCheck, List<HexagonObject> currentList = null)
    {
        Vector2 objectToCheckPos = objectToCheck.objectValues.gridPosition;
        Color objectToCheckColor = objectToCheck.objectValues.objectColor;

        if (currentList == null)
        {
            currentList = new List<HexagonObject>();
            currentList.Add(objectToCheck);
        }

        //will check neighbours
        HexagonObject rightUpperNeighbour, rightDownNeighbour, downNeighbour, leftDownNeighbour, leftUpperNeighbour, upNeighbour;
        if (objectToCheckPos.x % 2 == 0)
        {
            if (gridObjectDictionary.TryGetValue(objectToCheckPos + Vector2.right, out rightUpperNeighbour)
            && rightUpperNeighbour != null
            && rightUpperNeighbour.objectValues.objectColor == objectToCheckColor
            && !currentList.Any(x => x == rightUpperNeighbour))
            {
                currentList.Add(rightUpperNeighbour);
                CheckNeighbours(rightUpperNeighbour, currentList);
            }
            if (gridObjectDictionary.TryGetValue(objectToCheckPos + Vector2.right + Vector2.down, out rightDownNeighbour)
            && rightDownNeighbour != null
            && rightDownNeighbour.objectValues.objectColor == objectToCheckColor
            && !currentList.Any(x => x == rightDownNeighbour))
            {
                currentList.Add(rightDownNeighbour);
                CheckNeighbours(rightDownNeighbour, currentList);
            }
            if (gridObjectDictionary.TryGetValue(objectToCheckPos + Vector2.down, out downNeighbour)
            && downNeighbour != null
            && downNeighbour.objectValues.objectColor == objectToCheckColor
            && !currentList.Any(x => x == downNeighbour))
            {
                currentList.Add(downNeighbour);
                CheckNeighbours(downNeighbour, currentList);
            }
            if (gridObjectDictionary.TryGetValue(objectToCheckPos + Vector2.left + Vector2.down, out leftDownNeighbour)
            && leftDownNeighbour != null
            && leftDownNeighbour.objectValues.objectColor == objectToCheckColor
            && !currentList.Any(x => x == leftDownNeighbour))
            {
                currentList.Add(leftDownNeighbour);
                CheckNeighbours(leftDownNeighbour, currentList);
            }
            if (gridObjectDictionary.TryGetValue(objectToCheckPos + Vector2.left, out leftUpperNeighbour)
            && leftUpperNeighbour != null
            && leftUpperNeighbour.objectValues.objectColor == objectToCheckColor
            && !currentList.Any(x => x == leftUpperNeighbour))
            {
                currentList.Add(leftUpperNeighbour);
                CheckNeighbours(leftUpperNeighbour, currentList);
            }
            if (gridObjectDictionary.TryGetValue(objectToCheckPos + Vector2.up, out upNeighbour)
            && upNeighbour != null
            && upNeighbour.objectValues.objectColor == objectToCheckColor
            && !currentList.Any(x => x == upNeighbour))
            {
                currentList.Add(upNeighbour);
                CheckNeighbours(upNeighbour, currentList);
            }
        }
        else
        {
            if (gridObjectDictionary.TryGetValue(objectToCheckPos + Vector2.right + Vector2.up, out rightUpperNeighbour)
            && rightUpperNeighbour != null
            && rightUpperNeighbour.objectValues.objectColor == objectToCheckColor
            && !currentList.Any(x => x == rightUpperNeighbour))
            {
                currentList.Add(rightUpperNeighbour);
                CheckNeighbours(rightUpperNeighbour, currentList);
            }
            if (gridObjectDictionary.TryGetValue(objectToCheckPos + Vector2.right, out rightDownNeighbour)
            && rightDownNeighbour != null
            && rightDownNeighbour.objectValues.objectColor == objectToCheckColor
            && !currentList.Any(x => x == rightDownNeighbour))
            {
                currentList.Add(rightDownNeighbour);
                CheckNeighbours(rightDownNeighbour, currentList);
            }
            if (gridObjectDictionary.TryGetValue(objectToCheckPos + Vector2.down, out downNeighbour)
            && downNeighbour != null
            && downNeighbour.objectValues.objectColor == objectToCheckColor
            && !currentList.Any(x => x == downNeighbour))
            {
                currentList.Add(downNeighbour);
                CheckNeighbours(downNeighbour, currentList);
            }
            if (gridObjectDictionary.TryGetValue(objectToCheckPos + Vector2.left, out leftDownNeighbour)
            && leftDownNeighbour != null
            && leftDownNeighbour.objectValues.objectColor == objectToCheckColor
            && !currentList.Any(x => x == leftDownNeighbour))
            {
                currentList.Add(leftDownNeighbour);
                CheckNeighbours(leftDownNeighbour, currentList);
            }
            if (gridObjectDictionary.TryGetValue(objectToCheckPos + Vector2.left + Vector2.up, out leftUpperNeighbour)
            && leftUpperNeighbour != null
            && leftUpperNeighbour.objectValues.objectColor == objectToCheckColor
            && !currentList.Any(x => x == leftUpperNeighbour))
            {
                currentList.Add(leftUpperNeighbour);
                CheckNeighbours(leftUpperNeighbour, currentList);
            }
            if (gridObjectDictionary.TryGetValue(objectToCheckPos + Vector2.up, out upNeighbour)
            && upNeighbour != null
            && upNeighbour.objectValues.objectColor == objectToCheckColor
            && !currentList.Any(x => x == upNeighbour))
            {
                currentList.Add(upNeighbour);
                CheckNeighbours(upNeighbour, currentList);
            }
        }

        var templist = currentList.ToList();
        return templist;
    }

    public List<HexagonObject> GetClosestObjects(Vector3 pos)
    {
        Dictionary<HexagonObject, float> hexToDistance = new Dictionary<HexagonObject, float>();

        foreach (var item in gridObjectDictionary.Values)
        {
            if (item != null)
            {
                var currentDistance = Vector3.Distance(pos, GetWorldPosition(item));
                hexToDistance.Add(item, currentDistance);
            }
        }
        var tempList = hexToDistance.ToList();
        hexToDistance.Clear();
        tempList.Sort((x, y) => x.Value.CompareTo(y.Value));
        tempList.RemoveRange(3, tempList.Count - 3);

        return tempList.Select(x => x.Key).ToList();
    }


    IEnumerator RotationCoroutine(List<HexagonObject> objectsToMove, float timeToMove, Action callback = null)
    {
        Vector3[] startPositions = objectsToMove.Select(x => x.gameObject.transform.position).ToArray();
        Vector3[] endPositions = new Vector3[startPositions.Length];
        int endPositionCounter = 0;
        foreach (var item in objectsToMove)
        {
            endPositions[endPositionCounter] = GetWorldPosition(item);
        }

        Transform[] objectsToMoveTransforms = objectsToMove.Select(x => x.gameObject.transform).ToArray();
        float timeCounter = 0;

        while (timeCounter < timeToMove)
        {
            timeCounter += Time.deltaTime;
            for (int i = 0; i < objectsToMoveTransforms.Length; i++)
            {
                objectsToMoveTransforms[i].position = Vector3.Slerp(startPositions[i], GetWorldPosition(objectsToMove[i]), timeCounter / timeToMove);
            }
            yield return null;
        }

        foreach (var item in objectsToMove)
        {
            item.gameObject.transform.position = GetWorldPosition(item);
        }

        //For checking neighbours after coroutine
        foreach (var item in objectsToMove)
        {
            var neighbourList = CheckNeighbours(item);
            if (neighbourList.Count > 2)
            {
                foreach (var hex in neighbourList)
                {
                    gridObjectDictionary.Remove(hex.objectValues.gridPosition);
                    var psObject = ObjectPooler.Instance.GetObject(ObjectTypes.Particles);
                    psObject.transform.position = hex.gameObject.transform.position;
                    Utility.PlayParticles(psObject, hex.objectValues.objectColor);
                    ObjectPooler.Instance.ReturnObject(ObjectTypes.Hexagon, hex.gameObject);
                    StartCoroutine(Utility.Delay(2f, () => ObjectPooler.Instance.ReturnObject(ObjectTypes.Particles, psObject)));
                }
            }
        }

        //Removes the white outline
        foreach (var item in objectsToMove)
        {
            item.gameObject.GetComponentInChildren<Outline>().OutlineColor = Color.black;
        }

        objectsToMove.Clear();
        FakeGravity();
    }
    

    IEnumerator FakeGravityCoroutine(Transform objectToMove, Vector3 endPos, float time, Vector2 gridPos)
    {
        Vector3 startPosition = objectToMove.position;
        float timeCounter = 0;

        while (timeCounter < time)
        {
            timeCounter += Time.deltaTime;
            objectToMove.position = Vector3.Lerp(startPosition, endPos, timeCounter / time);
            yield return null;
        }
        objectToMove.position = endPos;
        gravityCheckCounter--;
    }

    IEnumerator CreateHexagonRoutine(float delay, int xIndex, int yStartIndex, int count)
    {
        yield return new WaitForSeconds(delay);
        for (int i = 0; i < count; i++)
        {
            CreateSingleObject(xIndex, yStartIndex + i + 1, GetWorldPosition(new Vector2(xIndex, yStartIndex + count * 2 + 1)));
            var gridObj = gridObjectDictionary[new Vector2(xIndex, yStartIndex + i + 1)];
            yield return new WaitForSeconds(delay);
            StartCoroutine(FakeGravityCoroutine(gridObjectDictionary[gridObj.objectValues.gridPosition].gameObject.transform, GetWorldPosition(gridObj), 0.3f, gridObj.objectValues.gridPosition));
        }
    }
}


public class HexagonObject
{
    public GridObject objectValues;
    public GameObject gameObject;

    public HexagonObject(GridObject _objectValues, GameObject _gameObject)
    {
        objectValues = _objectValues;
        gameObject = _gameObject;
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{


    List<HexagonObject> currentClosestObjects;
    Vector2? inputStartPosition;
    Vector2 inputCurrentPosition;

    private void Start()
    {
        currentClosestObjects = new List<HexagonObject>();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            inputStartPosition = Input.mousePosition;
            GetClosestObjectsFromMouse();
        }
        else if (Input.GetMouseButton(0) && inputStartPosition.HasValue)
        {
            HandleRotation();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            foreach (var item in currentClosestObjects)
            {
                item.gameObject.GetComponentInChildren<Outline>().OutlineColor = Color.black;
            }
        }
    }

    void GetClosestObjectsFromMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            currentClosestObjects = HexagonGrid.Instance.GetClosestObjects(hit.point);
            foreach (var item in currentClosestObjects)
            {
                item.gameObject.GetComponentInChildren<Outline>().OutlineColor = Color.white;
            }
        }
    }

    void HandleRotation()
    {
        inputCurrentPosition = Input.mousePosition;
        if (inputCurrentPosition.x - inputStartPosition.Value.x > 100f)
        {
            HexagonGrid.Instance.RotateObjects(currentClosestObjects, true);
            inputStartPosition = null;
        }
        else if (inputCurrentPosition.x - inputStartPosition.Value.x < -100f)
        {
            HexagonGrid.Instance.RotateObjects(currentClosestObjects, false);
            inputStartPosition = null;
        }
    }



}

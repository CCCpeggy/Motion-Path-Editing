using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragPoint : MonoBehaviour
{
    public Camera camera;
    GameObject selectedObj = null;
    private bool isMouseDown;
    private Vector3 lastMousePosition = Vector3.zero;
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) isMouseDown = true;
        else if (Input.GetMouseButtonUp(0)) isMouseDown = false;

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (isMouseDown && selectedObj == null)
        {
            if (Physics.Raycast(ray, out hit)) {
                Debug.DrawLine(camera.transform.position, hit.transform.position, Color.red, 0.1f, true);
                selectedObj = hit.transform.gameObject;
            }
        }
        else if (!isMouseDown){
            selectedObj = null;
        }
        TwoDMove();
    }

    private void TwoDMove()
    {
        if (selectedObj)
        {
            if (lastMousePosition != Vector3.zero)
            {
                Vector3 offset = camera.ScreenToWorldPoint(Input.mousePosition) - lastMousePosition;
                selectedObj.transform.position += offset;
            }
        }
        else if(isMouseDown) {
            Vector3 offset = camera.ScreenToWorldPoint(Input.mousePosition) - lastMousePosition;
            camera.transform.position -= offset;
        }
        lastMousePosition = camera.ScreenToWorldPoint(Input.mousePosition);
    }
}

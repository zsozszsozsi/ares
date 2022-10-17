using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;

public class CameraMovement : MonoBehaviour
{
    public Camera _cam;

    public Transform cameraTransform;

    public float movementSpeed;
    public float movementTime;
    public float rotationAmount;
    public float minPos;
    public float maxPos;

    public Vector3 zoomAmount;
    public float minZoom;
    public float maxZoom;

    private Vector3 newPosition;
    private Quaternion newRotation;
    private Vector3 newZoom;

    private Vector3 dragStartPositon;
    private Vector3 dragCurrentPositon;
    private Vector3 rotateStartPosition;
    private Vector3 rotateCurrentPositon;

    private bool isRotating = false;
    private bool isMoving = false;
    private bool isMoveToRotate = false; // mikor mozgatás közben nyomunk left_alt-ot

    PhotonView view;

    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();

        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (view.IsMine)
        {
            HandleMovement();
            HandleMouseInput();
            HandleTransform();
        }
    }

    /// <summary>
    /// Handling the camera position
    /// </summary>
    void HandleTransform()
    {
        newPosition.x = Mathf.Clamp(newPosition.x, minPos, maxPos);
        newPosition.z = Mathf.Clamp(newPosition.z, minPos, maxPos);

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);
    }

    /// <summary>
    /// Handling keyboard input
    /// </summary>
    void HandleMovement()
    {
        float keyboard_movementSpeed = movementSpeed / 2;
        //movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            newPosition += (transform.forward * keyboard_movementSpeed);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            newPosition += (transform.forward * -keyboard_movementSpeed);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            newPosition += (transform.right * keyboard_movementSpeed);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition += (transform.right * -keyboard_movementSpeed);
        }
    }

    /// <summary>
    /// Handling mouse input
    /// </summary>
    void HandleMouseInput()
    {
        //Zoom
        if(Input.mouseScrollDelta.y != 0)
        {
            newZoom += -Input.mouseScrollDelta.y * zoomAmount;

            newZoom.y = Mathf.Clamp(newZoom.y, -minZoom, maxZoom);
            newZoom.z = Mathf.Clamp(newZoom.z, -maxZoom, minZoom);
        }


        //moving
        if (Input.GetMouseButtonDown(1) && !Input.GetKey(KeyCode.LeftAlt))
        {
            isMoving = true;

            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

            float entry;

            if(plane.Raycast(ray, out entry))
            {
                dragStartPositon = ray.GetPoint(entry);
            }

        }
        if (Input.GetMouseButton(1) && !Input.GetKey(KeyCode.LeftAlt))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPositon = ray.GetPoint(entry);

                if(!isRotating && !isMoveToRotate)
                    newPosition = transform.position + dragStartPositon - dragCurrentPositon;
            }
        }

        //rotate
        if (Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftAlt))
        {
            if(!isMoving)
                isRotating = true;

            rotateStartPosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(2) || Input.GetMouseButton(1) && Input.GetKey(KeyCode.LeftAlt))
        {
            rotateCurrentPositon = Input.mousePosition;

            Vector3 diff = rotateStartPosition - rotateCurrentPositon;

            rotateStartPosition = rotateCurrentPositon;

            if(!isMoving)
                newRotation *= Quaternion.Euler(Vector3.up * (diff.x / 5f));
        }

        //stop moving when rotating, vice versa

        if(Input.GetKey(KeyCode.LeftAlt) && isMoving)
        {
            isMoving = false;
            isMoveToRotate = true;
        }

        if(Input.GetMouseButtonUp(1) && isMoveToRotate)
        {
            isMoveToRotate = false;
        }

        if (Input.GetMouseButtonUp(1) && isRotating)
        {
            isRotating = false;
        }

        if (Input.GetMouseButtonUp(2) && isRotating)
        {
            isRotating = false;
        }

        if (Input.GetMouseButtonUp(1) && isMoving)
        {
            isMoving = false;
        }

    }
}

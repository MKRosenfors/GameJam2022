using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InteractTester : MonoBehaviour
{
    [SerializeField] float speedMod;
    [SerializeField] float mouseSensitivity;
    [SerializeField] float interactRange;

    CharacterController cc;
    Camera playerCamera;

    List<IInteractable> interactables;
    float downVelocity;
    float xRotation;
    Vector3 moveVec;
    LayerMask interactableLayer;
    private void Start()
    {
        interactableLayer = LayerMask.GetMask("Interactable");
        cc = gameObject.GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        interactables = FindObjectsOfType<MonoBehaviour>().OfType<IInteractable>().ToList();
    }
    void Update()
    {
        CheckForInteractable();
        CheckInput();
        Move();
        Rotate();
        CameraRotate();
    }

    //Loops through the interactables list and calls the implemented interface-function "Interact"
    void InteractWithObjects()
    {
        Debug.Log(interactables.Count);
        for (int i = 0; i < interactables.Count; i++)
        {
            interactables[i].Interact();
        }
    }
    //Uses a raycast to search for interactable objects, if found they are stored in the interactables list
    void CheckForInteractable()
    {
        interactables.Clear();
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, interactRange, interactableLayer))
        {
            interactables.Add(hit.collider.gameObject.GetComponent<IInteractable>());
        }
    }
    void CheckInput()
    {
        moveVec = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            moveVec.z = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveVec.x = -1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveVec.z = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveVec.x = 1;
        }
        if (Input.GetKeyDown(KeyCode.Space) && cc.isGrounded)
        {
            downVelocity = -10;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            InteractWithObjects();
        }
    }
    #region Movement & Rotation
    void Move()
    {
        moveVec = moveVec.normalized;
        cc.Move(transform.forward * moveVec.z * Time.deltaTime * speedMod);
        cc.Move(transform.right * moveVec.x * Time.deltaTime * speedMod);
        downVelocity += Time.deltaTime * 20f;

        if (cc.isGrounded && downVelocity > 0)
        {
            downVelocity = 0.1f;
        }

        cc.Move(Vector3.down * downVelocity * Time.deltaTime);
    }
    
    void Rotate()
    {
        transform.Rotate(new Vector3(0f, Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime, 0f), Space.World);
    }
    void CameraRotate()
    {
        Transform cTransform = playerCamera.transform;

        xRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    #endregion

}

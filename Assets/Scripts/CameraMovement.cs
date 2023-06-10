using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;

[ExecuteInEditMode]

public class CameraMovement : MonoBehaviour
{

    public GameObject targetObject;

    Vector3 targetPosition;
    Vector3 orbitalVector;
    Vector3 angledVector;

    Vector3 origCamPos;

    Camera cam;
    public float cameraAngle = 0;
    float mouseScrollFactor = 0;
    float mouseScrollFactorClamped;
    float scrollLerpRadius;
    float scrollLerpHeight;
    float scrollLerpFOV;
    public float scrollLerpState = 0;
    public float scrollHeightOutValue = 7.37f;
    public float scrollHeightInValue = 5.1f;
    public float scrollRadiusOutValue = 2.94f;
    public float scrollRadiusInValue = 19.4f;
    public float scrollFOVOutValue = 43;
    public float scrollFOVInValue = 22;
    public float cameraSwingPosition = 0f;
    float swingControlOffset;
    public float swingControlSpeed = 1.78f;


    float cam_Radius;
    Vector3 heightValueVector;
    float heightValue;
    float radiusValue;

    LayerMask layerMask;

    [Range(1, 50)] public float radiusOffset;

    void Start()
    {
        cam = this.GetComponent<Camera>();
        origCamPos = transform.position;

        targetPosition = targetObject.transform.position;

        int layerNumber = 9;
        layerMask = 1 << layerNumber;

    }

    void Update()
    {
        Vector3 heightValueVector = new Vector3(0f, heightValue, 0f);

        cam_Radius = (Vector3.Distance(origCamPos, targetPosition) * 0.3f) + radiusValue;
        float angle = (Mathf.PI * 1.52f - (cameraSwingPosition + swingControlOffset));
        float sine = Mathf.Sin(angle);
        float cos = Mathf.Cos(angle);

        orbitalVector = new Vector3((cam_Radius * cos), 1, (cam_Radius * sine));
        angledVector = new Vector3(0, 0, 0) + targetObject.transform.position;

        cameraTargeting(orbitalVector, heightValueVector);

        orbitControl();

        scrollingBehavior();

        obstructionClearance(heightValueVector);
    }

    private Vector3 cameraTargeting(Vector3 orbitalVector, Vector3 heightValueVector)
    {

        // radius = (Vector3.Distance(origCamPos, targetPosition) * 0.3f) + mouseScrollFactor * 5;

        Vector3 lookAtOffset;

        lookAtOffset = new Vector3(0, cameraAngle, 0);

        transform.position = targetObject.transform.position + orbitalVector + heightValueVector;

        transform.LookAt(targetObject.transform.position + lookAtOffset);


        return transform.position;

    }
    private void orbitControl()
    {
  
        if (Input.GetKey(KeyCode.Q))
        {
            swingControlOffset += swingControlSpeed * Time.deltaTime;

        }
        if (Input.GetKey(KeyCode.E))
        {
            swingControlOffset -= swingControlSpeed * Time.deltaTime;
        }
    }

    /*
     * SCROLL BEHAVIOR NOTES
     * I could make the middle button a trigger from perspective to isometric
     * Scrolling in each mode  = zoom level
     */
    private void scrollingBehavior()
    {
        mouseScrollFactor += Input.mouseScrollDelta.y * 0.10f;
        mouseScrollFactorClamped = Math.Clamp(mouseScrollFactor, 0, 1);

        if (mouseScrollFactor > mouseScrollFactorClamped)
        {
            mouseScrollFactor = 1;
        }
        else if (mouseScrollFactor < mouseScrollFactorClamped)
        {
            mouseScrollFactor = 0;
        }

        scrollLerpHeight = Mathf.Lerp(scrollHeightOutValue, scrollHeightInValue, scrollLerpState);
        scrollLerpRadius = Mathf.Lerp(scrollRadiusOutValue, scrollRadiusInValue, scrollLerpState);
        scrollLerpFOV = Mathf.Lerp(scrollFOVOutValue, scrollFOVInValue, scrollLerpState);

        heightValue = scrollLerpHeight;
        radiusValue = scrollLerpRadius;
        cam.fieldOfView = scrollLerpFOV;

        scrollLerpState = mouseScrollFactorClamped;
    }

    /*
     * global variables for obstructionClearance():
     */
    public Material transparentMaterial;
    Material struckObjectMaterialMemory;
    static GameObject struckObject;
    GameObject struckObjectMemory = struckObject;
    bool alreadyHit = false;
    bool infoGathered = false;

    private void obstructionClearance(Vector3 heightValueVector)
    {
        Vector3 rayTarget = targetObject.transform.position - (targetObject.transform.position + orbitalVector + heightValueVector);
        Ray ray = new Ray(transform.position, rayTarget);
        RaycastHit hit;

        Debug.DrawRay(transform.position, rayTarget, Color.magenta);

        if (Physics.Raycast(ray, out hit, Vector3.Distance(transform.position, targetPosition), layerMask))
        {
            if (!alreadyHit)
            {
                materialToggle();
            } 
            else { return; }
        } 
        else 
        {
            if (struckObject == null) { alreadyHit = false;  return; }
            else
            {
                if (infoGathered)
                {
                struckObjectMemory.GetComponent<MeshRenderer>().material = struckObjectMaterialMemory;
                struckObject = null;
                struckObjectMemory = null;
                alreadyHit = false;
                infoGathered = false;
                }
            }
        }

        void materialToggle()
        {
            if (!infoGathered)
            {
                struckObject = hit.transform.gameObject;
                struckObjectMaterialMemory = struckObject.GetComponent<MeshRenderer>().material;
                struckObjectMemory = struckObject;
                infoGathered = true;
            } 
            else
            {
                hit.transform.gameObject.GetComponent<MeshRenderer>().material = transparentMaterial;
                alreadyHit = true;            
            }
        }
    }

}

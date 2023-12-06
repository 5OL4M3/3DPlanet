using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticCameraControl : MonoBehaviour
{
    private Transform target;
    public Transform sun;
    private float zoomVal;
    public Vector3 offset = new Vector3(0f, 2f, 5f);
    private int iteration = 0;
    public Vector3 birdEyeDirection = new Vector3(5f, 2f, 0f);
    public float birdEyeZoomVal = 450;

    private List<GameObject> observablePlanets = new List<GameObject>();

    void Start()
    {   
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allGameObjects)
        {
            // Check if the GameObject's name contains the word "planet"
            if (obj.name.ToLower().Contains("planet") || obj.name.ToLower().Contains("sun"))
            {
                // This GameObject has "planet" in its name
                observablePlanets.Add(obj);
                Debug.Log("Found planet: " + obj.name);
            }
        }

        target = observablePlanets[iteration].transform;
        zoomVal = target.localScale.x * 2;
        Vector3 direction = target.position - sun.position;
        transform.position = target.position + (zoomVal) * direction.normalized;

        birdEyeZoomVal = 450;
        Debug.Log("HOW MANY PLANETS: " + observablePlanets.Count);
    }

    void Update()
    {
        HandleTargetSwap();
        HandleMouseScroll();

        if (target.gameObject.name.ToLower().Contains("sun"))
        {
            transform.position = target.position + birdEyeZoomVal * birdEyeDirection.normalized + offset;
            transform.LookAt(target);
        }
        else if (target != null)
        {
            // Vector3 direction = target.position - sun.position;
            
            // transform.position = target.position + zoomVal * direction.normalized + offset;
            
            // //Debug.Log(direction);

            // transform.LookAt(target);
            float horizontalInput = 0.0f;
            float verticalInput = 0.0f;

            if (Input.GetKey(KeyCode.A))
            {
                horizontalInput = .5f;
            } else if (Input.GetKey(KeyCode.D))
            {
                horizontalInput = -.5f;
            }

            if (Input.GetKey(KeyCode.W))
            {
                verticalInput = .5f;
            } else if (Input.GetKey(KeyCode.S))
            {
                verticalInput = -.5f;
            }


            // float horizontalInput = Input.GetAxis("Horizontal");
            // float verticalInput = Input.GetAxis("Vertical");

            transform.RotateAround(target.position, Vector3.up, horizontalInput);
            transform.RotateAround(target.position, Vector3.right, verticalInput);

            transform.position = target.position - transform.forward * zoomVal;

            transform.LookAt(target);
        }
    }

    void HandleMouseScroll()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        
        if (scrollInput != 0.0f)
        {
            zoomVal += (scrollInput) * 2.0f;

            //Debug.Log(zoomVal);
            //Debug.Log("UP");
            zoomVal = Mathf.Clamp(zoomVal, (float)(target.localScale.x * .6), 100);
        }
    }

    void HandleTargetSwap()
    {
        if (iteration == observablePlanets.Count)
        {
            iteration = 0;
        }
        else if (iteration == -1)
        {
            iteration = observablePlanets.Count - 1;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            target = observablePlanets[iteration].transform;
            iteration += 1;
            Debug.Log("E Pressed: " + target.gameObject.name);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            target = observablePlanets[iteration].transform;
            iteration -= 1;
            Debug.Log("Q Pressed: " + target.gameObject.name);
        }
    }
}

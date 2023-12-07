using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using TMPro;
using System.Globalization;

public class StaticCameraControl : MonoBehaviour
{
    [SerializeField] private GameObject tmpTextIndicator;
    private Transform target;
    public Transform sun;
    private float zoomVal;
    public Vector3 offset = new Vector3(0f, 2f, 5f);
    private int iteration = 0;
    public Vector3 birdEyeDirection = new Vector3(5f, 2f, 0f);
    public float birdEyeZoomVal = 450;
    public float zoomSensitivity = 3.5f;

    private List<GameObject> observablePlanets = new List<GameObject>();

    void Start()
    {   
        GameObject sunobj = GameObject.FindGameObjectWithTag("Sun");
        if (sunobj == null)
        {
            Debug.LogError("No sun found");
            return;
        }

        GameObject[] planets = GameObject.FindGameObjectsWithTag("Planets");
        if (planets.Length == 0)
        {
            Debug.LogError("No planets found");
            return;
        }

        foreach (GameObject planet in planets)
        {
            observablePlanets.Add(planet);
        }

        sun = sunobj.transform;

        //sort by alphabetical
        observablePlanets.Sort((x, y) => string.Compare(x.name, y.name));
        //put sun first, remove old ref
        observablePlanets.Insert(0, sunobj);

        target = observablePlanets[iteration].transform;
        zoomVal = target.localScale.x * 2;
        Vector3 direction = target.position - sun.position;
        transform.position = target.position + (zoomVal) * direction.normalized;

        birdEyeZoomVal = 450;
        //Debug.Log("HOW MANY PLANETS: " + observablePlanets.Count);

        //set start to sun
        target = sun;
    }

    void Update()
    {
        HandleTargetSwap();
        HandleMouseScroll();
        _updateTMPTextIndicator();

        //break if no change
        if (target == null)
        {
            return;
        }

        if (target.gameObject.name.ToLower().Contains("sun"))
        {
            transform.position = target.position + birdEyeZoomVal * birdEyeDirection.normalized + offset;
            transform.LookAt(target);
        }
        else if (target != null)
        {
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

    private void _updateTMPTextIndicator()
    {
        TextMeshProUGUI _tmp = tmpTextIndicator.GetComponent<TextMeshProUGUI>();
        string _currentTransformName = _getCurrentTransformName();
        _tmp.text = _currentTransformName;
    }

    private string _getCurrentTransformName()
    {
        //if sun, grab a planet and return only the planet name, we can avoid using a solar system plan
        string name = target.gameObject.name;
        if (name.ToLower().Contains("sun"))
        {
            name = observablePlanets[1].name;
            Debug.Log("Name: " + name);
            //trim numerals from end by breaking at first space 
            //splits
            string[] splitName = name.Split(' ');
            if (splitName.Length == 3)
            {
                name = splitName[1];
            } else {
                name = splitName[1] + " " + splitName[2];
            }
            //Debug.Log("Name: " + name);
        }
        //remove "sun" and "planet" from name, correct formatting
        name = name.Replace("Sun", "");
        name = name.Replace("Planet", "");
        name = name.Trim();
        //name = name.ToLower();
        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        name = textInfo.ToTitleCase(name);
        return name;
    }

    void HandleMouseScroll()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        
        if (scrollInput != 0.0f)
        {
            zoomVal += (scrollInput) * zoomSensitivity;

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
            //Debug.Log("E Pressed: " + target.gameObject.name);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            target = observablePlanets[iteration].transform;
            iteration -= 1;
            //Debug.Log("Q Pressed: " + target.gameObject.name);
        }
    }
}

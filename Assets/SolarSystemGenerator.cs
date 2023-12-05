using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using DTT.Utils;

public class SolarSystemGenerator : MonoBehaviour
{
    private SolarSystemPlan setsolarSystemPlan;

    //set the solar system plan held in the solar system plan script
    void Start()
    {
        //get the solar system planner script in the same game object
        SolarSystemPlanner scriptobj = GetComponent<SolarSystemPlanner>();
        Debug.Log("Solar System Plan: " + scriptobj.solarSystemPlan);
        setsolarSystemPlan = scriptobj.solarSystemPlan;
    }

    //reset the solar system plan held in the solar system plan script
    [Button]
    public void DebugResetSolarSystemPlan()
    {
        SolarSystemPlanner _scriptobj = GetComponent<SolarSystemPlanner>();
        setsolarSystemPlan = _scriptobj.solarSystemPlan;
    }

    //Read the solar system plan held in the solar system plan script for number of planets and print them
    [Button]
    public void DebugReadSolarSystemPlan()
    {
        List<PlanetSettings> _planetSettings = setsolarSystemPlan.planets;
        foreach (PlanetSettings _planet in _planetSettings)
        {
            Debug.Log("Planet Distance: " + _planet.DistanceFromStar);
        }
    }

    //generate gameobjects for each planet in the solar system plan held in the solar system plan script
    [Button]
    public void DebugGenerateSolarSystem()
    {
        List<PlanetSettings> _planetSettings = setsolarSystemPlan.planets;
        int _planetCount = 1;
        foreach (PlanetSettings _planet in _planetSettings)
        {
            GameObject _planetObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _planetObj.name = "Planet" + _planet.DistanceFromStar;
            //divide radius by 5
            float _planetRadius = _planet.planetRadius / 10;

            //multiply distance by 2
            float _planetDistance = _planet.DistanceFromStar * 2;
            //ensure at least 20 units away from star
            if (_planetDistance < 20)
            {
                _planetDistance += 20;
            }

           
            _planetObj.transform.position = new Vector3(_planetDistance, 0, 0);
            _planetObj.transform.localScale = new Vector3(_planetRadius, _planetRadius, _planetRadius);
            //place planet at random rotation (set by planet seed) around the star
            int _planetSeed = _planet.seed;
            //Random.InitState(_planetSeed);
            Helpers.SetRandomSeed(_planetSeed);
            _planetObj.transform.RotateAround(Vector3.zero, Vector3.up, Random.Range(0, 360));
            float _planetRotationOffset = Random.Range(-80, 80);
            //tilt planet by random amount so its not one smooth plane relative to the star (at 0,0,0)
            _planetObj.transform.RotateAround(Vector3.zero, Vector3.right, _planetRotationOffset);
            //Random.InitState(System.Environment.TickCount);
            Helpers.ResetRandomSeed();

            //tag them as "Planets" for easy cleanup
            _planetObj.tag = "Planets";

            //assign a planet rotate script to the planet
            PlanetRotate _planetRotateScript = GeneratePlanetRotateScript(_planetObj, _planet);

            //assign a planet script to the planet
            Planet _planetScript = GeneratePlanetScript(_planetObj, _planet);

            //assign an atmosphere to the planet
            AttachAtmosphereToGO(_planetObj, _planet);

            _planetCount++;
        }

        //check there are no overlapping planets, if there are, move them apart
        GameObject[] _planets = GameObject.FindGameObjectsWithTag("Planets");
        foreach (GameObject _planet in _planets)
        {
            foreach (GameObject _planet2 in _planets)
            {
                if (_planet != _planet2)
                {
                    if (Vector3.Distance(_planet.transform.position, _planet2.transform.position) < _planet.transform.localScale.x + _planet2.transform.localScale.x)
                    {
                        _planet.transform.position = new Vector3(_planet.transform.position.x + 1, _planet.transform.position.y, _planet.transform.position.z);
                    }
                }
            }
        }
    }

    //private function helps generate variables for planets
    private PlanetRotate GeneratePlanetRotateScript(GameObject _planetObj, PlanetSettings _planet)
    {
        PlanetRotate _planetRotate = _planetObj.AddComponent<PlanetRotate>();
        //slow down rotaton of later planets and larger planets
        _planetRotate.RotateSpeed = _planet.planetOrbitSpeed;
        _planetRotate.RotateSpeed = (_planetRotate.RotateSpeed / (1 + Mathf.Log(_planet.planetRadius)));
        Debug.Log("Planet Rotate Speed: " + _planetRotate.RotateSpeed);
        //set the distance from the star
        _planetRotate.DistanceFromStar = _planet.DistanceFromStar;
        //set the planet radius
        //_planetRotate.PlanetRadius = _planet.planetRadius;
        //set the planet rotation speed
        _planetRotate.RotateSpeedSelf = _planet.planetOrbitSpeedSelf;
        return _planetRotate;
    }

    private Planet GeneratePlanetScript(GameObject _planetObj, PlanetSettings _planet)
    {
        Planet _planetScript = _planetObj.AddComponent<Planet>();
        //set the planet radius
        _planetScript.planetRadius = _planet.planetRadius;
        //get planet mass
        float _planetMass = _planet.planetMass;
        //get planet temperature
        float _planetTemperature = _planet.planetTemperature;
        //Create list if tuples for biomes probabilities
        _planet.Biomes = new List<(int, float)>();
        //add biomes to list
        _planet.Biomes.Add(((int)PlanetSettings.BiomesPlanets.Desert, 0.2f));

        _planetScript.biomeProbabilities = _planet.Biomes;

        

        return _planetScript;
    }

    private void AttachAtmosphereToGO(GameObject _planetObj, PlanetSettings _planet)
    {
        //create atmosphere
        GameObject _atmosphereObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _atmosphereObj.name = "Atmosphere" + _planet.DistanceFromStar;
        //parent atmosphere to planet
        
        float _atmosphereScale = _planet.planetRadius / 20;
        _atmosphereObj.transform.localScale = _planetObj.transform.localScale * 1.10f;
        _atmosphereObj.transform.parent = _planetObj.transform;
        //set position to planet position
        _atmosphereObj.transform.position = _planetObj.transform.position;
        //set atmosphere material to "MatAtmosphere" in the assets folder
        _atmosphereObj.GetComponent<Renderer>().material = Resources.Load("MatAtmosphere") as Material;
        //scale ScaleMultiply by planet radius and set material parameter
        _atmosphereObj.GetComponent<Renderer>().sharedMaterial.SetFloat("_ScaleMultiply", (1 + (_atmosphereScale * 0.7f)));
        
    }

    //destroy all gameobjects tagged as "Planets"
    [Button]
    public void DebugDestroySolarSystem()
    {
        GameObject[] _planets = GameObject.FindGameObjectsWithTag("Planets");
        foreach (GameObject _planet in _planets)
        {
            DestroyImmediate(_planet);
        }
    }

    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using DTT.Utils;

public class SolarSystemGenerator : MonoBehaviour
{
    [SerializeField]
    public ShapeSettings shapeSettings;
    public ShapeSettings moonSettings;

    private SolarSystemPlan setsolarSystemPlan;

    //set the solar system plan held in the solar system plan script
    void Start()
    {
        //get the solar system planner script in the same game object
        SolarSystemPlanner scriptobj = GetComponent<SolarSystemPlanner>();
        //Debug.Log("Solar System Plan: " + scriptobj.solarSystemPlan);
        setsolarSystemPlan = scriptobj.solarSystemPlan;
    }

    //reset the solar system plan held in the solar system plan script
    public void DebugResetSolarSystemPlan()
    {
        SolarSystemPlanner _scriptobj = GetComponent<SolarSystemPlanner>();
        setsolarSystemPlan = _scriptobj.solarSystemPlan;
    }

    //Read the solar system plan held in the solar system plan script for number of planets and print them
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
    public void GenerateSolarSystem()
    {
        //reset settings
        DebugResetSolarSystemPlan();
        //destroy all gameobjects tagged as "Planets"
        DestroySolarSystem();


        List<PlanetSettings> _planetSettings = setsolarSystemPlan.planets;
        int _planetCount = 1;
        foreach (PlanetSettings _planet in _planetSettings)
        {
            //create empty gameobject
            GameObject _planetObj = new GameObject();



            _planetObj.name = _planetSettings[_planetCount - 1].name;
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

            _planetScript.isMoon = false;
            _planetScript.planetSeed = _planetSeed;

            //assign an atmosphere to the planet
            AttachAtmosphereToGO(_planetObj, _planet);

            //assign shape settings to the planet script
            _planetScript.shapeSettings = shapeSettings;

            //trigger the planet to generate
            _planetScript.GeneratePlanet(_planetScript.PlanetSplitCount);

            //generate moons
            int _moonCount = _planet.moons.Count;
            for (int i = 0; i < _moonCount; i++)
            {
                //create empty gameobject
                GameObject _moonObj = new GameObject();
                _moonObj.name = "Moon" + i;
                //divide radius by 5
                float _moonRadius = _planet.planetRadius;
                //divide distance by 2
                float _moonDistance = _planet.planetRadius / 10;
                //ensure at least 3 units away from planet
                if (_moonDistance < 3)
                {
                    _moonDistance += 3;
                }
                //set position to planet position + offset
                _moonObj.transform.position = _planetObj.transform.position;
                //set scale to planet scale
                _moonObj.transform.localScale = new Vector3(_moonRadius, _moonRadius, _moonRadius);
                //place moon at random rotation (set by planet seed) around the planet
                int _moonSeed = _planet.seed + i;
                //Random.InitState(_moonSeed);
                Helpers.SetRandomSeed(_moonSeed);
                _moonObj.transform.RotateAround(_planetObj.transform.position, Vector3.up, Random.Range(0, 360));
                float _moonRotationOffset = Random.Range(-80, 80);
                //tilt moon by random amount so its not one smooth plane relative to the planet (at 0,0,0)
                _moonObj.transform.RotateAround(_planetObj.transform.position, Vector3.right, _moonRotationOffset);
                //Random.InitState(System.Environment.TickCount);
                Helpers.ResetRandomSeed();
                //tag them as "Planets" for easy cleanup
                _moonObj.tag = "Planets";
                //assign a planet rotate script to the moon
                PlanetRotate _moonRotateScript = GeneratePlanetRotateScript(_moonObj, _planet, true, _planetObj.transform);
                //assign a planet script to the moon
                Planet _moonScript = GeneratePlanetScript(_moonObj, _planet, true);
                //assign shape settings to the moon script
                _moonScript.shapeSettings = moonSettings;
                //set ismoon
                _moonScript.isMoon = true;
                //set moon as child of planet
                _moonObj.transform.parent = _planetObj.transform;
                //set moon distance from planet
                _moonObj.transform.localPosition = new Vector3(_moonDistance, 0, 0);
                //trigger moon to generate terrain
                _moonScript.GeneratePlanet(_moonScript.PlanetSplitCount);
            }

            _planetCount++;
        }

        //check there are no overlapping planets, if there are, move them apart
        GameObject[] _planets = GameObject.FindGameObjectsWithTag("Planets");
        foreach (GameObject _planet1 in _planets)
        {
            foreach (GameObject _planet2 in _planets)
            {
                if (_planet1 != _planet2)
                {
                    if (Vector3.Distance(_planet1.transform.position, _planet2.transform.position) < _planet1.transform.localScale.x + _planet2.transform.localScale.x)
                    {
                        _planet1.transform.position = new Vector3(_planet1.transform.position.x + 1, _planet1.transform.position.y, _planet1.transform.position.z);
                    }
                }
            }
        }
    
    }

    //private function helps generate variables for planets
    public PlanetRotate GeneratePlanetRotateScript(GameObject _planetObj, PlanetSettings _planet, bool isMoon=false, Transform centerpoint = null)
    {
        PlanetRotate _planetRotate = _planetObj.AddComponent<PlanetRotate>();
        //slow down rotaton of later planets and larger planets
        _planetRotate.RotateSpeed = _planet.planetOrbitSpeed;
        _planetRotate.RotateSpeed = (_planetRotate.RotateSpeed / (1 + Mathf.Log(_planet.planetRadius)));
        _planetRotate.RotateSpeed /= 3;
        //Debug.Log("Planet Rotate Speed: " + _planetRotate.RotateSpeed);
        //set the distance from the star
        _planetRotate.DistanceFromStar = _planet.DistanceFromStar;
        //set the planet radius
        //_planetRotate.PlanetRadius = _planet.planetRadius;
        //set the planet rotation speed
        _planetRotate.RotateSpeedSelf = _planet.planetOrbitSpeedSelf;
        _planetRotate.RotateSpeedSelf /= 3;
        _planetRotate.Centerpoint = centerpoint;
        _planetRotate.isMoon = isMoon;
        return _planetRotate;
    }

    public Planet GeneratePlanetScript(GameObject _planetObj, PlanetSettings _planet, bool isMoon=false)
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
        float _desertProb = 0.1f;
        float _forestProb = 0.1f;
        float _tundraProb = 0.1f;
        float _mountainProb = 0.1f;
        float _barrenProb = 0.1f;
        float _grasslandsProb = 0.1f;
        float _snowyMountainsProb = 0.1f;
        bool havePoles = true;
        //set biome probabilities based on temperature
        if (_planetTemperature < 20)
        {
            _tundraProb = 0.3f;
            _snowyMountainsProb = 0.3f;
            _mountainProb = 0.2f;
            _barrenProb = 0.1f;
            _grasslandsProb = 0.1f;
            _desertProb = 0.0f;
            _forestProb = 0.0f;
        } else if (_planetTemperature < 40)
        {
            _tundraProb = 0.2f;
            _snowyMountainsProb = 0.2f;
            _mountainProb = 0.1f;
            _barrenProb = 0.1f;
            _grasslandsProb = 0.2f;
            _desertProb = 0.1f;
            _forestProb = 0.1f;
        } else if (_planetTemperature < 60)
        {
            _tundraProb = 0.1f;
            _snowyMountainsProb = 0.1f;
            _mountainProb = 0.1f;
            _barrenProb = 0.1f;
            _grasslandsProb = 0.2f;
            _desertProb = 0.2f;
            _forestProb = 0.2f;
        } else if (_planetTemperature < 80)
        {
            _tundraProb = 0.0f;
            _snowyMountainsProb = 0.0f;
            _mountainProb = 0.1f;
            _barrenProb = 0.1f;
            _grasslandsProb = 0.2f;
            _desertProb = 0.2f;
            _forestProb = 0.4f;
        } else
        {
            _tundraProb = 0.0f;
            _snowyMountainsProb = 0.0f;
            _mountainProb = 0.0f;
            _barrenProb = 0.1f;
            _grasslandsProb = 0.1f;
            _desertProb = 0.2f;
            _forestProb = 0.6f;
            havePoles = false;
        }
        if (!isMoon)
        {
            _planet.Biomes.Add(((int)PlanetSettings.BiomesPlanets.Mountain, _mountainProb));
            _planet.Biomes.Add(((int)PlanetSettings.BiomesPlanets.Tundra, _tundraProb));
            _planet.Biomes.Add(((int)PlanetSettings.BiomesPlanets.SnowyMountains, _snowyMountainsProb));
            _planet.Biomes.Add(((int)PlanetSettings.BiomesPlanets.Grasslands, _grasslandsProb));
            _planet.Biomes.Add(((int)PlanetSettings.BiomesPlanets.Desert, _desertProb));
            _planet.Biomes.Add(((int)PlanetSettings.BiomesPlanets.Forest, _forestProb));
            _planet.Biomes.Add(((int)PlanetSettings.BiomesPlanets.Barren, _barrenProb));
            _planet.Biomes.Add(((int)PlanetSettings.BiomesPlanets.Barren, 0.7f));
            _planet.Biomes.Add(((int)PlanetSettings.BiomesPlanets.Mountain, 0.3f));
        }
        

        

        //Debug.Log("Temperature: " + _planetTemperature);
        //Debug.Log("Planet Mass: " + _planetMass);
        _planetScript.biomeProbabilities = _planet.Biomes;
        _planetScript.havePoles = havePoles;
        _planetScript.planetMass = _planetMass;
        _planetScript.planetTemperature = _planetTemperature;

        

        return _planetScript;
    }

    private void AttachAtmosphereToGO(GameObject _planetObj, PlanetSettings _planet)
    {
        //create atmosphere
        GameObject _atmosphereObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _atmosphereObj.name = "Atmosphere" + _planet.DistanceFromStar;
        //parent atmosphere to planet
        
        float _atmosphereScale = _planet.planetRadius / 300;
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
    public void DestroySolarSystem()
    {
        DebugResetSolarSystemPlan();
        GameObject[] _planets = GameObject.FindGameObjectsWithTag("Planets");
        foreach (GameObject _planet in _planets)
        {
            DestroyImmediate(_planet);
        }
    }
}


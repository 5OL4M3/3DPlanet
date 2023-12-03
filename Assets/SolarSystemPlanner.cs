using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.IO;
using DTT.Utils.Extensions;

public class SolarSystemPlanner : MonoBehaviour
{
    //Script generates a plan for a solar system, can also save and load the plan
    [SerializeField]
    public SolarSystemPlan solarSystemPlan;

    [SerializeField] 
    [ReadOnly]
    public GlobalVars globalSettings;

    public void Awake()
    {
        if (solarSystemPlan == null)
        {
            solarSystemPlan = new SolarSystemPlan();
        }

        if (globalSettings == null)
        {
            //grab from "settings" tag
            GameObject[] _settings = GameObject.FindGameObjectsWithTag("Settings");
            if (_settings.Length == 0)
            {
                Debug.LogError("No settings found");
                return;
            }
            if (_settings.Length > 1)
            {
                Debug.LogError("Multiple settings found");
                return;
            }
            GameObject _globalSettingsobj = _settings[0];
            Debug.Log("name: " + _globalSettingsobj.name);
            GlobalsLoader _GlobalsLoaderCompt = _globalSettingsobj.GetComponent<GlobalsLoader>();
            if (_GlobalsLoaderCompt == null)
            {
                Debug.LogError("No GlobalsLoader found");
                return;
            }
            globalSettings = _GlobalsLoaderCompt.globalVars;
            Debug.Log("Global settings Found");
        }
        Debug.Log("Global settings: " + globalSettings);
        Debug.Log("Global Setting Seed: " + globalSettings.seed);
    }



    //generate random solar system plan
    [Button]
    private void GenerateRandomSolarSystemPlan()
    {
        ResetRandomSeed();
        solarSystemPlan = new SolarSystemPlan();
        int seed = Random.Range(0, 1000000);
        StarSettings starSettings = new StarSettings();
        starSettings.seed = seed;
        seed += 1;
        starSettings.starRadius = Random.Range(20, 80) + Random.Range(-20, 20);
        starSettings.starMass = Random.Range(20, 80) + Random.Range(-20, 20);
        starSettings.starTemperature = Random.Range(20, 80) + Random.Range(-20, 20);
        solarSystemPlan.star = starSettings;

        int numberOfPlanets = Random.Range(1, 10);
        for (int i = 0; i < numberOfPlanets; i++)
        {
            PlanetSettings planetSettings = new PlanetSettings();
            planetSettings.seed = seed;
            SetRandomSeedToNumber(seed);
            seed += 1;
            
            planetSettings.planetRadius = Random.Range(30, 120) + Random.Range(-30, 30);
            planetSettings.planetMass = Random.Range(20, 80) + Random.Range(-20, 20);
            planetSettings.planetTemperature = Random.Range(20, 80) + Random.Range(-20, 20);
            planetSettings.planetMoons = Random.Range(0, 2);
            planetSettings.planetOrbit = Random.Range(10, 40) + Random.Range(-10, 10);
            planetSettings.planetOrbitSpeedSelf = Random.Range(10, 40) + Random.Range(-10, 10);
            planetSettings.planetOrbitSpeed = Random.Range(20, 80) + Random.Range(-20, 20);
            planetSettings.minDistanceBetweenMoons = Random.Range(20, 80) + Random.Range(-20, 20);
            planetSettings.maxDistanceBetweenMoons = Random.Range(20, 80) + Random.Range(-20, 20);
            planetSettings.VarDistanceBetweenMoons = Random.Range(20, 80) + Random.Range(-20, 20);
            planetSettings.DistanceFromStar = Random.Range(40, 200) + Random.Range(-20, 20);

            int numberOfMoons = Random.Range(0, 10);
            for (int j = 0; j < numberOfMoons; j++)
            {
                MoonSettings moonSettings = new MoonSettings();
                moonSettings.seed = seed;
                SetRandomSeedToNumber(seed);

                seed += 1;
                moonSettings.moonRadius = Random.Range(20, 80) + Random.Range(-20, 20);
                moonSettings.moonMass = Random.Range(20, 80) + Random.Range(-20, 20);
                moonSettings.moonTemperature = Random.Range(20, 80) + Random.Range(-20, 20);
                moonSettings.moonOrbit = Random.Range(20, 80) + Random.Range(-20, 20);
                moonSettings.moonOrbitSpeed = Random.Range(20, 80) + Random.Range(-20, 20);
                moonSettings.DistanceFromPlanet = Random.Range(20, 80) + Random.Range(-20, 20);
                planetSettings.moons.Add(moonSettings);
            }
            //sort moons by distance from planet
            planetSettings.moons.Sort((x, y) => x.DistanceFromPlanet.CompareTo(y.DistanceFromPlanet));
            solarSystemPlan.planets.Add(planetSettings);
            //sort planets by distance from star
            solarSystemPlan.planets.Sort((x, y) => x.DistanceFromStar.CompareTo(y.DistanceFromStar));
        }
    }

    //Generate using global settings
    [Button]
    private void GenerateSolarSystemPlanFromGlobalSettings()
    {
        DefaultStarSettings defStar = globalSettings.defaultStarSettings;
        DefaultPlanetSettings defPlanet = globalSettings.defaultPlanetSettings;
        DefaultMoonSettings defMoon = globalSettings.defaultMoonSettings;
        solarSystemPlan = new SolarSystemPlan();
        int seed = globalSettings.seed;
        StarSettings starSettings = new StarSettings();
        starSettings.seed = seed;
        SetRandomSeedToNumber(seed);
        seed += 1;

        starSettings.starRadius = Random.Range(defStar.starRadiusBaseMin, defStar.starRadiusBaseMax) + Random.Range(-defStar.starRadiusVarience, defStar.starRadiusVarience);
        starSettings.starMass = Random.Range(defStar.starMassBaseMin, defStar.starMassBaseMax) + Random.Range(-defStar.starMassVarience, defStar.starMassVarience);
        starSettings.starTemperature = Random.Range(defStar.starTemperatureBaseMin, defStar.starTemperatureBaseMax) + Random.Range(-defStar.starTemperatureVarience, defStar.starTemperatureVarience);
        solarSystemPlan.star = starSettings;
        float numberOfPlanetsFloat = Random.Range(defPlanet.AveragePlanets - defPlanet.PlanetVarience, defPlanet.AveragePlanets + defPlanet.PlanetVarience);
        int numberOfPlanets = Mathf.RoundToInt(numberOfPlanetsFloat);
        Debug.Log("Number of planets: " + numberOfPlanets);
        for (int i = 0; i < numberOfPlanets; i++)
        {
            PlanetSettings planetSettings = new PlanetSettings();
            planetSettings.seed = seed;
            SetRandomSeedToNumber(seed);
            seed += 1;

            planetSettings.planetRadius = Random.Range(defPlanet.planetRadiusBaseMin, defPlanet.planetRadiusBaseMax) + Random.Range(-defPlanet.planetRadiusVarience, defPlanet.planetRadiusVarience);
            planetSettings.planetMass = Random.Range(defPlanet.planetMassBaseMin, defPlanet.planetMassBaseMax) + Random.Range(-defPlanet.planetMassVarience, defPlanet.planetMassVarience);
            planetSettings.planetTemperature = Random.Range(defPlanet.planetTemperatureBaseMin, defPlanet.planetTemperatureBaseMax) + Random.Range(-defPlanet.planetTemperatureVarience, defPlanet.planetTemperatureVarience);
            planetSettings.planetOrbit = Random.Range(defPlanet.planetOrbitBaseMin, defPlanet.planetOrbitBaseMax) + Random.Range(-defPlanet.planetOrbitVarience, defPlanet.planetOrbitVarience);
            planetSettings.planetOrbitSpeed = Random.Range(defPlanet.planetOrbitSpeedBaseMin, defPlanet.planetOrbitSpeedBaseMax) + Random.Range(-defPlanet.planetOrbitSpeedVarience, defPlanet.planetOrbitSpeedVarience);
            planetSettings.planetOrbitSpeedSelf = Random.Range(defPlanet.planetOrbitSpeedSelfBaseMin, defPlanet.planetOrbitSpeedSelfBaseMax) + Random.Range(-defPlanet.planetOrbitSpeedSelfVarience, defPlanet.planetOrbitSpeedSelfVarience);
            planetSettings.DistanceFromStar = Random.Range(defPlanet.DistanceFromStarBaseMin, defPlanet.DistanceFromStarBaseMax) + Random.Range(-defPlanet.DistanceFromStarVarience, defPlanet.DistanceFromStarVarience);
            planetSettings.minDistanceBetweenMoons = defPlanet.minDistanceBetweenMoonsBaseMin;
            planetSettings.maxDistanceBetweenMoons = defPlanet.minDistanceBetweenMoonsBaseMax;
            planetSettings.VarDistanceBetweenMoons = defPlanet.minDistanceBetweenMoonsVarience;

            float AverageMoons = Random.Range(defPlanet.AverageMoons - defPlanet.MoonVarience, defPlanet.AverageMoons + defPlanet.MoonVarience);
            float MoonVarience = defPlanet.MoonVarience;


            float numberOfMoonsFloat = Random.Range(AverageMoons - MoonVarience, AverageMoons + MoonVarience);
            int numberOfMoons = Mathf.RoundToInt(numberOfMoonsFloat);
            Debug.Log("Number of Moons: " + numberOfMoons);

            for (int j = 0; j < numberOfMoons; j++)
            {
                MoonSettings moonSettings = new MoonSettings();
                moonSettings.seed = seed;
                SetRandomSeedToNumber(seed);
                seed += 1;

                moonSettings.moonRadius = Random.Range(defMoon.moonRadiusBaseMin, defMoon.moonRadiusBaseMax) + Random.Range(-defMoon.moonRadiusVarience, defMoon.moonRadiusVarience);
                moonSettings.moonMass = Random.Range(defMoon.moonMassBaseMin, defMoon.moonMassBaseMax) + Random.Range(-defMoon.moonMassVarience, defMoon.moonMassVarience);
                moonSettings.moonTemperature = Random.Range(defMoon.moonTemperatureBaseMin, defMoon.moonTemperatureBaseMax) + Random.Range(-defMoon.moonTemperatureVarience, defMoon.moonTemperatureVarience);
                moonSettings.moonOrbit = Random.Range(defMoon.moonOrbitBaseMin, defMoon.moonOrbitBaseMax) + Random.Range(-defMoon.moonOrbitVarience, defMoon.moonOrbitVarience);
                moonSettings.moonOrbitSpeed = Random.Range(defMoon.moonOrbitSpeedBaseMin, defMoon.moonOrbitSpeedBaseMax) + Random.Range(-defMoon.moonOrbitSpeedVarience, defMoon.moonOrbitSpeedVarience);
                moonSettings.DistanceFromPlanet = Random.Range(defMoon.DistanceFromPlanetBaseMin, defMoon.DistanceFromPlanetBaseMax) + Random.Range(-defMoon.DistanceFromPlanetVarience, defMoon.DistanceFromPlanetVarience);
                planetSettings.moons.Add(moonSettings);
                //sort moons by distance from planet
                planetSettings.moons.Sort((x, y) => x.DistanceFromPlanet.CompareTo(y.DistanceFromPlanet));
            }


            solarSystemPlan.planets.Add(planetSettings);
            //sort planets by distance from star
            solarSystemPlan.planets.Sort((x, y) => x.DistanceFromStar.CompareTo(y.DistanceFromStar));
        }
    }

        //Force refresh settings
    [Button]
    private void RefreshSettings()
    {
        //grab from "settings" tag
            GameObject[] settings = GameObject.FindGameObjectsWithTag("Settings");
            if (settings.Length == 0)
            {
                Debug.LogError("No settings found");
                return;
            }
            if (settings.Length > 1)
            {
                Debug.LogError("Multiple settings found");
                return;
            }
            GameObject globalSettingsobj = settings[0];
            Debug.Log("name: " + globalSettingsobj.name);
            GlobalsLoader GlobalsLoaderCompt = globalSettingsobj.GetComponent<GlobalsLoader>();
            if (GlobalsLoaderCompt == null)
            {
                Debug.LogError("No GlobalsLoader found");
                return;
            }
            globalSettings = GlobalsLoaderCompt.globalVars;
            Debug.Log("Global settings Found");
    }

    //clear the solar system plan
    [Button]
    private void ClearSolarSystemPlan()
    {
        solarSystemPlan = new SolarSystemPlan();
    }

    //Clear loaded Settings
    [Button]
    private void ClearLoadedSettings()
    {
        globalSettings = new GlobalVars();
    }

    //in streamable assets, create a text file called SolarSystemPlan.json
    [SerializeField] private string SolarSystemPlanPath = "SolarSystemPlan.json";
    
    [Button]
    private void SaveSolarSystemPlanToFileWithJSON()
    {
        //Write some text to the test.txt file
        string rootPath = Application.streamingAssetsPath;
        string filePath = System.IO.Path.Combine(rootPath, SolarSystemPlanPath);
        if (!System.IO.Directory.Exists(rootPath))
        {
            System.IO.Directory.CreateDirectory(rootPath);
        }
        if (!System.IO.File.Exists(filePath))
        {
            System.IO.File.Create(filePath).Dispose();
        }
        string json = JsonUtility.ToJson(solarSystemPlan);
        System.IO.File.WriteAllText(filePath, json);
        //print
        Debug.Log("Saved to " + filePath);

    }

    //create a button to load the solar system plan from a file
    [Button]
    private void LoadSolarSystemPlanFromFile()
    {
        string rootPath = Application.streamingAssetsPath;
        string filePath = System.IO.Path.Combine(rootPath, SolarSystemPlanPath);
        if (!System.IO.Directory.Exists(rootPath))
        {
            System.IO.Directory.CreateDirectory(rootPath);
        }
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogError("File does not exist");
            return;
        }
        string json = System.IO.File.ReadAllText(filePath);
        solarSystemPlan = JsonUtility.FromJson<SolarSystemPlan>(json);
        //print
        Debug.Log("Loaded from " + filePath);
    }

    //resets random seed
    private void ResetRandomSeed()
    {
        Random.InitState(System.Environment.TickCount);
    }

    //sets random seed to the seed of the solar system plan
    private void SetRandomSeedToNumber(int seed)
    {
        Random.InitState(seed);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class GlobalVars
{
    // Global script for storing global variables
    public int seed = 42;
    public bool RotateSolarSystem = true;
    public float RotateSpeed = 1;
    public DefaultPlanetSettings defaultPlanetSettings = new DefaultPlanetSettings();

    public DefaultStarSettings defaultStarSettings = new DefaultStarSettings();
    public DefaultMoonSettings defaultMoonSettings = new DefaultMoonSettings();
}

[System.Serializable]
public class DefaultStarSettings
{
    // Default star settings
    public float starRadiusVarience = 20;
    public float starRadiusBaseMin = 20;
    public float starRadiusBaseMax = 80;

    public float starMassVarience = 20;
    public float starMassBaseMin = 20;
    public float starMassBaseMax = 80;

    public float starTemperatureVarience = 20;
    public float starTemperatureBaseMin = 20;
    public float starTemperatureBaseMax = 80;
    public string name = "Solar System";
}

[System.Serializable]
public class StarSettings
{
    // Global script for storing global variables
    public int seed = 0;
    public float starRadius = 1;

    public float starMass = 1;

    public float starTemperature = 1;
    public string name = "Solar System";
}

[System.Serializable]
public class DefaultPlanetSettings
{
    // Default star settings
    public float planetRadiusVarience = 20;
    public float planetRadiusBaseMin = 40;
    public float planetRadiusBaseMax = 120;

    public float planetMassVarience = 20;
    public float planetMassBaseMin = 20;
    public float planetMassBaseMax = 80;

    public float planetTemperatureVarience = 20;
    public float planetTemperatureBaseMin = 20;
    public float planetTemperatureBaseMax = 80;

    public float planetOrbitVarience = 20;
    public float planetOrbitBaseMin = 20;
    public float planetOrbitBaseMax = 80;

    public float planetOrbitSpeedVarience = 10;
    public float planetOrbitSpeedBaseMin = 10;
    public float planetOrbitSpeedBaseMax = 40;
    public float planetOrbitSpeedSelfVarience = 10;
    public float planetOrbitSpeedSelfBaseMin = 10;
    public float planetOrbitSpeedSelfBaseMax = 40;

    public float minDistanceBetweenMoonsVarience = 15;
    public float minDistanceBetweenMoonsBaseMin = 15;
    public float minDistanceBetweenMoonsBaseMax = 60;

    public float DistanceFromStarVarience = 35;
    public float DistanceFromStarBaseMin = 35;
    public float DistanceFromStarBaseMax = 160;

    public float AverageMoons = 3;
    public float MoonVarience = 1;

    public float AveragePlanets = 5;
    public float PlanetVarience = 2;

    public string name = "Planet";
}

[System.Serializable]
public class PlanetSettings
{
    // Global script for storing global variables
    public int seed = 0;
    public float planetRadius = 1;

    public float planetMass = 1;

    public float planetTemperature = 1;

    public float planetMoons = 0;

    public float planetOrbit = 1;

    public float planetOrbitSpeed = 1;
    public float planetOrbitSpeedSelf = 1;

    public float minDistanceBetweenMoons = 1;
    public float maxDistanceBetweenMoons = 1;
    public float VarDistanceBetweenMoons = 1;

    public float DistanceFromStar = 1;

    public List<MoonSettings> moons = new List<MoonSettings>();

    public enum BiomesPlanets
    {
        Desert,
        Forest,
        Tundra,
        Mountain,
        Barren,
        Grasslands,
        SnowyMountains,
        None
    }
    public List<(int, float)> Biomes = new List<(int, float)>();

    public string name = "Planet";

}

[System.Serializable]
public class DefaultMoonSettings
{
    // Default star settings
    public float moonRadiusVarience = 20;
    public float moonRadiusBaseMin = 20;
    public float moonRadiusBaseMax = 80;

    public float moonMassVarience = 20;
    public float moonMassBaseMin = 20;
    public float moonMassBaseMax = 80;

    public float moonTemperatureVarience = 20;
    public float moonTemperatureBaseMin = 20;
    public float moonTemperatureBaseMax = 80;

    public float moonOrbitVarience = 20;
    public float moonOrbitBaseMin = 20;
    public float moonOrbitBaseMax = 80;

    public float moonOrbitSpeedVarience = 20;
    public float moonOrbitSpeedBaseMin = 20;
    public float moonOrbitSpeedBaseMax = 80;

    public float DistanceFromPlanetVarience = 20;
    public float DistanceFromPlanetBaseMin = 20;
    public float DistanceFromPlanetBaseMax = 80;

    public string name = "Moon";
}

[System.Serializable]
public class MoonSettings
{
    // Global script for storing global variables
    public int seed = 0;
    public float moonRadius = 1;

    public float moonMass = 1;

    public float moonTemperature = 1;

    public float moonOrbit = 1;

    public float moonOrbitSpeed = 1;

    public float DistanceFromPlanet = 1;

    public string name = "Moon";
}



[System.Serializable]
public class SolarSystemPlan
{
    public List<PlanetSettings> planets = new List<PlanetSettings>();
    public StarSettings star = new StarSettings();

    
}


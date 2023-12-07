using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeGenerator
{
    ShapeSettings shapeSettings;
    NoiseFilter[] noiseFilters;

    public ShapeGenerator(ShapeSettings shapeSettings)
    {
        this.shapeSettings = shapeSettings;
        noiseFilters = new NoiseFilter[shapeSettings.terrianSetting.Length];
        for (int i = 0; i < noiseFilters.Length; i++)
        {
            noiseFilters[i] = new NoiseFilter(shapeSettings.terrianSetting[i].noiseSettings, 0);
        }
    }

    public Vector3 CalculatePointOnPlanet(Vector3 pointOnUnitSphere, int seed)
    {
        float elevation = 0;
        float firstLayerValue = 0;
        float oceanLayer = 0;
        //Ocean Layer
        if (shapeSettings.oceanSetting.enable)
        {
            oceanLayer = shapeSettings.oceanSetting.Evaluate(pointOnUnitSphere, seed);
        }

        if (noiseFilters.Length > 0)
        {
            firstLayerValue = noiseFilters[0].Evaluate(pointOnUnitSphere, seed);
            if (shapeSettings.terrianSetting[0].enable)
            {
                elevation += firstLayerValue;
            }
        }

        //Continent Shape
        for (int i = 0; i < noiseFilters.Length; i++)
        {
            if (shapeSettings.terrianSetting[i].enable)
            {
                elevation += noiseFilters[i].Evaluate(pointOnUnitSphere, seed);
            }
            
        }

        elevation = (oceanLayer == 0) ? elevation : oceanLayer;
        return pointOnUnitSphere * shapeSettings.planetRadius * (1 + elevation);
    }

    public Vector3 CalculatePointOnOcean(Vector3 pointOnUnitSphere, int seed)
    {
        return pointOnUnitSphere * shapeSettings.planetRadius;
    }
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeGenerator
{
    ShapeSettings shapeSettings;
    INoiseFilter[] noiseFilters;

    public ShapeGenerator(ShapeSettings shapeSettings)
    {
        this.shapeSettings = shapeSettings;
        noiseFilters = new INoiseFilter[shapeSettings.noiseLayers.Length];
        for (int i = 0; i < noiseFilters.Length; i++)
        {
            noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter(shapeSettings.noiseLayers[i].noiseSettings);
        }
    }

    public Vector3 CalculatePointOnPlanet(Vector3 pointOnUnitSphere)
    {
        float elevation = 0;
        float firstLayerValue = 0;
        float oceanLayer = 0;
        //Ocean Layer
        if (shapeSettings.oceanSetting.enable)
        {
            oceanLayer = shapeSettings.oceanSetting.Evaluate(pointOnUnitSphere);
        }

        if (noiseFilters.Length > 0)
        {
            firstLayerValue = noiseFilters[0].Evaluate(pointOnUnitSphere);
            if (shapeSettings.noiseLayers[0].enable)
            {
                elevation += firstLayerValue;
            }
        }

        //Continent Shape
        for (int i = 0; i < noiseFilters.Length; i++)
        {
            if (shapeSettings.noiseLayers[i].enable)
            {
                float mask = (shapeSettings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;
                elevation += noiseFilters[i].Evaluate(pointOnUnitSphere) * mask;
            }
            
        }

        elevation = (oceanLayer == 0) ? elevation : oceanLayer;
        return pointOnUnitSphere * shapeSettings.planetRadius * (1 + elevation);
    }

    public Vector3 CalculatePointOnOcean(Vector3 pointOnUnitSphere)
    {
        return pointOnUnitSphere * shapeSettings.planetRadius;
    }
}



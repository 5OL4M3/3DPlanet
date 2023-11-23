using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFilter : INoiseFilter
{
    NoiseSettings.SimpleNoiseSettings settings;
    Noise noise = new Noise();

    public NoiseFilter(NoiseSettings.SimpleNoiseSettings settings)
    {
        this.settings = settings;
    }

    public float Evaluate(Vector3 point)
    {
        float noiseVal = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < settings.numberLayers; i++)
        {
            noiseVal += (noise.Evaluate(point * frequency + settings.center) + 1) / 2 * amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.persistence;
        }

        noiseVal = Mathf.Max(0, noiseVal - settings.minValue);
        return noiseVal * settings.strength;
    }
}

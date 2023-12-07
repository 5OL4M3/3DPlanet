using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidNoiseFilter : INoiseFilter
{
    NoiseSettings.RigidNoiseSettings settings;
    Noise noise = new Noise();

    public RigidNoiseFilter(NoiseSettings.RigidNoiseSettings settings, int seed)
    {
        this.settings = settings;

    }

    public float Evaluate(Vector3 point, int seed)
    {
        float noiseVal = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;
        float weight = 1;

        //add seed offset to center
        seed *= 10;
        seed %= 100;
        
        Vector3 offset = new Vector3(settings.center.x + seed, settings.center.y + seed, settings.center.z + seed);

        for (int i = 0; i < settings.numberLayers; i++)
        {
            //noiseVal += ((Mathf.PerlinNoise(point.x * frequency + settings.center.x, point.y * frequency + settings.center.y) + 1) / 2 * amplitude);
            float v = 1 - Mathf.Abs(noise.Evaluate(point * frequency + offset));
            //float v = 1 - Mathf.Abs(Mathf.PerlinNoise(point.x * frequency + settings.center.x, point.y * frequency + settings.center.y));
            v *= v;
            v *= weight;
            weight = Mathf.Clamp01(v * settings.weightMultiplier);

            noiseVal += v * amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.persistence;
        }

        noiseVal = Mathf.Max(0, noiseVal - settings.minValue);
        return noiseVal * settings.strength;
    }
}

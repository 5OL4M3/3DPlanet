using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFilter
{
    NoiseSettings settings;
    Noise noise = new Noise();

    public NoiseFilter(NoiseSettings settings, int seed)
    {
        this.settings = settings;

    }

    public float Evaluate(Vector3 point, int seed)
    {
        if (settings.filterType == NoiseSettings.FilterType.Simple)
        {
            float noiseVal = 0;
            float frequency = settings.baseRoughness;
            float amplitude = 1;

            

            for (int i = 0; i < settings.numberLayers; i++)
            {
                //add seed offset to center
                seed *= 5;
                seed %= 100;
                
                Vector3 offset = new Vector3(settings.center.x + seed, settings.center.y + seed, settings.center.z + seed);
                
                noiseVal += ((noise.Evaluate(point * frequency + offset) + 1) / 2 * amplitude);
                //noiseVal += ((Mathf.PerlinNoise(point.x * frequency + settings.center.x, point.y * frequency + settings.center.y) + 1) / 2 * amplitude);
                frequency *= settings.roughness;
                amplitude *= settings.persistence;
            }

            noiseVal = Mathf.Max(0, noiseVal - settings.minValue);
            return noiseVal * settings.strength;
        }
        else if (settings.filterType == NoiseSettings.FilterType.Rigid) 
        {
            float noiseVal = 0;
            float frequency = settings.baseRoughness;
            float amplitude = 1;
            float weight = 1;

            for (int i = 0; i < settings.numberLayers; i++)
            {
                //add seed offset to center
                seed *= 5;
                seed %= 100;
                
                Vector3 offset = new Vector3(settings.center.x + seed, settings.center.y + seed, settings.center.z + seed);

                float v = 1 - Mathf.Abs(noise.Evaluate(point * frequency + offset));
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
        
        return 0;
    }
}

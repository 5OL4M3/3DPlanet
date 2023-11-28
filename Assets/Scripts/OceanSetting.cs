using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OceanSetting
{
    public bool enable;
    [Range(-0.5f, 0.0f)]
    public float depth = -0.35f;
    public float baseRoughness = 1;
    public float roughness = 2;
    public float minValue = 1;
    public Vector3 center;
    private int numberLayers = 3;
    private float persistence = 0.5f;

    Noise noise = new Noise();

    public float Evaluate(Vector3 point)
    {
        float noiseVal = 0;
        float frequency = baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < numberLayers; i++)
        {
            noiseVal += (noise.Evaluate(point * frequency + center) + 1) / 2 * amplitude;
            frequency *= roughness;
            amplitude *= persistence;
        }

        noiseVal = Mathf.Max(0, noiseVal - minValue);

        return noiseVal * depth;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ShapeSettings : ScriptableObject
{
    public float planetRadius = 1;
    public NoiseLayer[] terrianSetting;
    public OceanSetting oceanSetting;

    [System.Serializable]
    public class NoiseLayer
    {
        public bool enable = true;
        public NoiseSettings noiseSettings;
    }
}
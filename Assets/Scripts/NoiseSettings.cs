using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public enum FilterType { Simple, Rigid };
    public FilterType filterType;

    public float strength = 1;
    [Range(1,8)]
    public int numberLayers = 1;
    public float baseRoughness = 1;
    public float roughness = 2;
    public float persistence = 0.5f; 
    public Vector3 center;
    public float minValue;

    [Space]    
    public float weightMultiplier = .8f;
    
}

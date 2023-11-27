using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRotate : MonoBehaviour
{
    private bool RotateSolarSystem;
    public float RotateSpeed;
    public float DistanceFromStar;
    public float PlanetRadius;
    private GlobalVars globalSettings;

    // Start is called before the first frame update
    void Awake()
    {
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

        RotateSolarSystem = globalSettings.RotateSolarSystem;
    }

    //fixed update for physics
    void FixedUpdate()
    {
        if (RotateSolarSystem)
        {
            transform.RotateAround(Vector3.zero, Vector3.up, globalSettings.RotateSpeed * RotateSpeed * Time.deltaTime);
        }
    }
    

}

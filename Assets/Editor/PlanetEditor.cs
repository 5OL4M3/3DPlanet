using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{
    Planet planet;
    Editor shapeEditor;
    Editor colorEditor;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                planet.GeneratePlanet(planet.PlanetSplitCount);
            }
        }
        
        if (GUILayout.Button("Generate Planet"))
        {
            planet.GeneratePlanet(planet.PlanetSplitCount);
        }
        
        DrawSettingsEditor(planet.shapeSettings, planet.OnShapeSettingsUpdated, ref planet.shapeSettingsFold, ref shapeEditor);
    }

    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool fold, ref Editor editor)
    {
        if (settings != null)
        {
            fold = EditorGUILayout.InspectorTitlebar(fold, settings);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (fold)
                {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();

                    if (check.changed)
                    {
                        if (onSettingsUpdated != null)
                        {
                            onSettingsUpdated();
                        }
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        planet = (Planet)target;
    }
}

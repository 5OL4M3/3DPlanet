using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {

    [Range(2,256)]
    public int resolution = 10;

    public ShapeSettings shapeSettings;
    public ColorSettings colorSettings;
    public enum FaceRenderMask {All, Top, Bottom, Left, Right, Front, Back};
    public FaceRenderMask faceRenderMask;

    public bool autoUpdate = true;
    public bool ocean = true;
    
    [HideInInspector]
    public bool shapeSettingsFold;
    [HideInInspector]
    public bool colorSettingsFold;

    ShapeGenerator shapeGenerator;

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    

	void Initialize()
    {
        shapeGenerator = new ShapeGenerator(shapeSettings);

        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
                
            }

            terrainFaces[i] = new TerrainFace(shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i]);
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int) faceRenderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace);
        }
    }

    MeshFilter[] oceanFilters;
    TerrainFace[] oceanFaces;
    void InitializeOcean()
    {
        shapeGenerator = new ShapeGenerator(shapeSettings);

        if (oceanFilters == null || oceanFilters.Length == 0)
        {
            oceanFilters = new MeshFilter[6];
        }
        oceanFaces = new TerrainFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            if (oceanFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                oceanFilters[i] = meshObj.AddComponent<MeshFilter>();
                oceanFilters[i].sharedMesh = new Mesh();
                
            }

            oceanFaces[i] = new TerrainFace(shapeGenerator, oceanFilters[i].sharedMesh, resolution, directions[i]);
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int) faceRenderMask - 1 == i;
            oceanFilters[i].gameObject.SetActive(renderFace);
        }
    }
    
    public void GeneratePlanet()
    {
        Initialize();
        GenerateMesh();
        InitializeOcean();
        GenerateOcean();
        GenerateColors();
        GenerateOceanColors();
    }

    public void OnShapeSettingsUpdated()
    {
        if (autoUpdate) {
            Initialize();
            GenerateMesh();
            InitializeOcean();
            GenerateOcean();
        }
    }

    public void OnColorSettingsUpdated()
    {
        if (autoUpdate) {
            Initialize();
            GenerateMesh();
            InitializeOcean();
            GenerateOcean();
        }
    }

    void GenerateMesh()
    {
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();
            }
        }
    }

    void GenerateOcean()
    {
        for (int i = 0; i < 6; i++)
        {
            if (ocean)
            {
                oceanFaces[i].ConstructOceanMesh();
            }
        }
    }

    void GenerateColors()
    {
        foreach (MeshFilter filter in meshFilters)
        {
            filter.GetComponent<MeshRenderer>().sharedMaterial.color = colorSettings.planetColor;
        }
    }

    void GenerateOceanColors()
    {
        foreach (MeshFilter filter in oceanFilters)
        {
            filter.GetComponent<MeshRenderer>().sharedMaterial.color = Color.blue; 
        }
    }
    
}
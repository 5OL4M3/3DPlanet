using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;

public class Planet : MonoBehaviour
{

    [Range(2, 256)]
    public int resolution = 96;

    public ShapeSettings shapeSettings;
    public ColorSettings colorSettings;
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back };
    public FaceRenderMask faceRenderMask;

    public bool autoUpdate = true;
    public bool ocean = true;

    [HideInInspector]
    public bool shapeSettingsFold;
    [HideInInspector]
    public bool colorSettingsFold;

    ShapeGenerator shapeGenerator;

    [SerializeField]
    [HideInInspector]
    //MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    public int PlanetSplitCount = 2;

    //public int PlanetMeshCount = 24;

    //MeshFilter[] oceanFilters;
    TerrainFace[] oceanFaces;

    //-----------------
    //Parameters from SolarSystemGenerator
    public float planetRadius = 50;



    void Initialize(out TerrainFace[] _returnedFaces, out MeshFilter[] _returnedMeshFilters, int _PlanetSplitCount)
    {
        int PlanetMeshCount = _PlanetSplitCount * _PlanetSplitCount * 6;

        shapeGenerator = new ShapeGenerator(shapeSettings);

        TerrainFace[] _terrainFaces = new TerrainFace[PlanetMeshCount];
        MeshFilter[] _meshFilters = new MeshFilter[PlanetMeshCount];
        int meshesPerFace = _PlanetSplitCount * _PlanetSplitCount;

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        //create parent gameobject for meshes
        GameObject meshObjParent = new GameObject("meshes");
        meshObjParent.transform.parent = transform;
        meshObjParent.transform.localPosition = Vector3.zero;
        

        //(1,1,1) is about 50 in absolute scale
        meshObjParent.transform.localScale = new Vector3(planetRadius / 50, planetRadius / 50, planetRadius / 50);
        Debug.Log("meshObjParent: " + meshObjParent + " scale: " + meshObjParent.transform.localScale);

        //create gameobjects for meshes
        for (int i = 0; i < PlanetMeshCount; i++)
        {
            if (_meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = meshObjParent.transform;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                _meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                _meshFilters[i].sharedMesh = new Mesh();

            }
        }

        //create terrain faces
        for (int i = 0; i < 6; i++)
        {
            Mesh[] meshArray = new Mesh[meshesPerFace];
            for (int j = 0; j < meshesPerFace; j++)
            {
                meshArray[j] = _meshFilters[i * meshesPerFace + j].sharedMesh;
            }
            _terrainFaces[i] = new TerrainFace(shapeGenerator, meshArray, resolution, directions[i % 6], _PlanetSplitCount);

        }

        //match render mask to mesh filters and enable/disable
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < meshesPerFace; j++)
            {
                if (faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i)
                {
                    _meshFilters[i * meshesPerFace + j].gameObject.SetActive(true);
                }
                else
                {
                    _meshFilters[i * meshesPerFace + j].gameObject.SetActive(false);
                }
            }
        }


        _returnedFaces = _terrainFaces;
        _returnedMeshFilters = _meshFilters;
    }




    public void GeneratePlanet(int _PlanetSplitCount)
    {
        //check for old meshes in children, delete if name includes "planetmesh" or "oceanmesh"
        foreach (Transform child in transform)
        {
            if (child.name.Contains("planetmesh") || child.name.Contains("oceanmesh") || child.name.Contains("meshes"))
            {
                DestroyImmediate(child.gameObject);
            }

        }
        MeshFilter[] mfland = new MeshFilter[_PlanetSplitCount * _PlanetSplitCount * 6];
        MeshFilter[] mfocean = new MeshFilter[_PlanetSplitCount * _PlanetSplitCount * 6];
        Initialize(out terrainFaces, out mfland, PlanetSplitCount);
        
        mfland = GenerateMesh(mfland, PlanetSplitCount);
        Initialize(out oceanFaces, out mfocean, 1);
        GenerateOcean(mfocean);
        //GenerateColors(mfland, colorSettings.planetColor);
        GenerateColors(mfocean, Color.blue);

        setToRandomMeshColors(mfland);
        //subDivideMeshes(meshFilters);
        //GenerateColors(newMeshFilters);
    }




    public void OnShapeSettingsUpdated()
    {
        if (autoUpdate)
        {
            GeneratePlanet(PlanetSplitCount);
        }
    }

    public void OnColorSettingsUpdated()
    {
        if (autoUpdate)
        {
            GeneratePlanet(PlanetSplitCount);
        }
    }

    MeshFilter[] GenerateMesh(MeshFilter[] _meshFilters, int _PlanetSplitCount)
    {   
        int PlanetMeshCount = _PlanetSplitCount * _PlanetSplitCount * 6;
        //use the first terrainface to generate the mesh for all

        if (_meshFilters[0].gameObject.activeSelf)
        {
            terrainFaces[0].ConstructLandMesh(_meshFilters);
        }

        //rename the rest of the meshes
        for (int i = 0; i < PlanetMeshCount; i++)
        {
            if (_meshFilters[i].gameObject.activeSelf)
            {
                _meshFilters[i].gameObject.name = "planetmesh";
                //position at 0 relative to parent
                _meshFilters[i].gameObject.transform.localPosition = Vector3.zero;
            }
        }

        return _meshFilters;
    }



    MeshFilter[] GenerateOcean(MeshFilter[] _meshFilters)
    {
        //use the first terrainface to generate the mesh for all

        if (_meshFilters[0].gameObject.activeSelf)
        {
            oceanFaces[0].ConstructOceanMesh(_meshFilters);
        }

        //rename the rest of the meshes
        for (int i = 0; i < 6; i++)
        {
            if (_meshFilters[i].gameObject.activeSelf)
            {
                _meshFilters[i].gameObject.name = "oceanmesh";
                //position at 0 relative to parent
                _meshFilters[i].gameObject.transform.localPosition = Vector3.zero;
            }
        }

        return _meshFilters;

    }

    void GenerateColors(MeshFilter[] mfs, Color col)
    {
        if (mfs == null || mfs.Length == 0)
        {
            return;
        }
        foreach (MeshFilter filter in mfs)
        {
            Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
            mat.color = col;
        }
    }

    void setToRandomMeshColors(MeshFilter[] mfs)
    {
        if (mfs == null || mfs.Length == 0)
        {
            return;
        }
        foreach (MeshFilter filter in mfs)
        {
            Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
            mat.color = Random.ColorHSV();
        }
    }

}

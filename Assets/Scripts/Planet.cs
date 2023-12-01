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
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    public int PlanetSplitCount = 2;

    public int PlanetMeshCount = 24;

    MeshFilter[] oceanFilters;
    TerrainFace[] oceanFaces;



    void Initialize(out TerrainFace[] _returnedFaces, out MeshFilter[] _returnedMeshFilters)
    {
        shapeGenerator = new ShapeGenerator(shapeSettings);

        _checkIfValid();
        TerrainFace[] _terrainFaces = new TerrainFace[PlanetMeshCount];
        MeshFilter[] _meshFilters = new MeshFilter[PlanetMeshCount];
        int meshesPerFace = PlanetSplitCount * PlanetSplitCount;

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        //create gameobjects for meshes
        for (int i = 0; i < PlanetMeshCount; i++)
        {
            if (_meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

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
            _terrainFaces[i] = new TerrainFace(shapeGenerator, meshArray, resolution, directions[i % 6], PlanetSplitCount);

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




    public void GeneratePlanet()
    {
        //check for old meshes in children, delete if name includes "planetmesh" or "oceanmesh"
        foreach (Transform child in transform)
        {
            if (child.name.Contains("planetmesh") || child.name.Contains("oceanmesh"))
            {
                DestroyImmediate(child.gameObject);
            }

        }
        MeshFilter[] mfland = new MeshFilter[PlanetMeshCount];
        MeshFilter[] mfocean = new MeshFilter[PlanetMeshCount];
        Initialize(out terrainFaces, out mfland);
        
        mfland = GenerateMesh(mfland);
        Initialize(out oceanFaces, out mfocean);
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
            GeneratePlanet();
        }
    }

    public void OnColorSettingsUpdated()
    {
        if (autoUpdate)
        {
            GeneratePlanet();
        }
    }

    MeshFilter[] GenerateMesh(MeshFilter[] _meshFilters)
    {
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
        for (int i = 0; i < PlanetMeshCount; i++)
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

        [SerializeField]
        [Button]
        public void _checkIfValid()
        {
            //refresh meshFilters
            meshFilters = new MeshFilter[PlanetMeshCount];
            oceanFilters = new MeshFilter[PlanetMeshCount];
            for (int i = 0; i < PlanetMeshCount; i++)
            {
                meshFilters[i] = null;
                oceanFilters[i] = null;
            }

            //refresh terrainFaces
            terrainFaces = new TerrainFace[PlanetMeshCount];
            oceanFaces = new TerrainFace[PlanetMeshCount];
            for (int i = 0; i < PlanetMeshCount; i++)
            {
                terrainFaces[i] = null;
                oceanFaces[i] = null;
            }
        }

    }

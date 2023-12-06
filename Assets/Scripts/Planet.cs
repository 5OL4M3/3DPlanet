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
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back };
    public FaceRenderMask faceRenderMask;

    public bool autoUpdate = true;
    public bool ocean = true;

    [HideInInspector]
    public bool shapeSettingsFold;

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
    //biome probability

    //bit of a hack but whatever
    private enum _BiomesPlanets
    {
        Desert,
        Forest,
        Tundra,
        Mountain,
        Barren,
        None
    }
    public List<(int, float)> biomeProbabilities = new List<(int, float)>();



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
        meshObjParent.transform.localScale = Vector3.one;
        


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
            if (child.name.Contains("submesh") || child.name.Contains("oceanmesh") || child.name.Contains("meshes"))
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
        ScalePlanetDownToNormalSizeMF(mfland);
        ScalePlanetDownToNormalSizeMF(mfocean);

        setToBiomesDebug(mfland);
        debugPrintBiomesProbs();
        //subDivideMeshes(meshFilters);
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

    private void ScalePlanetDownToNormalSizeTF(TerrainFace[] _tf)
    {
        //scale down to normal size
        //float scaleDown = 0.5f;
        //foreach (TerrainFace tf in _tf)
        //{
        //    tf.transform.localScale = new Vector3(scaleDown, scaleDown, scaleDown);
        //}
    }
    private void ScalePlanetDownToNormalSizeMF(MeshFilter[] _mf)
    {
        //scale down to normal size
        float scaleDown = 0.02f;
        foreach (MeshFilter mf in _mf)
        {
            mf.transform.localScale = new Vector3(scaleDown, scaleDown, scaleDown);
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
                _meshFilters[i].gameObject.name = "submesh";
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

        //Change this script
        foreach (MeshFilter filter in _meshFilters)
        {
            Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
            mat.color = Color.blue;
        }

        return _meshFilters;
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

    void setToBiomesDebug(MeshFilter[] mfs)
    {
        if (mfs == null || mfs.Length == 0)
        {
            return;
        }

        //get the distance from the center of the planet as height, min and max
        float minHeight = 100;
        float maxHeight = 0;
        //get min and max heights
        float minAvgHeightPolar = 10000;
        float maxAvgHeightPolar = 0;
        foreach (MeshFilter filter in mfs)
        {
            //get average height of mesh by getting all y values, clamping to (0,100) and averaging
            Mesh mesh = filter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            float AvgWorldheight = 0;
            Vector3 WorldPlanetCenter = transform.position;
            foreach (Vector3 vertex in vertices)
            {
                float wheight = vertex.y;
                AvgWorldheight += wheight;

                //get sphereical heights
                float distanceFromCenter = Vector3.Distance(vertex, WorldPlanetCenter);
                float height = distanceFromCenter - planetRadius;
                height = Mathf.Clamp(height, -10, 150);
                if (height < minHeight)
                {
                    minHeight = height;
                }
                if (height > maxHeight)
                {
                    maxHeight = height;
                }
            }
            AvgWorldheight /= vertices.Length;
            //Debug.Log("MESH1 Avg world height: " + AvgWorldheight);
            if (AvgWorldheight < minAvgHeightPolar)
            {
                //Debug.Log("MESH1 Avg world height: " + AvgWorldheight + " is less than minHeight: " + minAvgHeightPolar);
                minAvgHeightPolar = AvgWorldheight;
            }
            if (AvgWorldheight > maxAvgHeightPolar)
            {
                //Debug.Log("MESH1 Avg world height: " + AvgWorldheight + " is greater than maxHeight: " + maxAvgHeightPolar);
                maxAvgHeightPolar = AvgWorldheight;
            }
        }
        Debug.Log("Min height: " + minHeight + " Max height: " + maxHeight);
        Debug.Log("MinMax avg height: " + minAvgHeightPolar + " Max avg height: " + maxAvgHeightPolar);
        Debug.Log("Number of meshes: " + mfs.Length);
        foreach (MeshFilter filter in mfs)
        {
            //get average world height and average spherical height
            Mesh mesh = filter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            float sphereheight = 0;
            float worldheight = 0;
            Vector3 WorldPlanetCenter = transform.position;
            foreach (Vector3 vertex in vertices)
            {
                float wheight = vertex.y;
                worldheight += wheight;

                //get sphereical heights
                float distanceFromCenter = Vector3.Distance(vertex, WorldPlanetCenter);
                float height = distanceFromCenter - planetRadius;
                sphereheight += Mathf.Clamp(height, -10, 150);
            }
            worldheight /= vertices.Length;
            sphereheight /= vertices.Length;
            //normalize sphere height between 0 and 1
            sphereheight = (sphereheight - minHeight) / (maxHeight - minHeight);
            Debug.Log("MESH Avg world height: " + worldheight + " Avg spherical height: " + sphereheight);

            bool isPolarCap = false;
            if (worldheight < minAvgHeightPolar + 0.1f || worldheight > maxAvgHeightPolar - 0.1f)
            {
                isPolarCap = true;
            }

            bool isFlat = false;
            bool isRough = false;
            //flat, rough, or inbetween
            if (sphereheight < 0.3f)
            {
                isFlat = true;
            }
            else if (sphereheight > 0.7f)
            {
                isRough = true;
            }
            else
            {
                isFlat = false;
                isRough = false;
            }
            Debug.Log("isPolarCap: " + isPolarCap + " isFlat: " + isFlat + " isRough: " + isRough + " sphereheight: " + sphereheight + " minHeight: " + minHeight + " maxHeight: " + maxHeight);

            //color white if polar cap, blue if flat, green if rough, red if inbetween
            if (isPolarCap)
            {
                Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
                mat.color = Color.white;
            }
            else if (isFlat)
            {
                Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
                mat.color = Color.gray;
            }
            else if (isRough)
            {
                Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
                mat.color = Color.green;
            }
            else
            {
                Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
                mat.color = Color.red;
            }
        }
    }





    void debugPrintBiomesProbs()
    {
        foreach ((int, float) biome in biomeProbabilities)
        {
            _BiomesPlanets biomeName = (_BiomesPlanets)biome.Item1;
            Debug.Log("Biome: " + biomeName + " Probability: " + biome.Item2);
        }
    }


}

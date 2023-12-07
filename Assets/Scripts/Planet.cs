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
        Grasslands,
        SnowyMountains,
        None
    }
    [SerializeField]
    public List<(int, float)> biomeProbabilities = new List<(int, float)>();
    public bool havePoles = true;

    [ReadOnly]
    public float planetMass = 1;
    [ReadOnly]
    public float planetTemperature = 1;


    private Vector3 TowardsPlanetCenter = new Vector3(0, 0, 0);
    public bool isMoon;
    public int planetSeed = 0;



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

        //set planet center
        TowardsPlanetCenter = transform.position;
        


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
            if (child.name.Contains("submesh") || child.name.Contains("ocean") || child.name.Contains("meshes"))
            {
                DestroyImmediate(child.gameObject);
            }

        }
        MeshFilter[] mfland = new MeshFilter[_PlanetSplitCount * _PlanetSplitCount * 6];
        MeshFilter[] mfocean = new MeshFilter[_PlanetSplitCount * _PlanetSplitCount * 6];
        Initialize(out terrainFaces, out mfland, PlanetSplitCount);
        
        mfland = GenerateMesh(mfland, PlanetSplitCount);
        //Initialize(out oceanFaces, out mfocean, 1);
        //GenerateOcean(mfocean);
        ScalePlanetDownToNormalSizeMF(mfland);
        //ScalePlanetDownToNormalSizeMF(mfocean);

        //create a sphere gameobject for the ocean
        //Check if the current gameobject is called moon
        if (!isMoon)
        {
            GameObject oceanobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            oceanobj.name = "ocean";
            oceanobj.transform.parent = transform;
            oceanobj.transform.localPosition = Vector3.zero;
            oceanobj.transform.localScale = Vector3.one;
            //assign "OceanShader" to ocean
            Material oceanmat = new Material(Shader.Find("Shader Graphs/OceanShader"));
            oceanobj.GetComponent<Renderer>().sharedMaterial = oceanmat;   
            //assign the "RoughWater" texture to the ocean shader
            Texture2D tex = Resources.Load("RoughWater") as Texture2D; 
            //Debug.Log("tex: " + tex);
            oceanmat.SetTexture("_Normal_Map", tex);
        }
        
        
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
            terrainFaces[0].ConstructLandMesh(_meshFilters, planetSeed);
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
            if (AvgWorldheight < minAvgHeightPolar)
            {
                minAvgHeightPolar = AvgWorldheight;
            }
            if (AvgWorldheight > maxAvgHeightPolar)
            {
                maxAvgHeightPolar = AvgWorldheight;
            }
        }

        //make a folder for plants as a child to the planet
        GameObject plantsFolder = new GameObject("plants");
        plantsFolder.transform.parent = transform;
        plantsFolder.transform.localPosition = Vector3.zero;
        plantsFolder.transform.localScale = Vector3.one;


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

            float rand = Random.Range(0.0f, 1.0f);
            int biomeIndex = 0;

            //181, 154, 74
            Color desertYellow = new Color(181f / 255f, 154f / 255f, 74f / 255f);

            //96, 163, 67
            Color GrasslandsGreen = new Color(96f / 255f, 163f / 255f, 67f / 255f);

            //43, 186, 20
            Color ForestGreen = new Color(43f / 255f, 186f / 255f, 20f / 255f);

            //217, 217, 217
            Color SnowyWhite = new Color(217f / 255f, 217f / 255f, 217f / 255f);

            //145, 137, 137
            Color BarrenGrey = new Color(94f / 255f, 70f / 255f, 70f / 255f);

            //82, 81, 81
            Color MountainGrey = new Color(48f / 255f, 48f / 255f, 48f / 255f);

            //99, 99, 99
            Color MoonGrey = new Color(144f, 144f, 144f);

            //set barrengrey to moongrey if moon
            if (isMoon)
            {
                BarrenGrey = MoonGrey;
            }


            //color white if polar cap, blue if flat, green if rough, red if inbetween
            if (isPolarCap && havePoles)
            {
                Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
                mat.color = Color.white;
            } else if (isPolarCap) {
                Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
                mat.color = MountainGrey;
            }
            
            //get random biome
            float totalProb = 0;
            foreach ((int, float) biome in biomeProbabilities)
            {
                totalProb += biome.Item2;
            }
            //modify probs if flat or rough
            float isRoughModify = 1.5f;
            float isFlatModify = 1.5f;

            if(isRough)
            {
                isRoughModify = 1.3f;
                isFlatModify = 0.7f;
            } else if (isFlat)
            {
                isRoughModify = 0.7f;
                isFlatModify = 1.6f;
            }

            List<(int, float)> modifiedBiomeProbabilities = new List<(int, float)>();
            foreach ((int, float) biome in biomeProbabilities)
            {
                float modifiedProb = biome.Item2;
                if (biome.Item1 == (int)_BiomesPlanets.Desert)
                {
                    modifiedProb *= isFlatModify;
                }
                else if (biome.Item1 == (int)_BiomesPlanets.Grasslands)
                {
                    modifiedProb *= isFlatModify;
                }
                else if (biome.Item1 == (int)_BiomesPlanets.Forest)
                {
                    modifiedProb *= isFlatModify;
                }
                else if (biome.Item1 == (int)_BiomesPlanets.SnowyMountains)
                {
                    modifiedProb *= isRoughModify;
                }
                else if (biome.Item1 == (int)_BiomesPlanets.Tundra)
                {
                    modifiedProb *= isFlatModify;
                }
                else if (biome.Item1 == (int)_BiomesPlanets.Mountain)
                {
                    modifiedProb *= isRoughModify;
                }
                else if (biome.Item1 == (int)_BiomesPlanets.Barren)
                {
                    modifiedProb *= isFlatModify;
                }
                else if (biome.Item1 == (int)_BiomesPlanets.None)
                {
                    modifiedProb *= isRoughModify;
                }
                modifiedBiomeProbabilities.Add((biome.Item1, modifiedProb));
            }

            //choose biome
            float cumulativeProb = 0;
            foreach ((int, float) biome in modifiedBiomeProbabilities)
            {
                cumulativeProb += biome.Item2;
                if (rand < cumulativeProb / totalProb)
                {
                    biomeIndex = biome.Item1;
                    break;
                }
            }

            //set to barren if moon
            if (isMoon)
            {
                biomeIndex = (int)_BiomesPlanets.Mountain;
            }


            //set color based on biome
            switch ((_BiomesPlanets)biomeIndex)
            {
                case _BiomesPlanets.Desert:
                    Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
                    mat.color = desertYellow;
                    break;
                case _BiomesPlanets.Grasslands:
                    Material mat2 = filter.GetComponent<MeshRenderer>().sharedMaterial;
                    mat2.color = GrasslandsGreen;
                    break;
                case _BiomesPlanets.Forest:
                    Material mat3 = filter.GetComponent<MeshRenderer>().sharedMaterial;
                    mat3.color = ForestGreen;
                    break;
                case _BiomesPlanets.SnowyMountains:
                    Material mat4 = filter.GetComponent<MeshRenderer>().sharedMaterial;
                    mat4.color = SnowyWhite;
                    break;
                case _BiomesPlanets.Tundra:
                    Material mat5 = filter.GetComponent<MeshRenderer>().sharedMaterial;
                    mat5.color = Color.white;
                    break;
                case _BiomesPlanets.Mountain:
                    Material mat6 = filter.GetComponent<MeshRenderer>().sharedMaterial;
                    mat6.color = Color.white;
                    break;
                case _BiomesPlanets.Barren:
                    Material mat7 = filter.GetComponent<MeshRenderer>().sharedMaterial;
                    mat7.color = BarrenGrey;
                    break;
                case _BiomesPlanets.None:
                    Material mat8 = filter.GetComponent<MeshRenderer>().sharedMaterial;
                    mat8.color = Color.red;
                    break;
                default:
                    Material mat9 = filter.GetComponent<MeshRenderer>().sharedMaterial;
                    mat9.color = Color.red;
                    Debug.Log("Biome not found");
                    break;
            }
            

            //if biome is Forest, add trees
            if ((_BiomesPlanets)biomeIndex == _BiomesPlanets.Forest)
            {
                //tree chance
                float treeChance = 0.02f;

                //get mesh vertices
                Mesh meshForest = filter.sharedMesh;
                Vector3[] verticesForest = mesh.vertices;

                for (int i = 0; i < vertices.Length; i++)
                {
                    //get random number
                    float rand2 = Random.Range(0.0f, 1.0f);
                    if (rand2 < treeChance)
                    {
                        //get vertex position
                        Vector3 vertex = vertices[i];
                        //get vertex world position
                        Vector3 vertexWorldPos = filter.transform.TransformPoint(vertex);
                        //get random tree
                        GameObject tree = Resources.Load("Tree1") as GameObject;
                        //instantiate tree
                        GameObject treeInstance = Instantiate(tree, vertexWorldPos, Quaternion.identity);
                        //set tree parent to planet
                        treeInstance.transform.parent = transform;
                        //set tree position to vertex world position
                        Vector3 offset = (transform.position - vertexWorldPos).normalized * -0.15f;
                        //set tree position to vertex world position
                        treeInstance.transform.position = vertexWorldPos + offset;

                        //set tree rotation to face trunk down
                        treeInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, vertexWorldPos - transform.position);
                        //set tree scale to 0.1
                        treeInstance.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                        //move the tree to folder
                        treeInstance.transform.parent = plantsFolder.transform;
                    }
                }
            } else if((_BiomesPlanets)biomeIndex == _BiomesPlanets.Desert && !isMoon)
            {
                //tree chance
                float treeChance = 0.015f;

                //get mesh vertices
                Mesh meshForest = filter.sharedMesh;
                Vector3[] verticesForest = mesh.vertices;

                for (int i = 0; i < vertices.Length; i++)
                {
                    //get random number
                    float rand2 = Random.Range(0.0f, 1.0f);
                    if (rand2 < treeChance)
                    {
                        //get vertex position
                        Vector3 vertex = vertices[i];
                        //get vertex world position
                        Vector3 vertexWorldPos = filter.transform.TransformPoint(vertex);
                        //get random tree
                        GameObject tree = Resources.Load("Cactus") as GameObject;
                        //instantiate tree
                        GameObject treeInstance = Instantiate(tree, vertexWorldPos, Quaternion.identity);
                        //set tree parent to planet
                        treeInstance.transform.parent = transform;
                        //set tree position to vertex world position
                        Vector3 offset = (transform.position - vertexWorldPos).normalized * -0.12f;
                        //set tree position to vertex world position
                        treeInstance.transform.position = vertexWorldPos + offset;

                        //set tree rotation to face trunk down
                        treeInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, vertexWorldPos - transform.position);
                        //spin the tree randomly with respect to the planet
                        treeInstance.transform.RotateAround(transform.position, transform.up, Random.Range(0, 360));

                        //set tree scale to 0.1
                        treeInstance.transform.localScale = new Vector3(0.006f, 0.006f, 0.006f);

                        //move the tree to folder
                        treeInstance.transform.parent = plantsFolder.transform;
                    }
                }
            } else if((_BiomesPlanets)biomeIndex == _BiomesPlanets.Grasslands)
            {
                //tree chance
                float treeChance = 0.05f;

                //get mesh vertices
                Mesh meshForest = filter.sharedMesh;
                Vector3[] verticesForest = mesh.vertices;

                for (int i = 0; i < vertices.Length; i++)
                {
                    //get random number
                    float rand2 = Random.Range(0.0f, 1.0f);
                    if (rand2 < treeChance)
                    {
                        //get vertex position
                        Vector3 vertex = vertices[i];
                        //get vertex world position
                        Vector3 vertexWorldPos = filter.transform.TransformPoint(vertex);
                        //get random tree
                        GameObject tree = Resources.Load("Grass") as GameObject;
                        //instantiate tree
                        GameObject treeInstance = Instantiate(tree, vertexWorldPos, Quaternion.identity);
                        //set tree parent to planet
                        treeInstance.transform.parent = transform;
                        //set tree position to vertex world position
                        Vector3 offset = (transform.position - vertexWorldPos).normalized * -0.02f;
                        //set tree position to vertex world position
                        treeInstance.transform.position = vertexWorldPos + offset;

                        //set tree rotation to face trunk down
                        treeInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, vertexWorldPos - transform.position);
                        //set tree scale to 0.1
                        treeInstance.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

                        //move the tree to folder
                        treeInstance.transform.parent = plantsFolder.transform;
                    }
                }
            }

            //cull any trees that are under the ocean
            foreach (Transform child in plantsFolder.transform)
            {
                float distanceFromCenter = Vector3.Distance(child.position, transform.position);
                float height = planetRadius - distanceFromCenter;
                if (distanceFromCenter < 3f)
                {
                    //Debug.Log("child: " + child.position + " distance: " + distanceFromCenter + " height: " + height);
                    DestroyImmediate(child.gameObject);
                }
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

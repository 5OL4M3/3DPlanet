using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TerrainFace
{
    ShapeGenerator shapeGenerator;
    Mesh[] meshes;
    int resolution;
    Vector3 normal;
    Vector3 axisA;
    Vector3 axisB;
    int planetSplits;

    public delegate Vector3 GeneratorDelegate(Vector3 pos, int seed);
    public GeneratorDelegate oceanMeshCallback;
    public GeneratorDelegate landMeshCallback;

    public TerrainFace(ShapeGenerator shapeGenerator, Mesh[] meshes, int resolution, Vector3 normal, int planetSplits)
    {
        this.shapeGenerator = shapeGenerator;
        this.meshes = meshes;
        this.resolution = resolution;
        this.planetSplits = planetSplits;
        this.normal = normal;

        axisA = new Vector3(normal.y, normal.z, normal.x);
        axisB = Vector3.Cross(normal, axisA);
    }

    public void ConstructMesh(GeneratorDelegate generatorFunction, MeshFilter[] meshFilters, int planetSeed)
    {
        Assert.IsTrue(resolution % planetSplits == 0);
        Assert.IsTrue(resolution >= planetSplits * planetSplits);
        int meshesPerFace = planetSplits * planetSplits;
        int pieceRes = resolution / planetSplits;

        for (int face = 0; face < 6; face++)
        {
            //recalc axisA and axisB
            Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
            switch (face)
            {
                case 0:
                    normal = directions[0];
                    break;
                case 1:
                    normal = directions[1];
                    break;
                case 2:
                    normal = directions[2];
                    break;
                case 3:
                    normal = directions[3];
                    break;
                case 4:
                    normal = directions[4];
                    break;
                case 5:
                    normal = directions[5];
                    break;
            }
            axisA = new Vector3(normal.y, normal.z, normal.x);
            axisB = Vector3.Cross(normal, axisA);
            for (int idx = 0; idx < meshesPerFace; idx++)
            {
                int x = idx % planetSplits;
                int y = idx / planetSplits;

                Vector3[] vertices = new Vector3[pieceRes * pieceRes];
                int[] triangles = new int[(pieceRes - 1) * (pieceRes - 1) * 6];
                int triIndex = 0;
                int i = 0;
                for (int row = 0; row < pieceRes; row++)
                {
                    for (int col = 0; col < pieceRes; col++)
                    {
                        //scaled by meshes per face
                        Vector2 percent = new Vector2(col, row) / ((pieceRes - 1) * planetSplits);
                        percent.x += x * 1f / planetSplits;
                        percent.y += y * 1f / planetSplits;
                        //Debug.Log("Percent: " + percent);



                        Vector3 pointOnUnitCube = normal + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                        Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                        vertices[i] = generatorFunction(pointOnUnitSphere, planetSeed);

                        //If not the last col or the last row
                        if (col != pieceRes - 1 && row != pieceRes - 1)
                        {
                            triangles[triIndex] = i;
                            triangles[triIndex + 1] = i + pieceRes + 1;
                            triangles[triIndex + 2] = i + pieceRes;

                            triangles[triIndex + 3] = i;
                            triangles[triIndex + 4] = i + 1;
                            triangles[triIndex + 5] = i + pieceRes + 1;
                            triIndex += 6;
                        }

                        i++;
                    }
                }

                meshes[idx] = new Mesh();
                meshes[idx].vertices = vertices;
                meshes[idx].triangles = triangles;
                meshes[idx].RecalculateNormals();

                meshFilters[face * meshesPerFace + idx].sharedMesh = meshes[idx];



            }
        }

    }

    public void ConstructOceanMesh(MeshFilter[] meshFilters)
    {
        oceanMeshCallback = shapeGenerator.CalculatePointOnOcean;
        ConstructMesh(oceanMeshCallback, meshFilters, 0);
        Debug.Log("Finished constructing ocean mesh");
    }

    public void ConstructLandMesh(MeshFilter[] meshFilters, int planetSeed)
    {
        landMeshCallback = shapeGenerator.CalculatePointOnPlanet;
        ConstructMesh(landMeshCallback, meshFilters, planetSeed);
        Debug.Log("Finished constructing land mesh");
    }
}

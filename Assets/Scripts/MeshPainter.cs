using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshPainter : MonoBehaviour
{
    // Start is called before the first frame update
    private Mesh mesh;
    public GameObject syringe;
    private Material syringeMat;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        Vector4[] tangentsArray = mesh.tangents;
        for (int i = 0; i < tangentsArray.Length; i++)
        {
            tangentsArray[i] = new Vector4(0,0,0,0);
        }
        mesh.tangents = tangentsArray;
        mesh.uv2 = new Vector2[mesh.tangents.Length];
        mesh.uv3 = new Vector2[mesh.tangents.Length];
        gameObject.layer = 9;

        syringeMat = syringe.GetComponent<Renderer>().material;

        SplitMesh(mesh);


    }

    void SplitMesh(Mesh targetMesh)
    {
        int[] triangles = targetMesh.triangles;
        Vector3[] verts = targetMesh.vertices;
        Vector3[] normals = targetMesh.normals;
        Vector2[] uvs = targetMesh.uv;

        Vector3[] newVerts;
        Vector3[] newNormals;
        Vector2[] newUvs;
        Vector2[] newUv2s;
        Vector2[] newUv3s;

        int n = triangles.Length;
        newVerts = new Vector3[n];
        newNormals = new Vector3[n];
        newUvs = new Vector2[n];
        newUv2s = new Vector2[n];
        newUv3s = new Vector2[n];

        for (int i = 0; i < n; i++)
        {
            newVerts[i] = verts[triangles[i]];
            newNormals[i] = normals[triangles[i]];
            if (uvs.Length > 0)
            {
                newUvs[i] = uvs[triangles[i]];
            }
            triangles[i] = i;
        }
        targetMesh.vertices = newVerts;
        targetMesh.normals = newNormals;
        targetMesh.uv = newUvs;
        targetMesh.uv2 = newUv2s;
        targetMesh.uv3 = newUv3s;
        targetMesh.triangles = triangles;
    }

    // Update is called once per frame
    void Update()
    {

        //check if you are raycasting against this mesh
        RaycastHit hit;
        int layerMask = 1 << 9;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask) && Input.GetKey(KeyCode.LeftShift))
        {
            Vector4 currentSyringeComponents = new Vector4(
            syringeMat.GetFloat("_ColorPercent"),
            syringeMat.GetFloat("_GlitterPercent"),
            syringeMat.GetFloat("_PoisonPercent"),
            syringeMat.GetFloat("_RainbowPercent"));

            Color curColor = syringeMat.GetColor("_Color");
            Vector2 curColorCompressed1 = new Vector2(curColor.r, curColor.g);
            Vector2 curColorCompressed2 = new Vector2(curColor.b, 0);
            //Vector2 curColorCompressed = new Vector2(Mathf.Floor(255 * curColor.r), 255 * Mathf.Floor(255 * curColor.g) + Mathf.Floor(255 * curColor.b));
            int triangle = hit.triangleIndex;
            Vector4[] tangentsArray = mesh.tangents;
            Vector2[] uv2Array = mesh.uv2;
            Vector2[] uv3Array = mesh.uv3;
            int[] triangles = mesh.triangles;

            tangentsArray[triangles[hit.triangleIndex * 3 + 0]] = currentSyringeComponents;
            tangentsArray[triangles[hit.triangleIndex * 3 + 1]] = currentSyringeComponents;
            tangentsArray[triangles[hit.triangleIndex * 3 + 2]] = currentSyringeComponents;

            uv2Array[triangles[hit.triangleIndex * 3 + 0]] = curColorCompressed1;
            uv2Array[triangles[hit.triangleIndex * 3 + 1]] = curColorCompressed1;
            uv2Array[triangles[hit.triangleIndex * 3 + 2]] = curColorCompressed1;

            uv3Array[triangles[hit.triangleIndex * 3 + 0]] = curColorCompressed2;
            uv3Array[triangles[hit.triangleIndex * 3 + 1]] = curColorCompressed2;
            uv3Array[triangles[hit.triangleIndex * 3 + 2]] = curColorCompressed2;

            mesh.uv2 = uv2Array;
            mesh.uv3 = uv3Array;
            mesh.tangents = tangentsArray;
        }


    }

    void ColorVertex()
    {

    }

}

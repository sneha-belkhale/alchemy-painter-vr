using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct TrianglePaintState
{
    public Vector4 tangent;
    public Vector2 uv2;
    public Vector2 uv3;
    public Mesh mesh;
    public int index;
}

public class MeshPainterController : MonoBehaviour
{
    public GameObject syringe;
    private Material syringeMat;

    private Vector3Int lastHighlightedIndex;
    private Vector3Int lastHighlightedIndex2;
    private Mesh lastHighlightedMesh;


    private List<List<TrianglePaintState>> lastPaintedList; 
    private List<TrianglePaintState> lastTrianglePaintStates;

    // Start is called before the first frame update
    void Start()
    {
        syringeMat = syringe.GetComponent<Renderer>().material;
        GameObject[] paintableObjects = GameObject.FindGameObjectsWithTag("Paintable");
        InitPaintableObjects(paintableObjects);
        lastHighlightedIndex = new Vector3Int();
        lastHighlightedIndex2 = new Vector3Int();
        lastTrianglePaintStates = new List<TrianglePaintState>();

        lastPaintedList = new List<List<TrianglePaintState>>();
    }

    void InitPaintableObjects( GameObject[] paintableObjects)
    {
        for(int i = 0; i < paintableObjects.Length; i++)
        {
            GameObject go = paintableObjects[i];
            go.layer = 9;
            PrepareMesh(go.GetComponent<MeshFilter>().mesh);
        }
    }

    void PrepareMesh(Mesh targetMesh)
    {
        targetMesh.tangents = new Vector4[targetMesh.vertices.Length];

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

    void RemoveLastHighlight()
    {
        Vector2[] uv3Array = lastHighlightedMesh.uv3;
        uv3Array[lastHighlightedIndex.x].y = 0;
        uv3Array[lastHighlightedIndex.y].y = 0;
        uv3Array[lastHighlightedIndex.z].y = 0;
        //uv3Array[lastHighlightedIndex2.x].y = 0; //quads?
        //uv3Array[lastHighlightedIndex2.y].y = 0;
        //uv3Array[lastHighlightedIndex2.z].y = 0;
        lastHighlightedMesh.uv3 = uv3Array;
    }

    void UpdateLastPaintedState()
    {
        if (lastTrianglePaintStates.Count > 0)
        {
            lastPaintedList.Add(lastTrianglePaintStates);
            lastTrianglePaintStates = new List<TrianglePaintState>();

            if (lastPaintedList.Count > 100)
            {
                lastPaintedList.RemoveAt(0);
            }
        }
    }

    void Undo()
    {
        if(lastPaintedList.Count == 0)
        {
            return;
        }
        lastTrianglePaintStates = lastPaintedList[lastPaintedList.Count - 1];

        lastTrianglePaintStates.ForEach((lastTrianglePaintState) => {
            int[] triangles = lastTrianglePaintState.mesh.triangles;
            Vector4[] tangentsArray = lastTrianglePaintState.mesh.tangents;
            Vector2[] uv2Array = lastTrianglePaintState.mesh.uv2;
            Vector2[] uv3Array = lastTrianglePaintState.mesh.uv3;
            int tIdx = lastTrianglePaintState.index;
            tangentsArray[triangles[tIdx * 3 + 0]] = lastTrianglePaintState.tangent;
            tangentsArray[triangles[tIdx * 3 + 1]] = lastTrianglePaintState.tangent;
            tangentsArray[triangles[tIdx * 3 + 2]] = lastTrianglePaintState.tangent;

            uv2Array[triangles[tIdx * 3 + 0]] = lastTrianglePaintState.uv2;
            uv2Array[triangles[tIdx * 3 + 1]] = lastTrianglePaintState.uv2;
            uv2Array[triangles[tIdx * 3 + 2]] = lastTrianglePaintState.uv2;

            uv3Array[triangles[tIdx * 3 + 0]] = lastTrianglePaintState.uv3;
            uv3Array[triangles[tIdx * 3 + 1]] = lastTrianglePaintState.uv3;
            uv3Array[triangles[tIdx * 3 + 2]] = lastTrianglePaintState.uv3;
            lastTrianglePaintState.mesh.uv2 = uv2Array;
            lastTrianglePaintState.mesh.uv3 = uv3Array;
            lastTrianglePaintState.mesh.tangents = tangentsArray;
        });
        lastPaintedList.RemoveAt(lastPaintedList.Count - 1);
    }

    void HighlightTriangle(int tIdx, Mesh mesh)
    {
        //int idx;
        //if (hit.triangleIndex % 2 == 0) // quads?
        //{
        //    idx = hit.triangleIndex;
        //}
        //else
        //{
        //    idx = hit.triangleIndex - 1;
        //}
        int idx = tIdx;
        Vector2[] uv3Array = mesh.uv3;
        int[] triangles = mesh.triangles;

        uv3Array[triangles[idx * 3 + 0]].y = 1;
        uv3Array[triangles[idx * 3 + 1]].y = 1;
        uv3Array[triangles[idx * 3 + 2]].y = 1;
        //uv3Array[triangles[(idx + 1) * 3 + 0]].y = 1;
        //uv3Array[triangles[(idx + 1) * 3 + 1]].y = 1; // quads?
        //uv3Array[triangles[(idx + 1) * 3 + 2]].y = 1;
        mesh.uv3 = uv3Array;
        lastHighlightedIndex.x = triangles[idx * 3 + 0];
        lastHighlightedIndex.y = triangles[idx * 3 + 1];
        lastHighlightedIndex.z = triangles[idx * 3 + 2];
        //lastHighlightedIndex2.x = triangles[(idx + 1) * 3 + 0];
        //lastHighlightedIndex2.y = triangles[(idx + 1) * 3 + 1]; // quads?
        //lastHighlightedIndex2.z = triangles[(idx + 1) * 3 + 2];
        lastHighlightedMesh = mesh;
    }

    void ColorTriangleWithSyringe(int tIdx, Mesh mesh)
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
        int[] triangles = mesh.triangles;
        Vector4[] tangentsArray = mesh.tangents;
        Vector2[] uv2Array = mesh.uv2;
        Vector2[] uv3Array = mesh.uv3;

        //save last triangle paint state if different 
        if (currentSyringeComponents == tangentsArray[triangles[tIdx * 3 + 0]] && uv2Array[triangles[tIdx * 3 + 0]] == curColorCompressed1)
        {
            return;
        }

        TrianglePaintState trianglePaintState;
        trianglePaintState.tangent = tangentsArray[triangles[tIdx * 3 + 0]];
        trianglePaintState.uv2 = uv2Array[triangles[tIdx * 3 + 0]];
        trianglePaintState.uv3 = uv3Array[triangles[tIdx * 3 + 0]];
        trianglePaintState.mesh = mesh;
        trianglePaintState.index = tIdx;
        lastTrianglePaintStates.Add(trianglePaintState);

        tangentsArray[triangles[tIdx * 3 + 0]] = currentSyringeComponents;
        tangentsArray[triangles[tIdx * 3 + 1]] = currentSyringeComponents;
        tangentsArray[triangles[tIdx * 3 + 2]] = currentSyringeComponents;

        uv2Array[triangles[tIdx * 3 + 0]] = curColorCompressed1;
        uv2Array[triangles[tIdx * 3 + 1]] = curColorCompressed1;
        uv2Array[triangles[tIdx * 3 + 2]] = curColorCompressed1;

        uv3Array[triangles[tIdx * 3 + 0]] = curColorCompressed2;
        uv3Array[triangles[tIdx * 3 + 1]] = curColorCompressed2;
        uv3Array[triangles[tIdx * 3 + 2]] = curColorCompressed2;

        mesh.uv2 = uv2Array;
        mesh.uv3 = uv3Array;
        mesh.tangents = tangentsArray;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            Undo();
        }
        if (lastHighlightedMesh)
        {
            RemoveLastHighlight();
        }
        //check if you are raycasting against this mesh
        RaycastHit hit;
        int layerMask = 1 << 9;

        // ************** Keyboard Controls ************** //

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Mesh mesh = hit.collider.gameObject.GetComponent<MeshFilter>().mesh;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                ColorTriangleWithSyringe(hit.triangleIndex, mesh);
            }
            else
            {
                HighlightTriangle(hit.triangleIndex, mesh);
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            UpdateLastPaintedState();
        }

        // ************** VR Controls ************** //

        ray.origin = syringe.transform.position;
        ray.direction = -syringe.transform.up;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Mesh mesh = hit.collider.gameObject.GetComponent<MeshFilter>().mesh;

            if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
            {
                ColorTriangleWithSyringe(hit.triangleIndex, mesh);
            }
            else
            {
                HighlightTriangle(hit.triangleIndex, mesh);
            }
        }

        if (OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
            UpdateLastPaintedState();
        }
    }
}

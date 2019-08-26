using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

struct TrianglePaintState
{
    public Vector4 tangent;
    public Vector2 uv2;
    public Vector2 uv3;
    public Mesh mesh;
    public int index;
    public bool objectPaintMode;
}

public class MeshPainterController : MonoBehaviour
{

    public GameObject syringe;
    public GameObject ScenePicker;

    private Material syringeMat;

    public Vector3Int lastHighlightedIndex;
    public Vector2Int lastHighlightedBounds;
    public Mesh lastHighlightedMesh;
    private bool wireframeOn;

    public bool objectPaintMode;

    private string currentScenery = "";
    private float maxRaycastDist;

    private List<List<TrianglePaintState>> lastPaintedList; 
    private List<TrianglePaintState> lastTrianglePaintStates;
    public List<string> initializedSceneries;

    public Material uvMaterial;

    private RaycastHit lastRaycastHit;
    private bool raycasted;

    private Vector4[] tangentTemp;
    private Vector2[] uv2Temp;
    private Vector2[] uv3Temp;


    // Start is called before the first frame update
    void Start()
    {
        Physics.queriesHitBackfaces = true;

        syringeMat = syringe.GetComponent<Renderer>().material;

        lastHighlightedIndex = new Vector3Int(-1, -1, -1);
        lastHighlightedBounds = new Vector2Int(0, 0);
        lastTrianglePaintStates = new List<TrianglePaintState>();

        lastPaintedList = new List<List<TrianglePaintState>>();
        initializedSceneries = new List<string>();

        wireframeOn = false;
        objectPaintMode = false;

        maxRaycastDist = 7.1f;
    }

    void ExitToMenu()
    {
        DisableSceneryNamed(currentScenery);
        currentScenery = "";
        ScenePicker.SetActive(true);
        GameObject.Find("OVRCameraRig").GetComponent<VRMover>().ResetToInitialTransform();
    }

    public void DisableSceneryNamed(string sceneryName)
    {
        GameObject scenery = GameObject.Find(sceneryName).transform.GetChild(0).gameObject;
        for (int i = 0; i < scenery.transform.childCount; i++)
        {
            GameObject g = scenery.transform.GetChild(i).gameObject;
            scenery.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    public void EnableSceneryNamed(string sceneryName)
    {
        GameObject scenery = GameObject.Find(sceneryName).transform.GetChild(0).gameObject;
        for (int i = 0; i < scenery.transform.childCount; i++)
        {
            GameObject currentChild = scenery.transform.GetChild(i).gameObject;
            currentChild.SetActive(true);
            currentChild.GetComponent<MeshRenderer>().material.SetFloat("_DissolveAmount", -1);
        }
        if(!initializedSceneries.Contains(sceneryName))
        {
            InitSceneryNamed(sceneryName);
        }
        currentScenery = sceneryName;
        StartCoroutine("FadeOutWireframe");
    }

    public void InitSceneryNamed(string sceneryName)
    {
        GameObject scenery = GameObject.Find(sceneryName).transform.GetChild(0).gameObject;
        GameObject[] paintableObjects = new GameObject[scenery.transform.childCount];
        for (int i = 0; i < paintableObjects.Length; i++)
        {
            paintableObjects[i] = scenery.transform.GetChild(i).gameObject;
        }
        InitPaintableObjects(paintableObjects);
        string sceneryObjPath = Application.persistentDataPath + "/saved" + sceneryName;
        Debug.Log(sceneryObjPath);
        if (File.Exists(sceneryObjPath + "0.gd"))
        {
            SetPaintableObjects(paintableObjects, sceneryObjPath);
        }
        initializedSceneries.Add(sceneryName);
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
        int[] triangles = targetMesh.triangles;
        Vector3[] verts = targetMesh.vertices;
        Vector3[] normals = targetMesh.normals;
        Vector2[] uvs = targetMesh.uv;

        Vector3[] newVerts;
        Vector3[] newNormals;
        Vector4[] newTangents;
        Vector2[] newUvs;
        Vector2[] newUv2s;
        Vector2[] newUv3s;
        Vector2[] newUv4s;

        int n = triangles.Length;
        newVerts = new Vector3[n];
        newNormals = new Vector3[n];
        newTangents = new Vector4[n];
        newUvs = new Vector2[n];
        newUv2s = new Vector2[n];
        newUv3s = new Vector2[n];
        newUv4s = new Vector2[n];

        for (int i = 0; i < n; i++)
        {
            newVerts[i] = verts[triangles[i]];
            newNormals[i] = normals[triangles[i]];
            if (uvs.Length > 0)
            {
                newUvs[i] = uvs[triangles[i]];
            }
            triangles[i] = i;
            //barycentric coords 
            if (i%3 == 0)
            {
                newUv4s[i] = new Vector2(0, 1);
            } else if (i % 3 == 1)
            {
                newUv4s[i] = new Vector2(0, 0);
            }
            else
            {
                newUv4s[i] = new Vector2(1, 0);
            }
        }
        targetMesh.vertices = newVerts;
        targetMesh.normals = newNormals;
        targetMesh.tangents = newTangents;
        targetMesh.uv = newUvs;
        targetMesh.uv2 = newUv2s;
        targetMesh.uv3 = newUv3s;
        targetMesh.uv4 = newUv4s;
        targetMesh.triangles = triangles;
    }

    IEnumerator FadeInWireframe()
    {
        GameObject scenery = GameObject.Find(currentScenery);
        MeshRenderer[] meshRenderers = scenery.GetComponentsInChildren<MeshRenderer>();
        float curDissolveAmount = meshRenderers[0].material.GetFloat("_DissolveAmount");

        while(curDissolveAmount > -1)
        {
            curDissolveAmount = curDissolveAmount - Time.deltaTime;
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material.SetFloat("_DissolveAmount", curDissolveAmount);
            }
            yield return null;
        }
        yield return 0;
    }

    IEnumerator FadeOutWireframe()
    {
        GameObject scenery = GameObject.Find(currentScenery);
        MeshRenderer[] meshRenderers = scenery.GetComponentsInChildren<MeshRenderer>();

        float curDissolveAmount = meshRenderers[0].material.GetFloat("_DissolveAmount");

        while (curDissolveAmount < 1)
        {
            curDissolveAmount = curDissolveAmount + Time.deltaTime;
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material.SetFloat("_DissolveAmount", curDissolveAmount);
            }
            yield return null;
        }
        yield return 0;
    }

    void ToggleWireframe()
    {
        if (wireframeOn)
        {
            StartCoroutine("FadeOutWireframe");
            StopCoroutine("FadeInWireframe");
        }
        else
        {
            StartCoroutine("FadeInWireframe");
            StopCoroutine("FadeOutWireframe");
        }

        wireframeOn = !wireframeOn;
    }

    public void RemoveLastHighlight()
    {
        if (lastHighlightedIndex.x < 0) return;
        Vector2[] uv3Array = lastHighlightedMesh.uv3;
        int[] triangles = lastHighlightedMesh.triangles;
        for (int idx = lastHighlightedBounds.x; idx < lastHighlightedBounds.y; idx++)
        {

            uv3Array[triangles[idx * 3 + 0]].y = 0;
            uv3Array[triangles[idx * 3 + 1]].y = 0;
            uv3Array[triangles[idx * 3 + 2]].y = 0;
        }
        lastHighlightedMesh.uv3 = uv3Array;
        lastHighlightedIndex.Set(-1, -1, -1);
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

    void UndoWithoutCache()
    {
        lastTrianglePaintStates.ForEach((lastTrianglePaintState) => {
            int[] triangles = lastTrianglePaintState.mesh.triangles;
            tangentTemp = lastTrianglePaintState.mesh.tangents;
            uv2Temp = lastTrianglePaintState.mesh.uv2;
            uv3Temp = lastTrianglePaintState.mesh.uv3;
            int tIdx = lastTrianglePaintState.index;
            tangentTemp[triangles[tIdx * 3 + 0]] = lastTrianglePaintState.tangent;
            tangentTemp[triangles[tIdx * 3 + 1]] = lastTrianglePaintState.tangent;
            tangentTemp[triangles[tIdx * 3 + 2]] = lastTrianglePaintState.tangent;

            uv2Temp[triangles[tIdx * 3 + 0]] = lastTrianglePaintState.uv2;
            uv2Temp[triangles[tIdx * 3 + 1]] = lastTrianglePaintState.uv2;
            uv2Temp[triangles[tIdx * 3 + 2]] = lastTrianglePaintState.uv2;

            uv3Temp[triangles[tIdx * 3 + 0]] = lastTrianglePaintState.uv3;
            uv3Temp[triangles[tIdx * 3 + 1]] = lastTrianglePaintState.uv3;
            uv3Temp[triangles[tIdx * 3 + 2]] = lastTrianglePaintState.uv3;
            lastTrianglePaintState.mesh.uv2 = uv2Temp;
            lastTrianglePaintState.mesh.uv3 = uv3Temp;
            lastTrianglePaintState.mesh.tangents = tangentTemp;
        });
    }

    void UndoWithCache()
    {
        int[] triangles = lastTrianglePaintStates[0].mesh.triangles;
        tangentTemp = lastTrianglePaintStates[0].mesh.tangents;
        uv2Temp = lastTrianglePaintStates[0].mesh.uv2;
        uv3Temp = lastTrianglePaintStates[0].mesh.uv3;
        
        lastTrianglePaintStates.ForEach((lastTrianglePaintState) => {
            int tIdx = lastTrianglePaintState.index;
            tangentTemp[triangles[tIdx * 3 + 0]] = lastTrianglePaintState.tangent;
            tangentTemp[triangles[tIdx * 3 + 1]] = lastTrianglePaintState.tangent;
            tangentTemp[triangles[tIdx * 3 + 2]] = lastTrianglePaintState.tangent;

            uv2Temp[triangles[tIdx * 3 + 0]] = lastTrianglePaintState.uv2;
            uv2Temp[triangles[tIdx * 3 + 1]] = lastTrianglePaintState.uv2;
            uv2Temp[triangles[tIdx * 3 + 2]] = lastTrianglePaintState.uv2;

            uv3Temp[triangles[tIdx * 3 + 0]] = lastTrianglePaintState.uv3;
            uv3Temp[triangles[tIdx * 3 + 1]] = lastTrianglePaintState.uv3;
            uv3Temp[triangles[tIdx * 3 + 2]] = lastTrianglePaintState.uv3;
        });

        lastTrianglePaintStates[0].mesh.uv2 = uv2Temp;
        lastTrianglePaintStates[0].mesh.uv3 = uv3Temp;
        lastTrianglePaintStates[0].mesh.tangents = tangentTemp;
    }

    void Undo()
    {
        if(lastPaintedList.Count == 0)
        {
            return;
        }
        lastTrianglePaintStates = lastPaintedList[lastPaintedList.Count - 1];

        if (lastTrianglePaintStates[0].objectPaintMode)
        {
            //cache arrays; 
            UndoWithCache();
        }
        else
        {
            UndoWithoutCache();
        }

        lastPaintedList.RemoveAt(lastPaintedList.Count - 1);
        lastTrianglePaintStates = new List<TrianglePaintState>(); //refresh
    }

    void HighlightTriangle(int tStart, int tEnd, Mesh mesh)
    {
        if (tStart * 3 == lastHighlightedIndex.x && mesh.GetInstanceID() == lastHighlightedMesh.GetInstanceID()) return;

        RemoveLastHighlight();

        uv3Temp = mesh.uv3;
        int[] triangles = mesh.triangles;
        for (int idx = tStart; idx < tEnd; idx++)
        {

            uv3Temp[triangles[idx * 3 + 0]].y = 1;
            uv3Temp[triangles[idx * 3 + 1]].y = 1;
            uv3Temp[triangles[idx * 3 + 2]].y = 1;
        }
        mesh.uv3 = uv3Temp;
        lastHighlightedIndex.x = triangles[tStart * 3 + 0];
        lastHighlightedIndex.y = triangles[tStart * 3 + 1];
        lastHighlightedIndex.z = triangles[tStart * 3 + 2];

        lastHighlightedBounds = new Vector2Int(tStart, tEnd);
        lastHighlightedMesh = mesh;
    }

    void ColorTrianglesWithSyringe(int tStart, int tEnd, Mesh mesh)
    {
        Vector4 currentSyringeComponents = new Vector4(
        syringeMat.GetFloat("_ColorPercent"),
        syringeMat.GetFloat("_GlitterPercent"),
        syringeMat.GetFloat("_PoisonPercent"),
        syringeMat.GetFloat("_RainbowPercent"));

        Color curColor = syringeMat.GetColor("_Color").linear;
        Vector2 curColorCompressed1 = new Vector2(curColor.r, curColor.g);
        Vector2 curColorCompressed2 = new Vector2(curColor.b, 0);
        //Vector2 curColorCompressed = new Vector2(Mathf.Floor(255 * curColor.r), 255 * Mathf.Floor(255 * curColor.g) + Mathf.Floor(255 * curColor.b));


        //save last triangle paint state if different 
        if (currentSyringeComponents == mesh.tangents[mesh.triangles[tStart * 3 + 0]] && mesh.uv2[mesh.triangles[tStart * 3 + 0]] == curColorCompressed1)
        {
            return;
        }

        int[] triangles = mesh.triangles;
        tangentTemp = mesh.tangents;
        uv2Temp = mesh.uv2;
        uv3Temp = mesh.uv3;

        for (int tIdx = tStart; tIdx < tEnd; tIdx++)
        {
            TrianglePaintState trianglePaintState;
            trianglePaintState.tangent = tangentTemp[triangles[tIdx * 3 + 0]];
            trianglePaintState.uv2 = uv2Temp[triangles[tIdx * 3 + 0]];
            trianglePaintState.uv3 = new Vector2(uv3Temp[triangles[tIdx * 3 + 0]].x, 0);
            trianglePaintState.mesh = mesh;
            trianglePaintState.index = tIdx;
            trianglePaintState.objectPaintMode = objectPaintMode;
            lastTrianglePaintStates.Add(trianglePaintState);

            tangentTemp[triangles[tIdx * 3 + 0]] = currentSyringeComponents;
            tangentTemp[triangles[tIdx * 3 + 1]] = currentSyringeComponents;
            tangentTemp[triangles[tIdx * 3 + 2]] = currentSyringeComponents;

            uv2Temp[triangles[tIdx * 3 + 0]] = curColorCompressed1;
            uv2Temp[triangles[tIdx * 3 + 1]] = curColorCompressed1;
            uv2Temp[triangles[tIdx * 3 + 2]] = curColorCompressed1;

            uv3Temp[triangles[tIdx * 3 + 0]] = curColorCompressed2;
            uv3Temp[triangles[tIdx * 3 + 1]] = curColorCompressed2;
            uv3Temp[triangles[tIdx * 3 + 2]] = curColorCompressed2;
        }

        mesh.uv2 = uv2Temp;
        mesh.uv3 = uv3Temp;
        mesh.tangents = tangentTemp;
    }

    public void ResetActiveScene()
    {
        if(currentScenery == "")
        {
            return;
        }
        GameObject scenery = GameObject.Find(currentScenery);
        MeshFilter[] meshFilters = scenery.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < meshFilters.Length; i++)
        {
            Mesh mesh = meshFilters[i].mesh;
            tangentTemp = mesh.tangents;
            uv2Temp = mesh.uv2;
            uv3Temp = mesh.uv3;
            int l = mesh.tangents.Length;
            for (int j = 0; j < l; j++)
            {
                tangentTemp[j].Set(0, 0, 0, 0);
                uv2Temp[j].Set(0, 0);
                uv3Temp[j].Set(0, 0);
            }
            mesh.tangents = tangentTemp;
            mesh.uv2 = uv2Temp;
            mesh.uv3 = uv3Temp;
        }
    }

    void onRaycastHit(RaycastHit hit)
    {
        raycasted = true;
        lastRaycastHit = hit;
    }

    void LateUpdate()
    {
        bool colorEvent = Input.GetKey(KeyCode.LeftShift) || OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger);
        bool colorEndEvent = Input.GetKeyUp(KeyCode.LeftShift) || OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger);

        if (raycasted)
        {
            Mesh mesh = lastRaycastHit.collider.gameObject.GetComponent<MeshFilter>().mesh;
            if (colorEvent)
            {
                if (objectPaintMode)
                {
                    ColorTrianglesWithSyringe(0, mesh.triangles.Length/3, mesh);
                }
                else
                {
                    ColorTrianglesWithSyringe(lastRaycastHit.triangleIndex, lastRaycastHit.triangleIndex+1, mesh);
                }
            }
            else
            {
                if (objectPaintMode)
                {
                    HighlightTriangle(0, mesh.triangles.Length / 3, mesh);
                }
                else
                {
                    HighlightTriangle(lastRaycastHit.triangleIndex, lastRaycastHit.triangleIndex + 1, mesh);
                }
            }
        }
        else
        {
            RemoveLastHighlight();
        }

        if (colorEndEvent || objectPaintMode)
        {
            UpdateLastPaintedState();
        }

        raycasted = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U) || OVRInput.GetDown(OVRInput.RawButton.A))
        {
            Undo();
        }

        if (Input.GetKeyDown(KeyCode.I) || OVRInput.GetDown(OVRInput.RawButton.B))
        {
            ToggleWireframe();
        }

        if (Input.GetKeyDown(KeyCode.Tab) || OVRInput.GetDown(OVRInput.RawButton.Start))
        {
            ExitToMenu();
        }
    }

    private void OnApplicationPause()
    {
        SaveSceneriesToDisk();
    }

    private void OnApplicationQuit()
    {
        SaveSceneriesToDisk();
    }

    private void SaveSceneriesToDisk()
    {
        initializedSceneries.ForEach((sceneryName) =>
        {
            GameObject scenery = GameObject.Find(sceneryName).transform.GetChild(0).gameObject;
            MeshFilter[] meshFilters = scenery.GetComponentsInChildren<MeshFilter>();

            BinaryFormatter bf = new BinaryFormatter();

            for (int i = 0; i < meshFilters.Length; i++)
            {
                Mesh mesh = meshFilters[i].mesh;
                SerializableMesh sMesh = new SerializableMesh(mesh.tangents, mesh.uv2, mesh.uv3);

                FileStream file = File.Create(Application.persistentDataPath + "/saved" + sceneryName + i.ToString() + ".gd");
                bf.Serialize(file, sMesh);
                file.Close();
            }
        });
    }

    private void SetPaintableObjects(GameObject[] paintableObjects, string sceneryObjPath)
    {
        BinaryFormatter bf = new BinaryFormatter();
        for (int i = 0; i < paintableObjects.Length; i++)
        {
            FileStream file = File.Open(sceneryObjPath + i.ToString() + ".gd", FileMode.Open);
            SerializableMesh sMesh = (SerializableMesh)bf.Deserialize(file);
            file.Close();

            Mesh mesh = paintableObjects[i].GetComponent<MeshFilter>().mesh;
            CopyMeshFromSerializedMesh(mesh, sMesh);
        }
    }

    private void CopyMeshFromSerializedMesh(Mesh targetMesh, SerializableMesh sMesh)
    {
        int[] triangles = targetMesh.triangles;
        tangentTemp = targetMesh.tangents;
        uv2Temp = targetMesh.uv2;
        uv3Temp = targetMesh.uv3;
        int l = tangentTemp.Length;

        for (int i = 0; i < l; i++)
        {
            tangentTemp[i].Set(
            sMesh._tangents[i].x,
            sMesh._tangents[i].y,
            sMesh._tangents[i].z,
            sMesh._tangents[i].w);

            uv2Temp[i].Set(
            sMesh._uv2[i].x,
            sMesh._uv2[i].y
            );

            uv3Temp[i].Set(
            sMesh._uv3[i].x,
            sMesh._uv3[i].y
            );
        }
        targetMesh.tangents = tangentTemp;
        targetMesh.uv2 = uv2Temp;
        targetMesh.uv3 = uv3Temp;
    }
}

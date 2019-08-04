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
}

public class MeshPainterController : MonoBehaviour
{

    public GameObject syringe;
    public GameObject ScenePicker;

    private Material syringeMat;
    private GameObject rightController;

    public Vector3Int lastHighlightedIndex;
    public Mesh lastHighlightedMesh;
    private bool wireframeOn;

    public bool objectPaintMode;

    private string currentScenery;
    private float maxRaycastDist;

    private List<List<TrianglePaintState>> lastPaintedList; 
    private List<TrianglePaintState> lastTrianglePaintStates;
    public List<string> initializedSceneries;

    // Start is called before the first frame update
    void Start()
    {
        rightController = GameObject.Find("RightControllerAnchor");

        syringeMat = syringe.GetComponent<Renderer>().material;

        lastHighlightedIndex = new Vector3Int(-1, -1, -1);
        lastTrianglePaintStates = new List<TrianglePaintState>();

        lastPaintedList = new List<List<TrianglePaintState>>();
        initializedSceneries = new List<string>();

        wireframeOn = false;
        objectPaintMode = false;

        maxRaycastDist = 10.1f;
    }

    void ExitToMenu()
    {
        DisableSceneryNamed(currentScenery);
        ScenePicker.SetActive(true);
    }

    public void DisableSceneryNamed(string sceneryName)
    {
        GameObject scenery = GameObject.Find(sceneryName).transform.GetChild(0).gameObject;
        for (int i = 0; i < scenery.transform.childCount; i++)
        {
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

    void RemoveLastHighlight()
    {
        if (lastHighlightedIndex.x < 0) return;
        Vector2[] uv3Array = lastHighlightedMesh.uv3;
        uv3Array[lastHighlightedIndex.x].y = 0;
        uv3Array[lastHighlightedIndex.y].y = 0;
        uv3Array[lastHighlightedIndex.z].y = 0;
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
        if (tIdx * 3 == lastHighlightedIndex.x) return;

        int idx = tIdx;

        //check if already highlighted 
        Vector2[] uv3Array = mesh.uv3;
        int[] triangles = mesh.triangles;

        uv3Array[triangles[idx * 3 + 0]].y = 1;
        uv3Array[triangles[idx * 3 + 1]].y = 1;
        uv3Array[triangles[idx * 3 + 2]].y = 1;

        mesh.uv3 = uv3Array;
        lastHighlightedIndex.x = triangles[idx * 3 + 0];
        lastHighlightedIndex.y = triangles[idx * 3 + 1];
        lastHighlightedIndex.z = triangles[idx * 3 + 2];

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
        Vector4[] tangentsArray = mesh.tangents;
        Vector2[] uv2Array = mesh.uv2;
        Vector2[] uv3Array = mesh.uv3;

        for (int tIdx = tStart; tIdx < tEnd; tIdx++)
        {
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
        }

        mesh.uv2 = uv2Array;
        mesh.uv3 = uv3Array;
        mesh.tangents = tangentsArray;
    }

    void HandlePlatformUpdate(Ray ray, bool colorEvent, bool colorEndEvent)
    {
        RaycastHit hit;
        int layerMask = 1 << 9;

        if (Physics.Raycast(ray, out hit, maxRaycastDist, layerMask))
        {
            Mesh mesh = hit.collider.gameObject.GetComponent<MeshFilter>().mesh;
            if (colorEvent)
            {
                if (objectPaintMode)
                {
                    ColorTrianglesWithSyringe(0, mesh.triangles.Length/3, mesh);
                }
                else
                {
                    ColorTrianglesWithSyringe(hit.triangleIndex, hit.triangleIndex+1, mesh);
                }
            }
            else
            {
                HighlightTriangle(hit.triangleIndex, mesh);
            }
        }

        if (colorEndEvent)
        {
            UpdateLastPaintedState();
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool isConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);

        if (Input.GetKeyDown(KeyCode.U) || OVRInput.GetDown(OVRInput.RawButton.B))
        {
            Undo();
        }

        if (Input.GetKeyDown(KeyCode.I) || OVRInput.GetDown(OVRInput.RawButton.A))
        {
            ToggleWireframe();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ExitToMenu();
        }

        RemoveLastHighlight();

        // ************** Keyboard Controls ************** //
        if (!isConnected)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            HandlePlatformUpdate(ray, Input.GetKey(KeyCode.LeftShift), Input.GetKeyUp(KeyCode.LeftShift));
        }
        // ************** VR Controls ************** //
        else
        {
            Ray ray = new Ray(rightController.transform.position, rightController.transform.forward);
            HandlePlatformUpdate(ray, OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger), OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger));
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
        Vector4[] newTangents = targetMesh.tangents;
        Vector2[] newUv2s = targetMesh.uv2;
        Vector2[] newUv3s = targetMesh.uv3;

        for (int i = 0; i < newTangents.Length; i++)
        {
            newTangents[i] = new Vector4(
            sMesh._tangents[i].x,
            sMesh._tangents[i].y,
            sMesh._tangents[i].z,
            sMesh._tangents[i].w);

            newUv2s[i] = new Vector2(
            sMesh._uv2[i].x,
            sMesh._uv2[i].y
            );

            newUv3s[i] = new Vector2(
            sMesh._uv3[i].x,
            sMesh._uv3[i].y
            );
        }
        targetMesh.tangents = newTangents;
        targetMesh.uv2 = newUv2s;
        targetMesh.uv3 = newUv3s;
    }
}

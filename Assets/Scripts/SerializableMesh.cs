using UnityEngine;

[System.Serializable]
public struct SerializableVector4
{
    public float x;
    public float y;
    public float z;
    public float w;
}

[System.Serializable]
public struct SerializableVector2
{
    public float x;
    public float y;
}

[System.Serializable]
public class SerializableMesh {

    [SerializeField]
    public SerializableVector4[] _tangents;

    [SerializeField]
    public SerializableVector2[] _uv2;

    [SerializeField]
    public SerializableVector2[] _uv3;

    public SerializableMesh(Vector4[] tangents, Vector2[] uv2, Vector2[] uv3) {

        _tangents = new SerializableVector4[tangents.Length];
        _uv2 = new SerializableVector2[uv2.Length];
        _uv3 = new SerializableVector2[uv3.Length];

        for (int i = 0; i < tangents.Length; i++)
        {
            _tangents[i].x = tangents[i].x;
            _tangents[i].y = tangents[i].y;
            _tangents[i].z = tangents[i].z;
            _tangents[i].w = tangents[i].w;

            _uv2[i].x = uv2[i].x;
            _uv2[i].y = uv2[i].y;

            _uv3[i].x = uv3[i].x;
            _uv3[i].y = 0; // DO NOT SERIALIZE HIGHLIGHTING
        }
    }
        
}
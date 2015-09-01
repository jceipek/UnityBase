using UnityEngine;

// The concept of a MeshBuilder (and much of the code) is adapted from
// http://jayelinda.com/, Modelling by Numbers series

[RequireComponent(typeof(MeshFilter))]
public class TubeRenderer : MonoBehaviour {
    [SerializeField] float _thickness = 0.5f;
    [SerializeField] int _radialResolution = 3;
    [SerializeField] [VectorMagnitude] Vector3 _firstNormal = Vector3.right;
    [SerializeField] int _maxLength;
    [SerializeField] bool _fullRecomputeEveryFrame;
    ISegmentsProvider _provider;
    Vector3[] _normals;

    void OnValidate () {
        _maxLength = Mathf.Min(((int)ushort.MaxValue) / _radialResolution, _maxLength);
    }

    private MeshBuilder _tubeBuilder = new MeshBuilder();
    private MeshFilter _meshFilter;

    void Awake () {
        _provider = gameObject.GetInterface<ISegmentsProvider>();
        _normals = new Vector3[_maxLength];
    }

    void OnEnable () {
        _meshFilter = GetComponent<MeshFilter>();
    }

    void Start () {
        int maxVerts = _maxLength * (_radialResolution + 1);
        int maxIndices = (_maxLength - 1) * _radialResolution * 2 * 3;
        _tubeBuilder.Init(maxVerts, maxIndices);
    }

    void UpdateNormalVecs (ref Vector3[] normals, int startingIndex = 0) {
        // Compute the || transport frame as described at https://www.cs.indiana.edu/ftp/techreports/TR425.pdf
        // Debug.Log(startingIndex);
        int segmentCount = Mathf.Min(_maxLength, _provider.Count);
        int lastIndex = segmentCount - 1;
        normals[0] = _firstNormal;
        for (int i = startingIndex; i < lastIndex; i++) {
            Vector3 binormal = Vector3.Cross(_provider.Tangent(i), _provider.Tangent(i + 1));
            if (Mathf.Approximately(binormal.sqrMagnitude, 0f)) {
                normals[i + 1] = normals[i];
            } else {
                binormal = binormal.normalized;
                var theta = Vector3.Angle(_provider.Tangent(i), _provider.Tangent(i + 1));
                normals[i + 1] = Quaternion.AngleAxis(theta, binormal) * normals[i];
            }
        }
    }

    int _cachedSegmentCount;
    void Update () {
        int segmentCount = Mathf.Min(_maxLength, _provider.Count);
        if (segmentCount == 0) { return; }

        int startingIndex = 0;
        if (_fullRecomputeEveryFrame) {
            _tubeBuilder.Restart();
        } else if (_cachedSegmentCount > 0) {
            startingIndex = _cachedSegmentCount - 1;
            _tubeBuilder.RollbackTo(vertCount: startingIndex * _radialResolution,
                                    uvCount: 0,
                                    triIndexCount: Mathf.Max(startingIndex - 1, 0) * 3 * 2 * _radialResolution);
        } else {
            _tubeBuilder.Restart();
        }

        UpdateNormalVecs(ref _normals, Mathf.Max(startingIndex - 1, 0));
        for (int i = startingIndex; i < segmentCount; i++) {
            Quaternion rotation;
            rotation = Quaternion.LookRotation(_normals[i], _provider.Tangent(i));

            Vector3 centerPos = _provider.Segment(i);
            bool buildTriangles = i > 0;
            BuildRingWithoutUVs(_tubeBuilder,
                                _radialResolution,
                                centerPos,
                                _thickness,
                                buildTriangles,
                                rotation);
        }
        for (int i = 0; i < segmentCount; i++) {
#if UNITY_EDITOR
            Debug.DrawRay(_provider.Segment(i) + transform.position, _normals[i], Color.blue);
            Debug.DrawRay(_provider.Segment(i) + transform.position, _provider.Tangent(i), Color.green);
#endif
            float v = i / (float)segmentCount;
            BuildRingUVs(_tubeBuilder, _radialResolution, v);
        }
        _meshFilter.sharedMesh = _tubeBuilder.Recalculate();

        _cachedSegmentCount = segmentCount;
    }

    static void BuildRing (MeshBuilder meshBuilder, int segmentCount, Vector3 center, float radius,
                           float v, bool buildTriangles, Quaternion rotation) {

        // Precomputed Sine/Cosine circle drawing from http://slabode.exofire.net/circle_draw.shtml
        float theta = 2f * Mathf.PI / (float)segmentCount;
        float c = Mathf.Cos(theta);
        float s = Mathf.Sin(theta);
        float t;

        float x = radius;//we start at angle = 0
        float y = 0;

        // Since we haven't added any yet, we don't need to -1
        int ringBaseIndex = meshBuilder.VertexCount;
        for (int i = 0; i < segmentCount; i++) {
            Vector3 unitPosition = Vector3.zero;
            unitPosition.x = x;
            unitPosition.z = y;

            unitPosition = rotation * unitPosition;
            meshBuilder.AddVertex(center + unitPosition);// * radius
            meshBuilder.AddNormal(unitPosition);
            meshBuilder.AddUV(new Vector2((float)i / segmentCount, v));

            if (buildTriangles) {
                int vertsPerRow = segmentCount;
                int index0 = ringBaseIndex + i;
                int index1 = ringBaseIndex + MathHelpers.Mod(i - 1, segmentCount); // before base
                int index2 = ringBaseIndex + i - vertsPerRow; // below base
                int index3 = ringBaseIndex + MathHelpers.Mod(i - 1, segmentCount) - vertsPerRow; // before below base

                // Debug.Log(string.Format("TRI{0}>{1}>{2}", index0, index2, index1));
                meshBuilder.AddTriangle(index0, index2, index1);
                // Debug.Log(string.Format("TRI{0}>{1}>{2}", index2, index3, index1));
                meshBuilder.AddTriangle(index2, index3, index1);
            }

            t = x;
            x = c * x - s * y;
            y = s * t + c * y;
        }
    }

    static void BuildRingUVs (MeshBuilder meshBuilder, int segmentCount, float v) {
        for (int i = 0; i < segmentCount; i++) {
            meshBuilder.AddUV(new Vector2((float)i / segmentCount, v));
        }
    }

    static void BuildRingWithoutUVs (MeshBuilder meshBuilder, int segmentCount, Vector3 center, float radius,
                                     bool buildTriangles, Quaternion rotation) {

        // Precomputed Sine/Cosine circle drawing from http://slabode.exofire.net/circle_draw.shtml
        float theta = 2f * Mathf.PI / (float)segmentCount;
        float c = Mathf.Cos(theta);
        float s = Mathf.Sin(theta);
        float t;

        float x = radius;//we start at angle = 0
        float y = 0;

        // Since we haven't added any yet, we don't need to -1
        int ringBaseIndex = meshBuilder.VertexCount;
        for (int i = 0; i < segmentCount; i++) {
            Vector3 unitPosition = Vector3.zero;
            unitPosition.x = x;
            unitPosition.z = y;

            unitPosition = rotation * unitPosition;
            meshBuilder.AddVertex(center + unitPosition * radius);
            meshBuilder.AddNormal(unitPosition);

            if (buildTriangles) {
                int vertsPerRow = segmentCount;
                int index0 = ringBaseIndex + i;
                int index1 = ringBaseIndex + MathHelpers.Mod(i - 1, segmentCount); // before base
                int index2 = ringBaseIndex + i - vertsPerRow; // below base
                int index3 = ringBaseIndex + MathHelpers.Mod(i - 1, segmentCount) - vertsPerRow; // before below base

                // Debug.Log(string.Format("TRI{0}>{1}>{2}", index0, index2, index1));
                meshBuilder.AddTriangle(index0, index2, index1);
                // Debug.Log(string.Format("TRI{0}>{1}>{2}", index2, index3, index1));
                meshBuilder.AddTriangle(index2, index3, index1);
            }

            t = x;
            x = c * x - s * y;
            y = s * t + c * y;
        }
    }

    private class MeshBuilder {
        Mesh _mesh;
        bool _meshTooBig = false;
        int _maxCount;

        public int VertexCount { get; protected set; }
        Vector3[] _vertices;

        public int NormalCount { get; protected set; }
        Vector3[] _normals;

        public int UvCount { get; protected set; }
        Vector2[] _uvs;

        public int IndexCount { get; protected set; }
        int[] _indices;

        public void SetVertex (int index, Vector3 v) {
            _vertices[index] = v;
        }

        public void AddVertex (Vector3 newVertex) {
            if (VertexCount + 1 > _maxCount) {
                if (!_meshTooBig) {
                    Debug.LogError("Exceeded mesh vert limit! Mesh will disappear!");
                }
                _meshTooBig = true;
                return;
            }
            _vertices[VertexCount] = newVertex;
            VertexCount++;
        }

        public void AddNormal (Vector3 newNormal) {
            _normals[NormalCount] = newNormal;
            NormalCount++;
        }

        public void AddUV (Vector2 newUV) {
            _uvs[UvCount] = newUV;
            UvCount++;
        }

        public void AddTriangle (int index0, int index1, int index2) {
            _indices[IndexCount] = index0;
            IndexCount++;
            _indices[IndexCount] = index1;
            IndexCount++;
            _indices[IndexCount] = index2;
            IndexCount++;
        }

        public void Init (int maxVerts, int maxIndices) {
            _maxCount = maxVerts;
            _vertices = new Vector3[maxVerts];
            _normals = new Vector3[maxVerts];
            _uvs = new Vector2[maxVerts];
            _indices = new int[maxIndices];
            _mesh = new Mesh();
            _mesh.MarkDynamic();
        }

        public void RollbackTo (int vertCount, int triIndexCount) {
            IndexCount = triIndexCount;
            UvCount = vertCount;
            NormalCount = vertCount;
            VertexCount = vertCount;
        }

        public void RollbackTo (int vertCount, int uvCount, int triIndexCount) {
            IndexCount = triIndexCount;
            UvCount = uvCount;
            NormalCount = vertCount;
            VertexCount = vertCount;
        }

        public void Restart () {
            // Note: only call this if you are going to reuse
            // all of the elements again very soon.
            // Otherwise, parts of the old mesh will remain
            IndexCount = 0;
            UvCount = 0;
            NormalCount = 0;
            VertexCount = 0;
        }

        public Mesh Recalculate () {
            if (_meshTooBig) {
                return null;
            }
            _mesh.vertices = _vertices;
            _mesh.triangles = _indices;

            //Normals are optional. Only use them if we have the correct amount:
            if (NormalCount == VertexCount) { _mesh.normals = _normals; }

            //UVs are optional. Only use them if we have the correct amount:
            if (UvCount == VertexCount) { _mesh.uv = _uvs; }

            _mesh.RecalculateBounds();

            return _mesh;
        }
    }
}
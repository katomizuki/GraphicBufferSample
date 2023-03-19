using UnityEngine;
using UnityEngine.Rendering;

public class MeshBuilder : System.IDisposable 
{
   #region Public members

    // Mesh object reference
    public Mesh DynamicMesh { private set; get; }

    // Public constructor
    public MeshBuilder
      (in NoiseParameter param, ComputeShader compute)
    {
        _compute = compute;

        // Create a Mesh object as internal temporary.
        DynamicMesh = new Mesh();
        DynamicMesh.hideFlags = HideFlags.HideAndDontSave;

        // We want GraphicsBuffer access as Raw (ByteAddress) buffers.
        // 生バイトを使うぜ宣言！
        DynamicMesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
        DynamicMesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;

        // Mesh initialization
        ResetMesh(param);
        BuildMesh(param);
        UpdateBounds(param);
    }

    // IDisposable implementation
    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _vertexBuffer = null;

        _indexBuffer?.Dispose();
        _indexBuffer = null;

        Util.DestroyObjectSafe(DynamicMesh);
        DynamicMesh = null;
    }

    // Step and update method
    public void Update(in NoiseParameter param)
    {
        // Reset the mesh object if the triangle count has been changed.
        // 頂点数が合わなかったときにメッシュをリセットする＝＞最初は必ず通る
        if (param.TriangleCount * 3 != DynamicMesh.vertexCount)
            ResetMesh(param);

        // Time step
        if (Application.isPlaying)
            _noiseOffset += param.NoiseAnimation * Time.deltaTime;

        // Mesh update
        BuildMesh(param);
        UpdateBounds(param);
    }

    #endregion

    #region Private methods

    ComputeShader _compute;
    GraphicsBuffer _vertexBuffer;
    GraphicsBuffer _indexBuffer;
    Vector3 _noiseOffset;

    // Mesh object initialization/reset
    // 作り直す
    void ResetMesh(in NoiseParameter param)
    {
        // Dispose previous references.
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();

        // Vertex position: float32 x 3
        // 頂点データのFormatを決定。
        var vp = new VertexAttributeDescriptor
          (VertexAttribute.Position, VertexAttributeFormat.Float32, 3);

        // Vertex normal: float32 x 3
        // 法線のFormatも決定
        var vn = new VertexAttributeDescriptor
          (VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);

        // Vertex/index buffer formats
        var vertexCount = param.TriangleCount * 3;
        // それぞれ
        DynamicMesh.SetVertexBufferParams(vertexCount, vp, vn);
        DynamicMesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt32);

        // Submesh initialization
        // メッシュデータを更新するときに境界を再計算しないようにする　＝＞パフォーマンスを上げるために設定できる
        DynamicMesh.SetSubMesh(0, new SubMeshDescriptor(0, vertexCount),
                               MeshUpdateFlags.DontRecalculateBounds);

        // GraphicsBuffer references＝＞頂点バッファとインデックスバッファをゲットできる。
        _vertexBuffer = DynamicMesh.GetVertexBuffer(0);
        _indexBuffer = DynamicMesh.GetIndexBuffer();
    }

    // Execute the compute shader to build the mesh.
    void BuildMesh(in NoiseParameter param)
    {
        _compute.SetInt("TriangleCount", param.TriangleCount);
        _compute.SetFloat("TriangleExtent", param.TriangleExtent);
        _compute.SetFloat("NoiseFrequency", param.NoiseFrequency);
        _compute.SetFloat("NoiseAmplitude", param.NoiseAmplitude);
        _compute.SetVector("NoiseOffset", _noiseOffset);
        int kernelIndex = _compute.FindKernel("VertexBufferUpdate");
        if (kernelIndex != null)
        {
            Debug.Log("Pass1");
            int kernelIndex2 = _compute.FindKernel("IndexBufferUpdate");
            if (kernelIndex2 != null)
            {
                Debug.Log("Pass2");
                _compute.SetBuffer(kernelIndex, "Vertices", _vertexBuffer);
                _compute.DispatchThreads(kernelIndex, param.TriangleCount); 
                _compute.SetBuffer(kernelIndex2, "Indices", _indexBuffer);
                _compute.DispatchThreads(kernelIndex2, param.TriangleCount); 
            }
            else
            {
                Debug.LogWarning("Warning2");
            } 
        }
        else
        {
            Debug.LogWarning("Warning!!");
        }

    }

    void UpdateBounds(in NoiseParameter param)
      => DynamicMesh.bounds = new Bounds
           (Vector3.zero, Vector3.one * (1 + param.NoiseAmplitude * 2));

    #endregion
}
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class NoiseBallController : MonoBehaviour
{
    #region Editable attributes

    public NoiseParameter parameters = NoiseParameter.Default();

    #endregion

    #region Project asset reference

    [SerializeField] ComputeShader _compute = null;

    #endregion

    #region MonoBehaviour implementation

    MeshBuilder _builder;
    MeshFilter _filter;

    void OnDisable() => OnDestroy();

    void OnDestroy()
    {
        _builder?.Dispose();
        _builder = null;
    }

    void Update()
    {
        // Mesh builder initialization/update
        if (_builder == null)
            _builder = new MeshBuilder(parameters, _compute);
        else
            _builder.Update(parameters);

        // Mesh filter component lazy initialization and update
        if (_filter == null)
        {
            if (gameObject != null)
            {
                _filter = gameObject.GetComponent<MeshFilter>();
            } 
        }

        if (_filter != null)
        {
            Debug.Log("dynamic mesh set");
            _filter.sharedMesh = _builder.DynamicMesh; 
        } 
    }

    #endregion
}

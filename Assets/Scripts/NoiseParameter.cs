using UnityEngine;

[System.Serializable]
public struct NoiseParameter
{

    public int TriangleCount;
    public float TriangleExtent;
    public float NoiseFrequency;
    public float NoiseAmplitude;
    public Vector3 NoiseAnimation;

    public static NoiseParameter Default()
    {
        var s = new NoiseParameter();
        s.TriangleCount = 10000;
        s.TriangleExtent = 0.3f;
        s.NoiseFrequency = 2.2f;
        s.NoiseAmplitude = 0.85f;
        s.NoiseAnimation = new Vector3(0, 0.13f, 0.51f);
        return s;
    }
}

static class Util
{
    // "Please safely destroy this object in any situation!"
    public static void DestroyObjectSafe(Object o)
    {
        if (o == null) return;
        if (Application.isPlaying)
            Object.Destroy(o);
        else
            Object.DestroyImmediate(o);
    }
}

static class ComputeShaderExtensions
{
    // Execute a compute shader with specifying a minimum number of thread
    // count not by a thread GROUP count.
    public static void DispatchThreads
        (this ComputeShader compute, int kernel, int count)
    {
        uint x, y, z;
        compute.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
        var groups = (count + (int)x - 1) / (int)x;
        compute.Dispatch(kernel, groups, 1, 1);
    }
}
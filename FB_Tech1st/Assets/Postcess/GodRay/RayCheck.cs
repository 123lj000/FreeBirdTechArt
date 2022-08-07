using UnityEngine;

public class RayCheck : MonoBehaviour
{
    void Update()
    {
        // this example shows the different camera frustums when using asymmetric projection matrices (like those used by OpenVR).

        var camera = GetComponent<Camera>();
        Vector3[] frustumCorners = new Vector3[4];
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

        var worldSpaceCorner = camera.transform.TransformVector(frustumCorners[3]);
        Debug.DrawRay(camera.transform.position, worldSpaceCorner, Color.blue);
    }
}
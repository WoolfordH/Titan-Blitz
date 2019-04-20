using UnityEngine;

public class NormalsReplacementShader : MonoBehaviour {

    [SerializeField]
    Shader normalsShader;

    RenderTexture renderTexture;
    new Camera cam;

    private void Start()
    {
        Camera thisCamera = GetComponent<Camera>();

        renderTexture = new RenderTexture(thisCamera.pixelWidth, thisCamera.pixelHeight, 24);

        Shader.SetGlobalTexture("_CameraNormalsTexture", renderTexture);

        GameObject copy = new GameObject("Normals camera");
        cam = copy.AddComponent<Camera>();
        cam.CopyFrom(thisCamera);
        cam.transform.SetParent(transform);
        cam.targetTexture = renderTexture;
        cam.SetReplacementShader(normalsShader, "RenderType");
        cam.depth = thisCamera.depth;
    }
}

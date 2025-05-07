using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridSystem gridSystem;

    [Header("Iso Rotation")]
    [SerializeField, Tooltip("Default I guess: Tilt down from horizontal =30°")] private float tiltAngle = 30f;
    [SerializeField, Tooltip("Default I guess: Rotate around Y =45°")] private float swivelAngle = 45f;

    [Header("Padding & Clipping")]
    [Tooltip("Extra space around the edges")]
    [SerializeField] private float paddingFactor = 1.1f;
    [SerializeField] private float nearClip = 0.1f;
    [SerializeField] private float farClip = 100f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (gridSystem == null)
        {
            Debug.LogError("IsoGridCamera: please assign a GridSystem reference.");
            enabled = false;
            return;
        }

        //Calculate grid dimensions 
        float worldW = gridSystem._gridWidth * gridSystem._cellSize;
        float worldH = gridSystem._gridHeight * gridSystem._cellSize;
        float maxH = gridSystem._maxCellHeight;

        //Set up isometric rotation
        transform.rotation = Quaternion.Euler(tiltAngle, swivelAngle, 0f);

        //Build the eight corners of the grid 
        Vector3 center = new Vector3(worldW * 0.5f, maxH * 0.5f, worldH * 0.5f);
        Vector3 half = new Vector3(worldW * 0.5f, maxH * 0.5f, worldH * 0.5f);
        Vector3[] corners = new Vector3[8];
        int i = 0;
        for (int xi = -1; xi <= 1; xi += 2)
            for (int zi = -1; zi <= 1; zi += 2)
            {
                corners[i++] = center + new Vector3(half.x * xi, -half.y, half.z * zi);
                corners[i++] = center + new Vector3(half.x * xi, half.y, half.z * zi);
            }

        //Project corners into camera space & find extents
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        for (int j = 0; j < corners.Length; j++)
        {
            Vector3 cs = transform.InverseTransformPoint(corners[j]);
            minX = Mathf.Min(minX, cs.x);
            maxX = Mathf.Max(maxX, cs.x);
            minY = Mathf.Min(minY, cs.y);
            maxY = Mathf.Max(maxY, cs.y);
        }

        //Calculate orthographic size (half-height) with padding
        float aspect = (float)Screen.width / Screen.height;
        float halfH = (maxY - minY) * 0.5f;
        float halfW = (maxX - minX) * 0.5f / aspect;
        float orthoSize = Mathf.Max(halfH, halfW) * paddingFactor;

        //Place camera so that its look at the grid center
        Vector3 lookDir = transform.forward;
        Vector3 camPos = center - lookDir * 10f;
        float camSpaceZ = transform.InverseTransformPoint(center).z;
        camPos = center - lookDir * camSpaceZ;

        cam.orthographic = true;
        cam.orthographicSize = orthoSize;
        cam.nearClipPlane = nearClip;
        cam.farClipPlane = farClip;

        transform.position = camPos;
    }
}

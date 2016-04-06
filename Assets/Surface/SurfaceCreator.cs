using UnityEngine;
using System.Collections;


[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]
public class SurfaceCreator : MonoBehaviour {

    [Range(1, 200)]
    public int resolution;

    Mesh mesh;

    int currentResolution;


    public float frequency = 1f;

    [Range(1, 8)]
    public int octaves = 1;

    [Range(1f, 4f)]
    public float lacunarity = 2f;

    [Range(0f, 1f)]
    public float persistence = 0.5f;

    [Range(1, 3)]
    public int dimensions = 3;

    [Range(0, 1)]
    public float strength = 1f;

    public NoiseMethodType noiseType;

    public Gradient coloring;

    Vector3[] vertices;
    Vector3[] normals;
    Color[] color;

    public Vector3 offset;
    public Vector3 rotation;


    public bool coloringForStrength;

    public bool damping;
    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Refresh();
        }
    }

    void OnEnable()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
        }
        Refresh();
    }

    public void Refresh()
    {
        if (resolution != currentResolution)
        {
            CreateGrid();
        }

        Quaternion r = Quaternion.Euler(rotation);

        Vector3 point00 = r *  transform.TransformPoint(new Vector3(-0.5f, -0.5f));
        Vector3 point10 = r *  transform.TransformPoint(new Vector3(0.5f, -0.5f));
        Vector3 point01 = r *  transform.TransformPoint(new Vector3(-0.5f, 0.5f));
        Vector3 point11 = r * transform.TransformPoint(new Vector3(0.5f, 0.5f));

        NoiseMethod method = Noise.methods[(int)noiseType][dimensions - 1];
        float stepSize = 1f / resolution;

        float amplitude = damping ? strength / frequency : strength;
        for (int v = 0, i = 0; i <= resolution; i++)
        {
            Vector3 point0 = Vector3.Lerp(point00, point01, i * stepSize);
            Vector3 point1 = Vector3.Lerp(point10, point11, i * stepSize);
            for (int j = 0; j <= resolution; j++, v++)
            {
                Vector3 point = Vector3.Lerp(point0, point1, j * stepSize);
                float sample = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
                sample = noiseType == NoiseMethodType.Value ? (sample - .5f) : (sample * 0.5f);
                if (coloringForStrength)
                {
                    color[v] = coloring.Evaluate(sample + 0.5f);
                    sample *= amplitude;
                }
                else {
                    sample *= amplitude;
                    color[v] = coloring.Evaluate(sample + 0.5f);
                }
                vertices[v].y = sample;
            }
        }
        mesh.vertices = vertices;
        mesh.colors = color;
        mesh.RecalculateNormals();
        normals = mesh.normals;
    }

    public void CreateGrid()
    {
        currentResolution = resolution;
        mesh.Clear();

        vertices = new Vector3[(resolution + 1) * (resolution + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        normals = new Vector3[vertices.Length];
        color = new Color[vertices.Length];
        float stepSize = 1f / resolution;

        for (int v = 0, i = 0; i <= resolution; i++)
        {
            for (int j = 0; j <= resolution; j++,v++)
            {
                vertices[v] = new Vector3(j * stepSize - 0.5f, 0, i * stepSize - 0.5f);
                uv[v] = new Vector2(j*stepSize,i*stepSize);
                //normals[v] = Vector3.up;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.normals = normals;


        int[] triangles = new int[resolution*resolution*6];

        for (int i = 0,t=0,v =0; i < resolution; i++,v++)
        {
            for (int j = 0; j < resolution; j++,t+=6,v++)
            {
                triangles[t] = v;
                triangles[t+1] = v + resolution +1;
                triangles[t+2] = v +1;
                triangles[t+3] = v + 1;
                triangles[t+4] = v + resolution + 1;
                triangles[t+5] = v + resolution + 2;
            }
        }

        mesh.triangles = triangles;
        mesh.RecalculateNormals();


    }

    void CalculateNormal()
    {

    }
    public bool showNormals;

    private void OnDrawGizmosSelected()
    {
        if (showNormals && vertices != null)
        {
            Gizmos.color = Color.yellow;
            for (int v = 0; v < vertices.Length; v++)
            {
                Gizmos.DrawRay(vertices[v], normals[v]/resolution);
            }
        }
    }

}

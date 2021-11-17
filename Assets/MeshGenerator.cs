using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{

    private Mesh _mesh;

    public Vector3[] _vertices;
    private int[] _triangles;
    private Color[] _colors;

    private float _minHeight;
    private float _maxHeight;

    public int XSize = 20;
    public int ZSize = 20;

    public float Variation = 1f; //affects how much height variation there will be ]0;1[
    public float Magnitude = 1f; //affects how high the peaks will be ]0;inf[

    public Gradient TerrainGradient;

    public Graphe G;

    List<List<int>> Roads;
    List<Color> RoadColors;

    [Header("Random ?")]
    public bool RANDOMIZED_TERRAIN = false;

    [Header("Displays mesh grid")]
    public bool DISPLAY_GRID;

    [Header("Displays nodes id")]
    public bool DISPLAY_NODE_NAME;

    [Header("Displays roads weight")]
    public bool DISPLAY_ROAD_WEIGHT;

    [Header("Displays Main Road")]
    public bool DISPLAY_MAIN_ROAD;

    [Header("Displays Secondary Roads")]
    public bool DISPLAY_SECONDARY_ROADS;

    private List<int> GenerateRoad(int type, List<int> ParentRoad = null)
    {
        List<int> Road = new List<int>();
        List<int> Temp = new List<int>();
        // Main Road
        if (type == 1)
        {
            for (int i = 0; i < XSize+1; i++)
            {
                //int[] pred = Dijkstra(i);
                int[] pred = Dijkstra.Run(i, G, XSize, ZSize);
                for (int j = (XSize+1) * ZSize; j < (XSize + 1) * (ZSize + 1); j++)
                {
                    int cursor = j;
                    Temp.Clear();
                    while (cursor != -1)
                    {
                        Temp.Add(cursor);
                        cursor = pred[cursor];
                    }
                    if (Temp.Count > Road.Count)
                    {
                        Road = new List<int>(Temp);
                        Road.Reverse();
                    }
                }
            }
        }
        // Secondary Road
        else if (type == 2)
        {
            int start = ParentRoad[Random.Range(0, ParentRoad.Count)];
            int[] pred = Dijkstra.Run(start, G, XSize, ZSize);
            int width = 10;
            for (int i = -width; i <= width; i++)
            {
                for (int j = -width; j <= width; j++)
                {
                    if (i==0 && j==0) continue;
                    int end = start + i * (XSize+1) + j;
                    if (end < 0 || end >= (XSize+1)*(ZSize+1)) continue;
                    Temp.Clear();
                    while (end != -1)
                    {
                        Temp.Add(end);
                        end = pred[end];
                    }
                    if (Temp.Count > Road.Count && Temp.Count > 1 && Temp.Count < 10)
                    {
                        Road = new List<int>(Temp);
                        Road.Reverse();
                    }
                }
            }
        }
        // Tertiary Road
        else if (type == 3)
        {

        }
        for (int i = 0; i < Road.Count-1; i++)
        {
            G.adjMatrix[Road[i]][Road[i + 1]] = 0;
            G.adjMatrix[Road[i + 1]][Road[i]] = 0;
        }
        return Road;
    }

    

    public void GenerateRoads(int n)
    {
        G.GenerateGraphe(_vertices, XSize, ZSize, Magnitude);
        Roads.Clear();
        List<int> Road = GenerateRoad(1);
        Roads.Add(new List<int>(Road));
        for (int i = 0; i < n; i++)
        {
            Road = GenerateRoad(2, Roads[0]);
            if (Road.Count > 0)
                Roads.Add(new List<int>(Road));
        }

        RoadColors = new List<Color>();
        for (int i = 0; i < Roads.Count-1; i++)
        {
            RoadColors.Add(new Color(Random.value, Random.value, Random.value));
        }
    }

    void Start()
    {
        _mesh = new Mesh();
        G = new Graphe();
        GetComponent<MeshFilter>().mesh = _mesh;

        CreateShape();
        CreateMesh();
        G.GenerateGraphe(_vertices, XSize, ZSize, Magnitude);
        //GetComponent<RouteGenerator>().InitVertices(_vertices);
        Roads = new List<List<int>>();
        GenerateRoads(10);

        List<Vector3[]> nodes = new List<Vector3[]>();
        for (int i = 0; i < Roads.Count; i++)
        {
            List<Vector3> road = new List<Vector3>();
            for (int j = 0; j < Roads[i].Count; j++)
            {
                road.Add(_vertices[Roads[i][j]]);
            }
            nodes.Add(road.ToArray());
        }
    }

    void CreateShape()
    {
        _vertices = new Vector3[(XSize + 1) * (ZSize + 1)];

        for (int i = 0, z = 0; z <= ZSize; z++)
        {
            for (int x = 0; x <= XSize; x++)
            {
                Vector2 offset;
                if (RANDOMIZED_TERRAIN)
                    offset = new Vector2(Random.value, Random.value);
                else
                    offset = Vector2.zero;
                float y = Mathf.PerlinNoise(x * Variation + offset.x, z * Variation + offset.y) * Magnitude;
                _vertices[i] = new Vector3(x, y, z);

                if (y > _maxHeight)
                    _maxHeight = y;
                if (y < _minHeight)
                    _minHeight = y;

                i++;
            }
        }

        _triangles = new int[XSize * ZSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < ZSize; z++)
        {
            for (int x = 0; x < XSize; x++)
            {
                _triangles[tris + 0] = vert + 0;
                _triangles[tris + 1] = vert + XSize + 1;
                _triangles[tris + 2] = vert + 1;
                _triangles[tris + 3] = vert + 1;
                _triangles[tris + 4] = vert + XSize + 1;
                _triangles[tris + 5] = vert + XSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        _colors = new Color[_vertices.Length];

        for (int i = 0, z = 0; z <= ZSize; z++)
        {
            for (int x = 0; x <= XSize; x++)
            {
                float height = Mathf.InverseLerp(_minHeight, _maxHeight, _vertices[i].y);
                _colors[i] = TerrainGradient.Evaluate(height);
                i++;
            }
        }
    }

    void CreateMesh()
    {
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.colors = _colors;

        _mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        if (_vertices == null || Roads == null) return;

        for (int i = 0; i < _vertices.Length; i++)
        {
            if (DISPLAY_NODE_NAME) Handles.Label(_vertices[i] + Vector3.up, i + "");
            //Gizmos.DrawSphere(_vertices[i], .1f);
        }


        if (DISPLAY_GRID)
        {
            for (int i = 0; i < G.adjMatrix.Count; i++)
            {
                for (int j = 0; j < G.adjMatrix[i].Count; j++)
                {

                    if (G.adjMatrix[i][j] == 0f) { continue; }
                    if (G.adjMatrix[i][j] < Magnitude / 4f)
                    {
                        GUIStyle style = new GUIStyle();
                        style.alignment = TextAnchor.MiddleCenter;

                        if (G.adjMatrix[i][j] > (Magnitude / 4f) * 0.9f)
                        {
                            Gizmos.color = new Color(1f, 0.5f, 0f);
                            style.normal.textColor = new Color(1f, 0.5f, 0f);
                        }

                        if (DISPLAY_ROAD_WEIGHT) Handles.Label((G.vertices[i] + ((G.vertices[j] - G.vertices[i]) / 2f)) + Vector3.up * 0.1f, G.adjMatrix[i][j].ToString("0.00"), style);
                        //if (DISPLAY_ROAD_WEIGHT) Handles.Label(Vector3.zero, G.adjMatrix[i][j] + "");
                        Gizmos.DrawLine(G.vertices[i], G.vertices[j]);
                        Gizmos.color = Color.white;
                        style.normal.textColor = Color.white;
                    }
                }
            }
        }

        if (DISPLAY_SECONDARY_ROADS)
        {
            for (int i = 1; i < Roads.Count; i++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(_vertices[Roads[i][0]], .1f);
            }
        }
        

        //List<int> road2 = GenerateRoad(49, 32);
        //for (int i = 0; i < road2.Count - 1; i++)
        //{
        //    Gizmos.DrawLine(G.vertices[road2[i]], G.vertices[road2[i + 1]]);
        //}

        //List<int> road3 = GenerateRoad(82, 99);
        //for (int i = 0; i < road3.Count - 1; i++)
        //{
        //    Gizmos.DrawLine(G.vertices[road3[i]], G.vertices[road3[i + 1]]);
        //}

        Gizmos.color = Color.red;

        for (int i = 0; i < Roads.Count; i++)
        {
            if (Roads[i].Count == 0) continue;
            if (i == 0 && !DISPLAY_MAIN_ROAD) continue;
            else if (i != 0 && !DISPLAY_SECONDARY_ROADS) continue;
            switch (i)
            {
                case 0:
                    Gizmos.color = Color.red;
                    break;

                default:
                    Gizmos.color = RoadColors[i-1];
                    break;
            }
            for (int j = 0; j < Roads[i].Count - 1; j++)
            {
                Gizmos.DrawLine(G.vertices[Roads[i][j]] + Vector3.up * 0.1f, G.vertices[Roads[i][j + 1]] + Vector3.up * 0.1f);
            }
        }

    }

}

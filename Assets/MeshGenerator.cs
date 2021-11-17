using System.Collections.Generic;
using UnityEngine;

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


    public class Graphe
    {
        public List<Vector3> vertices;
        public List<List<float>> adjMatrix;

        public Graphe()
        {
            vertices = new List<Vector3>();
            adjMatrix = new List<List<float>>();
        }
    }

    private List<int> GenerateRoad(int type, List<int> ParentRoad = null)
    {
        List<int> Road = new List<int>();
        List<int> Temp = new List<int>();
        // Main Road
        if (type == 1)
        {
            for (int i = 0; i < XSize+1; i++)
            {
                int[] pred = Dijkstra(i);
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
            int[] pred = Dijkstra(start);
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

    private void GenerateGraphe()
    {
        for (int i = 0; i < _vertices.Length; i++)
            G.vertices.Add(_vertices[i]);

        // Init at 0
        for (int i = 0; i < (XSize+1)*(ZSize+1); i++)
        {
            G.adjMatrix.Add(new List<float>());
            for (int j = 0; j < (XSize+1)*(ZSize+1); j++)
            {
                G.adjMatrix[G.adjMatrix.Count - 1].Add(0f);
            }
        }

        for (int i = 0; i <= XSize; i++)
        {
            for (int j = 0; j <= ZSize; j++)
            {
                int a = i * (XSize + 1) + j;
                if (i != XSize)
                {
                    int b = (i + 1) * (XSize + 1) + j;
                    G.adjMatrix[a][b] = Mathf.Abs(_vertices[a].y - _vertices[b].y);
                    G.adjMatrix[b][a] = Mathf.Abs(_vertices[a].y - _vertices[b].y);
                }
                if (j != ZSize)
                {
                    int c = i * (XSize + 1) + (j + 1);
                    G.adjMatrix[a][c] = Mathf.Abs(_vertices[a].y - _vertices[c].y);
                    G.adjMatrix[c][a] = Mathf.Abs(_vertices[a].y - _vertices[c].y);
                }
            }
        }

        for (int i = 0; i < G.adjMatrix.Count; i++)
        {
            for (int j = 0; j < G.adjMatrix[i].Count; j++)
            {
                if (G.adjMatrix[i][j] > Magnitude / 4f)
                    G.adjMatrix[i][j] = 0f;
            }
        }
    }



    private int[] Dijkstra(int start)
    {
        List<int> poids = new List<int>();
        float[] distance = new float[(XSize+1) * (ZSize+1)];
        int[] pred = new int[(XSize + 1) * (ZSize + 1)];
        for (int i = 0; i < (XSize + 1) * (ZSize + 1); ++i)
        {
            pred[i] = -1;
        }

        for (int i = 0; i < (XSize + 1) * (ZSize + 1); ++i)
        {
            distance[i] = float.PositiveInfinity;
        }
        distance[start] = 0;
        int cursor = start;

        KeyValuePair<float, int> min;
        while (poids.Count < (XSize + 1) * (ZSize + 1))
        {
            min = new KeyValuePair<float, int> (float.PositiveInfinity, 0 );
            for (int i = 0; i < (XSize + 1) * (ZSize + 1); ++i)
            {
                if (!poids.Contains(i))
                    if (distance[i] < min.Key) { min = new KeyValuePair<float, int>(distance[i], i); break; }
            }

            cursor = min.Value;
            poids.Add(cursor);

            for (int i = 0; i < (XSize + 1) * (ZSize + 1); ++i)
            {
                if (!poids.Contains(i) && G.adjMatrix[cursor][i] > 0)
                {
                    if (distance[i] > distance[cursor] + G.adjMatrix[cursor][i])
                    {
                        distance[i] = distance[cursor] + G.adjMatrix[cursor][i];
                        pred[i] = cursor;
                    }
                }
            }
        }
        return pred;
    }

    public void GenerateRoads(int n)
    {
        GenerateGraphe();
        Roads.Clear();
        List<int> Road = GenerateRoad(1);
        Roads.Add(new List<int>(Road));
        for (int i = 0; i < n; i++)
        {
            Road = GenerateRoad(2, Roads[0]);
            if (Road.Count > 0)
                Roads.Add(new List<int>(Road));
        }
    }

    void Start()
    {
        _mesh = new Mesh();
        G = new Graphe();
        GetComponent<MeshFilter>().mesh = _mesh;

        CreateShape();
        UpdateMesh();
        GenerateGraphe();
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
        GetComponent<Bernstein>().nodes = nodes;
    }

    void CreateShape()
    {
        _vertices = new Vector3[(XSize + 1) * (ZSize + 1)];

        for (int i = 0, z = 0; z <= ZSize; z++)
        {
            for (int x = 0; x <= XSize; x++)
            {
                float y = Mathf.PerlinNoise(x * Variation, z * Variation) * Magnitude;
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

    void UpdateMesh()
    {
        _mesh.Clear();

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
            //UnityEditor.Handles.Label(_vertices[i] + Vector3.up, $"{i}");
            Gizmos.DrawSphere(_vertices[i], .1f);
        }


        //for (int i = 0; i < G.adjMatrix.Count; i++)
        //{
        //    for (int j = 0; j < G.adjMatrix[i].Count; j++)
        //    {

        //        if (G.adjMatrix[i][j] == 0f) { continue; }
        //        if (G.adjMatrix[i][j] < Magnitude / 4f)
        //        {
        //            if (G.adjMatrix[i][j] > (Magnitude / 4f) * 0.9f)
        //                Gizmos.color = new Color(1f, 0.5f, 0f);
        //            Gizmos.DrawLine(G.vertices[i], G.vertices[j]);
        //            Gizmos.color = Color.white;
        //        }
        //    }
        //}

        for (int i = 1; i < Roads.Count; i++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_vertices[Roads[i][0]], .1f);
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

        //Gizmos.color = Color.red;

        //for (int i = 0; i < Roads.Count; i++)
        //{
        //    if (Roads[i].Count == 0) continue;
        //    switch (i)
        //    {
        //        case 0: Gizmos.color = Color.red; break;
        //        default: Gizmos.color = Color.blue; break;
        //    }
        //    for (int j = 0; j < Roads[i].Count - 1; j++)
        //    {
        //        Gizmos.DrawLine(G.vertices[Roads[i][j]] + Vector3.up * 0.1f, G.vertices[Roads[i][j + 1]] + Vector3.up * 0.1f);
        //    }
        //}

    }

}

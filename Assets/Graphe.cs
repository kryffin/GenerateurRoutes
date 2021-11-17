using System.Collections.Generic;
using UnityEngine;

public class Graphe
{
    public List<Vector3> vertices;
    public List<List<float>> adjMatrix;

    public Graphe()
    {
        vertices = new List<Vector3>();
        adjMatrix = new List<List<float>>();
    }
    public void GenerateGraphe(Vector3[] verts, int XSize, int ZSize, float magnitude)
    {
        for (int i = 0; i < verts.Length; i++)
            vertices.Add(verts[i]);

        // Init at 0
        for (int i = 0; i < (XSize + 1) * (ZSize + 1); i++)
        {
            adjMatrix.Add(new List<float>());
            for (int j = 0; j < (XSize + 1) * (ZSize + 1); j++)
            {
                adjMatrix[adjMatrix.Count - 1].Add(0f);
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
                    adjMatrix[a][b] = Mathf.Abs(verts[a].y - verts[b].y);
                    adjMatrix[b][a] = Mathf.Abs(verts[a].y - verts[b].y);
                }
                if (j != ZSize)
                {
                    int c = i * (XSize + 1) + (j + 1);
                    adjMatrix[a][c] = Mathf.Abs(verts[a].y - verts[c].y);
                    adjMatrix[c][a] = Mathf.Abs(verts[a].y - verts[c].y);
                }
            }
        }

        for (int i = 0; i < adjMatrix.Count; i++)
        {
            for (int j = 0; j < adjMatrix[i].Count; j++)
            {
                if (adjMatrix[i][j] > magnitude / 4f)
                    adjMatrix[i][j] = 0f;
            }
        }
    }

}

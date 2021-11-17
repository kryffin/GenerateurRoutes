using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bernstein : MonoBehaviour
{
    public List<Vector3[]> nodes;

    [SerializeField]
    [Range(0f, 1000f)]
    private int precision = 1000;

    private int Fact(int n)
    {
        if (n <= 1) { return 1; }
        return n * Fact(n - 1);
    }

    private float GetBernstein(int n, int i, float t)
    {
        return (Fact(n) / (Fact(i) * Fact(n - i))) * Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (nodes == null || nodes.Count == 0) return;
        //Gizmos.color = Color.blue;
        //for (int i = 0; i < nodes.Length - 1; i++)
        //{
        //    Gizmos.DrawLine(nodes[i], nodes[i + 1]);
        //}

        Gizmos.color = Color.green;

        //float step = 1f / (float)precision;
        //for (int i = 0; i < nodes.Length-1; i+=3)
        //{
        //    Vector3[] array = { nodes[i], nodes[i + 1], nodes[i + 2], nodes[i + 3] };
        //    Vector3 prev = array[0];
        //    for (float t = 0f; t <= 1f; t += step)
        //    {
        //        Vector3 Q = Vector3.zero;
        //        for (int j = 0; j < array.Length; j++)
        //        {
        //            Q += array[j] * GetBernstein(array.Length - 1, j, t);
        //        }
        //        Gizmos.DrawLine(prev, Q);
        //        prev = Q;
        //    }
        //    Gizmos.DrawLine(prev, array[array.Length - 1]);
        //}

        float step = 1f / (float)precision;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (i == 0) Gizmos.color = Color.red;
            else Gizmos.color = Color.blue;

            Vector3[] road = nodes[i];
            for (int j= 0; j < road.Length - 3; j += 3)
            {
                Vector3[] array = { road[j], road[j + 1], road[j + 2], road[j + 3] };
                Vector3 prev = array[0];
                for (float t = 0f; t <= 1f; t += step)
                {
                    Vector3 Q = Vector3.zero;
                    for (int k = 0; k < array.Length; k++)
                    {
                        Q += array[k] * GetBernstein(array.Length - 1, k, t);
                    }
                    Gizmos.DrawLine(prev, Q);
                    prev = Q;
                }
                Gizmos.DrawLine(prev, array[array.Length - 1]);
            }
        }
    }
}

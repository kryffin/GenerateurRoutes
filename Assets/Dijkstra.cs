using System.Collections.Generic;

public static class Dijkstra
{
    public static int[] Run(int start, Graphe G, int XSize, int ZSize)
    {
        List<int> poids = new List<int>();
        float[] distance = new float[(XSize + 1) * (ZSize + 1)];
        int[] pred = new int[(XSize + 1) * (ZSize + 1)];

        for (int i = 0; i < (XSize + 1) * (ZSize + 1); ++i)
            pred[i] = -1;

        for (int i = 0; i < (XSize + 1) * (ZSize + 1); ++i)
            distance[i] = float.PositiveInfinity;

        distance[start] = 0;
        int cursor = start;

        KeyValuePair<float, int> min;
        while (poids.Count < (XSize + 1) * (ZSize + 1))
        {
            min = new KeyValuePair<float, int>(float.PositiveInfinity, 0);
            for (int i = 0; i < (XSize + 1) * (ZSize + 1); ++i)
                if (!poids.Contains(i) && distance[i] < min.Key)
                {
                    min = new KeyValuePair<float, int>(distance[i], i);
                    break;
                }

            cursor = min.Value;
            poids.Add(cursor);

            for (int i = 0; i < (XSize + 1) * (ZSize + 1); ++i)
                if (!poids.Contains(i) && G.adjMatrix[cursor][i] > 0 && distance[i] > distance[cursor] + G.adjMatrix[cursor][i])
                {
                    distance[i] = distance[cursor] + G.adjMatrix[cursor][i];
                    pred[i] = cursor;
                }
        }

        return pred;
    }
}

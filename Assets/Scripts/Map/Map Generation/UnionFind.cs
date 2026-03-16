using UnityEngine;

public class UnionFind
{
    private readonly int[] parent;
    private readonly byte[] rank;

    public UnionFind(int size)
    {
        parent = new int[size];
        rank = new byte[size];

        for (int i = 0; i < size; i++)
            parent[i] = i;
    }

    public int Find(int x)
    {
        while (parent[x] != x)
        {
            parent[x] = parent[parent[x]];
            x = parent[x];
        }

        return x;
    }

    public void Union(int a, int b)
    {
        int rootA = Find(a);
        int rootB = Find(b);

        if (rootA == rootB)
            return;

        if (rank[rootA] < rank[rootB])
            parent[rootA] = rootB;
        else if (rank[rootA] > rank[rootB])
            parent[rootB] = rootA;
        else
        {
            parent[rootB] = rootA;
            rank[rootA]++;
        }
    }
}

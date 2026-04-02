using System.Collections.Generic;

public class MinHeapWithSize
{
    private List<(GridCell cell, float key)> heap = new();
    private int capacity;

    public MinHeapWithSize(int capacity)
    {
        this.capacity = capacity;
    }

    public int Count => heap.Count;

    public List<(GridCell cell, float key)> GetItems() => heap;

    public void Push((GridCell cell, float key) item)
    {
        if (heap.Count < capacity)
        {
            heap.Add(item);
            HeapifyUp(heap.Count - 1);
        }
        else if (item.key > heap[0].key)
        {
            heap[0] = item;
            HeapifyDown(0);
        }
    }

    private void HeapifyUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (heap[parent].key <= heap[i].key) break;

            (heap[parent], heap[i]) = (heap[i], heap[parent]);
            i = parent;
        }
    }

    private void HeapifyDown(int i)
    {
        int last = heap.Count - 1;

        while (true)
        {
            int left = i * 2 + 1;
            int right = i * 2 + 2;
            int smallest = i;

            if (left <= last && heap[left].key < heap[smallest].key)
                smallest = left;

            if (right <= last && heap[right].key < heap[smallest].key)
                smallest = right;

            if (smallest == i) break;

            (heap[i], heap[smallest]) = (heap[smallest], heap[i]);
            i = smallest;
        }
    }

    public void Clear()
    {
        heap.Clear();
    }
}
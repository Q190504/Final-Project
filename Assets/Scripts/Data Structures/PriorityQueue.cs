using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    private readonly List<T> heap;
    private readonly IComparer<T> comparer;

    public int Count => heap.Count;

    public PriorityQueue(IComparer<T> comparer)
    {
        this.comparer = comparer;
        heap = new List<T>();
    }

    public void Enqueue(T item)
    {
        heap.Add(item);
        HeapifyUp(heap.Count - 1);
    }

    public T Dequeue()
    {
        if (heap.Count == 0)
            return default;

        T root = heap[0];
        int lastIndex = heap.Count - 1;

        heap[0] = heap[lastIndex];
        heap.RemoveAt(lastIndex);

        if (heap.Count > 0)
            HeapifyDown(0);

        return root;
    }

    public T Peek()
    {
        if (heap.Count == 0)
            return default;

        return heap[0];
    }

    public void Clear()
    {
        heap.Clear();
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;

            if (comparer.Compare(heap[index], heap[parent]) >= 0)
                break;

            Swap(index, parent);
            index = parent;
        }
    }

    private void HeapifyDown(int index)
    {
        int lastIndex = heap.Count - 1;

        while (true)
        {
            int left = index * 2 + 1;
            int right = index * 2 + 2;
            int smallest = index;

            if (left <= lastIndex && comparer.Compare(heap[left], heap[smallest]) < 0)
                smallest = left;

            if (right <= lastIndex && comparer.Compare(heap[right], heap[smallest]) < 0)
                smallest = right;

            if (smallest == index)
                break;

            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int a, int b)
    {
        (heap[a], heap[b]) = (heap[b], heap[a]);
    }
}
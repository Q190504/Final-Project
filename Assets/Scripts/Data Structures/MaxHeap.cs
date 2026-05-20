using System.Collections.Generic;

public class MaxHeap<T>
{
    private List<(T item, float priority)> heap = new();

    public int Count => heap.Count;

    public void Push(T item, float priority)
    {
        heap.Add((item, priority));
        HeapifyUp(heap.Count - 1);
    }

    public T Pop()
    {
        var root = heap[0].item;

        heap[0] = heap[^1];
        heap.RemoveAt(heap.Count - 1);

        HeapifyDown(0);

        return root;
    }

    public void Clear()
    {
        heap.Clear();
    }

    private void HeapifyUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) / 2;

            if (heap[i].priority <= heap[parent].priority)
                break;

            (heap[i], heap[parent]) = (heap[parent], heap[i]);

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

            if (left > last)
                break;

            int largest = left;

            if (right <= last &&
                heap[right].priority > heap[left].priority)
            {
                largest = right;
            }

            if (heap[i].priority >= heap[largest].priority)
                break;

            (heap[i], heap[largest]) = (heap[largest], heap[i]);

            i = largest;
        }
    }
}
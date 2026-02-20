using System;
using System.Collections.Generic;

public class FastPriorityQueue<T> where T : IHeapItem<T>
{
    private List<T> heap;
    private Comparison<T> comparison;

    public int Count => heap.Count;

    public FastPriorityQueue(Comparison<T> comparison)
    {
        this.heap = new List<T>();
        this.comparison = comparison;
    }

    public void Enqueue(T item)
    {
        item.HeapIndex = heap.Count;
        heap.Add(item);
        SortUp(item);
    }

    public T Dequeue()
    {
        T firstItem = heap[0];
        int lastIndex = heap.Count - 1;

        heap[0] = heap[lastIndex];
        heap[0].HeapIndex = 0;

        heap.RemoveAt(lastIndex);

        if (heap.Count > 0)
            SortDown(heap[0]);

        firstItem.HeapIndex = -1;

        return firstItem;
    }

    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    public void Clear()
    {
        heap.Clear();
    }

    private void SortUp(T item)
    {
        while (true)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;
            if (parentIndex < 0) break;

            T parent = heap[parentIndex];

            if (comparison(item, parent) < 0)
                Swap(item, parent);
            else
                break;
        }
    }

    private void SortDown(T item)
    {
        while (true)
        {
            int leftChild = item.HeapIndex * 2 + 1;
            int rightChild = item.HeapIndex * 2 + 2;
            int smallest = item.HeapIndex;

            if (leftChild < heap.Count &&
                comparison(heap[leftChild], heap[smallest]) < 0)
                smallest = leftChild;

            if (rightChild < heap.Count &&
                comparison(heap[rightChild], heap[smallest]) < 0)
                smallest = rightChild;

            if (smallest != item.HeapIndex)
                Swap(item, heap[smallest]);
            else
                break;
        }
    }

    private void Swap(T a, T b)
    {
        heap[a.HeapIndex] = b;
        heap[b.HeapIndex] = a;

        int temp = a.HeapIndex;
        a.HeapIndex = b.HeapIndex;
        b.HeapIndex = temp;
    }
}

using System;
using System.Collections.Generic;

public class GenericPriorityQueue<T>
{
    private List<T> data;
    private Comparison<T> comparison;

    public int Count => data.Count;

    public GenericPriorityQueue(Comparison<T> comparison)
    {
        this.data = new List<T>();
        this.comparison = comparison;
    }

    public void Push(T item)
    {
        data.Add(item);
        int ci = data.Count - 1;
        while (ci > 0)
        {
            int pi = (ci - 1) / 2;
            if (comparison(data[ci], data[pi]) >= 0) break;
            T tmp = data[ci]; data[ci] = data[pi]; data[pi] = tmp;
            ci = pi;
        }
    }

    public T Pop()
    {
        int li = data.Count - 1;
        T frontItem = data[0];
        data[0] = data[li];
        data.RemoveAt(li);

        --li;
        int pi = 0;
        while (true)
        {
            int ci = pi * 2 + 1;
            if (ci > li) break;
            int rc = ci + 1;
            if (rc <= li && comparison(data[rc], data[ci]) < 0) ci = rc;
            if (comparison(data[pi], data[ci]) <= 0) break;
            T tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp;
            pi = ci;
        }
        return frontItem;
    }

    public void Clear()
    {
        data.Clear();
    }
}
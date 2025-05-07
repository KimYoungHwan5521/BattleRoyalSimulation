using System.Collections.Generic;

public class ThreadSafeList<T>
{
    private List<T> internalList = new List<T>();
    private object listLock = new object();

    public void Add(T item)
    {
        lock (listLock)
        {
            internalList.Add(item);
        }
    }

    public void Remove(T item)
    {
        lock (listLock)
        {
            internalList.Remove(item);
        }
    }

    public bool Contains(T item)
    {
        lock (listLock)
        {
            return internalList.Contains(item);
        }
    }

    public int Count
    {
        get
        {
            lock (listLock)
            {
                return internalList.Count;
            }
        }
    }

    public T this[int index]
    {
        get
        {
            lock (listLock)
            {
                return internalList[index];
            }
        }
    }

    public List<T> ToList()
    {
        lock (listLock)
        {
            return new List<T>(internalList);
        }
    }
}

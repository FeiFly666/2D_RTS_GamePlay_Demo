using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
{
    protected virtual void Awake()
    {
        T[] managers = FindObjectsOfType<T>();

        if(managers.Length > 1)
        {
            Destroy(gameObject);
        }
    }

    public static T Get()
    {
        var tag = typeof(T).Name;
        GameObject manager = GameObject.FindWithTag(tag);

        if(manager != null)
        {
            return manager.GetComponent<T>();
        }
        else
        {
            GameObject go = new GameObject(tag);
            go.tag = tag;
            return manager.GetComponent<T>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.ObjectPool
{
    public class ObjectPool<T>where T: Component
    {
        private T prefab;
        private Transform parent;
        private Stack<T> stack = new Stack<T>();

        public ObjectPool(T prefab, int prewarmNum, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;

            Prewarm(prewarmNum);

        }
        private void Prewarm(int count)
        {
            for(int i = 0; i < count; i++)
            {
                T obj = GameObject.Instantiate(prefab, parent);

                obj.gameObject.SetActive(false);

                stack.Push(obj);
            }
        }

        public int Count => stack.Count;

        public T Get()
        {
            T obj;
            if (stack.Count > 0)
            {
                obj = stack.Pop();
            }
            else
            {
                obj = GameObject.Instantiate(prefab, parent);
            }

            obj.gameObject.SetActive(true);

            if(obj.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnSpawn();
            }

            return obj;
        }
        public void Release(T obj)
        {
            if (stack.Contains(obj))
            {
                //Debug.LogWarning($"物体{obj.name}已在池子中,禁止重复Release");
                return;
            }

            if (obj.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnDespawn();
            }

            obj.gameObject.SetActive(false);
            stack.Push(obj);
        }

    }
}

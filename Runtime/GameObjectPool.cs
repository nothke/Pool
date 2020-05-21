using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Nothke.Collections;

public class GameObjectPool : Pool<PoolableGameObject>
{
    public GameObjectPool(GameObject prefab, int capacity) : base(capacity)
    {
        for (int i = 0; i < capacity; i++)
        {
            array[i].gameObject = GameObject.Instantiate(prefab);
            array[i].gameObject.SetActive(false);
        }
    }

    public GameObject GetGO()
    {
        return Get().gameObject;
    }

    public void Release(GameObject item)
    {
        for (int i = 0; i < capacity; i++)
        {
            if (array[i].gameObject != item)
                continue;

            alive[i] = false;
            array[i].OnRelease();
        }
    }
}

public class PoolableGameObject : IPoolable
{
    public GameObject gameObject;

    public void Init(GameObject prefab)
    {
        gameObject = GameObject.Instantiate(prefab);
    }

    public void OnGet()
    {
        gameObject.SetActive(true);
    }

    public void OnRelease()
    {
        gameObject.SetActive(false);
    }
}
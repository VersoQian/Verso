using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI.Pool
{
    public sealed class MultiplyComponentPoolFactory : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] _prefabs;
        [SerializeField]
        private Transform _content;
        [SerializeField]
        private Transform _poolStorage;

        private readonly Dictionary<Type, HashSet<GameObject>> _instances;
        private Dictionary<Type, Queue<GameObject>> _pool;

        public Transform Content { get { return _content; } }

        public int CountInstances
        {
            get { return _instances.Count; }
        }

        public MultiplyComponentPoolFactory()
        {
            _instances = new Dictionary<Type, HashSet<GameObject>>();
            _pool = new Dictionary<Type, Queue<GameObject>>();
        }

        public void Dispose()
        {
            ReleaseAllInstances();

            foreach (var queue in _pool.Values)
            {
                foreach (GameObject gameObject in queue)
                {
                    GameObject.Destroy(gameObject);
                }
            }

            _pool.Clear();
        }
        
        public Component Get(Type type, Transform parent)
        {
            var pool = GetPool(type);

            if (pool.Count == 0)
            {
                GameObject result = GetInstance(type);
                pool.Enqueue(result);
            }

            var resultComponent = pool.Dequeue().GetComponent(type);
            if (null == resultComponent)
            {
                return resultComponent;
            }

            var go = resultComponent.gameObject;
            var t = resultComponent.transform;

            parent = (null == parent) ? _content : parent;

            if (t.parent != parent)
            {
                t.SetParent(parent, false);
            }

            GetInstances(type).Add(go);

            if (!go.activeSelf)
            {
                go.SetActive(true);
            }

            return resultComponent;
        }

        private GameObject GetInstance(Type type)
        {
            foreach (var prefab in _prefabs)
            {
                var component = prefab.gameObject.GetComponent(type);
                if (component != null)
                {
                    return Instantiate(prefab);
                }
            }

            return null;
        }

        public void Release<T>(T component) where T : Component
        {
            var go = component.gameObject;
            if (GetInstances(typeof(T)).Contains(go))
            {
                go.SetActive(false);
                if (_poolStorage)
                {
                    go.transform.SetParent(_poolStorage, false);
                }
                GetPool(typeof(T)).Enqueue(go);
                GetInstances(typeof(T)).Remove(go);
            }
        }

        public void ReleaseComponent(Component component)
        {
            Type type = component.GetType();

            var go = component.gameObject;
            if (GetInstances(type).Contains(go))
            {
                go.SetActive(false);
                if (_poolStorage)
                {
                    go.transform.SetParent(_poolStorage, false);
                }

                GetPool(type).Enqueue(go);
                GetInstances(type).Remove(go);
            }
        }

        public void ReleaseAllInstances()
        {
            foreach (var hashSet in _instances)
            {
                foreach (GameObject instance in hashSet.Value)
                {
                    instance.SetActive(false);
                    GetPool(hashSet.Key).Enqueue(instance);
                }
            }

            _instances.Clear();
        }

        public void SetNewPrefabs(GameObject[] prefabs)
        {
	        _prefabs = prefabs;
        }
        
        private Queue<GameObject> GetPool(Type type)
        {
            Queue<GameObject> result;

            if (!_pool.TryGetValue(type, out result))
            {
                var newQueue = result = new Queue<GameObject>();
                _pool.Add(type, newQueue);
            }

            return result;
        }

        private HashSet<GameObject> GetInstances(Type type)
        {
            HashSet<GameObject> result;

            if (!_instances.TryGetValue(type, out result))
            {
                var newHashSet = result = new HashSet<GameObject>();
                _instances.Add(type, newHashSet);
            }

            return result;
        }
    }
}

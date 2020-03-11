using System.Collections;
using System.Collections.Generic;

namespace CerealDevelopment.TimeManagement
{
    internal class UnityComponentsList<T> where T : IUnityComponent
    {
        private readonly List<T> instances = new List<T>();
        private readonly List<int> ids = new List<int>();

        private int count;
        public int Count
        {
            get => count;
        }
        public T this[int i]
        {
            get { return instances[i]; }
            set
            {
                instances[i] = value;
                ids[i] = value.GetInstanceID();
            }
        }

        public UnityComponentsList()
        {

        }

        public UnityComponentsList(List<T> list)
        {
            AddRange(list);
        }

        public T GetByID(int instanceID)
        {
            for (int i = 0; i < count; i++)
            {
                if (ids[i] == instanceID)
                {
                    return instances[i];
                }
            }
            return default(T);
        }

        public void Clear()
        {
            instances.Clear();
            ids.Clear();
            count = 0;
        }

        public void Add(T value)
        {
            instances.Add(value);
            ids.Add(value.GetInstanceID());

            count++;
        }

        public void AddRange(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var obj = list[i];
                ids.Add(obj.GetInstanceID());
                instances.Add(obj);
            }
            count += list.Count;
        }


        public bool AddUnique(T value)
        {
            if (!Contains(value))
            {
                Add(value);
                return true;
            }
            return false;
        }

        public void AddUniqueRange(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                AddUnique(list[i]);
            }
        }

        public bool Contains(T value)
        {
            var id = value.GetInstanceID();
            for (int i = 0; i < count; i++)
            {
                if (id == ids[i])
                {
                    return true;
                }
            }
            return false;
        }


        public void RemoveAtWithReorder(int index)
        {
            var last = count - 1;
            if (index > last)
            {
                throw new System.ArgumentOutOfRangeException();
            }
            if (index < last)
            {
                ids[index] = ids[last];
                instances[index] = instances[last];
                ids.RemoveAt(last);
                instances.RemoveAt(last);
            }
            else
            {
                ids.RemoveAt(index);
                instances.RemoveAt(index);
            }
            count--;
        }

        public bool RemoveSwapBack(T value)
        {
            var id = value.GetInstanceID();
            for (int i = 0; i < count; i++)
            {
                if (id == ids[i])
                {
                    var last = count - 1;
                    if (i < last)
                    {
                        ids[i] = ids[last];
                        instances[i] = instances[last];
                        ids.RemoveAt(last);
                        instances.RemoveAt(last);
                    }
                    else
                    {
                        ids.RemoveAt(i);
                        instances.RemoveAt(i);
                    }
                    count--;
                    return true;
                }
            }
            return false;
        }

        public bool Remove(T value)
        {
            var id = value.GetInstanceID();
            for (int i = 0; i < count; i++)
            {
                if (id == ids[i])
                {
                    ids.RemoveAt(i);
                    instances.RemoveAt(i);
                    count--;
                    return true;
                }
            }
            return false;
        }

        public int IndexOf(T value)
        {
            var id = value.GetInstanceID();
            for (int i = 0; i < count; i++)
            {
                if (ids[i] == id)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
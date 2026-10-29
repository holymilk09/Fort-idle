using System.Collections.Generic;
using UnityEngine;

namespace FortIdle
{
    public static class FortPool
    {
        static readonly Dictionary<int, Stack<GameObject>> bins = new Dictionary<int, Stack<GameObject>>();

        public static GameObject Take(int key, System.Func<GameObject> create)
        {
            Stack<GameObject> stack;
            if (bins.TryGetValue(key, out stack))
            {
                while (stack.Count > 0)
                {
                    GameObject go = stack.Pop();
                    if (go == null)
                        continue;
                    go.SetActive(true);
                    return go;
                }
            }

            return create();
        }

        public static void Give(int key, GameObject go)
        {
            if (go == null)
                return;
            go.SetActive(false);
            Stack<GameObject> stack;
            if (!bins.TryGetValue(key, out stack))
            {
                stack = new Stack<GameObject>();
                bins[key] = stack;
            }

            stack.Push(go);
        }
    }
}

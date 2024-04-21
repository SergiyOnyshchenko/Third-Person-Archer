using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public static class ProtertyExtensions
{
    public static T GetProperty<TF, T>(this TF[] properties) where T : MonoBehaviour where TF : MonoBehaviour
    {
        for (int i = 0; i < properties.Length; i++)
        {
            if (properties[i] != null && properties[i].GetType() == typeof(T) && properties[i] is T t)
            {
                return t;
            }
        }

        return null;
    }

    public static bool TryGetProperty<TF, T>(this TF[] properties, out T t) where T : MonoBehaviour where TF : MonoBehaviour
    {
        for (int i = 0; i < properties.Length; i++)
        {
            if (properties[i].GetType() == typeof(T))
            {
                t = properties[i] as T;
                return true;
            }
        }

        t = null;
        return false;
    }

    public static bool TryGetPropertys<TF, T>(this TF[] properties, out List<T> t) where T : MonoBehaviour where TF : MonoBehaviour
    {   
        t = new List<T>();

        for (int i = 0; i < properties.Length; i++)
        {
            if (properties[i] != null && properties[i].GetType() == typeof(T))
                t.Add(properties[i] as T);
        }

        if(t.Count > 0)
            return true;
        else
            return false;
    }

    public static void TryGetProperty<TF, T>(this TF[] properties, UnityAction<T> success, UnityAction fail = null)
        where T : MonoBehaviour where TF : MonoBehaviour
    {
        for (int i = 0; i < properties.Length; i++)
        {
            if (properties[i] != null && properties[i].GetType() == typeof(T))
            {
                success?.Invoke(properties[i] as T);
                return;
            }
        }

        fail?.Invoke();
    }

    public static bool HasProperty<TF, T>(this TF[] properties) where T : MonoBehaviour where TF : MonoBehaviour
    {
        for (int i = 0; i < properties.Length; i++)
        {
            if (properties[i] != null && properties[i].GetType() == typeof(T))
            {
                return true;
            }
        }

        return false;
    }

    public static void AddProperty<T>(this T[] properties, T t) where T : MonoBehaviour
    {
        T[] props = new T[properties.Length + 1];

        for (int i = 0; i < properties.Length; i++)
        {
            props[i] = properties[i];
        }

        props[properties.Length - 1] = t;
        properties = props;
    }
}

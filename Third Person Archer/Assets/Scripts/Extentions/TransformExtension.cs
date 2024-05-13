using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension
{
    public static Transform GetParent(Transform child)
    {
        Transform current = child;
        bool isSearching = true;

        while (isSearching)
        {
            if (current.parent == null)
                isSearching = false;
            else
                current = current.parent;
        }

        return current;
    }

    public static Transform GetParentWithTag(Transform child)
    {
        Transform current = child;
        bool isSearching = true;

        while (isSearching)
        {
            if (current.parent == null)
            {
                isSearching = false;
            }
            else
            {
                if (current.tag == "Parent")
                    isSearching = false;
                else
                    current = current.parent;
            }

        }

        return current;
    }
}


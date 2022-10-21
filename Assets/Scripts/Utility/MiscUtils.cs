
using UnityEngine;

public static class MiscUtils 
{
    public static Transform FindGrandChildByName(Transform transform, string name)
    {
        foreach (Transform child in transform)
        {
            if (child.name == name)
                return child;
            var childTransform = FindGrandChildByName(child, name);
            if (childTransform != null)
                return childTransform;
        }

        return null;
    }
}

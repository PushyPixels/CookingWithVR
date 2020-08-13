using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensionMethods
{
    public static void SetLayerRecursively(this GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach(Transform child in obj.transform)
        {
            child.gameObject.SetLayerRecursively(layer);
        }
    }
}

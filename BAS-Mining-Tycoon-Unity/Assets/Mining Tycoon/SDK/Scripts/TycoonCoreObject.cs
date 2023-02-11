using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

[ExecuteInEditMode]
public class TycoonCoreObject : MonoBehaviour
{
    [SerializeField] private CoreType type;
    private CoreType lastType;

    private void OnValidate()
    {
        if (lastType != type)
        {
            ValidateCustomReference(true);
            lastType = type;
        }

        // Rename.
        name = type.ToString();

        // Ensure the zone exists in references.
        ValidateCustomReference();
    }

    private void OnDestroy()
    {
        ValidateCustomReference(true);
        if (type == CoreType.Shop) Debug.LogError($"MAKE SURE YOUR TYCOON HAS A {type}!!");
    }

    private void OnEnable()
    {
        // Ensure the zone exists in references.
        ValidateCustomReference();
    }

    /// <summary>
    /// Add or remove this zone.
    /// </summary>
    private void ValidateCustomReference(bool remove = false)
    {
#if UNITY_EDITOR
        if (!UnityEditor.PrefabUtility.IsPartOfPrefabInstance(gameObject))
        { return; }
#endif

        // Dirty but cope.
        Level level = FindObjectOfType<Level>();

        foreach (var reference in level.customReferences)
        {
            if (string.CompareOrdinal(reference.name, type.ToString()) == 0)
            {
                if (reference.transforms.Contains(transform))
                {
                    if (remove)
                    {
                        reference.transforms.Remove(transform);
                        Debug.Log($"{transform.name} removed from {type}!");
                    }
                }
                else
                {
                    if (type == CoreType.Shop)
                    {
                        Debug.LogError("SHOP ALREADY EXISTS, YOU CAN ONLY HAVE 1 SHOP IN THE LEVEL!");
                        continue;
                    }

                    reference.transforms.Add(transform);
                    Debug.Log($"{transform.name} added to {type}!");
                }

                return;
            }
        }

        if (!remove)
        {
            foreach (var reference in level.customReferences)
            {
                if (string.CompareOrdinal(reference.name, type.ToString()) == 0)
                {
                    reference.transforms.Clear();
                    reference.transforms.Add(transform);

                    Debug.Log($"{type} has been configured!");
                    return;
                }
            }

            level.customReferences.Add(new Level.CustomReference()
            {
                name = type.ToString(),
                transforms = new List<Transform>() { transform }
            });

            Debug.Log($"{type} has been configured!");
        }
    }
}
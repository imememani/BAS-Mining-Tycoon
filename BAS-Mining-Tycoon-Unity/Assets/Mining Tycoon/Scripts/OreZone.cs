using ThunderRoad;
using UnityEngine;

[ExecuteInEditMode]
public class OreZone : MonoBehaviour
{
    [SerializeField] private Tier oreTier = Tier.Copper;
    private Tier lastTier;

    private Color GetTierColour()
    {
        switch (oreTier)
        {
            case Tier.Copper: ColorUtility.TryParseHtmlString("#fc6603", out Color copper); return copper;

            case Tier.Iron: ColorUtility.TryParseHtmlString("#333333", out Color iron); return iron;

            case Tier.Ruby: ColorUtility.TryParseHtmlString("#de0b1d", out Color ruby); return ruby;

            case Tier.Gold: ColorUtility.TryParseHtmlString("#dec50b", out Color gold); return gold;
        }

        return Color.black;
    }

    private void OnValidate()
    {
        if (lastTier != oreTier)
        {
            ValidateCustomReference(true);
            lastTier = oreTier;
        }

        // Rename.
        name = $"{oreTier}-Zone";
        transform.localRotation = Quaternion.identity;

        // Ensure the zone exists in references.
        ValidateCustomReference();
    }

    private void OnDestroy()
    {
        ValidateCustomReference(true);
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
        // Dirty but cope.
        Level level = FindObjectOfType<Level>();

        foreach (var reference in level.customReferences)
        {
            if (string.CompareOrdinal(reference.name, "Zones") == 0)
            {
                if (reference.transforms.Contains(transform))
                {
                    if (remove)
                    {
                        reference.transforms.Remove(transform);
                        Debug.Log($"{transform.name} removed from zones!");
                    }
                }
                else
                {
                    reference.transforms.Add(transform);
                    Debug.Log($"{transform.name} added to zones!");
                }

                return;
            }
        }

        if (!remove)
        {
            level.customReferences.Add(new Level.CustomReference()
            {
                name = "Zones",
                transforms = new System.Collections.Generic.List<Transform>() { transform }
            });

            Debug.Log($"{transform.name} added to zones!");
        }
    }

    private void OnDrawGizmos()
    {
        Color colour = GetTierColour();
        Gizmos.color = colour;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        colour.a = 0.35f;
        Gizmos.color = colour;
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
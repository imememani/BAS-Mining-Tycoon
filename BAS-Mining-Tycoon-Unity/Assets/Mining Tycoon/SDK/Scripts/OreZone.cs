using ThunderRoad;
using UnityEngine;

[ExecuteInEditMode]
public class OreZone : MonoBehaviour
{
    [SerializeField] private Tier oreTier = Tier.Very_Common;
    private Tier lastTier;

    private Color GetTierColour()
    {
        switch (oreTier)
        {
            case Tier.Very_Common: ColorUtility.TryParseHtmlString("#07f2b4", out Color vcommon); return vcommon;

            case Tier.Common: ColorUtility.TryParseHtmlString("#0891a6", out Color common); return common;

            case Tier.UnCommon: ColorUtility.TryParseHtmlString("#044a87", out Color uncommon); return uncommon;

            case Tier.Rare: ColorUtility.TryParseHtmlString("#f29007", out Color rare); return rare;

            case Tier.Very_Rare: ColorUtility.TryParseHtmlString("#b33e14", out Color vrare); return vrare;

            case Tier.Extremely_Rare: ColorUtility.TryParseHtmlString("#960303", out Color erare); return erare;

            case Tier.So_Rare_Theres_Only_One: ColorUtility.TryParseHtmlString("#080808", out Color fuckingrare); return fuckingrare;
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
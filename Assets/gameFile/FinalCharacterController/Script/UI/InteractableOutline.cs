using System.Collections.Generic;
using UnityEngine;

public class InteractableOutline : MonoBehaviour
{
    public Renderer[] targetRenderers;
    public Material outlineMaterial; // Should be a unique reference for this component

    private readonly Dictionary<Renderer, Material[]> originalMaterials = new();
    private bool isHighlighted;

    private void Awake()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = GetComponentsInChildren<Renderer>();
        }
    }

    public void EnableOutline()
    {
        if (isHighlighted) return;
        isHighlighted = true;

        foreach (var rend in targetRenderers)
        {
            if (!originalMaterials.ContainsKey(rend))
            {
                originalMaterials[rend] = rend.sharedMaterials;
            }

            var mats = rend.materials;
            var newMats = new Material[mats.Length + 1];
            mats.CopyTo(newMats, 0);
            newMats[mats.Length] = outlineMaterial;

            rend.materials = newMats;
        }
    }

    public void DisableOutline()
    {
        if (!isHighlighted) return;
        isHighlighted = false;

        foreach (var rend in targetRenderers)
        {
            if (originalMaterials.TryGetValue(rend, out var original))
            {
                rend.materials = original;
            }
        }

        originalMaterials.Clear();
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VisualClue : ProximityInteractableBase 
{
    [Header("Highlight Settings")]
    [SerializeField] private List<Renderer> highlightRenderers = new List<Renderer>();
    [SerializeField] private Color highlightEmissionColor = Color.cyan;
    [SerializeField] private float emissionIntensity = 1.5f;

    private readonly List<Color> _originalEmissionColours = new List<Color>();
    private bool _materialsCached;

    protected virtual void Awake()
    {
        CacheOriginalColours();
    }

    protected override void OnPlayerEnteredRange()
    {
        EnableHighlight(true);
    }

    protected override void OnPlayerExitedRange()
    {
        EnableHighlight(false);
    }

    private void CacheOriginalColours()
    {
        if (_materialsCached)
        {
            return;
        }

        _materialsCached = true;
        _originalEmissionColours.Clear();

        foreach (var renderer in highlightRenderers)
        {
            if (renderer == null)
            {
                _originalEmissionColours.Add(Color.black);
                continue;
            }

            var colour = renderer.sharedMaterial != null && renderer.sharedMaterial.IsKeywordEnabled("_EMISSION")
                ? renderer.sharedMaterial.GetColor("_EmissionColor")
                : Color.black;

            _originalEmissionColours.Add(colour);
        }
    }

    private void EnableHighlight(bool enabled)
    {
        for (var i = 0; i < highlightRenderers.Count; i++)
        {
            var renderer = highlightRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            var material = renderer.material;
            if (material == null)
            {
                continue;
            }

            if (enabled)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", highlightEmissionColor * emissionIntensity);
            }
            else
            {
                if (i < _originalEmissionColours.Count)
                {
                    material.SetColor("_EmissionColor", _originalEmissionColours[i]);
                }
                else
                {
                    material.SetColor("_EmissionColor", Color.black);
                }
            }
        }
    }
}

using UnityEngine;

[System.Serializable]
public class ToonShadeProperties
{
    public float ambientStrength;
    public Color diffuseColor;
    public Color specularColor;
    public Color fresnelColor;
    public float fresnelSize;
    public float lightingCutoff;
    public float falloffAmount;

    public ToonShadeProperties(Material material)
    {
        if (material.HasProperty("_AmbientStrength"))
            ambientStrength = material.GetFloat("_AmbientStrength");

        if (material.HasProperty("_DiffuseColor"))
            diffuseColor = material.GetColor("_DiffuseColor");

        if (material.HasProperty("_SpecularColor"))
            specularColor = material.GetColor("_SpecularColor");

        if (material.HasProperty("_FresnelColor"))
            fresnelColor = material.GetColor("_FresnelColor");

        if (material.HasProperty("_FresnelSize"))
            fresnelSize = material.GetFloat("_FresnelSize");

        if (material.HasProperty("_LightingCutoff"))
            lightingCutoff = material.GetFloat("_LightingCutoff");

        if (material.HasProperty("_FalloffAmount"))
            falloffAmount = material.GetFloat("_FalloffAmount");
    }
}

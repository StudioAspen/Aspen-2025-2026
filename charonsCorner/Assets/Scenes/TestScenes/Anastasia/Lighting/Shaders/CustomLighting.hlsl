#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

struct CustomLightingData
{
    // Position and orientation
    float3 normalWS;

    // Surface attributes
    float3 albedo;
};

#ifndef SHADERGRAPH_PREVIEW
float3 CustomLightHandling(CustomLightingData d, Light light)
{

    float3 radiance = light.color;

    float diffuse = saturate(dot(d.normalWS, light.direction));

    float3 color = d.albedo * radiance * diffuse;

    return color;
}
#endif

float3 CalculateCustomLighting(CustomLightingData d) {
#ifdef SHADERGRAPH_PREVIEW
    // In preview, estimate diffuse + specular
    float3 lightDir = float3(0.5, 0.5, 0);
    float intensity = saturate(dot(d.normalWS, lightDir));
    return d.albedo * intensity;
#else

    // Get the main light. Located in URP/ShaderLibrary/Lighting.hlsl
    Light mainLight = GetMainLight();

    float3 color = 0;
    // Shade the main light
    color += CustomLightHandling(d, mainLight);

    return color;
#endif
}

void CalculateCustomLighting_float(float3 Normal, float3 Albedo,
    out float3 Color)
{

    CustomLightingData d;
    d.normalWS = Normal;
    d.albedo = Albedo;

    Color = CalculateCustomLighting(d);
}

#endif
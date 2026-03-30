Diffuse = MainDiffuse;
//Specular = MainSpecular;
Color = MainColor * MainDiffuse;

#ifndef SHADERGRAPH_PREVIEW

  uint pixelLightCount = GetAdditionalLightsCount();

  LIGHT_LOOP_BEGIN(pixelLightCount)
  
    //get light color and direction
    lightIndex = GetPerObjectLightIndex(lightIndex);
    Light light = GetAdditionalPerObjectLight(lightIndex, WorldPosition);

    //calculate shadows
    light.shadowAttenuation = AdditionalLightRealtimeShadow(lightIndex, WorldPosition, light.direction);
    float atten = light.distanceAttenuation * light.shadowAttenuation; // - shadowPower;

    //if certain distance, make shadow color black?
    //#if light.distanceAttenuation > 50x


    //calculate diffuse and specular
    float NdotL = saturate(dot(WorldNormal, light.direction) * 0.5 + 0.5);
    float thisDiff = atten * NdotL;
    //float thisSpec = LightingSpecular(thisDiff, light.direction, WorldNormal, WorldView, 1, Smoothness);

    //accumalate light
    Diffuse += thisDiff;
    //Specular += thisSpecu;
    Color += light.color * thisDiff; //+ thisSpec);

  LIGHT_LOOP_END

  float total = Diffuse; //+ dot(Specular, float3(0.333, 0.333, 0.333));
  Color = total <= 0 ? MainColor : Color / total;

#endif
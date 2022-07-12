#define SHADER_NAME BlinnPhongFS

#include "Camera.glsl";
#include "Sprite3D.glsl"

#include "BlinnPhongFrag.glsl";

void getBinnPhongSurfaceParams(inout Surface surface, in PixelParams pixel)
{
    surface.diffuseColor = u_DiffuseColor.rgb;
    surface.alpha = u_DiffuseColor.a;

#if defined(COLOR) && defined(ENABLEVERTEXCOLOR)
    surface.diffuseColor *= pixel.vertexColor.xyz;
    surface.alpha *= pixel.vertexColor.a;
#endif // COLOR && ENABLEVERTEXCOLOR

    vec2 uv = pixel.uv0;

#ifdef DIFFUSEMAP
    vec4 diffuseSampler = texture2D(u_DiffuseTexture, uv);
    surface.diffuseColor *= u_DiffuseColor.rgb * diffuseSampler.rgb * u_AlbedoIntensity;
    surface.alpha *= diffuseSampler.a;
#endif // DIFFUSEMAP

    surface.diffuseColor *= u_AlbedoIntensity;

#ifdef ALPHATEST
    if (surface.alpha < u_AlphaTestValue)
	{
	    discard;
	}
#endif // ALPHATEST

#ifdef SPECULARMAP
    vec4 specularSampler = texture2D(u_SpecularTexture, uv);
    surface.gloss = specularSampler.rgb;
#else // SPECULARMAP
    #ifdef DIFFUSEMAP
    surface.gloss = vec3(diffuseSampler.a);
    #else // DIFFUSEMAP
    surface.gloss = vec3(1.0, 1.0, 1.0);
    #endif // DIFFUSEMAP
#endif // SPECULARMAP
    surface.specularColor = u_MaterialSpecular.rgb;
    surface.shininess = u_Shininess;
}

void main()
{
    PixelParams pixel;
    getPixelParams(pixel);

    Surface surface;
    getBinnPhongSurfaceParams(surface, pixel);

    vec3 surfaceColor = vec3(0.0);

#if defined(LIGHTING)
    vec3 lightingColor = BlinnPhongLighting(surface, pixel);
    surfaceColor += lightingColor;
#endif // LIGHTING

    // todo  GI ?

#ifdef FOG
    surfaceColor = sceneLitFog(surfaceColor);
#endif // FOG

    gl_FragColor = vec4(surfaceColor, surface.alpha);
}
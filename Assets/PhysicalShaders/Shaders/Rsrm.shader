/*  Author's info:
 *  - Blog  : http://n00body.squarespace.com/
 *  - E-mail: crunchy.bytes.blog@gmail.com
 *
 *  Usage Requirements:
 *  - If you use a large portion of my codebase, please credit either myself
 *      or the original author for portions of code that have been borrowed
 *  - If you wish to use bits of my code in a commercial project, please use
 *      the provided information to contact me and tell me about it.
 */

Shader "Physical/Core/Rsrm" {
Properties {
    _Color         		("Light Bake Tint", Color)          		= (1,1,1,1)
	_MainTex          	("Light Bake Color (RGB)", 2D)      		= "white" {}
    _AlbedoTint         ("Albedo Tint", Color)              		= (1,1,1,1)
    _AlbedoMap          ("Albedo (RGB)", 2D)                		= "white" {}
    _ReflectanceScale   ("Reflectance Tint", Color)        			= (1,1,1,1)
    _ReflectanceMap     ("F0 (RGB) Glossiness (A)", 2D)     		= "white" {}
    _AoMap              ("Ambient Occlusion (G)", 2D)       		= "white" {}
    _BumpMap            ("Normal Map", 2D)                   		= "bump" {}
    _IncandescenceTint  ("Incandescence Tint", Color)       		= (1,1,1,1)
    _IncandescenceScale ("Incandescence Intensity", Float)  		= 0
    _IncandescenceMap   ("Incandescence (RGB)", 2D)         		= "white" {}
	
    _Rsrm               ("Radially-Symmetric Reflection Map", 2D)	= "black" {}
}
    
SubShader { 
    Tags { "RenderType" = "Opaque" }
    LOD 300
    
CGPROGRAM
    #pragma surface surf PhysicalBrdf noambient novertexlights
	#pragma exclude_renderers flash gles
    #pragma target 3.0
	
	#include "PhysicalShading.cginc"
	                  
	struct Input {
	    float2 uv_MainTex; 
	    float3 viewDir;
	    float3 worldRefl;
	    float3 worldNormal;
	    INTERNAL_DATA
	};
        
	float4 _AlbedoTint; 
	float4 _ReflectanceScale;
	float4 _IncandescenceTint;
	float _IncandescenceScale;
	sampler2D _AoMap;
	sampler2D _AlbedoMap; 
	sampler2D _ReflectanceMap;
	sampler2D _IncandescenceMap;
	sampler2D _BumpMap;
	sampler2D _Rsrm;
		
	void surf (Input IN, inout PhysicalSurfaceOutput o) {    
	    half4 albedo = _AlbedoTint * tex2D(_AlbedoMap, IN.uv_MainTex);
	    half4 reflectance = _ReflectanceScale * tex2D(_ReflectanceMap, IN.uv_MainTex);
	    
	    o.Occlusion = tex2D(_AoMap, IN.uv_MainTex).g;
	    o.Albedo    = albedo.rgb;	    
	    o.Normal    = RecoverNormal(tex2D(_BumpMap, IN.uv_MainTex)).xyz;  
	    o.F0		= reflectance.rgb;
	    o.Specular  = reflectance.a;
	    o.Emission  = DeGamma(_IncandescenceScale) * _IncandescenceTint.rgb * tex2D(_IncandescenceMap, IN.uv_MainTex).rgb;
	
    	half dotNE = saturate(dot(o.Normal, normalize(IN.viewDir))); 
    	half3 n = normalize((half3)WorldNormalVector(IN, o.Normal));
    	half3 r = normalize((half3)WorldReflectionVector(IN, o.Normal));
		ImageBasedLighting(_Rsrm, n, r, dotNE, o);
	}
ENDCG
}
    
Fallback "Bumped Specular"
}
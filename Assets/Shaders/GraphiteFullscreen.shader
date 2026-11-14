Shader "Custom/Graphite Fullscreen"
{
	Properties
	{
		_Contrast ("Contrast", Range(1, 4)) = 1.15
		_ColorPreservation ("Color Preservation", Range(0, 1)) = 0.9
		_EdgeStrength ("Outline Strength", Range(0, 3)) = 0.12
		_EdgeThreshold ("Outline Threshold", Range(0.005, 0.2)) = 0.16
		_GrainStrength ("Pencil Grain", Range(0, 0.3)) = 0.012
		_PaperStrength ("Paper Texture", Range(0, 0.3)) = 0.02
		_HatchStrength ("Graphite Hatching", Range(0, 0.5)) = 0.005
	}

	SubShader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" }

		Pass
		{
			Name "GraphiteFullscreen"
			ZWrite Off
			ZTest Always
			Cull Off

			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

			float _Contrast;
			float _ColorPreservation;
			float _EdgeStrength;
			float _EdgeThreshold;
			float _GrainStrength;
			float _PaperStrength;
			float _HatchStrength;

			float LuminanceAt(float2 uv)
			{
				half3 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;
				return dot(color, float3(0.2126, 0.7152, 0.0722));
			}

			float Hash21(float2 coord)
			{
				return frac(sin(dot(coord, float2(127.1, 311.7))) * 43758.5453);
			}

			half4 Frag(Varyings input) : SV_Target
			{
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
				float2 uv = input.texcoord;
				half3 sourceColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).rgb;
				float center = LuminanceAt(uv);
				float2 pixel = _BlitTexture_TexelSize.xy;

				float horizontal = abs(LuminanceAt(uv + float2(pixel.x, 0)) - LuminanceAt(uv - float2(pixel.x, 0)));
				float vertical = abs(LuminanceAt(uv + float2(0, pixel.y)) - LuminanceAt(uv - float2(0, pixel.y)));
				float diagonalA = abs(LuminanceAt(uv + pixel) - LuminanceAt(uv - pixel));
				float diagonalB = abs(LuminanceAt(uv + float2(pixel.x, -pixel.y)) - LuminanceAt(uv - float2(pixel.x, -pixel.y)));
			float edge = max(max(horizontal, vertical), max(diagonalA, diagonalB));
			edge = smoothstep(_EdgeThreshold, _EdgeThreshold + 0.2, edge);

				float graphite = saturate((center - 0.5) * _Contrast + 0.5);
				graphite = saturate(graphite - edge * _EdgeStrength);

				float2 screenPixel = floor(uv * _ScreenParams.xy);
				float grain = Hash21(screenPixel) - 0.5;
				float paper = Hash21(floor(screenPixel * 0.28)) - 0.5;
				graphite += grain * _GrainStrength + paper * _PaperStrength;

				float darkArea = 1.0 - smoothstep(0.2, 0.78, graphite);
				float hatchA = smoothstep(0.82, 0.98, sin((screenPixel.x + screenPixel.y) * 0.075));
				float hatchB = smoothstep(0.88, 0.99, sin((screenPixel.x - screenPixel.y) * 0.065));
				graphite -= (hatchA + hatchB * 0.45) * darkArea * _HatchStrength;

				graphite = saturate(graphite);

				float chroma = max(sourceColor.r, max(sourceColor.g, sourceColor.b)) -
					min(sourceColor.r, min(sourceColor.g, sourceColor.b));
				float preserveColor = smoothstep(0.025, 0.2, chroma) * _ColorPreservation;
				float graphiteShade = saturate(graphite / max(center, 0.001));
				half3 coloredGraphite = sourceColor * graphiteShade;
				half3 outputColor = lerp(graphite.xxx, coloredGraphite, preserveColor);
				return half4(outputColor, 1.0);
			}
			ENDHLSL
		}
	}
}

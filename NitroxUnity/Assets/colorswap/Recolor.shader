Shader "Nitrox/Recolor"
{
    // GPU replacement for NitroxClient's HsvSwapper CPU pixel loop: samples the source texture and, within up
    // to 4 independent UV-space regions, replaces the player's equipment base color the same way HueSwapper /
    // HueSaturationVibrancySwapper did on the CPU. Intended to be driven via Graphics.Blit(source, dest, mat)
    // with region/range/replacement-color uniforms set on the shared material immediately beforehand - it never
    // touches lighting and knows nothing about the Marmoset player-model shader; only the baked RenderTexture
    // this produces gets dropped into that shader's existing _MainTex/_SpecTex slots.
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define NITROX_MAX_RECOLOR_REGIONS 4

            sampler2D _MainTex;

            // xMin, yMin, xMax, yMax in UV space (0-1). Non-rect (whole-texture) managers pass (0,0,1,1).
            float4 _RegionRect[NITROX_MAX_RECOLOR_REGIONS];
            // Each range uses only .xy (min, max); .zw are unused padding so every array can be a Vector4[] on the C# side.
            float4 _RegionHueRange[NITROX_MAX_RECOLOR_REGIONS];
            float4 _RegionSatRange[NITROX_MAX_RECOLOR_REGIONS];
            float4 _RegionVibRange[NITROX_MAX_RECOLOR_REGIONS];
            float4 _RegionAlphaRange[NITROX_MAX_RECOLOR_REGIONS];
            // 0 = hue-only (HueSwapper: keep original saturation/vibrancy), 1 = hue+saturation+vibrancy (HueSaturationVibrancySwapper).
            float _RegionSwapMode[NITROX_MAX_RECOLOR_REGIONS];
            int _RegionCount;
            // Player color pre-converted to HSV once per Blit call (hue, saturation, vibrancy).
            float3 _ReplacementHSV;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Mirrors UnityEngine.Color.RGBToHSV (RGBToHSVHelper) exactly, so range boundaries behave the same
            // as the CPU HsvSwapper this replaces.
            void RGBToHSV(float3 rgb, out float h, out float s, out float v)
            {
                float dominant, one, two, offset;
                if (rgb.b > rgb.g && rgb.b > rgb.r)
                {
                    dominant = rgb.b; one = rgb.r; two = rgb.g; offset = 4.0;
                }
                else if (rgb.g > rgb.r)
                {
                    dominant = rgb.g; one = rgb.b; two = rgb.r; offset = 2.0;
                }
                else
                {
                    dominant = rgb.r; one = rgb.g; two = rgb.b; offset = 0.0;
                }

                v = dominant;
                if (v != 0.0)
                {
                    float m = min(one, two);
                    float delta = v - m;
                    if (delta != 0.0)
                    {
                        s = delta / v;
                        h = offset + (one - two) / delta;
                    }
                    else
                    {
                        s = 0.0;
                        h = offset + (one - two);
                    }
                    h /= 6.0;
                    if (h < 0.0)
                    {
                        h += 1.0;
                    }
                }
                else
                {
                    s = 0.0;
                    h = 0.0;
                }
            }

            // Mirrors UnityEngine.Color.HSVToRGB exactly.
            float3 HSVToRGB(float h, float s, float v)
            {
                if (s == 0.0)
                {
                    return float3(v, v, v);
                }
                if (v == 0.0)
                {
                    return float3(0.0, 0.0, 0.0);
                }

                float num1 = h * 6.0;
                int num2 = (int)floor(num1);
                float num3 = num1 - num2;
                float num4 = v * (1.0 - s);
                float num5 = v * (1.0 - s * num3);
                float num6 = v * (1.0 - s * (1.0 - num3));

                // num2 is 0-5 for any well-formed h in [0,1); case 5 and the rare -1 (from float rounding
                // right at the h=0 wrap) both fall through to the same result, matching Unity's HSVToRGB switch.
                if (num2 == 0) return float3(v, num6, num4);
                if (num2 == 1) return float3(num5, v, num4);
                if (num2 == 2) return float3(num4, v, num6);
                if (num2 == 3) return float3(num4, num5, v);
                if (num2 == 4) return float3(num6, num4, v);
                return float3(v, num4, num5);
            }

            bool InRect(float2 uv, float4 rect)
            {
                return uv.x >= rect.x && uv.x <= rect.z && uv.y >= rect.y && uv.y <= rect.w;
            }

            // Mirrors ColorValueRange.Covers: inclusive on both ends.
            bool Covers(float value, float4 range)
            {
                return value >= range.x && value <= range.y;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, i.uv);

                for (int r = 0; r < _RegionCount; r++)
                {
                    if (!InRect(i.uv, _RegionRect[r]))
                    {
                        continue;
                    }

                    float h, s, v;
                    RGBToHSV(color.rgb, h, s, v);

                    if (Covers(h, _RegionHueRange[r]) &&
                        Covers(s, _RegionSatRange[r]) &&
                        Covers(v, _RegionVibRange[r]) &&
                        Covers(color.a, _RegionAlphaRange[r]))
                    {
                        color.rgb = _RegionSwapMode[r] < 0.5
                            ? HSVToRGB(_ReplacementHSV.x, s, v)
                            : HSVToRGB(_ReplacementHSV.x, _ReplacementHSV.y, _ReplacementHSV.z);
                    }

                    // Regions are non-overlapping - stop after the containing region is found.
                    break;
                }

                return color;
            }
            ENDCG
        }
    }
}

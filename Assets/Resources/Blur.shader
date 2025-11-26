Shader "Custom/Blur"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Spread("Standard Deviation (Spread)", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #define E 2.71828f

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        float4 _MainTex_TexelSize;

        CBUFFER_START(UnityPerMaterial)
            float _Spread;
        CBUFFER_END

        float gaussian(int x, float spread)
        {
            float sigmaSqu = spread * spread;
            return (1.0 / sqrt(TWO_PI * sigmaSqu)) * pow(E, -(x * x) / (2.0 * sigmaSqu));
        }

        struct appdata
        {
            float4 positionOS : POSITION;
            float2 uv         : TEXCOORD0;
        };

        struct v2f
        {
            float4 positionCS : SV_POSITION;
            float2 uv         : TEXCOORD0;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
            o.uv = v.uv;
            return o;
        }

        float4 SampleTex(float2 uv)
        {
            return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
        }

        // Compute an odd kernel size based on spread
        void ComputeKernel(out int lower, out int upper, float spread)
        {
            float s = max(spread, 0.0001);
            int gridSize = (int)ceil(s * 6.0);
            if ((gridSize & 1) == 0)
                gridSize++;

            upper = (gridSize - 1) / 2;
            lower = -upper;
        }

        ENDHLSL

        // -------- Horizontal blur pass (Pass 0) --------
        Pass
        {
            Name "Horizontal"

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag_horizontal

            float4 frag_horizontal(v2f i) : SV_Target
            {
                float3 col     = 0;
                float  gridSum = 0;

                int upper, lower;
                ComputeKernel(lower, upper, _Spread);

                [loop]
                for (int x = lower; x <= upper; ++x)
                {
                    float g = gaussian(x, _Spread);
                    gridSum += g;

                    float2 uv = i.uv + float2(_MainTex_TexelSize.x * x, 0.0);
                    col += g * SampleTex(uv).rgb;
                }

                col /= max(gridSum, 1e-5);
                return float4(col, 1.0);
            }
            ENDHLSL
        }

        // -------- Vertical blur pass (Pass 1) --------
        Pass
        {
            Name "Vertical"

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag_vertical

            float4 frag_vertical(v2f i) : SV_Target
            {
                float3 col     = 0;
                float  gridSum = 0;

                int upper, lower;
                ComputeKernel(lower, upper, _Spread);

                [loop]
                for (int y = lower; y <= upper; ++y)
                {
                    float g = gaussian(y, _Spread);
                    gridSum += g;

                    float2 uv = i.uv + float2(0.0, _MainTex_TexelSize.y * y);
                    col += g * SampleTex(uv).rgb;
                }

                col /= max(gridSum, 1e-5);
                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }
}


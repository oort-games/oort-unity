Shader "OortUnity/Samples/Icon Sample Unlit"
{
    Properties
    {
        _Color ("Color", Color) = (0.15, 0.65, 0.9, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Geometry"
            "RenderType" = "Opaque"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            float4x4 unity_ObjectToWorld;
            float4x4 unity_MatrixVP;
            float4 _Color;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float4 positionWS = mul(unity_ObjectToWorld, input.positionOS);
                output.positionCS = mul(unity_MatrixVP, positionWS);
                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}

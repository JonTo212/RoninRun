Shader "Custom/InvisibleShadowCaster" {
    SubShader {
        Tags { "Queue" = "Overlay" }  // Ensure it's rendered last
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            // Basic shadow caster settings
            ZWrite On
            ColorMask 0  // No color is written
        }
    }

    // Additional pass for rendering to the camera as invisible
    SubShader {
        Tags { "Queue" = "Overlay" }  // Ensure it's rendered last
        Pass {
            Name "Invisible"
            Tags { "LightMode" = "Always" }

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask 0
        }
    }

    Fallback "Diffuse"
}


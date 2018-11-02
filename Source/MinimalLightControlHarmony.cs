using System;
using Verse;
using Harmony;
using System.Reflection;
using RimWorld;
using UnityEngine;

namespace MinimalLightControl
{

    [StaticConstructorOnStartup]
    static class MinimalLightControlHarmony
    {

        public static readonly Texture2D UsePlanetDayNightSystem = ContentFinder<Texture2D>.Get("UI/Buttons/MLCToggle", true);
        public static bool MLCOn = true;

        [HarmonyPatch(typeof(RimWorld.PlaySettings), "DoPlaySettingsGlobalControls")]
        public static class DoPlaySettingsGlobalControlsPatch
        {
            public static void Postfix(WidgetRow row, bool worldView)
            {
                if(!worldView)
                    row.ToggleableIcon(ref MLCOn, UsePlanetDayNightSystem, "MLC_Toggle_Tooltip".Translate(), SoundDefOf.Mouseover_ButtonToggle, null);
            }
        }

        [HarmonyPatch(typeof(Verse.SkyManager), "CurrentSkyTarget")]
        public static class CurrentSkyTargetPatch
        {
            public static void Postfix(ref SkyTarget __result)
            {
                if (!MLCOn) return;

                float modifier = MinimalLightControl.Instance.MLCModifier / 10f;
                if (MinimalLightControl.Instance.MLCMode == MinimalLightControl.MLCModes.Darker)
                {
                    modifier = modifier * 0.75f + 0.25f;
                    if (ColorGetBrightness(__result.colors.sky) < modifier)
                        ColorChangeBrightness(ref __result.colors.sky, modifier, true);
                    // there is no need to change other values
                    /*
                    if (ColorGetBrightness(__result.colors.shadow) < modifier)
                        ColorChangeBrightness(ref __result.colors.shadow, modifier, true);
                    if (ColorGetBrightness(__result.colors.overlay) < modifier)
                        ColorChangeBrightness(ref __result.colors.overlay, modifier, true);
                    //*/
                }
                else
                {
                    modifier = 1f - modifier /* * 0.75f */;
                    ColorChangeBrightness(ref __result.colors.sky, modifier);
                    /*
                    ColorChangeBrightness(ref __result.colors.shadow, modifier);
                    ColorChangeBrightness(ref __result.colors.overlay, modifier);
                    //*/
                }
            }
        }

        // Get Brightness from HSL
        public static float ColorGetBrightness(Color rgb)
        {
            return (Mathf.Max(rgb.r, rgb.g, rgb.b) + Mathf.Min(rgb.r, rgb.g, rgb.b)) / 2f;
        }

        // convert RGB -> HSL, change L, convert back HSL -> RGB
        public static void ColorChangeBrightness(ref Color rgb, float modifier, bool setModifier = false)
        {
            // based on https://stackoverflow.com/a/9493060
            // RGB to HSL
            float max = Mathf.Max(rgb.r, rgb.g, rgb.b);
            float min = Mathf.Min(rgb.r, rgb.g, rgb.b);
            float h = 0f, s = 0f, l = (max + min) / 2f;

            if (max == min)
            {
                h = s = 0f; // achromatic
            }
            else
            {
                float d = max - min;
                if (l > 0.5f)
                    s = d / (2f - max - min);
                else
                    s = d / (max + min);

                if (rgb.r >= rgb.g && rgb.r >= rgb.b)
                    h = (rgb.g - rgb.b) / d + (rgb.g < rgb.b ? 6f : 0f);
                else if (rgb.g >= rgb.b)
                    h = (rgb.b - rgb.r) / d + 2f;
                else
                    h = (rgb.r - rgb.g) / d + 4f;

                h /= 6f;
            }

            modifier = Mathf.Clamp01(modifier);
            // set brightness
            if (setModifier)
                l = modifier;
            // calculate relative brightness
            else
            {
                if (l < 0.75f)
                {
                    l = 0.75f - l;
                    l = Mathf.Min(l, 0.75f) * modifier;
                    l = 0.75f - l;
                }
            }

            // HSL to RGB
            if (s == 0f)
            {
                rgb.r = rgb.g = rgb.b = l; // achromatic
            }
            else
            {

                float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
                float p = 2f * l - q;
                rgb.r = hue2rgb(p, q, h + 1f / 3f);
                rgb.g = hue2rgb(p, q, h);
                rgb.b = hue2rgb(p, q, h - 1f / 3f);
            }
        }

        // helper function for HSL -> RGB conversion
        public static float hue2rgb(float p, float q, float t)
        {
            if (t < 0f) t += 1;
            else if (t > 1f) t -= 1;
            else if (t < 1f / 6f) return p + (q - p) * 6f * t;
            else if (t < 1f / 2f) return q;
            else if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;

            return p;
        }

    }
}
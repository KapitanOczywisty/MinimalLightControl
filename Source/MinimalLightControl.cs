using System;
using Verse;
using UnityEngine;
using RimWorld;
using HugsLib;
using HugsLib.Settings;

namespace MinimalLightControl
{
    public class MinimalLightControl : ModBase
    {
        public override string ModIdentifier
        {
            get { return "MinimalLightControl"; }
        }

        public static MinimalLightControl Instance { get; private set; }

        public MinimalLightControl()
        {
            Instance = this;
        }

        //public SettingHandle<bool> UseMLC { get; private set; }
        public SettingHandle<MLCModes> MLCMode { get; private set; }
        public SettingHandle<int> MLCModifier { get; private set; }
        public enum MLCModes { Darker, Relative }

        public override void DefsLoaded()
        {
            /*UseMLC = Settings.GetHandle<bool>("UseMLC", "MLC_Enabled_Title".Translate(), "MLC_Enabled_Desc".Translate(), true);*/
            MLCMode = Settings.GetHandle<MLCModes>("MLCMode", "MLC_Mode_Title".Translate(), "MLC_Mode_Desc".Translate(), MLCModes.Darker, null, "MLC_Mode_");
            MLCModifier = Settings.GetHandle<int>("MLCModifier", "MLC_Modifier_Title".Translate(), "MLC_Modifier_Desc".Translate(), 7, Validators.IntRangeValidator(0, 10));
        }

    }
}
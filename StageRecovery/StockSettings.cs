using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using KSP.Localization;



namespace StageRecovery
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class SR1 : GameParameters.CustomParameterNode
    {
        public override string Title { get { return ""; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Stage Recovery"; } }
        public override string DisplaySection { get { return "Stage Recovery"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }

        internal static bool settingsFlatRateModel = false;
        internal static bool settingsUseDREVelocity = true;
        internal static bool settingsUseDistanceOverride = false;

        [GameParameters.CustomParameterUI("#StageRecovery_StockSettings_ModEnabled")]//Mod Enabled
        public bool SREnabled = true;


        [GameParameters.CustomParameterUI("#StageRecovery_StockSettings_UseDistanceOverride",//Use Distance Override
            toolTip = "#StageRecovery_StockSettings_UseDistanceOverride_desc")]//Enable Distance Override to use a specified value for distance modification rather than calculating it
        public bool UseDistanceOverride = false;

        [GameParameters.CustomParameterUI("#StageRecovery_StockSettings_FlatRateModel",//Enable Flat Rate Model
            toolTip = "#StageRecovery_StockSettings_FlatRateModel_desc")]//Disabled this to use a Variable Rate Model
        public bool FlatRateModel = false;

        [GameParameters.CustomParameterUI("#StageRecovery_StockSettings_UseDREVelocity")]//Use the DRE Velocity
        public bool UseDREVelocity = true;


        [GameParameters.CustomParameterUI("#StageRecovery_StockSettings_PreRecover",//Pre-Recover Vessels
            toolTip = "#StageRecovery_StockSettings_PreRecover_desc")]//Recover Kerbals before a ship is deleted
        public bool PreRecover = true;

        [GameParameters.CustomParameterUI("#StageRecovery_StockSettings_ShowFailureMessages")]//Failure Messages
        public bool ShowFailureMessages = true;

        [GameParameters.CustomParameterUI("#StageRecovery_StockSettings_ShowSuccessMessages")]//Success Messages
        public bool ShowSuccessMessages = true;

        [GameParameters.CustomParameterUI("#StageRecovery_StockSettings_PoweredRecovery")]//Try Powered Recovery
        public bool PoweredRecovery = true;

        [GameParameters.CustomParameterUI("#StageRecovery_StockSettings_RecoverClamps")]//Recover Clamps
        public bool RecoverClamps = true;

        [GameParameters.CustomParameterUI("#StageRecovery_StockSettings_UseUpgrades")]//Tie Into Upgrades
        public bool UseUpgrades = true;

        [GameParameters.CustomParameterUI("#StageRecovery_StockSettings_hideSpaceCenterButton",//Hide the SpaceCenter button
            toolTip = "#StageRecovery_StockSettings_hideSpaceCenterButton_desc")]//The button merely opens a window directing you to these settings pages
        public bool hideSpaceCenterButton = false;

        [GameParameters.CustomParameterUI("#StageRecover_StockSettings_disableRecalcRealchute",
            toolTip = "#StageRecover_StockSettings_disableRecalcRealchute_desc")]//The button merely opens a window directing you to these settings pages
        public bool disableRecalcRealchute = false;



        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {

            settingsFlatRateModel = FlatRateModel;
            settingsUseDREVelocity = UseDREVelocity;
            settingsUseDistanceOverride = UseDistanceOverride;

            return true;
        }
        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }
    /// <summary>
    ///
    /// </summary>

    public class SR2 : GameParameters.CustomParameterNode
    {
        public override string Title { get { return "Rate Model Settings"; } }//Localizer.Format("")
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Stage Recovery"; } }
        public override string DisplaySection { get { return "Stage Recovery"; } }
        public override int SectionOrder { get { return 2; } }
        public override bool HasPresets { get { return false; } }

        /// ///////////////
        // RecoveryModifier
        /// ///////////////
        public float recoveryModifier = 0.75f;
        [GameParameters.CustomFloatParameterUI("#StageRecovery_StockSettings_RecoveryModifier", minValue = 0.0f, maxValue = 100.0f,
         toolTip = "#StageRecovery_StockSettings_RecoveryModifier_desc")]//Flat Rate: Recovery Modifier (%)""Modifies recovery payout by this percentage
        public float RecoveryMod
        {
            get { return recoveryModifier * 100; }
            set { recoveryModifier = value / 100.0f; }
        }
        public float RecoveryModifier
        {
            get { return recoveryModifier; }
            set { recoveryModifier = value; }
        }
        /// ///////////////




        [GameParameters.CustomFloatParameterUI("#StageRecovery_StockSettings_CutoffVelocity", minValue = 2.0f, maxValue = 12.0f, displayFormat = "F1",
         toolTip = "#StageRecovery_StockSettings_CutoffVelocity_desc")]//Flat Rate: Cutoff Velocity""Maximum velocity for recovery
        public double CutoffVelocity = 10f;


        [GameParameters.CustomFloatParameterUI("#StageRecovery_StockSettings_HighCutoffVelocity", minValue = 2.0f, maxValue = 12.0f, displayFormat = "F1",
         toolTip = "#StageRecovery_StockSettings_HighCutoffVelocity_desc")]//Variable Rate: High Cutoff Velocity""Maximum velocity for recovery
        public double HighCut = 12f;

        [GameParameters.CustomFloatParameterUI("#StageRecovery_StockSettings_LowCutoffVelocity", minValue = 2.0f, maxValue = 12.0f, displayFormat = "F1",
         toolTip = "#StageRecovery_StockSettings_LowCutoffVelocity_desc")]//Variable Rate: Low Cutoff Velocity""Maximum velocity for total recovery
        public double LowCut = 6f;



        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            HighCut = (float)Math.Max(Math.Round(HighCut, 1), LowCut + 0.1);

            return true; //otherwise return true
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            //if (HighLogic.CurrentGame == null || HighLogic.CurrentGame.Parameters == null)
            //    return true;

            if (SR1.settingsFlatRateModel)
            {
                if (member.Name == "HighCut" ||
                    member.Name == "LowCut")
                    return false;
            }
            else
            {
                if (member.Name == "RecoveryMod" ||
                    member.Name == "CutoffVelocity")
                    return false;

            }
            return true;
        }
        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }

    /////
    /////

    public class SR3 : GameParameters.CustomParameterNode
    {
        public override string Title { get { return ""; } } // column heading
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Stage Recovery"; } }
        public override string DisplaySection { get { return "Stage Recovery"; } }
        public override int SectionOrder { get { return 3; } }
        public override bool HasPresets { get { return false; } }

        /// /////////////
        // GlobalModifier
        /// /////////////
        public float globalModifier = 1.0f;
        [GameParameters.CustomFloatParameterUI("#StageRecovery_StockSettings_GlobalModifier", minValue = 0.0f, maxValue = 100.0f,
         toolTip = "#StageRecovery_StockSettings_GlobalModifier_desc")]//Global Modifier (%)""Modifies final payout by this percentage
        public float GlobalMod
        {
            get { return globalModifier * 100; }
            set { globalModifier = value / 100.0f; }
        }
        public float GlobalModifier
        {
            get { return globalModifier; }
            set { globalModifier = value; }
        }

        /// ///////////////
        // DistanceOverride
        /// ///////////////
        public float distanceOverride = 0.01f;
        [GameParameters.CustomFloatParameterUI("#StageRecovery_StockSettings_DistanceOverride", minValue = 1f, maxValue = 100.0f,
                 toolTip = "#StageRecovery_StockSettings_DistanceOverride_desc")]//Distance Override (%)""If >= 0, will use this as a distance modifier instead of calculating it
        public float DistanceOver
        {
            get { return distanceOverride * 100; }
            set { distanceOverride = value / 100.0f; }
        }
        public float DistanceOverride
        {
            get { return distanceOverride; }
            set { distanceOverride = value; }
        }


        /// ///////////////////////
        // DeadlyReentrymaxVelocity
        /// ///////////////////////
        public float DeadlyReentryMaxVelocity = 2000f;
        [GameParameters.CustomIntParameterUI("#StageRecovery_StockSettings_DREVelocity2", minValue = 0, maxValue = 6000, stepSize = 200,//DRE Velocity 2
                 toolTip = "#StageRecovery_StockSettings_DREVelocity2_desc")]//If >= 0, will use this as a distance modifier instead of calculating it
        public int DreVelocity
        {
            get { return (int)DeadlyReentryMaxVelocity; }
            set { DeadlyReentryMaxVelocity = (float)value; }
        }


        [GameParameters.CustomFloatParameterUI("#StageRecovery_StockSettings_PoweredTWR", minValue = 1.0f, maxValue = 12.0f, stepCount = 111, displayFormat = "F1",//Powered TWR
        toolTip = "#StageRecovery_StockSettings_PoweredTWR_desc")]//Minimum TWR needed for a powered recovery
        public double MinTWR = 1.0f;


        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            return true; //otherwise return true
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            //if (HighLogic.CurrentGame == null || HighLogic.CurrentGame.Parameters == null)
            //    return true;

            if (member.Name == "DreVelocity")
                return SR1.settingsUseDREVelocity;
            if (member.Name == "DistanceOver")
                return SR1.settingsUseDistanceOverride;
            return true;
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }
    }




    public sealed class Settings1
    {
        public static SR1 Instance
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<SR1>();
            }
        }
    }

    public sealed class Settings2
    {
        public static SR2 Instance
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<SR2>();
            }
        }
    }

    public sealed class Settings3
    {
        public static SR3 Instance
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<SR3>();
            }
        }
    }
}

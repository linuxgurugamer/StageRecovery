using KSP.Localization;
using System;
using System.Collections;
using System.Reflection;

namespace StageRecovery
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class SR1 : GameParameters.CustomParameterNode
    {
        public override string Title { get { return Localizer.Format("#StageRecovery_StockSettings_TitleGeneral"); } }
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

#if false
        [GameParameters.CustomParameterUI("#StageRecovery_StockSettings_hideSpaceCenterButton",//Hide the SpaceCenter button
            toolTip = "#StageRecovery_StockSettings_hideSpaceCenterButton_desc")]//The button merely opens a window directing you to these settings pages
        public bool hideSpaceCenterButton = false;
#endif

        #region AutoCalc delay

#if false
        public int autocalcDelayMs = 500;

        [GameParameters.CustomFloatParameterUI("#StageRecovery_StockSettings_AutoCalcDelay", minValue = 0, maxValue = 10,
                 toolTip = "#StageRecovery_StockSettings_AutoCalcDelay_desc")]
        public int autocalcDelaySec
        {
            get { return autocalcDelayMs / 1000; }
            set { autocalcDelayMs = value * 1000; }
        }
#endif



        [GameParameters.CustomFloatParameterUI("#StageRecovery_StockSettings_AutoCalcDelay", minValue = 0.0f, maxValue = 10.0f, stepCount = 101, displayFormat ="F1",
 toolTip = "#StageRecovery_StockSettings_AutoCalcDelay_desc")]
        public float autocalcDelaySec = 0.5f;





#endregion AutoCalc delay

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
        public override string Title { get { return Localizer.Format("#StageRecovery_StockSettings_TitleRateModel"); } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "Stage Recovery"; } }
        public override string DisplaySection { get { return "Stage Recovery"; } }
        public override int SectionOrder { get { return 2; } }
        public override bool HasPresets { get { return false; } }

#region RecoveryModifier

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

#endregion RecoveryModifier

        [GameParameters.CustomFloatParameterUI("#StageRecovery_StockSettings_CutoffVelocity", minValue = 2.0f, maxValue = 12.0f, displayFormat = "F1",
         toolTip = "#StageRecovery_StockSettings_CutoffVelocity_desc")]//Flat Rate: Cutoff Velocity""Maximum velocity for recovery
        public double CutoffVelocity = 10f;

#region HighCut

        [GameParameters.CustomFloatParameterUI("#StageRecovery_StockSettings_HighCutoffVelocity", minValue = 2.0f, maxValue = 12.0f, displayFormat = "F1",
         toolTip = "#StageRecovery_StockSettings_HighCutoffVelocity_desc")]//Variable Rate: High Cutoff Velocity""Maximum velocity for recovery
        public double HighCutField = 12f;

        public double HighCut
        {
            get { return (float)Math.Max(Math.Round(HighCutField, 1), LowCut + 0.1); }
            set { HighCutField = value; }
        }

#endregion HighCut

        [GameParameters.CustomFloatParameterUI("#StageRecovery_StockSettings_LowCutoffVelocity", minValue = 2.0f, maxValue = 12.0f, displayFormat = "F1",
         toolTip = "#StageRecovery_StockSettings_LowCutoffVelocity_desc")]//Variable Rate: Low Cutoff Velocity""Maximum velocity for total recovery
        public double LowCut = 6f;

#region GlobalModifier

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

#endregion GlobalModifier

#region DistanceOverride

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

#endregion DistanceOverride

#region DeadlyReentryMaxVelocity

        public float DeadlyReentryMaxVelocity = 2000f;

        [GameParameters.CustomIntParameterUI("#StageRecovery_StockSettings_DREVelocity2", minValue = 0, maxValue = 6000, stepSize = 200,//DRE Velocity 2
                 toolTip = "#StageRecovery_StockSettings_DREVelocity2_desc")]//If >= 0, will use this as a distance modifier instead of calculating it
        public int DreVelocity
        {
            get { return (int)DeadlyReentryMaxVelocity; }
            set { DeadlyReentryMaxVelocity = (float)value; }
        }

#endregion DeadlyReentryMaxVelocity

        [GameParameters.CustomFloatParameterUI("#StageRecovery_StockSettings_PoweredTWR", minValue = 1.0f, maxValue = 12.0f, stepCount = 111, displayFormat = "F1",//Powered TWR
        toolTip = "#StageRecovery_StockSettings_PoweredTWR_desc")]//Minimum TWR needed for a powered recovery
        public double MinTWR = 1.0f;

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            return true;
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            switch (member.Name)
            {
                case nameof(HighCut):
                case nameof(LowCut):
                    if (SR1.settingsFlatRateModel)
                        return false;
                    break;

                case nameof(RecoveryMod):
                case nameof(CutoffVelocity):
                    if (!SR1.settingsFlatRateModel)
                        return false;
                    break;

                case nameof(DreVelocity):
                    return SR1.settingsUseDREVelocity;

                case nameof(DistanceOver):
                    return SR1.settingsUseDistanceOverride;
            }

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
}
using KSP.UI.Screens;
using System;
using UnityEngine;
using ToolbarControl_NS;
using ClickThroughFix;

namespace StageRecovery
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(SettingsGUI.MODID, SettingsGUI.MODNAME);
            Settings.Instance.SetRootPath(KSPUtil.ApplicationRootPath);
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.UI.Screens;

namespace StageRecovery
{
    //The settings class. It handles all interactions with the settings file and is related to the GUI for changing the settings.
    public sealed class Settings
    {
        //This is the Settings instance. Only one exists and it's how we interact with the settings
        private static readonly Settings instance = new Settings();

        //This is the instance of the SettingsGUI, where we can change settings in game. This is how we interact with that class.
        public SettingsGUI gui = new SettingsGUI();

        // Not supposed to set the ApplicatoinrootPath like this anymore
        private string pluginDataPath; // = KSPUtil.ApplicationRootPath + "GameData/StageRecovery/PluginData";
        //The path for the settings file (Config.txt)
        private string filePath = "";


        public bool Clicked = false;
        public List<RecoveryItem> RecoveredStages, DestroyedStages;
        public IgnoreList BlackList = new IgnoreList();

        //The constructor for the settings class. It sets the values to default (which are then replaced when Load() is called)
        private Settings()
        {
            RecoveredStages = new List<RecoveryItem>();
            DestroyedStages = new List<RecoveryItem>();
        }

        public void SetRootPath(string rootPath)
        {
            pluginDataPath = rootPath + "GameData/StageRecovery/PluginData";
             filePath = pluginDataPath + "/Config.txt";
       }
        public static Settings Instance
        {
            get
            {
                return instance;
            }
        }

        public void ClearStageLists()
        {
            RecoveredStages.Clear();
            DestroyedStages.Clear();
            gui.flightGUI.NullifySelected();
        }
    }

    public class IgnoreList
    {
        //Set the default ignore items (fairings, escape systems, flags, and asteroids (which are referred to as potatoroids))
        public List<string> ignore = new List<string> { "fairing", "escape system", "flag", "potato" };
        string filePath = KSPUtil.ApplicationRootPath + "GameData/StageRecovery/ignore.txt";
        public void Load()
        {
            if (System.IO.File.Exists(filePath))
            {
                ignore = System.IO.File.ReadAllLines(filePath).ToList();
            }
        }

        public void Save()
        {
            System.IO.File.WriteAllLines(filePath, ignore.ToArray());
        }

        public bool Contains(string item)
        {
            if (ignore.Count == 0)
            {
                Load();
            }

            return ignore.FirstOrDefault(s => item.ToLower().Contains(s)) != null;
        }

        public void Add(string item)
        {
            if (!ignore.Contains(item.ToLower()))
            {
                ignore.Add(item.ToLower());
            }
        }

        public void Remove(string item)
        {
            if (ignore.Contains(item.ToLower()))
            {
                ignore.Remove(item.ToLower());
            }
        }
    }

}
/*
Copyright (C) 2018  Michael Marvin

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


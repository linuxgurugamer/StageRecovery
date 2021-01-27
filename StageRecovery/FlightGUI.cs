using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.Localization;

namespace StageRecovery
{
    //This class contains all the stuff for the in-flight GUI which SR is using as it's primary display of info
    public class FlightGUI
    {
        //This variable controls whether we show the GUI
        public bool showFlightGUI = false;

        //This Rect object controls the physical window (size and location)
        public Rect flightWindowRect = new Rect((Screen.width-600)/2, (Screen.height-480)/2, 240, 1);

        //This is all stuff we need to keep constant between draws
        private int firstToolbarIndex = -1, infoBarIndex = 0;
        private Vector2 stagesScroll, infoScroll;
        private RecoveryItem selectedStage;
        //And this does the actual drawing
        public void DrawFlightGUI(int windowID)
        {
            //Start with a vertical, then a horizontal (stage list and stage info), then another vertical (stage list).
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(225));
            //Draw the toolbar that selects between recovered and destroyed stages
            int temp = firstToolbarIndex;
            //firstToolbarIndex = GUILayout.Toolbar(firstToolbarIndex, new string[] { "Recovered", "Destroyed" });
            GUILayout.BeginHorizontal();
            bool active = GUILayout.Toggle(firstToolbarIndex == 0, Localizer.Format("#StageRecovery_Recovered") + (Settings.Instance.RecoveredStages.Count > 0 ? " ("+Settings.Instance.RecoveredStages.Count+")" : ""), GUI.skin.button);//"Recovered"
            if (!active && firstToolbarIndex == 0)
            {
                firstToolbarIndex = -1;
            }
            else if (active)
            {
                firstToolbarIndex = 0;
            }

            active = GUILayout.Toggle(firstToolbarIndex == 1, Localizer.Format("#StageRecovery_Destroyed") + (Settings.Instance.DestroyedStages.Count > 0 ? " (" + Settings.Instance.DestroyedStages.Count + ")" : ""), GUI.skin.button);//"Destroyed"
            if (!active && firstToolbarIndex == 1)
            {
                firstToolbarIndex = -1;
            }
            else if (active)
            {
                firstToolbarIndex = 1;
            }

            if (temp != firstToolbarIndex)
            {
                NullifySelected();
                if (firstToolbarIndex == -1)
                {
                    flightWindowRect.height = 1;
                }
                else
                {
                    flightWindowRect.height = 480;
                }
            }
            GUILayout.EndHorizontal();
            //NullifySelected will set the selectedStage to null and reset the toolbar

           // GUILayout.Label("FMRS: " + (StageRecovery.FMRS_Enabled() ? "Active" : "Inactive"));

            if (firstToolbarIndex >= 0)
            {
                //Begin listing the recovered/destryoed stages in a scroll view (so you can scroll if it's too long)
                GUILayout.Label((firstToolbarIndex == 0 ? Localizer.Format("#StageRecovery_Recovered") : Localizer.Format("#StageRecovery_Destroyed")) + Localizer.Format("#StageRecovery_Stages"));//"Recovered""Destroyed"" Stages:"
                stagesScroll = GUILayout.BeginScrollView(stagesScroll, HighLogic.Skin.textArea);

                RecoveryItem deleteThis = null;
                //List all recovered stages
                if (firstToolbarIndex == 0)
                {
                    for (int i = 0; i < Settings.Instance.RecoveredStages.Count; i++)
                    //foreach (RecoveryItem stage in Settings.Instance.RecoveredStages)
                    {
                        RecoveryItem stage = Settings.Instance.RecoveredStages[i];
                        string buttonText = stage.StageName;
                        if (stage == selectedStage)
                        {
                            buttonText = "--  " + buttonText + "  --";
                        }

                        if (GUILayout.Button(buttonText))
                        {
                            if (Input.GetMouseButtonUp(0))
                            {
                                //If you select the same stage again it will minimize the list
                                if (selectedStage == stage)
                                {
                                    selectedStage = null;
                                }
                                else
                                {
                                    selectedStage = stage;
                                }
                            }
                            else if (Input.GetMouseButtonUp(1))
                            {
                                //Right clicking deletes the stage
                                deleteThis = stage;
                            }
                        }
                    }
                }
                //List all destroyed stages
                else if (firstToolbarIndex == 1)
                {
                    for (int i = 0; i < Settings.Instance.DestroyedStages.Count; i++)
                    //foreach (RecoveryItem stage in Settings.Instance.DestroyedStages)
                    {
                        RecoveryItem stage = Settings.Instance.DestroyedStages[i];
                        string buttonText = stage.StageName;
                        if (stage == selectedStage)
                        {
                            buttonText = "--  " + buttonText + "  --";
                        }

                        if (GUILayout.Button(buttonText))
                        {
                            if (Input.GetMouseButtonUp(0))
                            {
                                //If you select the same stage again it will minimize the list
                                if (selectedStage == stage)
                                {
                                    selectedStage = null;
                                }
                                else
                                {
                                    selectedStage = stage;
                                }
                            }
                            else if (Input.GetMouseButtonUp(1))
                            {
                                //Right clicking deletes the stage
                                deleteThis = stage;
                            }
                        }
                    }
                }

                if (deleteThis != null)
                {
                    if (deleteThis == selectedStage)
                    {
                        NullifySelected();
                    }

                    if (firstToolbarIndex == 0)
                    {
                        Settings.Instance.RecoveredStages.Remove(deleteThis);
                    }
                    else
                    {
                        Settings.Instance.DestroyedStages.Remove(deleteThis);
                    }
                }

                //End the list of stages
                GUILayout.EndScrollView();
            }

            //GUILayout.Label("FMRS: " + StageRecovery.FMRS_Enabled(false).ToString());
            //GUILayout.Label("Chutes: " + StageRecovery.FMRS_Enabled(true).ToString());

            GUILayout.EndVertical();

            //If a stage is selected we show the info for it
            if (selectedStage != null)
            {
                //Make the window larger to accomodate the info
                if (flightWindowRect.width != 600)
                {
                    flightWindowRect.width = 600;
                }

                GUILayout.BeginVertical(HighLogic.Skin.textArea);
                //Show a toolbar with options for specific data, defaulting to the Parts list
                if (selectedStage.propRemaining.Count > 0)
                    infoBarIndex = GUILayout.Toolbar(infoBarIndex, new string[] { Localizer.Format("#StageRecovery_InfoBar_Parts"), Localizer.Format("#StageRecovery_InfoBar_Crew"), Localizer.Format("#StageRecovery_InfoBar_Science"), Localizer.Format("#StageRecovery_InfoBar_Info"), Localizer.Format("#StageRecovery_InfoBar_Fuel") });//"Parts""Crew""Science""Info""Fuel"
                else
                {
                    if (infoBarIndex == 4)
                        infoBarIndex = 3;
                    infoBarIndex = GUILayout.Toolbar(infoBarIndex, new string[] { Localizer.Format("#StageRecovery_InfoBar_Parts"), Localizer.Format("#StageRecovery_InfoBar_Crew"), Localizer.Format("#StageRecovery_InfoBar_Science"), Localizer.Format("#StageRecovery_InfoBar_Info") });//"Parts", "Crew", "Science", "Info"
                }
                //List the stage name and whether it was recovered or destroyed
                GUILayout.Label(Localizer.Format("#StageRecovery_StagesName", selectedStage.StageName));//"Stage name: " + 
                GUILayout.Label(Localizer.Format("#StageRecovery_Status", (selectedStage.Recovered ? Localizer.Format("#StageRecovery_Recovered") : Localizer.Format("#StageRecovery_Destroyed"))));//"Status: " + "RECOVERED""DESTROYED"
                //Put everything in a scroll view in case it is too much data for the window to display
                infoScroll = GUILayout.BeginScrollView(infoScroll);                

                //Depending on the selected data view we display different things (split into different functions for ease)
                switch (infoBarIndex)
                {
                    case 0: DrawPartsInfo(); break;
                    case 1: DrawCrewInfo(); break;
                    case 2: DrawScienceInfo(); break;
                    case 3: DrawAdvancedInfo(); break;
                    case 4: DrawFuelInfo(); break;
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
                //End the info side of the window
            }
            //If no stage is selected we reset the window size back to 240
            else
            {
                if (flightWindowRect.width != 240)
                {
                    flightWindowRect.width = 240;
                }
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            //End the entire window

            //Make it draggable
            if (!Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
            {
                GUI.DragWindow();
            }
        }

        //Set the selected stage to null and reset the info toolbar to "Parts"
        public void NullifySelected()
        {
            selectedStage = null;
            infoBarIndex = 0;
        }

        //Draw all the info for recovered/destroyed parts
        private void DrawPartsInfo()
        {
            //List all of the parts and their recovered costs (or value if destroyed)
            GUILayout.Label(Localizer.Format("#StageRecovery_PartsonStage"));//"Parts on Stage:"
            for (int i=0; i<selectedStage.PartsRecovered.Count; i++)
            {
                string name = selectedStage.PartsRecovered.Keys.ElementAt(i);
                int amt = selectedStage.PartsRecovered.Values.ElementAt(i);
                double cost = selectedStage.Costs.Values.ElementAt(i);
                double percent = selectedStage.Recovered ? selectedStage.RecoveryPercent :  1;
                GUILayout.Label(amt + "x " + name + " @ " + Math.Round(cost * percent, 2) + ": " + Math.Round(cost * amt * percent, 2));
            }

            //If the stage was recovered, list the refunds for parts, fuel, and total, along with the overall percentage
            if (selectedStage.Recovered)
            {
                GUILayout.Label(Localizer.Format("#StageRecovery_TotalPartsrefunded", Math.Round(selectedStage.DryReturns, 2)));//"\nTotal refunded for parts: " + 
                GUILayout.Label(Localizer.Format("#StageRecovery_TotalFuelrefunded", Math.Round(selectedStage.FuelReturns, 2)));//"Total refunded for fuel: " + 
                GUILayout.Label(Localizer.Format("#StageRecovery_Totalrefunded", Math.Round(selectedStage.FundsReturned, 2)));//"Total refunds: " + 
                GUILayout.Label(Localizer.Format("#StageRecovery_Percentrefunded", Math.Round(100 * selectedStage.RecoveryPercent, 2)));//"Percent refunded: " +  + "%"
                GUILayout.Label(Localizer.Format("#StageRecovery_Totalvalue", Math.Round(selectedStage.FundsOriginal, 2)));//"Total value: " + 
            }
            else //Otherwise just display the total value of the parts
            {
                GUILayout.Label(Localizer.Format("#StageRecovery_TotaPartlvalue", Math.Round(selectedStage.FundsOriginal, 2)));//"\nTotal Part Value: " + 
            }
        }

        //Draw all the info for recovered/destroyed parts
        private void DrawFuelInfo()
        {
            //List all of the parts and their recovered costs (or value if destroyed)
            GUILayout.Label(Localizer.Format("#StageRecovery_RemainingFuel"));//"Remaining Fuel:\n"

            //If the stage was recovered, list the remaining fuel, if any
            if (selectedStage.Recovered)
            {
                if (selectedStage.propRemaining.Count > 0)
                {
                    foreach (var r in selectedStage.propRemaining)
                    {
                        GUILayout.Label(r.Key + ": " + r.Value.ToString("N1"));
                    }
                }
            }
        }
        //This displays what crew members were onboard the stage, if any (recovered or not)
        private void DrawCrewInfo()
        {
            GUILayout.Label(Localizer.Format("#StageRecovery_CrewOnboard"));//"Crew Onboard:"
            if (selectedStage.KerbalsOnboard.Count == 0)
            {
                GUILayout.Label(Localizer.Format("#StageRecovery_None"));//"None"
            }
            else
            {
                for (int i = 0; i < selectedStage.KerbalsOnboard.Count; i++)
                //foreach (CrewWithSeat kerbal in selectedStage.KerbalsOnboard)
                {
                    CrewWithSeat kerbal = selectedStage.KerbalsOnboard[i];
                    GUILayout.Label(kerbal.CrewMember.name);
                }
            }
        }


        //This lists all the science experiments recovered and the total number of points
        private void DrawScienceInfo()
        {
            //List the total number of science points recovered
            //GUILayout.Label("Total Science Recovered: " + (selectedStage.ScienceExperiments.Count == 0 ? "None" : selectedStage.ScienceRecovered.ToString()));
            if (selectedStage.ScienceExperiments.Count != 0)
            {
                //List all of the experiments recovered (including data amounts and titles)
                GUILayout.Label(Localizer.Format("#StageRecovery_Experiments"));//"Experiments:"
                for (int i = 0; i < selectedStage.ScienceExperiments.Count; i++)
                //foreach (string experiment in selectedStage.ScienceExperiments)
                {
                    string experiment = selectedStage.ScienceExperiments[i];
                    GUILayout.Label(experiment);
                }
            }
        }

        //This displays info about distance from KSC, terminal velocity, and all that miscellanous info
        private void DrawAdvancedInfo()
        {
            //Display distance, module used, and terminal velocity
            GUILayout.Label(Localizer.Format("#StageRecovery_KSCDistance", Math.Round(selectedStage.KSCDistance/1000, 2)));//"Distance from KSC: " +  + "km"
            GUILayout.Label(Localizer.Format("#StageRecovery_ParachuteModule", selectedStage.ParachuteModule));//"Parachute Module used: " + 
            GUILayout.Label(Localizer.Format("#StageRecovery_Terminalvelocity", selectedStage.Vt));//"Terminal velocity: "+ + " m/s"
            //List the Vt required for maximal/partial recovery
            if (Settings1.Instance.FlatRateModel)
            {
                GUILayout.Label(Localizer.Format("#StageRecovery_CutoffVelocity", Settings2.Instance.CutoffVelocity));//"Maximum velocity for recovery: " +  + " m/s"
            }
            else
            {
                GUILayout.Label(Localizer.Format("#StageRecovery_HighCut", Settings2.Instance.HighCut));//"Maximum velocity for recovery: " +  + " m/s"
                GUILayout.Label(Localizer.Format("#StageRecovery_LowCut", Settings2.Instance.LowCut));//"Maximum velocity for total recovery: " +  + " m/s"
            }

            //List the percent refunded, broken down into distance and speed amounts
            GUILayout.Label(Localizer.Format("#StageRecovery_RecoveryPercent", Math.Round(100*selectedStage.RecoveryPercent, 2)));//"\nPercent refunded: "+  + "%"
            GUILayout.Label(Localizer.Format("#StageRecovery_DistancePercent", Math.Round(100 * selectedStage.DistancePercent, 2)));//"    --Distance: " +  + "%"
            GUILayout.Label(Localizer.Format("#StageRecovery_SpeedPercent", Math.Round(100 * selectedStage.SpeedPercent, 2)));//"    --Speed: " +  + "%"
            if (Settings2.Instance.GlobalModifier != 1.0F)
            {
                GUILayout.Label(Localizer.Format("#StageRecovery_GlobalModifier", Math.Round(100 * Settings2.Instance.GlobalModifier, 2)));//"    --Global: " +  + "%"
            }
            GUILayout.Label(Localizer.Format("#StageRecovery_FundsReturned", Math.Round(selectedStage.FundsReturned, 2)));//"Total refunds: " + 
            GUILayout.Label(Localizer.Format("#StageRecovery_FundsOriginal", Math.Round(selectedStage.FundsOriginal, 2)));//"Total value: " + 
            
            //If the stage was burned up, display this and the velocity it was going
            if (selectedStage.burnedUp)
            {
                GUILayout.Label(Localizer.Format("#StageRecovery_burnedUp"));//"\nStage burned up on reentry!"
                GUILayout.Label(Localizer.Format("#StageRecovery_srfSpeed", selectedStage.vessel.srfSpeed));//"Surface Speed: " + 
            }

            //If powered recovery was attempted (and fuel was used) then display that and the fuel amounts consumed
            if (selectedStage.poweredRecovery)
            {
                GUILayout.Label(Localizer.Format("#StageRecovery_poweredRecovery"));//"\nPowered recovery was attempted."
                GUILayout.Label(Localizer.Format("#StageRecovery_poweredRecovery2"));//"Fuel consumed:"
                foreach (KeyValuePair<string, double> fuel in selectedStage.fuelUsed)
                {
                    GUILayout.Label(Localizer.Format("#StageRecovery_fuelUsed", fuel.Key,fuel.Value));//// + " : " +  + " units"
                }
            }

            if (selectedStage.noControl)
            {
                GUILayout.Label(Localizer.Format("#StageRecovery_noControl"));//"\nPowered recovery was attempted but no form of control was found."
                GUILayout.Label(Localizer.Format("#StageRecovery_noControl2"));//"Include a pilot or probe with SAS to use powered recovery."

            }
        }
    }
}

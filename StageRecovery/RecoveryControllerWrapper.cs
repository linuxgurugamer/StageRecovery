using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using KSP.UI.Screens;
using static StageRecovery.StageRecovery;

//
// RecoveryControllerWrapper
//
// usage
//  Copy the RecoveryControllerWrapper.cs file into your project.
//  Edit the namespace in RecoveryControllerWrapper.cs to match your plugin's namespace.
//  Use the RecoveryControllerWrapper Plugin's API
//  You can use RecoveryControllerWrapper.RecoveryControllerAvailable to check if the RecoveryControllerWrapper Plugin is actually available. 
//  Note that you must not call any other RecoveryControllerWrapper Plugin API methods if this returns false.
//
// Functions available
//
//      string RecoveryControllerWrapper.RecoveryControllerAvailable()
//      string RecoveryControllerWrapper.RegisterMod(string modName)
//      string RecoveryControllerWrapper.UnRegisterMod(string modName)
//      string RecoveryControllerWrapper.ControllingMod(Vessel v)
//
//  To use:
//
//      Your mod must register itself first, passing in your modname.  This MUST be done before going into either the editor or flight
//      You can unregister you rmod if you like
//      Call the ControllingMod with the vessel to find out which mod is registered to have control of it.

// TODO: Change to your plugin's namespace here.
namespace StageRecovery
{

    /**********************************************************\
    *          --- DO NOT EDIT BELOW THIS COMMENT ---          *
    *                                                          *
    * This file contains classes and interfaces to use the     *
    * Toolbar Plugin without creating a hard dependency on it. *
    *                                                          *
    * There is nothing in this file that needs to be edited    *
    * by hand.                                                 *
    *                                                          *
    *          --- DO NOT EDIT BELOW THIS COMMENT ---          *
    \**********************************************************/
    

    class RecoveryControllerWrapper
    {
        private static bool? recoveryControllerAvailable;
        private static Type calledType;

        public static bool RecoveryControllerAvailable
        {
            get
            {
                Log.Info("RecoveryControllerAvailable");
                if (recoveryControllerAvailable == null)
                {
                    recoveryControllerAvailable = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "RecoveryController");
                    if (recoveryControllerAvailable == true)
                    {
                        calledType = Type.GetType("RecoveryController.RecoveryController,RecoveryController");
                    }
                    else
                        Log.Info("RecoveryController NOT available");
                }
                return recoveryControllerAvailable.GetValueOrDefault();
            }
        }

        static object CallRecoveryController(string func, object modName)
        {
            if (!RecoveryControllerAvailable)
            {
                return null;
            }
            Log.Info("CallRecoveryController, func: " + func);
            try
            {
                 
                if (calledType != null)
                {
                    Log.Info("calledtype not null: " + calledType.ToString());
                    MonoBehaviour rcRef = (MonoBehaviour)UnityEngine.Object.FindObjectOfType(calledType); //assumes only one instance of class Historian exists as this command returns first instance found, also must inherit MonoBehavior for this command to work. Getting a reference to your Historian object another way would work also.
                    if (rcRef != null)
                    {
                        Log.Info("rcRef not null");
                        MethodInfo myMethod = calledType.GetMethod(func, BindingFlags.Instance | BindingFlags.Public);

                        if (myMethod != null)
                        {
                            Log.Info("myMethod not null");
                            object magicValue;
                            if (modName != null)
                            {
                                magicValue = myMethod.Invoke(rcRef, new object[] { modName });
                            }
                            else
                            {
                                magicValue = myMethod.Invoke(rcRef, null);
                            }

                            return magicValue;
                        }
                        else
                        {
                            Log.Info("[SR] " + func + " not available in RecoveryController");                           
                        }
                    }
                    else
                    {
                        Log.Info("[SR] " + func + "  failed");
                        return null;
                    }
                }
                Log.Info("calledtype failed");
                return null;
            }
            catch (Exception e)
            {
                Log.Info("[SR] Error calling type: " + e);
                return null;
            }
        }

        public static  bool RegisterModWithRecoveryController(string modName)
        {
            Log.Info("RegisterModWithRecoveryController");
            var s = CallRecoveryController("RegisterMod", modName);
            if (s == null)
            {
                Log.Info("RegisterMod, CallRecoveryController returned null");
                return false;
            }
            Log.Info("RegisterMod returning: " + ((bool)s).ToString());
            return (bool)s;
        }

        public static  bool UnRegisterMod(string modName)
        {
            var s = CallRecoveryController("UnRegisterMod", modName);
            if (s == null)
            {
                return false;
            }

            return (bool)s;
        }

        public static string ControllingMod(Vessel v)
        {
            var s = CallRecoveryController("ControllingMod", v);
            return s as string;
        }
    }
}

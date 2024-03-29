// SPDX-FileCopyrightText: 2009-2020 TRUMPF Laser GmbH, authors: C-Labs
//
// SPDX-License-Identifier: MPL-2.0

using nsCDEngine.BaseClasses;
using nsCDEngine.Engines;
using nsCDEngine.Engines.NMIService;
using nsCDEngine.Engines.ThingService;
using nsCDEngine.ViewModels;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CDMyHelloWorld
{
    public class eCDMyHelloWorldDeviceTypes : TheDeviceTypeEnum
    {
        public const string cdeThingDeviceTypeA = "My Device Type A";
    }

    [EngineAssetInfo(
        FriendlyName = strFriendlyName,
        Capabilities = new[] { eThingCaps.ConfigManagement, },
        EngineID = "{bb22d7d8-531b-43be-a001-1474cca28ca6}",
        IsService = true,
        LongDescription = "This service...",
        IconUrl = "toplogo-150.png", // TODO Add your own icon
        Developer = "C-Labs", // TODO Add your own name and URL
        DeveloperUrl = "http://www.c-labs.com",
        ManifestFiles = new string[] { }
    )]
    class cdePluginService1 : ThePluginBase
    {
        // TODO: Set plugin friendly name for InitEngineAssets (optional)
        public const String strFriendlyName = "My Hello World Service";

        public override bool Init()
        {
            if (!mIsInitCalled)
            {
                mIsInitCalled = true;
                MyBaseThing.StatusLevel = 1;
                MyBaseThing.LastMessage = "Service has started";

                MyBaseThing.RegisterEvent(eEngineEvents.IncomingMessage, HandleMessage);
                MyBaseEngine.RegisterEvent(eEngineEvents.ThingDeleted, OnThingDeleted);

                // If not lengthy initialized you can remove cdeRunasync and call this synchronously
                TheCommonUtils.cdeRunAsync(MyBaseEngine.GetEngineName() + " Init Services", true, (o) =>
                {
                    // Perform any long-running initialization (i.e. network access, file access) here that must finish before other plug-ins or the C-DEngine can use the plug-in
                    InitServices();

                    // Declare the thing initialized 
                    mIsInitialized = true; // For future IsInit() calls
                    FireEvent(eThingEvents.Initialized, this, true, true); // Notify the C-DEngine and other plug-ins that the thing is initialized
                    MyBaseEngine.ProcessInitialized(); //Set the status of the Base Engine according to the status of the Things it manages
                });
            }
            return false;
        }


        // User-interface defintion
        TheDashboardInfo mMyDashboard;

        public override bool CreateUX()
        {
            if (!mIsUXInitCalled)
            {
                mIsUXInitCalled = true;

                mMyDashboard = TheNMIEngine.AddDashboard(MyBaseThing, new TheDashboardInfo(MyBaseEngine, "My Hello Worlds"));

                var tFlds = TheNMIEngine.CreateEngineForms(MyBaseThing, TheThing.GetSafeThingGuid(MyBaseThing, "MYNAME"), "List of Worlds", null, 20, 0x0F, 0xF0, TheNMIEngine.GetNodeForCategory(), "REFFRESHME", true, new eCDMyHelloWorldDeviceTypes(), eCDMyHelloWorldDeviceTypes.cdeThingDeviceTypeA);
                TheFormInfo tForm = tFlds["Form"] as TheFormInfo;
                tForm.AddButtonText = "Add new World";

                TheNMIEngine.RegisterEngine(MyBaseEngine); //Registers this engine and its resources with the C-DEngine
                mIsUXInitialized = true;
            }
            return true;
        }

        void InitServices()
        {
            List<TheThing> tDevList = TheThingRegistry.GetThingsOfEngine(MyBaseThing.EngineName);
            foreach (TheThing tDev in tDevList)
            {
                if (!tDev.HasLiveObject)
                {
                    // There is no .Net class instance associated with this TheThing: create and register it, so the C-DEngine can find it
                    switch (tDev.DeviceType)
                    {
                        // If your class does not follow the naming convention CDMyPlugin1.<fieldname>, you may need to instantiate it explicitly like this:
                        //case eCDMyHelloWorldDeviceTypes.cdeThingDeviceTypeA:
                        //    TheThingRegistry.RegisterThing(new cdeThingDeviceTypeA(tDev, this));
                        //    break;
                        default:
                            // Assume the eCDMyHelloWorldDeviceTypes field names match the class names: find the field that corresponds to the TheThing.DeviceType being requested
                            var fields = typeof(eCDMyHelloWorldDeviceTypes).GetFields();
                            foreach (FieldInfo deviceTypeField in fields)
                            {
                                var deviceTypeValue = deviceTypeField.GetValue(new eCDMyHelloWorldDeviceTypes()) as string;
                                if (deviceTypeValue == null || deviceTypeField.FieldType.FullName != "System.String") continue;
                                if (deviceTypeValue == tDev.DeviceType)
                                {
                                    // Found a matching field: create an instance of the class based on the field's name, and register it with the C-DEngine
                                    var deviceTypeClass = Type.GetType("CDMyHelloWorld." + deviceTypeField.Name);
                                    if (deviceTypeClass == null)
                                    {
                                        deviceTypeClass = Type.GetType("CDMyHelloWorld.ViewModel." + deviceTypeField.Name);
                                    }
                                    if (deviceTypeClass != null)
                                    {
                                        TheThingRegistry.RegisterThing(Activator.CreateInstance(deviceTypeClass, tDev, this) as ICDEThing);
                                    }
                                    break;
                                }
                            }
                            break;
                    }
                }
            }
            MyBaseEngine.SetStatusLevel(-1); //Calculates the current statuslevel of the service/engine
        }

        void OnThingDeleted(ICDEThing pEngine, object pDeletedThing) // CODE REVIEW: Is this really still needed?
        {
            if (pDeletedThing is ICDEThing)
            {
                //TODO: Stop Resources, Thread etc associated with this Thing
                ((ICDEThing)pDeletedThing).FireEvent(eEngineEvents.ShutdownEvent, pEngine, null, false);
            }
        }

        //TODO: Step 4: Write your Business Logic

        #region Message Handling
        public override void HandleMessage(ICDEThing sender, object pIncoming)
        {
            TheProcessMessage pMsg = pIncoming as TheProcessMessage;
            if (pMsg == null) return;

            string[] cmd = pMsg.Message.TXT.Split(':');
            switch (cmd[0])
            {
                case "CDE_INITIALIZED":
                    MyBaseEngine.SetInitialized(pMsg.Message);      //Sets the Service to "Ready". ProcessInitialized() internally contains a call to this Handler and allows for checks right before SetInitialized() is called. 
                    break;
                case "REFFRESHME":
                    InitServices();
                    mMyDashboard.Reload(pMsg, false);
                    break;
            }
        }
        #endregion
    }
}

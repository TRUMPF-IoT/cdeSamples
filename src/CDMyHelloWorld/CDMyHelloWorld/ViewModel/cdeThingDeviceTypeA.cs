// SPDX-FileCopyrightText: 2009-2020 TRUMPF Laser GmbH, authors: C-Labs
//
// SPDX-License-Identifier: MPL-2.0

// TODO: Add reference for C-DEngine.dll
// TODO: Make sure plugin file name starts with either CDMy or C-DMy
using nsCDEngine.BaseClasses;
using nsCDEngine.Communication;
using nsCDEngine.Engines;
using nsCDEngine.Engines.NMIService;
using nsCDEngine.Engines.ThingService;
using nsCDEngine.ViewModels;
using System;

namespace CDMyHelloWorld
{
    [DeviceType(DeviceType = eCDMyHelloWorldDeviceTypes.cdeThingDeviceTypeA, Description = "This Thing does...", Capabilities = new[] { eThingCaps.ConfigManagement })]
    class cdeThingDeviceTypeA : TheThingBase
    {
        // Base object references 
        protected IBaseEngine MyBaseEngine;    // Base engine (service)

        // User-interface defintion
        protected TheFormInfo MyStatusForm;
        protected TheDashPanelInfo MyStatusFormDashPanel;

        //
        // C-DEngine properties are wrapped inside C# properties.
        // This is a recommended practice.
        // Also recommended, the use of the 'GetSafe...' and 'SetSafe...' methods.
        public bool IsConnected
        {
            get { return TheThing.MemberGetSafePropertyBool(MyBaseThing); }
            set { TheThing.MemberSetSafePropertyBool(MyBaseThing, value); }
        }

        [ConfigProperty]
        public bool AutoConnect
        {
            get { return TheThing.MemberGetSafePropertyBool(MyBaseThing); }
            set { TheThing.MemberSetSafePropertyBool(MyBaseThing, value); }
        }

        public cdeThingDeviceTypeA(TheThing tBaseThing, ICDEPlugin pPluginBase)
        {
            MyBaseThing = tBaseThing ?? new TheThing();
            MyBaseEngine = pPluginBase.GetBaseEngine();
            MyBaseThing.EngineName = MyBaseEngine.GetEngineName();
            MyBaseThing.SetIThingObject(this);

            //TODO: Add your DeviceType to the plug-in's eCDMyHelloWorldDeviceTypes class
            MyBaseThing.DeviceType = eCDMyHelloWorldDeviceTypes.cdeThingDeviceTypeA;
        }

        public override bool Init()
        {
            if (!mIsInitCalled)
            {
                mIsInitCalled = true;
                IsConnected = false;
                MyBaseEngine.RegisterEvent(eEngineEvents.ShutdownEvent, DoEndMe);
                DoInit();
                if (AutoConnect)
                    Connect(null);
                mIsInitialized = true;
            }
            return true;
        }

        protected virtual void DoInit()
        {

        }

        protected virtual void DoEndMe(ICDEThing pEngine, object notused)
        {

        }


        public override bool CreateUX()
        {
            if (!mIsUXInitCalled)
            {
                mIsUXInitCalled = true;

                var tHead = TheNMIEngine.AddStandardForm(MyBaseThing, MyBaseThing.FriendlyName);
                MyStatusForm = tHead["Form"] as TheFormInfo; // TheNMIEngine.AddForm(new TheFormInfo(MyBaseThing) { FormTitle = MyBaseThing.DeviceType, DefaultView = eDefaultView.Form, PropertyBag = new ThePropertyBag { "MaxTileWidth=6" } });
                MyStatusFormDashPanel = tHead["DashIcon"] as TheDashPanelInfo;
                var tBlock = TheNMIEngine.AddStatusBlock(MyBaseThing, MyStatusForm, 2);
                tBlock["Group"].SetParent(1);

                tBlock = TheNMIEngine.AddConnectivityBlock(MyBaseThing, MyStatusForm, 120, sinkConnect);
                tBlock["Group"].SetParent(1);

                DoCreateUX(tHead["Form"] as TheFormInfo);
                mIsUXInitialized = true;
            }
            return true;
        }

        protected virtual void DoCreateUX(TheFormInfo pForm)
        {

        }

        void sinkConnect(TheProcessMessage pMsg, bool DoConnect)
        {
            if (DoConnect)
                Connect(pMsg);
            else
                Disconnect(pMsg);
        }

        public virtual void Connect(TheProcessMessage pMsg)
        {
            TheCommCore.PublishCentral(new TSM { ENG = MyBaseEngine.GetEngineName(), TXT = "HELLO" }, true);
        }
        public virtual void Disconnect(TheProcessMessage pMsg)
        {

        }

        public override bool Delete()
        {
            DoEndMe(this, null);
            //TODO: Remove any residuals in the cache folder like StorageMirrors
            //i.e. MyStorageMirror.RemoveStore();
            return true;
        }
    }
}

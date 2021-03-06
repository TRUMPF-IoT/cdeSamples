// SPDX-FileCopyrightText: 2009-2020 TRUMPF Laser GmbH, authors: C-Labs
//
// SPDX-License-Identifier: MPL-2.0

﻿using System;
using System.Collections.Generic;

using nsCDEngine.BaseClasses;
using nsCDEngine.Communication;
using nsCDEngine.Engines;
using nsCDEngine.Engines.NMIService;
using nsCDEngine.Engines.ThingService;
using nsCDEngine.ViewModels;


namespace CDMyHelloHTTPInterceptor
{
    class cdePluginService1 : ICDEPlugin, ICDEThing
    {
        // Base object references 
        protected TheThing MyBaseThing;      // Base thing
        private IBaseEngine MyBaseEngine;    // Base engine (service)

        // Initialization flags
        protected bool mIsInitStarted = false;
        protected bool mIsInitCompleted = false;
        protected bool mIsUXInitStarted = false;
        protected bool mIsUXInitCompleted = false;

        // User-interface defintion
        TheDashboardInfo mMyDashboard;

        Guid guidEngineID = new Guid("{5833E5E9-76CA-49AA-BDB4-F8009A777D25}");

        String strFriendlyName = "Hello HTTP Interceptor Service";

        #region ICDEPlugin - interface methods for service (engine)
        public IBaseEngine GetBaseEngine()
        {
            return MyBaseEngine;
        }

        /// <summary>
        /// InitEngineAssets - The C-DEngine calls this initialization
        /// function as part of registering this service (engine)
        /// </summary>
        /// <param name="pBase">The C-DEngine creates a base engine object.
        /// This parameter is a reference to that base engine object.
        /// We keep a copy because it will be very useful to us.
        /// </param>
        public void InitEngineAssets(IBaseEngine pBase)
        {
            MyBaseEngine = pBase;

            MyBaseEngine.SetEngineID(guidEngineID);          // Unique identifier for our service (engine)
            MyBaseEngine.SetFriendlyName(strFriendlyName);

            MyBaseEngine.SetEngineName(GetType().FullName);  // Can be any arbitrary name - recommended is the class name
            MyBaseEngine.SetEngineType(GetType());           // Has to be the type of this class
            MyBaseEngine.SetEngineService(true);             // Keep True if this class is a service

            MyBaseEngine.SetPluginInfo("This service...",       // Describe plugin for Plugin Store
                                       0,                       // pPrice - retail price (default = 0)
                                       null,                    // Custom home page - default = /ServiceID
                                       "toplogo-150.png",       // pIcon - custom icon.
                                       "C-Labs",                // pDeveloper - name of the plugin developer.
                                       "http://www.c-labs.com", // pDeveloperUrl - URL to developer home page.
                                       new List<string>() { }); // pCategories - Search categories for service.
        }
        #endregion

        #region ICDEThing - interface methods (rare to override)
        public bool IsInit()
        {
            return mIsInitCompleted;
        }
        public bool IsUXInit()
        {
            return mIsUXInitCompleted;
        }

        public TheThing GetBaseThing()
        {
            return MyBaseThing;
        }
        public void SetBaseThing(TheThing pThing)
        {
            MyBaseThing = pThing;
        }

        public cdeP GetProperty(string pName, bool DoCreate)
        {
            return MyBaseThing?.GetProperty(pName, DoCreate);
        }
        public cdeP SetProperty(string pName, object pValue)
        {
            return MyBaseThing?.SetProperty(pName, pValue);
        }

        public void RegisterEvent(string pName, Action<ICDEThing, object> pCallBack)
        {
            MyBaseThing?.RegisterEvent(pName, pCallBack);
        }
        public void UnregisterEvent(string pName, Action<ICDEThing, object> pCallBack)
        {
            MyBaseThing?.UnregisterEvent(pName, pCallBack);
        }
        public void FireEvent(string pEventName, ICDEThing sender, object pPara, bool FireAsync)
        {
            MyBaseThing?.FireEvent(pEventName, sender, pPara, FireAsync);
        }
        public bool HasRegisteredEvents(string pEventName)
        {
            if (MyBaseThing != null)
                return MyBaseThing.HasRegisteredEvents(pEventName);
            return false;
        }
        #endregion


        public bool Init()
        {
            if (!mIsInitStarted)
            {
                mIsInitStarted = true;
                MyBaseThing.RegisterEvent(eEngineEvents.IncomingMessage, HandleMessage);
                // Additional initialization processing goes here

                // Registering an Http Interceptor that just returns a hello message.
                TheCommCore.MyHttpService.RegisterHttpInterceptorB4("/HelloHTTPInterceptor", sinkHelloHttpInterceptor);

                // Registering an Http Interceptor that returns a dynamic web page.
                TheCommCore.MyHttpService.RegisterHttpInterceptorB4("/relay.html", sinkRelay);

                //// Injecting scripts into C-DEngine context.
                //TheCommCore.MyHttpService.RegisterGlobalScriptInterceptor("/MyStuff.js", (o) => 
                //{
                //});

                //TheRequestData p = new TheRequestData();
                //string strOut = TheCommCore.MyHttpService.CreateHttpHeader(p);
                //List<string> pGlobalScripts = TheCommCore.MyHttpService.GetGlobalScripts();

                //// Providing status info to C-DEngine status page.
                //// Request to append status details to the /CDESTATUS.ASPX page for ?ALL or ?DIAG
                //TheCommCore.MyHttpService.RegisterStatusRequest((o) =>
                //{
                //});

                MyBaseThing.StatusLevel = 1;
                MyBaseThing.LastMessage = "Hello HTTP Interceptor Service has started.";
                mIsInitCompleted = true;
                MyBaseEngine.ProcessInitialized();
            }
            return true;
        }

        private void sinkRelay(TheRequestData pRequest)
        {
            pRequest.ResponseMimeType = "text/html";
            pRequest.ResponseBufferStr = "<html>"
                                        + "    <body>"
                                        + "        <h1>This is My *Dynamic* Relay Server</h1>"
                                        + "    </body>"
                                        + "</html>";
            pRequest.ResponseBuffer = TheCommonUtils.CUTF8String2Array(pRequest.ResponseBufferStr);
            pRequest.StatusCode = (int)eHttpStatusCode.OK;
            pRequest.DontCompress = true;
            pRequest.AllowStatePush = false;
        }

        // Parsing input parameters and returning a value.
        private void sinkHelloHttpInterceptor(TheRequestData pRequest)
        {
            string Query = pRequest.RequestUri.Query;
            if (Query.StartsWith("?"))
                Query = Query.Substring(1);
            Dictionary<string, string> tQ = TheCommonUtils.ParseQueryString(Query);

            pRequest.ResponseMimeType = "text/plain";
            pRequest.ResponseBufferStr = "Hello From Http Interceptor!";
            pRequest.ResponseBuffer = TheCommonUtils.CUTF8String2Array(pRequest.ResponseBufferStr);
            pRequest.StatusCode = (int)eHttpStatusCode.OK;
            pRequest.DontCompress = true;
            pRequest.AllowStatePush = false;
        }

        // Simplest valid Http Interceptor.
        private void sinkSimplestValidHttpInterceptor(TheRequestData pRequest)
        {
            pRequest.ResponseMimeType = "text/html";
            pRequest.ResponseBuffer = new byte[1];
            pRequest.ResponseBuffer[0] = 0;
            pRequest.StatusCode = (int)eHttpStatusCode.OK;
            pRequest.DontCompress = true;
            pRequest.AllowStatePush = false;
        }


        public bool CreateUX()
        {
            if (!mIsUXInitStarted)
            {
                mIsUXInitStarted = true;
                mMyDashboard = TheNMIEngine.AddDashboard(MyBaseThing, new TheDashboardInfo(MyBaseEngine, "My Hello HTTP Interceptor Plugin"));

                var tFlds = TheNMIEngine.AddStandardForm(MyBaseThing, "Hello HTTP Interceptor Service");
                var tMyForm = tFlds["Form"] as TheFormInfo;

                TheNMIEngine.AddSmartControl(MyBaseThing, tMyForm, eFieldType.SingleEnded, 2, 2, 0, "Interceptor URL:", "InterceptorURL", new nmiCtrlSingleEnded() { ParentFld = 1, Value="/HelloHTTPInterceptor" });

                TheNMIEngine.AddAboutButton4(MyBaseThing, mMyDashboard, null, true);
                mIsUXInitCompleted = true;
            }
            return true;
        }

        public bool Delete()
        {
            return true;
        }



        #region Message Handling
        /// <summary>
        /// Handles Messages sent from a host sub-engine to its clients
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pIncoming"></param>
        public void HandleMessage(ICDEThing sender, object pIncoming)
        {
            TheProcessMessage pMsg = pIncoming as TheProcessMessage;
            if (pMsg == null) return;

            string[] cmd = pMsg.Message.TXT.Split(':');
            switch (cmd[0])
            {
                case "CDE_INITIALIZED":
                    MyBaseEngine.SetInitialized(pMsg.Message);
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}

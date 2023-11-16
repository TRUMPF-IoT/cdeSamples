using cdeASPNetMiddleware;
using nsCDEngine.BaseClasses;
using nsCDEngine.Security;
using nsCDEngine.ViewModels;

namespace RelayStart
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls($"http://0.0.0.0:{builder.Configuration["STATIONPORT"]}");

            //builder.Services.AddRazorPages(); //If you want the /pages/* content to work

            StartCDE(builder.Configuration);
            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseWebSockets();
            app.UseCDEAspNetWsHandler();

            //app.UseHttpsRedirection(); //if you want to use SSL - can be tricky on Docker
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCDEAspNetRestHandler();

            //app.MapRazorPages(); //If you want the /pages/* content to work

            app.Run();
        }

        private static void StartCDE(IConfiguration Configuration)
        {
            TheBaseAssets.MySettings = new TheASPCoreSettings(Configuration);
            if (TheBaseAssets.MasterSwitch) return;
            var t = TheScopeManager.SetApplicationID("/cVjzPfjlO;{@QMj:jWpW]HKKEmed[llSlNUAtoE`]G?");  //This is the SDK Open Source ID. To get an OEM key, please contact info@c-labs.com
            TheBaseApplication BaseApp = new TheBaseApplication();

            #region This allows to put a file with in "ClientBin/cache/dns_name.txt" with the DNS Name of the Gateway that can be used for URL verification and OPC UA Certificate generation
            string tGWDNS = null;
            try
            {
                string tDnsFile = AppDomain.CurrentDomain.BaseDirectory;
                if (!string.IsNullOrEmpty(tDnsFile) && !tDnsFile.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    tDnsFile += Path.DirectorySeparatorChar.ToString();
                tDnsFile += $"ClientBin{Path.DirectorySeparatorChar}cache{Path.DirectorySeparatorChar}dns_name.txt";
                if (File.Exists(tDnsFile))
                    tGWDNS = File.ReadAllText(tDnsFile);
            }
            catch (Exception)
            {
                //intentionally
            }
            if (string.IsNullOrEmpty(tGWDNS))
                tGWDNS = TheBaseAssets.MySettings.GetSetting("CDE_DNS_NAME");
            #endregion

            TheServiceHostInfo theServiceHostInfo = new TheServiceHostInfo(cdeHostType.ASPCore, cdeNodeType.Relay, AppDomain.CurrentDomain.BaseDirectory) //Tells the C-DEngine what host type is used.
            {
                ApplicationName = "Sample-Relay",
                cdeMID = TheCommonUtils.CGuid("{6B27EE91-EE58-4764-9E4E-70DD7F378F8A}"), //Random Guid but should remain constant
                TrgtSrv = "Cloud",
                Title = "Sample Relay",
                ApplicationTitle = "My NMI Portal",
                Description = "My Sample Relay Host",
                CurrentVersion = TheCommonUtils.GetAssemblyVersion(BaseApp),
                MyStationName = $"MyGW@{tGWDNS}",
                NodeName = $"{tGWDNS}",
                SiteName = "https://cloud.C-Labs.com",
                DefHomePage = "/NMIAUTO"
            };
            var tDic = Configuration.GetChildren().ToDictionary(x => x.Key, x => x.Value);
            tDic["UseUserMapper"] = "true"; //Enforces User Management of the Relay
            if (!string.IsNullOrEmpty(tGWDNS))
            {
                tDic["MyStationURL"] = $"http://{tGWDNS}:{TheBaseAssets.MySettings.GetSetting("STATIONPORT")}";
            }
            else
                tDic["MyStationURL"] = $"http://gateway.local:{TheBaseAssets.MySettings.GetSetting("STATIONPORT")}";
            var tDbgLevel = TheCommonUtils.CInt(TheBaseAssets.MySettings.GetSetting("CDE_DEBUG"));
            tDic["DEBUGLEVEL"] = $"{tDbgLevel}";
            TheBaseAssets.MyServiceHostInfo = theServiceHostInfo;
            if (!BaseApp.StartBaseApplication(null, tDic))
            {
                cdeASPNetRestHandler.ExpiredText = "...Relay failed during startup - verify your AppID and HSI settings";
            }
        }
    }
}
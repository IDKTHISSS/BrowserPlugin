using JCorePanelBase;
using OpenQA.Selenium.Chrome;
using SeleniumUndetectedChromeDriver;
using SteamAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static SteamKit2.Internal.CMsgRemoteClientBroadcastStatus;

namespace BrowserPlugin
{
    public class PluginActions : JCAccountActionBase
    {
        public void LoadActions(JCSteamAccount Account)
        {
            if (Account == null) return;
            if (Account.MaFile == null) return;
            AddAction(new JCAction("OpenBrowser", "Open Browser", OpenBrowser, "Steam"));
        }
        private async Task OpenBrowser(JCSteamAccountInstance Account)
        {
            Account.SetInWork(true);
            Account.SetWorkStatus("Logging");
            await Account.AccountInfo.CheckSession();
            Account.SetWorkStatus("Starting browser");
            UndetectedChromeDriver _driver;
            ChromeDriverService service = ChromeDriverService.CreateDefaultService(AppDomain.CurrentDomain.BaseDirectory + @"\Dependents\chromedriver.exe");

            service.HideCommandPromptWindow = true;
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--disable-features=IsolateOrigins,site-per-process");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--proxy-server='direct://'");
            options.AddArgument("--proxy-bypass-list=*");
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArguments("--disable-backgrounding-occluded-windows", "--disable-renderer-backgrounding");
            options.AddArgument($"--load-extension={AppDomain.CurrentDomain.BaseDirectory + @"\Dependents\SteamInventoryHelper"}");
            _driver = UndetectedChromeDriver.Create(
            driverExecutablePath: AppDomain.CurrentDomain.BaseDirectory + @"\Dependents\chromedriver.exe",
            options: options,
            hideCommandPromptWindow: true);

            CookieContainer cookieContainer = JCorePanelBase.Utils.GetCookies(Account.AccountInfo.MaFile.Session);
            _driver.Navigate().GoToUrl("https://store.steampowered.com/");
            foreach (System.Net.Cookie cookie in cookieContainer.GetCookies(new Uri("https://steamcommunity.com/")))
            {
                try
                {
                    if (cookie.Name != "mobileClient" && cookie.Name != "mobileClientVersion")
                        _driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(cookie.Name, cookie.Value));

                }
                catch (Exception ex)
                {
                    return;
                }
            }
            _driver.Navigate().GoToUrl("https://steamcommunity.com/profiles/" + Account.AccountInfo.MaFile.Session.SteamID);
            foreach (System.Net.Cookie cookie in cookieContainer.GetCookies(new Uri("https://steamcommunity.com/")))
            {
                try
                {
                    if (cookie.Name != "mobileClient" && cookie.Name != "mobileClientVersion")
                        _driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(cookie.Name, cookie.Value));

                }
                catch (Exception ex)
                {
                    return;
                }
            }
           

            _driver.Navigate().Refresh();
            Account.SetInWork(false);
            while (_driver.WindowHandles.Count > 0)
            {
                Thread.Sleep(1000);
            }
            _driver.Quit();
        }
        
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace KeyauthMultiAppExample
{
    internal class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern ushort GlobalFindAtom(string lpString);

        #region Servers
        //We have to duplicate this to support more than one application (More of like a server)
        //Now it is time to do the same for Form for your panel
        public static Server1.api Server1 = new Server1.api(
            name: "PREMIUM",
            ownerid: "JOzxAPywrc",
            version: "6.7"
        );
        public static Server2.api Server2 = new Server2.api(
            name: "PREMIUM",
            ownerid: "nrsUJq4kdU",
            version: "1.0"
        );
        #endregion
        static void Main(string[] args)
        {
            Console.Title = "Loader";
            Console.WriteLine("\n\n Connecting..");
            string Server = "";
            securityChecks();//This is compulsary otherwise it wont work 
            //SERVERA OR SERVERB anything you want to name each apps

            //Now finally before i forget we need to make sure the user enter the correct server and also you can apply this same logic to windows form 
            Console.WriteLine("\n\n Please Enter your server E.G (SERVERA)");
            Server = Console.ReadLine();

            if(Server == "SERVERA")
            {
                //Now paste those things we cut
                Server1.init();


                if (!Server1.response.success)
                {
                    Console.WriteLine("\n Status: " + Server1.response.message);
                    Thread.Sleep(1500);
                    TerminateProcess(GetCurrentProcess(), 1);
                }

                Console.Write("\n [1] Login\n [2] Register\n [3] Upgrade\n [4] License key only\n [5] Forgot password\n\n Choose option: ");

                string username, password, key, email, code;

                int option = int.Parse(Console.ReadLine());
                switch (option)
                {
                    case 1:
                        Console.Write("\n\n Enter username: ");
                        username = Console.ReadLine();
                        Console.Write("\n\n Enter password: ");
                        password = Console.ReadLine();
                        Console.Write("\n\n Enter 2fa code: (not using 2fa? Just press enter) ");
                        code = Console.ReadLine();
                        Server1.login(username, password, code);
                        break;
                    case 2:
                        Console.Write("\n\n Enter username: ");
                        username = Console.ReadLine();
                        Console.Write("\n\n Enter password: ");
                        password = Console.ReadLine();
                        Console.Write("\n\n Enter license: ");
                        key = Console.ReadLine();
                        Console.Write("\n\n Enter email (just press enter if none): ");
                        email = Console.ReadLine();
                        Server1.register(username, password, key, email);
                        break;
                    case 3:
                        Console.Write("\n\n Enter username: ");
                        username = Console.ReadLine();
                        Console.Write("\n\n Enter license: ");
                        key = Console.ReadLine();
                        Server1.upgrade(username, key);
                        // don't proceed to app, user hasn't authenticated yet.
                        Console.WriteLine("\n Status: " + Server1.response.message);
                        Thread.Sleep(2500);
                        TerminateProcess(GetCurrentProcess(), 1);
                        break;
                    case 4:
                        Console.Write("\n\n Enter license: ");
                        key = Console.ReadLine();
                        Console.Write("\n\n Enter 2fa code: (not using 2fa? Just press enter");
                        code = Console.ReadLine();
                        Server1.license(key, code);
                        break;
                    case 5:
                        Console.Write("\n\n Enter username: ");
                        username = Console.ReadLine();
                        Console.Write("\n\n Enter email: ");
                        email = Console.ReadLine();
                        Server1.forgot(username, email);
                        // don't proceed to app, user hasn't authenticated yet.
                        Console.WriteLine("\n Status: " + Server1.response.message);
                        Thread.Sleep(2500);
                        TerminateProcess(GetCurrentProcess(), 1);
                        break;
                    default:
                        Console.WriteLine("\n\n Invalid Selection");
                        Thread.Sleep(2500);
                        TerminateProcess(GetCurrentProcess(), 1);
                        break; // no point in this other than to not get error from IDE
                }

                if (!Server1.response.success)
                {
                    Console.WriteLine("\n Status: " + Server1.response.message);
                    Thread.Sleep(2500);
                    TerminateProcess(GetCurrentProcess(), 1);
                }

                Console.WriteLine("\n Logged In!"); // at this point, the client has been authenticated. Put the code you want to run after here

                if (string.IsNullOrEmpty(Server1.response.message)) TerminateProcess(GetCurrentProcess(), 1);

                // user data
                Console.WriteLine("\n User data:");
                Console.WriteLine(" Username: " + Server1.user_data.username);
                Console.WriteLine(" License: " + Server1.user_data.subscriptions[0].key); // this can be used if the user used a license, username, and password for register. It'll display the license assigned to the user
                Console.WriteLine(" IP address: " + Server1.user_data.ip);
                Console.WriteLine(" Hardware-Id: " + Server1.user_data.hwid);
                Console.WriteLine(" Created at: " + UnixTimeToDateTime(long.Parse(Server1.user_data.createdate)));
                if (!string.IsNullOrEmpty(Server1.user_data.lastlogin)) // don't show last login on register since there is no last login at that point
                    Console.WriteLine(" Last login at: " + UnixTimeToDateTime(long.Parse(Server1.user_data.lastlogin)));
                Console.WriteLine(" Your subscription(s):");
                for (var i = 0; i < Server1.user_data.subscriptions.Count; i++)
                {
                    Console.WriteLine(" Subscription name: " + Server1.user_data.subscriptions[i].subscription + " - Expires at: " + UnixTimeToDateTime(long.Parse(Server1.user_data.subscriptions[i].expiry)) + " - Time left in seconds: " + Server1.user_data.subscriptions[i].timeleft);
                }

                Console.Write("\n [1] Enable 2FA\n [2] Disable 2FA\n Choose option: ");
                int tfaOptions = int.Parse(Console.ReadLine());
                switch (tfaOptions)
                {
                    case 1:
                        Server1.enable2fa();
                        break;
                    case 2:
                        Console.Write("Enter your 6 digit authorization code: ");
                        code = Console.ReadLine();
                        Server1.disable2fa(code);
                        break;
                    default:
                        Console.WriteLine("\n\n Invalid Selection");
                        Thread.Sleep(2500);
                        TerminateProcess(GetCurrentProcess(), 1);
                        break; // no point in this other than to not get error from IDE
                }
            }
            else if(Server == "SERVERB")
            {
                //Now paste those things we cut
                Server2.init();


                if (!Server2.response.success)
                {
                    Console.WriteLine("\n Status: " + Server2.response.message);
                    Thread.Sleep(1500);
                    TerminateProcess(GetCurrentProcess(), 1);
                }

                Console.Write("\n [1] Login\n [2] Register\n [3] Upgrade\n [4] License key only\n [5] Forgot password\n\n Choose option: ");

                string username, password, key, email, code;

                int option = int.Parse(Console.ReadLine());
                switch (option)
                {
                    case 1:
                        Console.Write("\n\n Enter username: ");
                        username = Console.ReadLine();
                        Console.Write("\n\n Enter password: ");
                        password = Console.ReadLine();
                        Console.Write("\n\n Enter 2fa code: (not using 2fa? Just press enter) ");
                        code = Console.ReadLine();
                        Server2.login(username, password, code);
                        break;
                    case 2:
                        Console.Write("\n\n Enter username: ");
                        username = Console.ReadLine();
                        Console.Write("\n\n Enter password: ");
                        password = Console.ReadLine();
                        Console.Write("\n\n Enter license: ");
                        key = Console.ReadLine();
                        Console.Write("\n\n Enter email (just press enter if none): ");
                        email = Console.ReadLine();
                        Server2.register(username, password, key, email);
                        break;
                    case 3:
                        Console.Write("\n\n Enter username: ");
                        username = Console.ReadLine();
                        Console.Write("\n\n Enter license: ");
                        key = Console.ReadLine();
                        Server2.upgrade(username, key);
                        // don't proceed to app, user hasn't authenticated yet.
                        Console.WriteLine("\n Status: " + Server2.response.message);
                        Thread.Sleep(2500);
                        TerminateProcess(GetCurrentProcess(), 1);
                        break;
                    case 4:
                        Console.Write("\n\n Enter license: ");
                        key = Console.ReadLine();
                        Console.Write("\n\n Enter 2fa code: (not using 2fa? Just press enter");
                        code = Console.ReadLine();
                        Server2.license(key, code);
                        break;
                    case 5:
                        Console.Write("\n\n Enter username: ");
                        username = Console.ReadLine();
                        Console.Write("\n\n Enter email: ");
                        email = Console.ReadLine();
                        Server2.forgot(username, email);
                        // don't proceed to app, user hasn't authenticated yet.
                        Console.WriteLine("\n Status: " + Server2.response.message);
                        Thread.Sleep(2500);
                        TerminateProcess(GetCurrentProcess(), 1);
                        break;
                    default:
                        Console.WriteLine("\n\n Invalid Selection");
                        Thread.Sleep(2500);
                        TerminateProcess(GetCurrentProcess(), 1);
                        break; // no point in this other than to not get error from IDE
                }

                if (!Server2.response.success)
                {
                    Console.WriteLine("\n Status: " + Server2.response.message);
                    Thread.Sleep(2500);
                    TerminateProcess(GetCurrentProcess(), 1);
                }

                Console.WriteLine("\n Logged In!"); // at this point, the client has been authenticated. Put the code you want to run after here

                if (string.IsNullOrEmpty(Server2.response.message)) TerminateProcess(GetCurrentProcess(), 1);

                // user data
                Console.WriteLine("\n User data:");
                Console.WriteLine(" Username: " + Server2.user_data.username);
                Console.WriteLine(" License: " + Server2.user_data.subscriptions[0].key); // this can be used if the user used a license, username, and password for register. It'll display the license assigned to the user
                Console.WriteLine(" IP address: " + Server2.user_data.ip);
                Console.WriteLine(" Hardware-Id: " + Server2.user_data.hwid);
                Console.WriteLine(" Created at: " + UnixTimeToDateTime(long.Parse(Server2.user_data.createdate)));
                if (!string.IsNullOrEmpty(Server2.user_data.lastlogin)) // don't show last login on register since there is no last login at that point
                    Console.WriteLine(" Last login at: " + UnixTimeToDateTime(long.Parse(Server2.user_data.lastlogin)));
                Console.WriteLine(" Your subscription(s):");
                for (var i = 0; i < Server2.user_data.subscriptions.Count; i++)
                {
                    Console.WriteLine(" Subscription name: " + Server2.user_data.subscriptions[i].subscription + " - Expires at: " + UnixTimeToDateTime(long.Parse(Server2.user_data.subscriptions[i].expiry)) + " - Time left in seconds: " + Server2.user_data.subscriptions[i].timeleft);
                }

                Console.Write("\n [1] Enable 2FA\n [2] Disable 2FA\n Choose option: ");
                int tfaOptions = int.Parse(Console.ReadLine());
                switch (tfaOptions)
                {
                    case 1:
                        Server2.enable2fa();
                        break;
                    case 2:
                        Console.Write("Enter your 6 digit authorization code: ");
                        code = Console.ReadLine();
                        Server2.disable2fa(code);
                        break;
                    default:
                        Console.WriteLine("\n\n Invalid Selection");
                        Thread.Sleep(2500);
                        TerminateProcess(GetCurrentProcess(), 1);
                        break; // no point in this other than to not get error from IDE
                }
            }
            else
            {
                Console.WriteLine("\n\n Invalid Server Contact your developer");
            }


                Console.WriteLine("\n Closing in five seconds...");
            Thread.Sleep(-1);
            Environment.Exit(0);
        }

        public static bool SubExist(string name)
        {
            if (Server1.user_data.subscriptions.Exists(x => x.subscription == name))
                return true;
            return false;
        }

        public static DateTime UnixTimeToDateTime(long unixtime)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Local);
            try
            {
                dtDateTime = dtDateTime.AddSeconds(unixtime).ToLocalTime();
            }
            catch
            {
                dtDateTime = DateTime.MaxValue;
            }
            return dtDateTime;
        }

        static void checkAtom()
        {
            Thread atomCheckThread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(60000); // give people 1 minute to login

                    ushort foundAtom = GlobalFindAtom(Server1.ownerid);
                    if (foundAtom == 0)
                    {
                        TerminateProcess(GetCurrentProcess(), 1);
                    }
                }
            });

            atomCheckThread.IsBackground = true; // Ensure the thread does not block program exit
            atomCheckThread.Start();
        }
        static string random_string()
        {
            string str = null;

            Random random = new Random();
            for (int i = 0; i < 5; i++)
            {
                str += Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65))).ToString();
            }
            return str;
        }
        static void securityChecks()
        {
            // check if the Loader was executed by a different program
            var frames = new StackTrace().GetFrames();
            foreach (var frame in frames)
            {
                MethodBase method = frame.GetMethod();
                if (method != null && method.DeclaringType?.Assembly != Assembly.GetExecutingAssembly())
                {
                    TerminateProcess(GetCurrentProcess(), 1);
                }
            }

            // check if HarmonyLib is attempting to poison our program
            var harmonyAssembly = AppDomain.CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "0Harmony");

            if (harmonyAssembly != null)
            {
                TerminateProcess(GetCurrentProcess(), 1);
            }

            checkAtom();
        }
    }
}
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsgoHaxOverlay.JsonOffsets;
using LowLevelInput.Hooks;

namespace CsgoHaxOverlay
{
    static class Program
    {
        private static readonly ConsoleSpiner Spin = new ConsoleSpiner();
        private static LittleOverlay _overlay;
        private static InputManager _manager;
        public static bool IsTriggerBot { get; set; } = true;
        public static bool IsAimEnabled { get; set; } = true;
        public static bool IsAimInAction { get; set; }
        public static bool IsRadarEnabled { get; set; }
        public static bool IsEspEnabled { get; set; }
        public static bool IsBunnyHopEnabled { get; set; }
        public static bool IsCrosshairDraw { get; set; }
        private static bool _offsetsLoaded;
        public static Vector2 ScreenSize;
        static void Main()
        {
            LoadOffsets();
            while (!_offsetsLoaded)
            {
                Thread.Sleep(50);
            }

            int miny, maxy;
            var minx = miny = int.MaxValue;
            var maxx = maxy = int.MinValue;

            foreach (var screen in Screen.AllScreens)
            {
                var bounds = screen.Bounds;
                minx = Math.Min(minx, (int)bounds.X);
                miny = Math.Min(miny, (int)bounds.Y);
                maxx = Math.Max(maxx, (int)bounds.Right);
                maxy = Math.Max(maxy, (int)bounds.Bottom);
            }
            ScreenSize = new Vector2(maxx - minx, maxy - miny);
            _overlay = new LittleOverlay(maxx - minx, maxy - miny);

            while (true)
            {
                if (GetProcessesByName(_overlay.ProcessName, out var process))
                {
                    try
                    {
                        LittleOverlay.Client = process.DllImageAddress("client_panorama.dll");
                        if (LittleOverlay.Client == (IntPtr)0)
                        {
                            Printer.PrintInfo("> Client.dll NOT loaded: " + LittleOverlay.Client);
                            Thread.Sleep(1000);
                            continue;
                        }
                        Printer.PrintInfo("> Client.dll loaded: " + LittleOverlay.Client);
                    }
                    catch (Exception e)
                    {
                        Printer.PrintException(e);
                        Printer.PrintError("> Error while loading Client.dll");
                        Thread.Sleep(1000);
                        continue;
                    }
                    try
                    {
                        LittleOverlay.Engine = process.DllImageAddress("engine.dll");
                        if (LittleOverlay.Engine == (IntPtr)0)
                        {
                            Printer.PrintInfo("> Engine.dll NOT loaded: " + LittleOverlay.Client);
                            Thread.Sleep(1000);
                            continue;
                        }
                        Printer.PrintInfo("> Engine.dll loaded: " + LittleOverlay.Engine);
                    }
                    catch (Exception)
                    {
                        Printer.PrintError("> Error while loading Engine.dll");
                        Thread.Sleep(1000);
                        continue;
                    }

                    try
                    {
                        _manager = new InputManager();

                        _manager.OnKeyboardEvent += ManagerOnOnKeyboardEvent;

                        _manager.OnMouseEvent += ManagerOnOnMouseEvent;

                        _manager.Initialize();

                        Printer.PrintInfo("> InputManager loaded: ");
                    }
                    catch (Exception e)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    Console.WriteLine(new string('-', Console.WindowWidth - 1));
                    Printer.PrintSuccess("Injected{0}", new string(' ', 15));
                    Printer.PrintSuccess("Process Id: {0}", process.Id);
                    Printer.PrintSuccess("Process Handel: {0}", process.Handle);
                    Console.WriteLine(new string('-', Console.WindowWidth - 1));
                    Printer.PrintInfo("  Hotkeys: \n");
                    Printer.PrintEncolored("> F5        - Esp ", ConsoleColor.Green);
                    Printer.PrintEncolored("> F6        - Radar ", ConsoleColor.Green);
                    Printer.PrintEncolored("> F7        - Trigger ", ConsoleColor.Green);
                    Printer.PrintEncolored("> F8        - Aim ", ConsoleColor.Green);
                    Printer.PrintEncolored("> F9        - Crosshair ", ConsoleColor.Green);
                    Printer.PrintEncolored("> F10        - Bunny ", ConsoleColor.Green);
                    Printer.PrintEncolored("> Alt       - Aiming", ConsoleColor.Green);
                    Printer.PrintEncolored("> xButton2  - Trigger ", ConsoleColor.Green);
                    Printer.PrintEncolored("> Del       - Close cheat!", ConsoleColor.Green);

                    _overlay.Run(process);
                    break;
                }
                Spin.Turn();
                Thread.Sleep(100);

            }

            while (!_isEnd)
            {
                Thread.Sleep(200);
            }
        }

        private static async void LoadOffsets()
        {
            Printer.PrintInfo($"Try to load offsets");
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(new Uri("https://github.com/frk1/hazedumper/raw/master/csgo.json"));
                var result = await response.Content.ReadAsStringAsync();
                var gitHubOffsets = GitHubOffsets.FromJson(result);
                var type = typeof(Netvars);
                foreach (var p in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
                {
                    var value = p.GetValue(null);
                    var name = p.Name.Substring(1);
                    name = name.Substring(0, name.IndexOf('>'));
                    Printer.PrintInfo($"Netvar: {name} -> 0x{gitHubOffsets.Netvars[name]:X4}");
                    p.SetValue(value, gitHubOffsets.Netvars[name]);
                }
                type = typeof(Signatures);
                foreach (var p in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
                {
                    var value = p.GetValue(null);
                    var name = p.Name.Substring(1);
                    name = name.Substring(0, name.IndexOf('>'));
                    Printer.PrintInfo($"Signature: {name} -> 0x{gitHubOffsets.Signatures[name]:X4}");
                    p.SetValue(value, gitHubOffsets.Signatures[name]);
                }
            }
            catch (Exception e)
            {
                Printer.PrintError($"Can't load offsets from github.");
                Printer.PrintException(e);
            }
            Printer.PrintSuccess($"Loaded");
            await Task.Delay(1000);
            Console.Clear();

            _offsetsLoaded = true;
        }

        private static bool _isEnd;
        private static void ManagerOnOnMouseEvent(VirtualKeyCode virtualKeyCode, KeyState keyState, int i, int i1)
        {
            if (virtualKeyCode == VirtualKeyCode.Xbutton2 && keyState == KeyState.Down)
            {
                IsTriggerBot = !IsTriggerBot;
            }
        }

        private static void ManagerOnOnKeyboardEvent(VirtualKeyCode virtualKeyCode, KeyState keyState)
        {
            if (virtualKeyCode == VirtualKeyCode.Lmenu)
            {
                switch (keyState)
                {
                    case KeyState.Down:
                        IsAimInAction = true;
                        break;
                    case KeyState.Up:
                        IsAimInAction = false;
                        _overlay.Closed = null;
                        _overlay.IsCanNewTarget = true;
                        break;

                }
            }

            if (virtualKeyCode == VirtualKeyCode.Space)
            {
                switch (keyState)
                {
                    case KeyState.Down:
                        IsBunnyHopSpaced = true;
                        break;
                    case KeyState.Up:
                        IsBunnyHopSpaced = false;
                        break;

                }
            }

                if (keyState != KeyState.Down) return;
            switch (virtualKeyCode)
            {
                case VirtualKeyCode.Delete:
                    ExitFromCheat();
                    break;
                case VirtualKeyCode.F9:
                    IsCrosshairDraw = !IsCrosshairDraw;
                    break;
                case VirtualKeyCode.F10:
                    IsBunnyHopEnabled = !IsBunnyHopEnabled;
                    break;
                case VirtualKeyCode.F8:
                    IsAimEnabled = !IsAimEnabled;
                    break;
                case VirtualKeyCode.F7:
                    IsTriggerBot = !IsTriggerBot;
                    break;
                case VirtualKeyCode.F6:
                    IsRadarEnabled = !IsRadarEnabled;
                    break;
                case VirtualKeyCode.F5:
                    IsEspEnabled = !IsEspEnabled;
                    break;
            }
        }

        public static bool IsBunnyHopSpaced { get; set; }

        public static void ExitFromCheat()
        {
            _manager.Dispose();
            _overlay.DestroyInstance();
            _isEnd = true;
            Application.Exit();
        }

        private static bool GetProcessesByName(string pName, out Process process)
        {
            /*foreach (var process1 in Process.GetProcesses())
            {
                Console.WriteLine(process1);
            }*/
            var pList = Process.GetProcessesByName(pName);
            process = pList.Length > 0 ? pList[0] : null;
            return process != null;
        }

        private static void TestFilter()
        {
            LowLevelInput.WindowsHooks.WindowsHookFilter.Filter += (key, state) =>
            {
                if (key == VirtualKeyCode.A) return true; // filter event

                return false; // event passes
            };

            var manager = new InputManager();

            manager.OnKeyboardEvent += (key, state) =>
            {
                Console.WriteLine("Key: " + key + ", State: " + state);
            };

            manager.Initialize();

            while (true)
            {
                var line = Console.ReadLine();

                if (string.IsNullOrEmpty(line)) continue;

                if (line.ToLower() == "exit") break;
            }
        }

        private static IntPtr DllImageAddress(this Process process, string dllname)
        {
            var modules = process.Modules;
            /*foreach (var module in modules.Cast<ProcessModule>())
            {
                if (module.ModuleName == dllname)
                {
                    Printer.PrintInfo(module.BaseAddress.ToString());
                    return module.BaseAddress;
                }
            }*/
            foreach (
                var procmodule in modules.Cast<ProcessModule>().Where(procmodule => dllname == procmodule.ModuleName))
            {
                return procmodule.BaseAddress;
            }
            return IntPtr.Zero;
            //throw null;
        }
    }
}

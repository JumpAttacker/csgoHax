using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsgoHaxOverlay.Entities;
using GameOverlay.Graphics;
using GameOverlay.Graphics.Primitives;
using GameOverlay.Utilities;
using GameOverlay.Windows;

namespace CsgoHaxOverlay
{
    public class LittleOverlay
    {
        private readonly int _sizeX;
        private readonly int _sizeY;
        private OverlayWindow _window;
        private D2DDevice _device;
        private FrameTimer _frameTimer;

        private bool _initializeGraphicObjects;

        private D2DColor _backgroundColor;

        private D2DFont _font;

        private D2DSolidColorBrush _blackBrush;

        private D2DSolidColorBrush _redBrush;
        private D2DSolidColorBrush _greenBrush;
        private D2DSolidColorBrush _blueBrush;

        private D2DLinearGradientBrush _gradient;

        private D2DImage _image;
        public string GameName = "Counter-Strike: Global Offensive";
        public string GameClass = "Valve001";
        public string ProcessName = "csgo";

        public static List<Player> Players;
        public static IntPtr Client = IntPtr.Zero;
        public static IntPtr Engine = IntPtr.Zero;

        private static ProcUtils _procUtils;

        public static LocalPlayer LocalPlayer;
        // Process
        private static Process _process;
        public LittleOverlay(int sizeX, int sizeY)
        {
            _sizeX = sizeX;
            _sizeY = sizeY;
            Printer.PrintInfo($"Init new window: [{sizeX}:{sizeY}]");
            _window = new OverlayWindow(new OverlayOptions
            {
                BypassTopmost = false,
                Height = sizeX,
                Width = sizeY * 2,
                //ClassName = GameClass,
                MenuName = "Test Menu",
                WindowTitle = "CasgoOverlayHax",
                X = 0,
                Y = 0
            });
        }


        public void Run(Process process)
        {
            _process = process;
            SetupInstance();
            var targetWnd = Managed.FindWindow(GameClass, GameName);
            Managed.SetForegroundWindow(targetWnd);
        }

        ~LittleOverlay()
        {
            //DestroyInstance();
        }

        public List<JustEntity> ListOfGlow = new List<JustEntity>();
        public bool InGame;
        private void SetupInstance()
        {
            _device = new D2DDevice(new DeviceOptions
            {
                AntiAliasing = true,
                Hwnd = _window.WindowHandle,
                MeasureFps = true,
                MultiThreaded = false,
                VSync = false
            });

            _procUtils = new ProcUtils(ProcessName,
                WinApi.ProcessAccessFlags.VirtualMemoryRead | WinApi.ProcessAccessFlags.VirtualMemoryWrite |
                WinApi.ProcessAccessFlags.VirtualMemoryOperation);
            MemUtils.Handle = _procUtils.Handle;

            _frameTimer = new FrameTimer(_device, 100);

            _window.OnWindowBoundsChanged += _window_OnWindowBoundsChanged;

            _initializeGraphicObjects = true;

            _frameTimer.OnFrameStarting += _frameTimer_OnFrameStarting;
            _frameTimer.OnFrame += _frameTimer_OnFrame;

            _frameTimer.Start();
            Task.Run(async () =>
            {
                while (true)
                {
                    var targetWnd = Managed.FindWindow(GameClass, GameName);
                    if (targetWnd != IntPtr.Zero)
                    {
                        Managed.GetWindowRect(targetWnd, out var targetSize);

                        if (targetSize.Left < 0 && targetSize.Top < 0 && targetSize.Right < 0 &&
                            targetSize.Bottom < 0 ||
                            GetWindow() != ProcessName || !IsGameRun())
                        {
                            CanDraw = false;
                        }
                        else
                        {
                            CanDraw = true;
                        }
                    }
                    else
                    {
                        CanDraw = false;
                    }
                    await Task.Delay(500);
                }
            });

            LocalPlayer = new LocalPlayer();
            Players = new List<Player>();
            for (var i = 0; i < MaxPlayersOnMap; i++)
            {
                var p = new Player(i);
                if (p.Entity == LocalPlayer.Entity)
                    continue;

                Players.Add(p);
            }

            Task.Run(async () =>
            {
                while (!IsInGame())
                {
                    await Task.Delay(500);
                }
                LocalPlayer.Update();
                Loaded();
            });
        }

        private void Loaded()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    if (InGame)
                    {
                        LocalPlayer.Update();
                        foreach (var player in Players)
                        {
                            player.Update();
                        }
                        if (Program.IsEspEnabled)
                            foreach (var justEntity in ListOfGlow.ToList())
                            {
                                justEntity.Update();
                            }
                        await Task.Delay(1);
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                }
            });
            Task.Run(async () =>
            {
                while (true)
                {
                    if (Program.IsTriggerBot && InGame)
                    {
                        if (LocalPlayer.PlayerInCrosshair != null)
                        {
                            var myWeapon = LocalPlayer.GetCurrentWeapon();
                            if (myWeapon == WeaponHandler.ItemDefinitionIndex.WeaponAwp ||
                                myWeapon == WeaponHandler.ItemDefinitionIndex.WeaponScar20 ||
                                myWeapon == WeaponHandler.ItemDefinitionIndex.WeaponG3sg1 ||
                                myWeapon == WeaponHandler.ItemDefinitionIndex.WeaponSsg08)
                            {
                                if (LocalPlayer.IsScoping())
                                {
                                    WinApi.mouse_event(WinApi.MOUSEEVENTF.LEFTDOWN, 0, 0, 0, 0);
                                    await Task.Delay(1);
                                    WinApi.mouse_event(WinApi.MOUSEEVENTF.LEFTUP, 0, 0, 0, 0);
                                }
                            }
                            else
                            {
                                WinApi.mouse_event(WinApi.MOUSEEVENTF.LEFTDOWN, 0, 0, 0, 0);
                                await Task.Delay(1);
                                WinApi.mouse_event(WinApi.MOUSEEVENTF.LEFTUP, 0, 0, 0, 0);
                            }
                        }
                    }
                    else
                    {
                        await Task.Delay(100);
                    }
                }
            });
            Task.Run(async () =>
            {
                while (true)
                {
                    if (Program.IsAimEnabled && Program.IsAimInAction && InGame)
                    {
                        if (IsCanNewTarget)
                            foreach (var player in Players.Where(x => x.IsValid && x.IsEnemy && x.IsAlive))
                            {
                                var w2SHead = MathUtils.WorldToScreen(LocalPlayer.ViewMatrix,
                                    new Vector2(_sizeX, _sizeY),
                                    player.GetEntityBonePosition(8));
                                FindClosest(player, w2SHead);
                            }
                        if (Closed != null)
                        {
                            IsCanNewTarget = false;
                        }
                        if (Closed == null || LocalPlayer.GetWeaponClip() <= 0 || !Closed.IsAlive || !Closed.IsValid)
                        {
                            Closed = null;
                            continue;
                        }

                        AimFunction();
                    }
                    else
                    {
                        IsCanNewTarget = true;
                        Closed = null;
                        await Task.Delay(1);
                    }
                }

            });
            Task.Run(async () =>
            {
                var ignoreClassList= new List<int>()
                {
                    38, 50, 139, 242
                };
                while (true)
                {
                    if (Program.IsEspEnabled && InGame)
                    {
                        var glowObj = MemUtils.ReadInt32(Client + Signatures.dwGlowObjectManager);
                        var glowCount = MemUtils.ReadInt32(Client + Signatures.dwGlowObjectManager + 4);
                        /*Printer.PrintInfo($"GlowCOunt: {glowCount}");
                        Printer.PrintInfo(
                            $"Classes {MemUtils.ReadString(Client + Signatures.dwGetAllClasses, 128, Encoding.ASCII)}");*/
                        for (var i = 0; i <= glowCount; i++)
                        {
                            var entity = MemUtils.ReadInt32((IntPtr) (glowObj + i * 0x38));

                            if (entity == 0)
                                continue;

                            var one = MemUtils.ReadInt32((IntPtr) (entity + 8));
                            var two = MemUtils.ReadInt32((IntPtr) (one + 2 * 4));
                            var three = MemUtils.ReadInt32((IntPtr) (two + 1));
                            var classId = MemUtils.ReadInt32((IntPtr) (three + 20));
                            //Printer.PrintInfo($"[{i}] Entity: {entity} | {one} | {two} | {three} | {classId}");

                            if (classId > 0 && ignoreClassList.All(x => x != classId))
                            {
                                if (!ListOfGlow.Any(x => x.Entity.Equals(entity)))
                                {
                                    ListOfGlow.Add(new JustEntity(entity));
                                }
                                MemUtils.WriteFloat((IntPtr)(glowObj + i * 0x38 + 0x4), 255);
                                MemUtils.WriteFloat((IntPtr)(glowObj + i * 0x38 + 0x8), 0);
                                MemUtils.WriteFloat((IntPtr)(glowObj + i * 0x38 + 0xC), 255);
                                MemUtils.WriteFloat((IntPtr)(glowObj + i * 0x38 + 0x10), 255);
                                MemUtils.Write((IntPtr)(glowObj + i * 0x38 + 0x24), new byte[] { 1 });
                                MemUtils.Write((IntPtr)(glowObj + i * 0x38 + 0x25), new byte[] { 0 });
                            }
                        }

                        foreach (var p in Players.Where(x => x.IsValid && x.IsAlive))
                        {
                            var glowIndex = MemUtils.ReadInt32((IntPtr) (p.Entity + Netvars.m_iGlowIndex));
                            if (p.IsAlly)
                            {
                                MemUtils.WriteFloat((IntPtr) (glowObj + glowIndex * 0x38 + 0x4), 0);
                                MemUtils.WriteFloat((IntPtr) (glowObj + glowIndex * 0x38 + 0x8), 155);
                                MemUtils.WriteFloat((IntPtr) (glowObj + glowIndex * 0x38 + 0xC), 255);
                                MemUtils.WriteFloat((IntPtr) (glowObj + glowIndex * 0x38 + 0x10), 255);
                                MemUtils.Write((IntPtr) (glowObj + glowIndex * 0x38 + 0x24), new byte[] {1});
                                MemUtils.Write((IntPtr) (glowObj + glowIndex * 0x38 + 0x25), new byte[] {0});
                            }
                            else
                            {
                                MemUtils.WriteFloat((IntPtr) (glowObj + glowIndex * 0x38 + 0x4), 255);
                                MemUtils.WriteFloat((IntPtr) (glowObj + glowIndex * 0x38 + 0x8), 0);
                                MemUtils.WriteFloat((IntPtr) (glowObj + glowIndex * 0x38 + 0xC), 0);
                                MemUtils.WriteFloat((IntPtr) (glowObj + glowIndex * 0x38 + 0x10), 255);
                                MemUtils.Write((IntPtr) (glowObj + glowIndex * 0x38 + 0x24), new byte[] {1});
                                MemUtils.Write((IntPtr) (glowObj + glowIndex * 0x38 + 0x25), new byte[] {0});
                            }
                        }
                    }
                    await Task.Delay(10);
                }

            });

            Task.Run(async () =>
            {
                while (true)
                {
                    if (!Program.IsBunnyHopEnabled || !Program.IsBunnyHopSpaced || !LocalPlayer.IsValid ||
                        !LocalPlayer.IsAlive) continue;
                    var flags = MemUtils.ReadInt32((IntPtr)LocalPlayer.Entity + Netvars.m_fFlags);
                    if (flags == (int) FlagType.Jump) continue;
                    MemUtils.WriteInt32(Client + Signatures.dwForceJump, 5);
                    await Task.Delay(25);
                    MemUtils.WriteInt32(Client + Signatures.dwForceJump, 4);
                    await Task.Delay(50);
                }
            });

            Task.Run(async () =>
            {
                while (true)
                {
                    var newInGame = IsInGame();
                    if (newInGame && newInGame != InGame)
                    {
                        ClientState = MemUtils.ReadInt32(Engine + Signatures.dwClientState);
                        _mapName = MemUtils.ReadString((IntPtr)ClientState + Signatures.dwClientState_Map, 32, Encoding.ASCII).TrimStart('_');

                        Players.Clear();
                        MaxPlayersOnMap = GetMapPlayers();

                        Printer.PrintSuccess($"NewMapLoaded: {_mapName} (MaxPlayers: {_mapName})");
                        for (var i = 0; i < MaxPlayersOnMap; i++)
                        {
                            var p = new Player(i);
                            if (p.Entity == LocalPlayer.Entity)
                                continue;

                            Players.Add(p);
                        }
                    }
                    InGame = newInGame;
                    await Task.Delay(200);
                }
            });
        }

        public int MaxPlayersOnMap { get; set; }

        private enum FlagType
        {
            Stay = 257,
            Sit = 263,
            Sit2 = 261,
            Jump = 256
        }
        private static bool IsGameRun()
        {
            return Process.GetProcesses().Any(p => p.ProcessName == _process.ProcessName);
        }
        private static string GetWindow()
        {
            var hWnd = Managed.GetForegroundWindow();
            Managed.GetWindowThreadProcessId(hWnd, out var pid);
            using (var p = Process.GetProcessById(pid))
            {
                return p.ProcessName;
            }
        }
        public bool CanDraw;
        public void DestroyInstance()
        {
            _frameTimer.Stop();

            _frameTimer.Dispose();
            _device.Dispose();
            _window.Dispose();

            _window = null;
            _device = null;
            _frameTimer = null;
        }

        private void _window_OnWindowBoundsChanged(int x, int y, int width, int height)
        {
            if (_device == null) return;
            if (!_device.IsInitialized) return;

            _device.Resize(width, height);
        }

        private void _frameTimer_OnFrameStarting(FrameTimer timer, D2DDevice device)
        {
            if (!_initializeGraphicObjects) return;

            if (!device.IsInitialized) return;
            if (device.IsDrawing) return;

            _backgroundColor = new D2DColor(0, 0, 0, 0);

            _font = _device.CreateFont(new FontOptions()
            {
                Bold = false,
                FontFamilyName = "Arial",
                FontSize = 16,
                Italic = false,
                WordWrapping = true
            });

            // colors automatically normalize values to fit. you can use 1.0f but also 255.0f.
            _blackBrush = device.CreateSolidColorBrush(0x0, 0x0, 0x0, 0xFF);

            _redBrush = device.CreateSolidColorBrush(0xFF, 0x0, 0x0, 0xFF);
            _greenBrush = device.CreateSolidColorBrush(0x0, 0xFF, 0x0, 155);
            _blueBrush = device.CreateSolidColorBrush(0x0, 50, 0xFF, 50);

            _gradient = new D2DLinearGradientBrush(device, new D2DColor(0, 0, 80), new D2DColor(0x88, 0, 125), new D2DColor(0, 0, 225));

            // loads an image from resource bytes (.png in this case)

            _initializeGraphicObjects = false;

        }

        private void _frameTimer_OnFrame(FrameTimer timer, D2DDevice device)
        {
            // the render loop will call device.BeginScene() and device.EndScene() for us

            if (!device.IsDrawing)
            {
                _initializeGraphicObjects = true;
                return;
            }

            // clear the scene / fill it with our background

            device.ClearScene(_backgroundColor);


            if (!CanDraw)
                return;
            // text

            // the background is dynamically adjusted to the text's size
            var esp = Program.IsEspEnabled ? '+' : '-';
            var radar = Program.IsRadarEnabled ? '+' : '-';
            var trigger = Program.IsTriggerBot ? '+' : '-';
            var aim = Program.IsAimEnabled ? '+' : '-';
            var cross = Program.IsCrosshairDraw ? '+' : '-';
            var bunny = Program.IsBunnyHopEnabled ? '+' : '-';
            
            device.DrawTextWithBackground(
                $"Fps: {device.FramesPerSecond} [ Esp(F5): {esp} ] [ Radar(F6): {radar} ] [ Trigger(F7): {trigger} ] [ Aim(F8): {aim} ] [ Cross(F9): {cross} ] [ Bunny(F10): {bunny} ]",
                10, 50, _font, _redBrush, _blackBrush);
            
            if (IsInGame())
            {
                if (LocalPlayer == null)
                    return;
                

                device.DrawTextWithBackground(
                    $"Me: (Map: {_mapName}) {Closed?.Name} {LocalPlayer.Health} {LocalPlayer.Team} {LocalPlayer.Position} Max:{MaxPlayersOnMap}",
                    10, 80, _font, _redBrush, _blackBrush);

                if (Program.IsCrosshairDraw)
                {
                    device.FillCircle(_sizeX / 2f, _sizeY / 2f, Fov, _blueBrush);
                    device.DrawCrosshair(CrosshairStyle.Plus, new Point(_sizeX / 2f, _sizeY / 2f), 15, 1, _blackBrush);
                }

                foreach (var justEntity in ListOfGlow.ToList())
                {
                    try
                    {
                        var w2S = justEntity.W2SPosition;
                        if (w2S.X <= 0 || w2S.Y <= 0 || w2S.X >= Program.ScreenSize.X || w2S.Y >= Program.ScreenSize.Y)
                            continue;

                        /*if (Program.IsEspEnabled)
                        {
                            var classId = justEntity.ClassId;

                            device.DrawTextWithBackground($"[{classId}]", w2S.X, w2S.Y, _font, _redBrush,
                                _blackBrush);
                        }*/
                    }
                    catch (Exception e)
                    {
                        
                    }
                    
                }
            }
            else
            {
                device.DrawTextWithBackground($"Not in game", 10, 70, _font, _redBrush, _blackBrush);
            }
            return;
            // primitives

            device.DrawCircle(100, 100, 50, 2.0f, _redBrush);
            device.DrawDashedCircle(250, 100, 50, 2.0f, _greenBrush);

            // Rectangle.Create offers a method to create rectangles with x, y, width, heigth 
            device.DrawRectangle(Rectangle.Create(350, 50, 100, 100), 2.0f, _blueBrush);
            device.DrawRoundedRectangle(RoundedRectangle.Create(500, 50, 100, 100, 6.0f), 2.0f, _redBrush);

            device.DrawTriangle(650, 150, 750, 150, 700, 50, _greenBrush, 2.0f);

            // lines

            device.DrawLine(50, 175, 750, 175, 2.0f, _blueBrush);
            device.DrawDashedLine(50, 200, 750, 200, 2.0f, _redBrush);

            // outlines & filled

            device.OutlineCircle(100, 275, 50, 4.0f, _redBrush, _blackBrush);
            device.FillCircle(250, 275, 50, _greenBrush);

            device.OutlineRectangle(Rectangle.Create(350, 225, 100, 100), 4.0f, _blueBrush, _blackBrush);

            _gradient.SetRange(500, 225, 600, 325);
            device.FillRoundedRectangle(RoundedRectangle.Create(500, 225, 100, 100, 6.0f), _gradient);

            device.FillTriangle(650, 325, 750, 325, 700, 225, _greenBrush);


            // images

            //device.DrawImage(_image, 310, 375);
        }


        public static int GetMapPlayers()
        {
            if (ClientState == 0)
                ClientState = MemUtils.ReadInt32(Engine + Signatures.dwClientState);
            return MemUtils.ReadInt32((IntPtr)(ClientState + Signatures.dwClientState_MaxPlayer));
        }

        public static int ClientState { get; set; }

        public static bool IsInGame()
        {
            if (ClientState == 0)
                ClientState = MemUtils.ReadInt32(Engine + Signatures.dwClientState);
            return MemUtils.ReadByte((IntPtr)(ClientState + Signatures.dwClientState_State)) == 6;
        }

        private Vector2 Center => new Vector2(_sizeX / 2f, _sizeY / 2f);
        private double _closedRealDist = 0;
        private float Fov = 200;
        public Player Closed;
        public bool IsCanNewTarget = true;
        private string _mapName;

        private void FindClosest(Player player, Vector2 w2SheadPos)
        {
            var dist = MathUtils.Distance(Center, w2SheadPos);
            if (dist >= Fov) return;
            var before = MathUtils.CalcAngle2(LocalPlayer,player.HeadPosition, LocalPlayer.ViewAngles);
            before = MathUtils.ClampAngle(before);
            var realDist = MathUtils.GetRealDistance(LocalPlayer, before, player);
            if (Closed == null)
            {
                Closed = player;
                _closedRealDist = realDist;
            }
            else if (realDist <= _closedRealDist)
            {
                Closed = player;
                _closedRealDist = realDist;
            }
        }
        private void AimFunction()
        {
            var newAng = MathUtils.CalcAngle2(LocalPlayer,Closed.GetEntityBonePosition(8), LocalPlayer.ViewAngles);
            newAng = MathUtils.ClampAngle(newAng);
            MemUtils.WriteVector3((IntPtr)(ClientState + Signatures.dwClientState_ViewAngles), newAng);

            WinApi.mouse_event(WinApi.MOUSEEVENTF.LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(1);
            WinApi.mouse_event(WinApi.MOUSEEVENTF.LEFTUP, 0, 0, 0, 0);
            //Thread.Sleep(10);
        }

        /*private void DrawPlayer(Player player, Vector2 pos, Player me, int i, double d2)
        {
            var color = player.Team == me.Team ? Color.YellowGreen : new Color(201, 28, 28);
            if (i == 0)
            {
                color = Color.Gold;
            }
            //Draw ellipse
            _device.DrawDashedRectangle((int)(pos.X - PlayerBoxSize / 2f), (int)(pos.Y - PlayerBoxSize / 2f), PlayerBoxSize, PlayerBoxSize,
                color);

            if (d2 != 0 && player.Team != me.Team)
            {
                var text = "[" + (int)d2 + "]";
                var x = (int)(pos.X - PlayerBoxSize / 2f);
                var y = (int)(pos.Y - PlayerBoxSize / 2f - PlayerBoxSize * 2);
                DrawText(x, y, text, Color.Red, true, _fontSmall2);

            }
            
        }*/
    }
}
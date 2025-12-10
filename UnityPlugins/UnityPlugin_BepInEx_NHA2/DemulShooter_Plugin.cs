using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityPlugin_BepInEx_Core;
using UnityPlugin_BepInEx_IniFile;

namespace BepInEx_DemulShooter_Plugin
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class DemulShooter_Plugin : BaseUnityPlugin
    {
        public const String pluginGuid = "argonlefou.demulshooter.nha2";
        public const String pluginName = "NightHunter2_BepInEx_DemulShooter_Plugin";
        public const String pluginVersion = "17.0.0.0";
        public const String pluginConfigFile = "NightHunter2_BepInEx_DemulShooter_Plugin.ini";

        public static BepInEx.Logging.ManualLogSource MyLogger;

        public static DemulShooter_Plugin Instance = null;

        public static readonly int MAX_PLAYERS = 2;

        //custom Input Data
        public static PluginController[] PluginControllers = new PluginController[MAX_PLAYERS];
        public static bool EnableInputHack = false;         //By default, no input hack on the plugin. Enabled once DemulShooter is connected (without -noinput flag)

        //Using custom Button as Input.GetKeyDown is not correctly detected in non-unity Thread
        public static PluginControllerButton Exit_Key = new PluginControllerButton((int)KeyCode.Escape);
        public static PluginControllerButton Test_Key = new PluginControllerButton((int) KeyCode.Alpha0);
        public static PluginControllerButton Coin_Key = new PluginControllerButton((int) KeyCode.Alpha5);
        public static PluginControllerButton MenuDown_Key = new PluginControllerButton((int) KeyCode.DownArrow);
        public static PluginControllerButton MenuSelect_Key = new PluginControllerButton((int) KeyCode.Return);
        public static PluginControllerButton WheelLeft_Key = new PluginControllerButton((int) KeyCode.LeftArrow);
        public static PluginControllerButton WheelRight_Key = new PluginControllerButton((int) KeyCode.RightArrow);

        //TCP server data for Inputs/Outputs
        private TcpListener _TcpListener;
        private Thread _TcpListenerThread;
        private TcpClient _TcpClient;
        private int _TcpPort = 33610;
        private static NetworkStream _TcpStream;

        public static TcpOutputData OutputData;
        private TcpOutputData _OutputDataBefore;
        private TcpInputData _InputData;

        public static bool CrossHairVisibility = true;
        public static bool GunVisibility = true;

        //Custom resolution
        public static int ScreenWidth = 1920;
        public static int ScreenHeight = 1080;
        public static bool Fullscreen = true;
        public static bool ForceResolution = false;

        private static readonly string SPRITE_1P_BLUE_FILE = "BepInEx\\plugins\\Assets\\image_1P_Blue.png";
        private static readonly string SPRITE_1P_RED_FILE = "BepInEx\\plugins\\Assets\\image_1P_Red.png";
        private static readonly string SPRITE_2P_BLUE_FILE = "BepInEx\\plugins\\Assets\\image_2P_Blue.png";
        private static readonly string SPRITE_2P_RED_FILE = "BepInEx\\plugins\\Assets\\image_2P_Red.png";
        public static Sprite Sprite_1P_Blue;
        public static Sprite Sprite_1P_Red;
        public static Sprite Sprite_2P_Blue;
        public static Sprite Sprite_2P_Red;

        public void Awake()
        {
            Instance = this;

            MyLogger = Logger;
            MyLogger.LogMessage("Plugin Loaded");
            Harmony harmony = new Harmony(pluginGuid);

            OutputData = new TcpOutputData(MAX_PLAYERS);
            _OutputDataBefore = new TcpOutputData(MAX_PLAYERS);
            _InputData = new TcpInputData(MAX_PLAYERS);

            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                PluginControllers[i] = new PluginController(i);
            }

            // Start TcpServer	
            _TcpListenerThread = new Thread(new ThreadStart(TcpClientThreadLoop));
            _TcpListenerThread.IsBackground = true;
            _TcpListenerThread.Start();

            MyLogger.LogMessage(Instance.GetType().Name + "." + MethodBase.GetCurrentMethod().Name + "(): Loading custom config : " + BepInEx.Paths.PluginPath + @"/" + pluginConfigFile);
            INIFile Plugin_IniFile = new INIFile(BepInEx.Paths.PluginPath + @"/" + pluginConfigFile);

            if (File.Exists(Plugin_IniFile.FInfo.FullName))
            {
                try
                {
                    Plugin_IniFile.IniReadIntValue("Video", "WIDTH", ref ScreenWidth);
                    Plugin_IniFile.IniReadIntValue("Video", "HEIGHT", ref ScreenHeight);

                    Plugin_IniFile.IniReadBoolValue("Video", "FULLSCREEN", ref Fullscreen);
                    Plugin_IniFile.IniReadBoolValue("Video", "FORCE_RESOLUTION", ref ForceResolution);

                    for (int i = 0; i < MAX_PLAYERS; i++)
                    {
                        PluginControllers[i].InputButtons[(int)PluginController.MyInputButtons.Start].SetKeyCode(Plugin_IniFile.IniReadValue("INPUT_KEYS", "P" + (i + 1).ToString() + "_START"));
                    }

                    Exit_Key.SetKeyCode(Plugin_IniFile.IniReadValue("INPUT_KEYS", "EXIT"));
                    Test_Key.SetKeyCode(Plugin_IniFile.IniReadValue("INPUT_KEYS", "TEST"));
                    Coin_Key.SetKeyCode(Plugin_IniFile.IniReadValue("INPUT_KEYS", "COIN"));
                    MenuDown_Key.SetKeyCode(Plugin_IniFile.IniReadValue("INPUT_KEYS", "MENU_DOWN"));
                    MenuSelect_Key.SetKeyCode(Plugin_IniFile.IniReadValue("INPUT_KEYS", "MENU_SELECT"));
                    WheelLeft_Key.SetKeyCode(Plugin_IniFile.IniReadValue("INPUT_KEYS", "WHEEL_LEFT"));
                    WheelRight_Key.SetKeyCode(Plugin_IniFile.IniReadValue("INPUT_KEYS", "WHEEL_RIGHT"));
                }
                catch (Exception Ex)
                {
                    MyLogger.LogError(Instance.GetType().Name + "." + MethodBase.GetCurrentMethod().Name + "(): Error reading config file : " + Plugin_IniFile.FInfo.FullName);
                    MyLogger.LogError(Ex.Message.ToString());
                }
            }
            else
            {
                MyLogger.LogWarning(Instance.GetType().Name + "." + MethodBase.GetCurrentMethod().Name + "():" + Plugin_IniFile.FInfo.FullName + " not found");
            }

            MyLogger.LogMessage("Graphics Engine: " + SystemInfo.graphicsDeviceVersion);

            Texture2D texture = LoadTextureFromFile(SPRITE_1P_BLUE_FILE);
            Sprite_1P_Blue = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            texture = LoadTextureFromFile(SPRITE_1P_RED_FILE);
            Sprite_1P_Red = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            texture = LoadTextureFromFile(SPRITE_2P_BLUE_FILE);
            Sprite_2P_Blue = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            texture = LoadTextureFromFile(SPRITE_2P_RED_FILE);
            Sprite_2P_Red = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            harmony.PatchAll();
        }

        public void Start()
        { }

        public void Update()
        {
            //Custom Button handling
            Exit_Key.SetButton(Input.GetKey((KeyCode)Exit_Key.KeyCode));
            Test_Key.SetButton(Input.GetKey((KeyCode)Test_Key.KeyCode));
            Coin_Key.SetButton(Input.GetKey((KeyCode)Coin_Key.KeyCode));
            MenuDown_Key.SetButton(Input.GetKey((KeyCode)MenuDown_Key.KeyCode));
            MenuSelect_Key.SetButton(Input.GetKey((KeyCode)MenuSelect_Key.KeyCode));
            WheelLeft_Key.SetButton(Input.GetKey((KeyCode)WheelLeft_Key.KeyCode));
            WheelRight_Key.SetButton(Input.GetKey((KeyCode)WheelRight_Key.KeyCode));

            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                PluginControllers[i].SetButton(PluginController.MyInputButtons.Start, Input.GetKey((KeyCode)PluginControllers[i].InputButtons[(int)PluginController.MyInputButtons.Start].KeyCode) ? (byte)1 : (byte)0);

                if (!EnableInputHack)
                {
                    PluginControllers[i].SetAimingValues(Input.mousePosition);
                    PluginControllers[i].SetButton(PluginController.MyInputButtons.Trigger, Input.GetMouseButton(0) ? (byte)1 : (byte)0);
                    PluginControllers[i].SetButton(PluginController.MyInputButtons.Reload, Input.GetMouseButton(1) ? (byte)1 : (byte)0);
                    PluginControllers[i].SetButton(PluginController.MyInputButtons.Action, Input.GetMouseButton(2) ? (byte)1 : (byte)0);
                }
            }

            //Quit
            if (Exit_Key.GetButtonDown())
                UnityEngine.Application.Quit();

            //Custom buttons handling
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                if (PluginControllers[i].GetButtonDown(PluginController.MyInputButtons.Reload))
                    input_obj_change_bullet.change_weapon(i + 1); // PlayerNum is 1 or 2

                if (PluginControllers[i].GetButtonDown(PluginController.MyInputButtons.Action))
                    input_obj_big_power.big_power_work(i + 1); // PlayerNum is 1 or 2
            }


            //Fetching Outputs
            try
            {
                OutputData.Credits = zhichi_hanshu_houtai.get_no_use_coins();
                for (int i = 0; i < MAX_PLAYERS; i++)
                {

                    OutputData.Life[i] = game_run_core.my_static_game_run.mygame_players.get_game_player(i + 1).get_curr_blood();
                    OutputData.GunType[i] = (int)zhichi_hanshu_gun_wheel_mark_manage.zchs_get_gun_type(i + 1);
                    OutputData.IsPlaying[i] = zhichi_hanshu_game_players.get_player_by_num(i + 1).is_living() ? (byte)1 : (byte)0;
                }
            } catch { }

            //Checking for a change in output to send or not
            byte[] bOutputData = OutputData.ToByteArray();
            byte[] bOutputDataBefore = _OutputDataBefore.ToByteArray();
            for (int i = 0; i < bOutputData.Length; i++)
            {
                if (bOutputData[i] != bOutputDataBefore[i])
                {
                    SendOutputs();
                    break;
                }
            }

            //Save current state
            _OutputDataBefore.Update(bOutputData);
        }

        public void OnDestroy()
        {
            MyLogger.LogMessage(Instance.GetType().Name + "." + MethodBase.GetCurrentMethod().Name + "()");
            _TcpListener.Server.Close();
        }

        private void HarmonyPatch(Harmony hHarmony, Type OriginalClass, String OriginalMethod, Type ReplacementClass, String ReplacementMethod)
        {
            MethodInfo original = AccessTools.Method(OriginalClass, OriginalMethod);
            MethodInfo patch = AccessTools.Method(ReplacementClass, ReplacementMethod);
            hHarmony.Patch(original, new HarmonyMethod(patch));
        }

        /// <summary> 	
        /// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
        /// </summary> 	
        private void TcpClientThreadLoop()
        {
            try
            {
                // Create listener on localhost port 8052. 			
                _TcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), _TcpPort);
                _TcpListener.Start();
                MyLogger.LogMessage(Instance.GetType().Name + "." + MethodBase.GetCurrentMethod().Name + "(): TCP Server is listening on Port " + _TcpPort);

                Byte[] bytes = new Byte[1024];
                while (true)
                {
                    using (_TcpClient = _TcpListener.AcceptTcpClient())
                    {
                        MyLogger.LogMessage(Instance.GetType().Name + "." + MethodBase.GetCurrentMethod().Name + "(): TCP Client connected !");
                        using (_TcpStream = _TcpClient.GetStream())
                        {
                            //Send outputs at connection, if DemulShooter connects during game, between events
                            SendOutputs();
                            while (true)
                            {
                                int Length = 0;
                                try
                                {
                                    Length = _TcpStream.Read(bytes, 0, bytes.Length);
                                    //If Tcpclient gets disconnected, Read should return 0 bytes, so we can handle disconnection to allow a new connection
                                    if (Length == 0)
                                        break;
                                    byte[] InputBuffer = new byte[Length];
                                    Array.Copy(bytes, 0, InputBuffer, 0, Length);
                                    _InputData.Update(InputBuffer);
                                    //- Debug ONLY
                                    //MyLogger.LogMessage(Instance.GetType().Name + "." + MethodBase.GetCurrentMethod().Name + "(): client message received as: " + _InputData.ToString());
                                    //- Debug ONLY

                                    //lock (MutexLocker_Inputs)
                                    //{
                                    for (int i = 0; i < MAX_PLAYERS; i++)
                                    {
                                        PluginControllers[i].SetAimingValues(new Vector3(_InputData.Axis_X[i], _InputData.Axis_Y[i]));
                                        PluginControllers[i].SetButton(PluginController.MyInputButtons.Trigger, _InputData.Trigger[i]);
                                        PluginControllers[i].SetButton(PluginController.MyInputButtons.Action, _InputData.Action[i]);
                                        PluginControllers[i].SetButton(PluginController.MyInputButtons.Reload, _InputData.ChangeWeapon[i]);
                                    }
                                    CrossHairVisibility = _InputData.HideCrosshairs == 1 ? false : true;
                                    EnableInputHack = _InputData.EnableInputsHack == 1 ? true : false;
                                    GunVisibility = _InputData.HideGuns == 1 ? false : true;    
                                    //}
                                }
                                catch
                                {
                                    //Connnection Error ?
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (SocketException socketException)
            {
                MyLogger.LogMessage(Instance.GetType().Name + "." + MethodBase.GetCurrentMethod().Name + "(): SocketException " + socketException.ToString());
            }
        }

        /// <summary>
        /// Send output data over the TCP connection
        /// When TcpClient is disconnected, _TcpClient is Disposed and can't be acces to check if null or not => need Try/Catch
        /// </summary>
        public static void SendOutputs()
        {
            try
            {
                if (Instance._TcpClient == null)
                    return;

                if (_TcpStream == null)
                    return;

                // Get a stream object for writing. 			
                if (_TcpStream.CanWrite)
                {
                    TcpPacket p = new TcpPacket(OutputData.ToByteArray(), TcpPacket.PacketHeader.Outputs);
                    byte[] Buffer = p.GetFullPacket();
                    //- Debug ONLY
                    //MyLogger.LogMessage(Instance.GetType().Name + "." + MethodBase.GetCurrentMethod().Name + "():  Sending data : " + p.ToString());
                    //- Debug ONLY
                    //lock (MutexLocker_Outputs)
                    //{
                    //Resetting event flags for next packets                    
                    for (int i = 0; i < MAX_PLAYERS; i++)
                    {
                        OutputData.Recoil[i] = 0;
                        OutputData.Damaged[i] = 0;
                    }
                    //}
                    _TcpStream.Write(Buffer, 0, Buffer.Length);
                }
            }
            catch (Exception Ex)
            {
                MyLogger.LogMessage(Instance.GetType().Name + "." + MethodBase.GetCurrentMethod().Name + "(): Socket exception: " + Ex);
            }
        }

        /// <summary>
        /// For deebug, printing StackTrace to trace calls to API we're looking for
        /// </summary>
        public static void PrintStackTrace()
        {
            MyLogger.LogMessage(System.Environment.StackTrace);
        }

        private static Texture2D LoadTextureFromFile(string FilePath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(FilePath))
            {
                fileData = File.ReadAllBytes(FilePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                MyLogger.LogMessage(string.Concat(new object[]
                {
                    "Texture created from ",
                    FilePath,
                    " :  width = ",
                    tex.width,
                    ", height =  ",
                    tex.height,
                    ",",
                    tex.dimension.ToString()
                }));
            }
            else
            {
                MyLogger.LogError("TokiPlugin.Awake() => File not found : " + FilePath);
            }
            return tex;
        }
    }
}

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
        public const String pluginGuid = "argonlefou.demulshooter.pvz";
        public const String pluginName = "PVZ_BepInEx_DemulShooter_Plugin";
        public const String pluginVersion = "17.0.0.0";
        public const String pluginConfigFile = "PVZ_BepInEx_DemulShooter_Plugin.ini";

        public static BepInEx.Logging.ManualLogSource MyLogger;

        public static DemulShooter_Plugin Instance = null;

        public static readonly int MAX_PLAYERS = 1;

        //custom Input Data
        public static PluginController[] PluginControllers = new PluginController[MAX_PLAYERS];
        public static bool EnableInputHack = false;         //By default, no input hack on the plugin. Enabled once DemulShooter is connected (without -noinput flag)

        //Using custom Button as Input.GetKeyDown is not correctly detected in non-unity Thread
        public static PluginControllerButton Exit_Key;
        public static PluginControllerButton Test_Key;

        //TCP server data for Inputs/Outputs
        private TcpListener _TcpListener;
        private Thread _TcpListenerThread;
        private TcpClient _TcpClient;
        private int _TcpPort = 33610;
        private static NetworkStream _TcpStream;

        private TcpInputData _InputData;

        public static bool CrossHairVisibility = true;

        //Custom resolution
        public static int ScreenWidth = 1920;
        public static int ScreenHeight = 1080;
        public static bool Fullscreen = true;
        public static bool ForceResolution = false;

        public void Awake()
        {
            Instance = this;

            MyLogger = Logger;
            MyLogger.LogMessage("Plugin Loaded");
            Harmony harmony = new Harmony(pluginGuid);

            _InputData = new TcpInputData(MAX_PLAYERS);

            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                PluginControllers[i] = new PluginController(i);
            }

            Exit_Key = new PluginControllerButton((int)KeyCode.Escape);
            Test_Key = new PluginControllerButton((int)KeyCode.Alpha0);

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
                    Plugin_IniFile.IniReadIntValue("VIDEO", "WIDTH", ref ScreenWidth);
					Plugin_IniFile.IniReadIntValue("VIDEO", "HEIGHT", ref ScreenHeight);

					Plugin_IniFile.IniReadBoolValue("VIDEO", "FULLSCREEN", ref Fullscreen);
					Plugin_IniFile.IniReadBoolValue("VIDEO", "FORCE_RESOLUTION", ref ForceResolution);

                    for (int i = 0; i < MAX_PLAYERS; i++)
                    {

                        PluginControllers[i].InputButtons[(int)PluginController.MyInputButtons.Start].SetKeyCode(Plugin_IniFile.IniReadValue("INPUT_KEYS", "P" + (i + 1).ToString() + "_START"));
                        PluginControllers[i].InputButtons[(int)PluginController.MyInputButtons.Coin].SetKeyCode(Plugin_IniFile.IniReadValue("INPUT_KEYS", "P" + (i + 1).ToString() + "_COIN"));
                    }

                    Exit_Key.SetKeyCode(Plugin_IniFile.IniReadValue("INPUT_KEYS", "EXIT"));
                    Test_Key.SetKeyCode(Plugin_IniFile.IniReadValue("INPUT_KEYS", "TEST"));
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

            harmony.PatchAll();
        }

        public void Start()
        {}

        public void Update()
        {           
            //Custom Button handling
            Exit_Key.SetButton(Input.GetKey((KeyCode)Exit_Key.KeyCode));
            Test_Key.SetButton(Input.GetKey((KeyCode)Test_Key.KeyCode));
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                PluginControllers[i].SetButton(PluginController.MyInputButtons.Start, Input.GetKey((KeyCode)PluginControllers[i].InputButtons[(int)PluginController.MyInputButtons.Start].KeyCode) ? (byte)1 : (byte)0);
                PluginControllers[i].SetButton(PluginController.MyInputButtons.Coin, Input.GetKey((KeyCode)PluginControllers[i].InputButtons[(int)PluginController.MyInputButtons.Coin].KeyCode) ? (byte)1 : (byte)0);

                if (!EnableInputHack)
                {
                    PluginControllers[i].SetAimingValues(Input.mousePosition);
                    PluginControllers[i].SetButton(PluginController.MyInputButtons.Trigger, Input.GetMouseButton(0) ? (byte)1 : (byte)0);
                    PluginControllers[i].SetButton(PluginController.MyInputButtons.Reload, Input.GetMouseButton(1) ? (byte)1 : (byte)0);
                }
            }

            //Quit
            if (Exit_Key.GetButtonDown())
                UnityEngine.Application.Quit();

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
                                    }
                                    CrossHairVisibility = _InputData.HideCrosshairs == 1 ? false : true;
                                    EnableInputHack = _InputData.EnableInputsHack == 1 ? true : false;
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
        /// For deebug, printing StackTrace to trace calls to API we're looking for
        /// </summary>
        public static void PrintStackTrace()
        {
            MyLogger.LogMessage(System.Environment.StackTrace);
        }
    }
}

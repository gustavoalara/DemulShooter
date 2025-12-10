using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Instrumentation;
using System.Reflection;
using DsCore;
using DsCore.Config;
using DsCore.IPC;
using DsCore.MameOutput;

namespace DemulShooter.Games
{
    public class Game_ArcadepcFireHero : Game__Unity
    {
        private class InputData : Base_InputData
        {
            public float[] Axis_X = null;
            public float[] Axis_Y = null;

            public InputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }

        private class OutputData : Base_OutputData
        {
            public byte[] IsPlaying = null;
            public byte[] StartLed = null;
            public byte Leftflash = 0;
            public byte Rightflash = 0;
            public byte ConsoleLight = 0;
            public byte FireLight = 0;
            public byte Fog = 0;
            public byte WaterPump = 0;
            public byte[] GunShake = null;
            public byte[] SmallGun = null;
            public byte[] BigGun = null;
            public byte[] WaterRotate = null;
            public byte[] Ticket = null;
            public float[] Life = null;
            public byte[] Damaged = null;
            public int[] Credits = null;

            public OutputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }

        private const int MAX_PLAYERS = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        public Game_ArcadepcFireHero(String RomName)
            : base(RomName, "FireHero", "FireFighter2")
        {
           _KnownMd5Prints.Add("Fire Hero v1.4.6", "a1b853ec0d1a597afc41ac7a448166f3");

            _InputData = new InputData(MAX_PLAYERS);
            _OutputData = new OutputData(MAX_PLAYERS);

            _tProcess.Start();
            Logger.WriteLog("Waiting for " + _RomName + " game to hook.....");
        }

        /// <summary>
        /// Overriding the Procedure as the game Window Title can be changed in the game ini files
        /// So looking in those file to know what we are looking for
        /// </summary>
        protected override void tProcess_Elapsed(Object Sender, EventArgs e)
        {
            if (!_ProcessHooked)
            {
                try
                {
                    Process[] processes = Process.GetProcessesByName(_Target_Process_Name);
                    if (processes.Length > 0)
                    {
                        _TargetProcess = processes[0];
                        _ProcessHandle = _TargetProcess.Handle;
                        _TargetProcess_MemoryBaseAddress = _TargetProcess.MainModule.BaseAddress;

                        //Looking for the game's window based on it's Title
                        _GameWindowHandle = IntPtr.Zero;
                        if (_TargetProcess_MemoryBaseAddress != IntPtr.Zero)
                        {
                            // The game may start with other Windows than the main one (BepInEx console, other stuff.....) so we need to filter
                            // the displayed window according to the Title, if DemulShooter is started before the game,  to hook the correct one
                            String PlayerSetting_FilePath = _TargetProcess.MainModule.FileName.Replace(_Target_Process_Name + ".exe", _Target_Process_Name + @"_Data\StreamingAssets\Setting\PlayerSetting.ini");
                            Logger.WriteLog(PlayerSetting_FilePath);
                            if (File.Exists(PlayerSetting_FilePath))
                            {
                                try
                                {
                                    INIFile Player_IniFile = new INIFile(PlayerSetting_FilePath);
                                    _MainWindowTitle = Player_IniFile.IniReadValue("Window", "Title");
                                    Logger.WriteLog("Found " + PlayerSetting_FilePath + " value to change Window Title to : " + _MainWindowTitle);
                                }
                                catch (Exception Ex)
                                {
                                    Logger.WriteLog("Error reading config file : " + PlayerSetting_FilePath);
                                    Logger.WriteLog(Ex.Message.ToString());
                                }
                            }
                            else
                            {
                                Logger.WriteLog(PlayerSetting_FilePath + " not found");
                            }

                            if (FindGameWindow_Equals(_MainWindowTitle))
                            {
                                String AssemblyDllPath = _TargetProcess.MainModule.FileName.Replace(_Target_Process_Name + ".exe", _Target_Process_Name + @"_Data\Managed\Assembly-CSharp.dll");
                                CheckMd5(AssemblyDllPath);

                                //Start TcpClient to dial with Unity Game
                                _Tcpclient = new DsTcp_Client("127.0.0.1", DsTcp_Client.DS_TCP_CLIENT_PORT);
                                _Tcpclient.PacketReceived += DsTcp_Client_PacketReceived;
                                _Tcpclient.TcpConnected += DsTcp_client_TcpConnected;
                                _Tcpclient.Connect();

                                if (_DisableInputHack)
                                    Logger.WriteLog("Input Hack disabled");

                                _ProcessHooked = true;
                                RaiseGameHookedEvent();
                            }
                            else
                            {
                                Logger.WriteLog("Game Window not found");
                                return;
                            }
                        }
                    }
                }
                catch
                {
                    Logger.WriteLog("Error trying to hook " + _Target_Process_Name + ".exe");
                }
            }
            else
            {
                Process[] processes = Process.GetProcessesByName(_Target_Process_Name);
                if (processes.Length <= 0)
                {
                    _ProcessHooked = false;
                    _TargetProcess = null;
                    _ProcessHandle = IntPtr.Zero;
                    _TargetProcess_MemoryBaseAddress = IntPtr.Zero;
                    Logger.WriteLog(_Target_Process_Name + ".exe closed");
                    System.Windows.Forms.Application.Exit();
                }
            }
        }

        #region Inputs

        /// <summary>
        /// Writing Axis and Buttons data in memory
        /// </summary>
        public override void SendInput(PlayerSettings PlayerData)
        {
            if (!_DisableInputHack && PlayerData.ID <= MAX_PLAYERS)
            {
                float AxisX = PlayerData.RIController.Computed_X;
                float AxisY = PlayerData.RIController.Computed_Y;

                ((InputData)_InputData).Axis_X[PlayerData.ID - 1] = AxisX;
                ((InputData)_InputData).Axis_Y[PlayerData.ID - 1] = AxisY;

                if (_HideCrosshair)
                    _InputData.HideCrosshairs = 1;
                else
                    _InputData.HideCrosshairs = 0;

                _Tcpclient.SendMessage(_InputData.ToByteArray());
            }
        }

        #endregion

        #region Outputs

        /// <summary>
        /// Create the Output list that we will be looking for and forward to MameHooker
        /// </summary>
        protected override void CreateOutputList()
        {
            _Outputs = new List<GameOutput>();
            _Outputs.Add(new GameOutput(OutputId.P1_LmpStart));
            _Outputs.Add(new GameOutput(OutputId.P2_LmpStart));
            _Outputs.Add(new GameOutput(OutputId.LmpLeft));
            _Outputs.Add(new GameOutput(OutputId.LmpRight));
            _Outputs.Add(new GameOutput(OutputId.Lmp_RedLight));
            _Outputs.Add(new GameOutput(OutputId.LmpPanel));
            _Outputs.Add(new GameOutput(OutputId.WaterPump));
            _Outputs.Add(new GameOutput(OutputId.P1_WaterFire));
            _Outputs.Add(new GameOutput(OutputId.P2_WaterFire));
            _Outputs.Add(new GameOutput(OutputId.P1_BigGun));
            _Outputs.Add(new GameOutput(OutputId.P2_BigGun));
            _Outputs.Add(new GameOutput(OutputId.P1_WaterRotate));
            _Outputs.Add(new GameOutput(OutputId.P2_WaterRotate));
            _Outputs.Add(new GameOutput(OutputId.SmokeSwitch));
            _Outputs.Add(new GameOutput(OutputId.P1_GunMotor));
            _Outputs.Add(new GameOutput(OutputId.P2_GunMotor));
            _Outputs.Add(new GameOutput(OutputId.P1_Life));
            _Outputs.Add(new GameOutput(OutputId.P2_Life));
            _Outputs.Add(new GameOutput(OutputId.P1_Damaged));
            _Outputs.Add(new GameOutput(OutputId.P2_Damaged));
            _Outputs.Add(new GameOutput(OutputId.P1_Credits));
            _Outputs.Add(new GameOutput(OutputId.P2_Credits));

        }

        /// <summary>
        /// Update all Outputs values before sending them to MameHooker
        /// </summary>
        public override void UpdateOutputValues()
        {
            //Nothing to do here, update will be done by the Tcp packet received event
        }

        protected override void DsTcp_Client_PacketReceived(Object Sender, DsTcp_Client.PacketReceivedEventArgs e)
        {
            if (e.Packet.GetHeader() == DsTcp_TcpPacket.PacketHeader.Outputs)
            {
                _OutputData.Update(e.Packet.GetPayload());

                SetOutputValue(OutputId.P1_LmpStart, ((OutputData)_OutputData).StartLed[0]);
                SetOutputValue(OutputId.P2_LmpStart, ((OutputData)_OutputData).StartLed[1]);
                SetOutputValue(OutputId.LmpLeft, ((OutputData)_OutputData).Leftflash);
                SetOutputValue(OutputId.LmpRight, ((OutputData)_OutputData).Rightflash);
                SetOutputValue(OutputId.Lmp_RedLight, ((OutputData)_OutputData).FireLight);
                SetOutputValue(OutputId.LmpPanel, ((OutputData)_OutputData).ConsoleLight);
                SetOutputValue(OutputId.WaterPump, ((OutputData)_OutputData).WaterPump);
                SetOutputValue(OutputId.P1_WaterFire, ((OutputData)_OutputData).SmallGun[0]);
                SetOutputValue(OutputId.P2_WaterFire, ((OutputData)_OutputData).SmallGun[1]);
                SetOutputValue(OutputId.P1_BigGun, ((OutputData)_OutputData).BigGun[0]);
                SetOutputValue(OutputId.P2_BigGun, ((OutputData)_OutputData).BigGun[1]);
                SetOutputValue(OutputId.P1_WaterRotate, ((OutputData)_OutputData).WaterRotate[0]);
                SetOutputValue(OutputId.P2_WaterRotate, ((OutputData)_OutputData).WaterRotate[1]);
                SetOutputValue(OutputId.SmokeSwitch, ((OutputData)_OutputData).Fog);
                SetOutputValue(OutputId.P1_GunMotor, ((OutputData)_OutputData).GunShake[0]);
                SetOutputValue(OutputId.P2_GunMotor, ((OutputData)_OutputData).GunShake[1]);
                SetOutputValue(OutputId.P1_Life, (int)((OutputData)_OutputData).Life[0]);
                SetOutputValue(OutputId.P2_Life, (int)((OutputData)_OutputData).Life[1]);
                SetOutputValue(OutputId.P1_Damaged, ((OutputData)_OutputData).Damaged[0]);
                SetOutputValue(OutputId.P2_Damaged, ((OutputData)_OutputData).Damaged[1]);
                SetOutputValue(OutputId.P1_Credits, ((OutputData)_OutputData).Credits[0]);
                SetOutputValue(OutputId.P2_Credits, ((OutputData)_OutputData).Credits[1]);
            }
        }

        #endregion
    }
}

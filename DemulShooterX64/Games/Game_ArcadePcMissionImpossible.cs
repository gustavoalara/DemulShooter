using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DsCore;
using DsCore.Config;
using DsCore.IPC;
using DsCore.MameOutput;
using DsCore.RawInput;

namespace DemulShooterX64.Games
{
    public class Game_ArcadepcMissionImpossible : Game__Unity
    {
        private class InputData : Base_InputData
        {
            public float[] Axis_X = null;
            public float[] Axis_Y = null;
            public byte[] TriggerL = null;
            public byte[] TriggerR = null;
            public byte[] Reload = null;

            public InputData(int PlayerNumber) : base(PlayerNumber)
            {                 
            }
        }

        private class OutputData : Base_OutputData
        {
            public byte[] IsPlaying = null;
            public byte[] Recoil = null;
            public byte[] Damaged = null;
            public int[] Life = null;
            public int[] AmmoGunL = null;
            public int[] AmmoGunR = null;
            public int Credits = 0;

            public OutputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }

        private const int MAX_PLAYERS = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        public Game_ArcadepcMissionImpossible(String RomName)
            : base(RomName, "MissionImpossible", "MissionImpossible")
        {
            _KnownMd5Prints.Add("Mission Impossible Arcade v201123 - Original", "aa47159c7b394366da6e7b8f402c52ef");

            _InputData = new InputData(MAX_PLAYERS);
            _OutputData = new OutputData(MAX_PLAYERS);

            _tProcess.Start();
            Logger.WriteLog("Waiting for " + _RomName + " game to hook.....");
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

                if ((PlayerData.RIController.Computed_Buttons & RawInputcontrollerButtonEvent.OnScreenTriggerDown) != 0)
                {
                    if (Configurator.GetInstance().MissionImpossible_MergeTriggers)
                    {
                        ((InputData)_InputData).TriggerL[PlayerData.ID - 1] = 1;
                        ((InputData)_InputData).TriggerR[PlayerData.ID - 1] = 1;
                    }
                    else
                    {
                        ((InputData)_InputData).TriggerL[PlayerData.ID - 1] = 1;
                    }
                }
                if ((PlayerData.RIController.Computed_Buttons & RawInputcontrollerButtonEvent.OnScreenTriggerUp) != 0)
                {
                    if (Configurator.GetInstance().MissionImpossible_MergeTriggers)
                    {
                        ((InputData)_InputData).TriggerL[PlayerData.ID - 1] = 0;
                        ((InputData)_InputData).TriggerR[PlayerData.ID - 1] = 0;
                    }
                    else
                    {
                        ((InputData)_InputData).TriggerL[PlayerData.ID - 1] = 0;
                    }
                }

                if ((PlayerData.RIController.Computed_Buttons & RawInputcontrollerButtonEvent.ActionDown) != 0)
                {
                    if (!Configurator.GetInstance().MissionImpossible_MergeTriggers)
                    {
                        ((InputData)_InputData).TriggerR[PlayerData.ID - 1] = 1;
                    }
                }
                if ((PlayerData.RIController.Computed_Buttons & RawInputcontrollerButtonEvent.ActionUp) != 0)
                {
                    if (!Configurator.GetInstance().MissionImpossible_MergeTriggers)
                    {
                        ((InputData)_InputData).TriggerR[PlayerData.ID - 1] = 0;
                    }
                }

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
            _Outputs.Add(new SyncBlinkingGameOutput(OutputId.P1_CtmLmpStart, 500));
            _Outputs.Add(new SyncBlinkingGameOutput(OutputId.P2_CtmLmpStart, 500));
            _Outputs.Add(new GameOutput(OutputId.P1_Ammo));
            _Outputs.Add(new GameOutput(OutputId.P2_Ammo));
            _Outputs.Add(new AsyncGameOutput(OutputId.P1_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
            _Outputs.Add(new AsyncGameOutput(OutputId.P2_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
            _Outputs.Add(new GameOutput(OutputId.P1_Life));
            _Outputs.Add(new GameOutput(OutputId.P2_Life));
            _Outputs.Add(new AsyncGameOutput(OutputId.P1_Damaged, Configurator.GetInstance().OutputCustomDamagedDelay, 100, 0));
            _Outputs.Add(new AsyncGameOutput(OutputId.P2_Damaged, Configurator.GetInstance().OutputCustomDamagedDelay, 100, 0));
            _Outputs.Add(new GameOutput(OutputId.Credits));
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

                if (((OutputData)_OutputData).IsPlaying[0] == 1)
                {
                    SetOutputValue(OutputId.P1_CtmLmpStart, 0);
                    SetOutputValue(OutputId.P1_Ammo, (int)(((OutputData)_OutputData).AmmoGunL[0] + ((OutputData)_OutputData).AmmoGunR[0]));
                    SetOutputValue(OutputId.P1_CtmRecoil, ((OutputData)_OutputData).Recoil[0]);
                    SetOutputValue(OutputId.P1_Life, (int)((OutputData)_OutputData).Life[0]);
                    SetOutputValue(OutputId.P1_Damaged, ((OutputData)_OutputData).Damaged[0]);
                }
                else
                {
                    SetOutputValue(OutputId.P1_CtmLmpStart, -1);
                    SetOutputValue(OutputId.P1_Ammo, 0);
                    SetOutputValue(OutputId.P1_Life, 0);
                }

                if (((OutputData)_OutputData).IsPlaying[1] == 1)
                {
                    SetOutputValue(OutputId.P2_CtmLmpStart, 0);
                    SetOutputValue(OutputId.P2_Ammo, (int)(((OutputData)_OutputData).AmmoGunL[1] + ((OutputData)_OutputData).AmmoGunR[1]));
                    SetOutputValue(OutputId.P2_CtmRecoil, ((OutputData)_OutputData).Recoil[1]);
                    SetOutputValue(OutputId.P2_Life, (int)((OutputData)_OutputData).Life[1]);
                    SetOutputValue(OutputId.P2_Damaged, ((OutputData)_OutputData).Damaged[1]);
                }
                else
                {
                    SetOutputValue(OutputId.P2_CtmLmpStart, -1);
                    SetOutputValue(OutputId.P2_Ammo, 0);
                    SetOutputValue(OutputId.P2_Life, 0);
                }


                SetOutputValue(OutputId.Credits, ((OutputData)_OutputData).Credits);
            }
        }

        #endregion
    }
}

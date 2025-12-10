using System;
using System.Collections.Generic;
using DsCore;
using DsCore.Config;
using DsCore.IPC;
using DsCore.MameOutput;
using DsCore.RawInput;

namespace DemulShooterX64.Games
{
    public class Game_ArcadepcNightHunterArcade : Game__Unity
    {
        private class InputData : Base_InputData
        {
            public float[] Axis_X = null;
            public float[] Axis_Y = null;
            public byte[] Trigger = null;
            public byte[] Action = null;
            public byte[] ChangeWeapon = null;
            public byte HideGuns = 0;

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
            public int[] GunType = null;
            public int Credits = 0;

            public OutputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }

        private const int MAX_PLAYERS = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        public Game_ArcadepcNightHunterArcade(String RomName)
            : base(RomName, "qumo2_en", "test_gun")
        {
            _KnownMd5Prints.Add("Night Hunter v2.0.6 - Clean Dump", "1d6cbc2b0ebaf0bacbcbea84aa0f4a27");
            _KnownMd5Prints.Add("Night Hunter v2.0.6 - Dongle patched", "d223a04447e84073a178a14d50349da8");
            _KnownMd5Prints.Add("Night Hunter v3.0.2 - Clean Dump", "a7ddc5d05f9081b2fb5086cdd807deae");

            _InputData = new InputData(MAX_PLAYERS);
            _OutputData = new OutputData(MAX_PLAYERS);

            _tProcess.Start();
            Logger.WriteLog("Waiting for " + _RomName + " game to hook.....");
        }

        protected override void DsTcp_client_TcpConnected(Object Sender, EventArgs e)
        {
            if (_HideCrosshair)
                _InputData.HideCrosshairs = 1;
            else
                _InputData.HideCrosshairs = 0;

            if (_DisableInputHack)
                _InputData.EnableInputsHack = 0;
            else
                _InputData.EnableInputsHack = 1;

            if (_HideGuns)
                ((InputData)_InputData).HideGuns = 1;
            else
                ((InputData)_InputData).HideGuns = 0;

            _Tcpclient.SendMessage(_InputData.ToByteArray());
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
                    ((InputData)_InputData).Trigger[PlayerData.ID - 1] = 1;
                if ((PlayerData.RIController.Computed_Buttons & RawInputcontrollerButtonEvent.OnScreenTriggerUp) != 0)
                    ((InputData)_InputData).Trigger[PlayerData.ID - 1] = 0;

                if ((PlayerData.RIController.Computed_Buttons & RawInputcontrollerButtonEvent.ActionDown) != 0)
                    ((InputData)_InputData).Action[PlayerData.ID - 1] = 1;
                if ((PlayerData.RIController.Computed_Buttons & RawInputcontrollerButtonEvent.ActionUp) != 0)
                    ((InputData)_InputData).Action[PlayerData.ID - 1] = 0;

                if ((PlayerData.RIController.Computed_Buttons & RawInputcontrollerButtonEvent.OffScreenTriggerDown) != 0)
                    ((InputData)_InputData).ChangeWeapon[PlayerData.ID - 1] = 1;
                if ((PlayerData.RIController.Computed_Buttons & RawInputcontrollerButtonEvent.OffScreenTriggerUp) != 0)
                    ((InputData)_InputData).ChangeWeapon[PlayerData.ID - 1] = 0;

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
            _Outputs.Add(new SyncBlinkingGameOutput(OutputId.P1_LmpStart, 500));
            _Outputs.Add(new SyncBlinkingGameOutput(OutputId.P2_LmpStart, 500));
            _Outputs.Add(new AsyncGameOutput(OutputId.P1_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
            _Outputs.Add(new AsyncGameOutput(OutputId.P2_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
            _Outputs.Add(new GameOutput(OutputId.P1_GunType));
            _Outputs.Add(new GameOutput(OutputId.P2_GunType));
            _Outputs.Add(new GameOutput(OutputId.P1_Life));
            _Outputs.Add(new GameOutput(OutputId.P2_Life));
            _Outputs.Add(new GameOutput(OutputId.LmpBillboard));    //Will be triggered when a super weapon is active
            _Outputs.Add(new AsyncGameOutput(OutputId.P1_Damaged, Configurator.GetInstance().OutputCustomDamagedDelay, 100, 0));
            _Outputs.Add(new AsyncGameOutput(OutputId.P2_Damaged, Configurator.GetInstance().OutputCustomDamagedDelay, 100, 0));
            _Outputs.Add(new GameOutput(OutputId.Credits));
        }

        /// <summary>
        /// Update all Outputs values before sending them to MameHooker
        /// </summary>
        protected override void DsTcp_Client_PacketReceived(Object Sender, DsTcp_Client.PacketReceivedEventArgs e)
        {
            if (e.Packet.GetHeader() == DsTcp_TcpPacket.PacketHeader.Outputs)
            {
                _OutputData.Update(e.Packet.GetPayload());

                //GunType:
                //0 = No
                //1 = Blue
                //2 = Red
                //3 = Big Blue
                //4 = Big Red

                int SuperGunAvailable = 0;
                if (((OutputData)_OutputData).IsPlaying[0] == 1)
                {
                    SetOutputValue(OutputId.P1_LmpStart, 0);
                    SetOutputValue(OutputId.P1_CtmRecoil, ((OutputData)_OutputData).Recoil[0]);
                    SetOutputValue(OutputId.P1_Life, (int)((OutputData)_OutputData).Life[0]);
                    SetOutputValue(OutputId.P1_Damaged, ((OutputData)_OutputData).Damaged[0]);
                    SetOutputValue(OutputId.P1_GunType, ((OutputData)_OutputData).GunType[0]);
                    if (((OutputData)_OutputData).GunType[0] > 2)
                        SuperGunAvailable = 1;
                }
                else
                {
                    SetOutputValue(OutputId.P1_LmpStart, -1);
                    SetOutputValue(OutputId.P1_Ammo, 0);
                    SetOutputValue(OutputId.P1_Life, 0);
                    SetOutputValue(OutputId.P1_GunType, 0);
                }

                if (((OutputData)_OutputData).IsPlaying[1] == 1)
                {
                    SetOutputValue(OutputId.P2_LmpStart, 0);
                    SetOutputValue(OutputId.P2_CtmRecoil, ((OutputData)_OutputData).Recoil[1]);
                    SetOutputValue(OutputId.P2_Life, (int)((OutputData)_OutputData).Life[1]);
                    SetOutputValue(OutputId.P2_Damaged, ((OutputData)_OutputData).Damaged[1]);
                    SetOutputValue(OutputId.P2_GunType, ((OutputData)_OutputData).GunType[1]);
                    if (((OutputData)_OutputData).GunType[1] > 2)
                        SuperGunAvailable = 1;
                }
                else
                {
                    SetOutputValue(OutputId.P2_LmpStart, -1);
                    SetOutputValue(OutputId.P2_Ammo, 0);
                    SetOutputValue(OutputId.P2_Life, 0);
                    SetOutputValue(OutputId.P2_GunType, 0);
                }

                SetOutputValue(OutputId.LmpBillboard, SuperGunAvailable);
                SetOutputValue(OutputId.Credits, ((OutputData)_OutputData).Credits);
            }
        }

        #endregion
    }
}

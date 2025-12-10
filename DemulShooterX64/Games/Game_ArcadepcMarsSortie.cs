using System;
using System.Collections.Generic;
using DsCore;
using DsCore.Config;
using DsCore.IPC;
using DsCore.MameOutput;
using DsCore.RawInput;

namespace DemulShooterX64.Games
{
    internal class Game_ArcadepcMarsSortie : Game__Unity
    {
        private class InputData : Base_InputData
        {
            public float[] Axis_X = null;
            public float[] Axis_Y = null;
            public byte[] Trigger = null;
            public byte[] ChangeWeapon = null;

            public InputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }

        private class OutputData : Base_OutputData
        {
            public byte[] IsPlaying = null;
            public byte[] Recoil = null;
            public byte[] WeaponId = null;
            public int[] Ammo = null;
            public int[] Credits = null;
            public byte Flashlight = 0;

            public OutputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }

        private const int MAX_PLAYERS = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        public Game_ArcadepcMarsSortie(String RomName)
            : base(RomName, "Shooter", "Shooter")
        {
            _KnownMd5Prints.Add("Mars Sortie v1.46.9 - Original", "01a643a3f615a22338c2505bcb1b9609");

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
                    ((InputData)_InputData).ChangeWeapon[PlayerData.ID - 1] = 1;
                if ((PlayerData.RIController.Computed_Buttons & RawInputcontrollerButtonEvent.ActionUp) != 0)
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
            _Outputs.Add(new GameOutput(OutputId.Lmp_WhiteStrobe));
            _Outputs.Add(new SyncBlinkingGameOutput(OutputId.P1_CtmLmpStart, 500));
            _Outputs.Add(new SyncBlinkingGameOutput(OutputId.P2_CtmLmpStart, 500));
            _Outputs.Add(new SyncBlinkingGameOutput(OutputId.P3_CtmLmpStart, 500));
            _Outputs.Add(new SyncBlinkingGameOutput(OutputId.P4_CtmLmpStart, 500));
            _Outputs.Add(new AsyncGameOutput(OutputId.P1_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
            _Outputs.Add(new AsyncGameOutput(OutputId.P2_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
            _Outputs.Add(new AsyncGameOutput(OutputId.P3_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
            _Outputs.Add(new AsyncGameOutput(OutputId.P4_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
            _Outputs.Add(new GameOutput(OutputId.P1_Credits));
            _Outputs.Add(new GameOutput(OutputId.P2_Credits));
            _Outputs.Add(new GameOutput(OutputId.P3_Credits));
            _Outputs.Add(new GameOutput(OutputId.P4_Credits));
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
                SetOutputValue(OutputId.Lmp_WhiteStrobe, ((OutputData)_OutputData).Flashlight);

                //Handling Start Lamps based on player status
                if (((OutputData)_OutputData).IsPlaying[0] == 0)
                    SetOutputValue(OutputId.P1_CtmLmpStart, -1);
                else
                    SetOutputValue(OutputId.P1_CtmLmpStart, 0);

                if (((OutputData)_OutputData).IsPlaying[1] == 0)
                    SetOutputValue(OutputId.P2_CtmLmpStart, -1);
                else
                    SetOutputValue(OutputId.P2_CtmLmpStart, 0);

                if (((OutputData)_OutputData).IsPlaying[2] == 0)
                    SetOutputValue(OutputId.P3_CtmLmpStart, -1);
                else
                    SetOutputValue(OutputId.P3_CtmLmpStart, 0);

                if (((OutputData)_OutputData).IsPlaying[3] == 0)
                    SetOutputValue(OutputId.P4_CtmLmpStart, -1);
                else
                    SetOutputValue(OutputId.P4_CtmLmpStart, 0);

                SetOutputValue(OutputId.P1_CtmRecoil, ((OutputData)_OutputData).Recoil[0]);
                SetOutputValue(OutputId.P2_CtmRecoil, ((OutputData)_OutputData).Recoil[1]);
                SetOutputValue(OutputId.P3_CtmRecoil, ((OutputData)_OutputData).Recoil[2]);
                SetOutputValue(OutputId.P4_CtmRecoil, ((OutputData)_OutputData).Recoil[3]);
                SetOutputValue(OutputId.P1_Credits, ((OutputData)_OutputData).Credits[0]);
                SetOutputValue(OutputId.P2_Credits, ((OutputData)_OutputData).Credits[1]);
                SetOutputValue(OutputId.P3_Credits, ((OutputData)_OutputData).Credits[2]);
                SetOutputValue(OutputId.P4_Credits, ((OutputData)_OutputData).Credits[3]);
            }
        }

        #endregion
    }
}

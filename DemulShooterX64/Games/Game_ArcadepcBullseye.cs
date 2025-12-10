using System;
using System.Collections.Generic;
using DsCore;
using DsCore.Config;
using DsCore.IPC;
using DsCore.MameOutput;
using DsCore.RawInput;

namespace DemulShooterX64.Games
{
    public class Game_ArcadepcBullseye : Game__Unity
    {
        private class InputData : Base_InputData
        {
            //Game Inputs
            public float[] Axis_X = null;
            public float[] Axis_Y = null;
            public byte[] Trigger = null;

            public InputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }

        private class OutputData : Base_OutputData
        {
            public byte IsPlaying = 0;
            public byte Recoil = 0;
            public byte LED_LeftButton = 0;
            public byte LED_RightButton = 0;
            public byte LED_StartButton = 0;
            public byte LED_RedLight = 0;
            public byte LED_Screen = 0;
            public byte Damaged = 0;
            public int Ammo = 0;
            public int Credits = 0;

            public OutputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }

        private const int MAX_PLAYERS = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        public Game_ArcadepcBullseye(String RomName)
            : base(RomName, "WW2021", "WW2021")
        {
            _KnownMd5Prints.Add("Bullseye Crackshot v4.14.40417 - Original", "F5DC92EB3412CBE3D93E66D840D14B3F");

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
                    ((InputData)_InputData).Trigger[PlayerData.ID - 1] = 1;
                if ((PlayerData.RIController.Computed_Buttons & RawInputcontrollerButtonEvent.OnScreenTriggerUp) != 0)
                    ((InputData)_InputData).Trigger[PlayerData.ID - 1] = 0;

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
            _Outputs.Add(new GameOutput(OutputId.LmpLeft));
            _Outputs.Add(new GameOutput(OutputId.LmpRight));
            _Outputs.Add(new GameOutput(OutputId.Lmp_RedLight));
            _Outputs.Add(new GameOutput(OutputId.LmpBillboard));
            _Outputs.Add(new AsyncGameOutput(OutputId.P1_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
            _Outputs.Add(new GameOutput(OutputId.P1_Credits));
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

                SetOutputValue(OutputId.P1_LmpStart, ((OutputData)_OutputData).LED_StartButton);
                SetOutputValue(OutputId.LmpLeft, ((OutputData)_OutputData).LED_LeftButton);
                SetOutputValue(OutputId.LmpRight, ((OutputData)_OutputData).LED_RightButton);
                SetOutputValue(OutputId.Lmp_RedLight, ((OutputData)_OutputData).LED_RedLight);
                SetOutputValue(OutputId.LmpBillboard, ((OutputData)_OutputData).LED_Screen);
                SetOutputValue(OutputId.P1_CtmRecoil, ((OutputData)_OutputData).Recoil);
                SetOutputValue(OutputId.P1_Ammo, (int)((OutputData)_OutputData).Ammo);
                SetOutputValue(OutputId.P1_Credits, (int)((OutputData)_OutputData).Credits);
            }
        }

        #endregion

    }
}

using System;
using System.Collections.Generic;
using DsCore;
using DsCore.Config;
using DsCore.IPC;
using DsCore.MameOutput;
using DsCore.RawInput;

namespace DemulShooterX64.Games
{
    internal class Game_RtNerfArcade : Game__Unity
    {
        private class InputData : Base_InputData
        {
            public float[] Axis_X = null;
            public float[] Axis_Y = null;
            public byte[] Trigger = null;

            public InputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }

        private class OutputData : Base_OutputData
        {
            public byte[] IsPlaying = null;
            public byte[] Recoil = null;
            public UInt32[] Credits = null;
            public UInt16 P1_Lmp_Start = 0;
            public UInt16 P1_Lmp_SeatPuck = 0;
            public UInt16 P1_Lmp_SeatMarquee = 0;
            public UInt16 P1_Lmp_SeatRear_R = 0;
            public UInt16 P1_Lmp_SeatRear_O = 0;
            public UInt16 P1_Lmp_SeatRear_B = 0;
            public UInt16 P2_Lmp_Start = 0;
            public UInt16 P2_Lmp_SeatPuck = 0;
            public UInt16 P2_Lmp_SeatMarquee = 0;
            public UInt16 P2_Lmp_SeatRear_R = 0;
            public UInt16 P2_Lmp_SeatRear_O = 0;
            public UInt16 P2_Lmp_SeatRear_B = 0;
            public UInt16 Cab_Lmp_R = 0;
            public UInt16 Cab_Lmp_G = 0;
            public UInt16 Cab_Lmp_B = 0;
            public UInt16 Cab_Lmp_RearSeat = 0;

            public OutputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }

        private const int MAX_PLAYERS = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        public Game_RtNerfArcade(String RomName)
            : base(RomName, "Nerf", "Nerf")
        {
            _KnownMd5Prints.Add("Nerf Arcade v1.55", "7f40b5a56501507b9e899f1d58401817");

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
            if (!_DisableInputHack)
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
            _Outputs.Add(new GameOutput(OutputId.P1_Lmp_SeatPuck));
            _Outputs.Add(new GameOutput(OutputId.P1_Lmp_SeatMarquee));
            _Outputs.Add(new GameOutput(OutputId.P1_Lmp_SeatSpeaker_R));
            _Outputs.Add(new GameOutput(OutputId.P1_Lmp_SeatSpeaker_O));
            _Outputs.Add(new GameOutput(OutputId.P1_Lmp_SeatSpeaker_B));
            _Outputs.Add(new GameOutput(OutputId.P2_LmpStart));
            _Outputs.Add(new GameOutput(OutputId.P2_Lmp_SeatPuck));
            _Outputs.Add(new GameOutput(OutputId.P2_Lmp_SeatMarquee));
            _Outputs.Add(new GameOutput(OutputId.P2_Lmp_SeatSpeaker_R));
            _Outputs.Add(new GameOutput(OutputId.P2_Lmp_SeatSpeaker_O));
            _Outputs.Add(new GameOutput(OutputId.P2_Lmp_SeatSpeaker_B));
            _Outputs.Add(new GameOutput(OutputId.Lmp_TMolding_R));
            _Outputs.Add(new GameOutput(OutputId.Lmp_TMolding_G));
            _Outputs.Add(new GameOutput(OutputId.Lmp_TMolding_B));
            _Outputs.Add(new GameOutput(OutputId.Lmp_SeatDownLight));
            _Outputs.Add(new AsyncGameOutput(OutputId.P1_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
            _Outputs.Add(new AsyncGameOutput(OutputId.P2_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
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

                SetOutputValue(OutputId.P1_CtmRecoil, ((OutputData)_OutputData).Recoil[0]);
                SetOutputValue(OutputId.P2_CtmRecoil, ((OutputData)_OutputData).Recoil[1]);
                SetOutputValue(OutputId.P1_Credits, (int)((OutputData)_OutputData).Credits[0]);
                SetOutputValue(OutputId.P2_Credits, (int)((OutputData)_OutputData).Credits[1]);
                //Lamps
                SetOutputValue(OutputId.P1_LmpStart, ((OutputData)_OutputData).P1_Lmp_Start);
                SetOutputValue(OutputId.P1_Lmp_SeatPuck, ((OutputData)_OutputData).P1_Lmp_SeatPuck);
                SetOutputValue(OutputId.P1_Lmp_SeatMarquee, ((OutputData)_OutputData).P1_Lmp_SeatMarquee);
                SetOutputValue(OutputId.P1_Lmp_SeatSpeaker_R, ((OutputData)_OutputData).P1_Lmp_SeatRear_R);
                SetOutputValue(OutputId.P1_Lmp_SeatSpeaker_O, ((OutputData)_OutputData).P1_Lmp_SeatRear_O);
                SetOutputValue(OutputId.P1_Lmp_SeatSpeaker_B, ((OutputData)_OutputData).P1_Lmp_SeatRear_B);
                SetOutputValue(OutputId.P2_LmpStart, ((OutputData)_OutputData).P2_Lmp_Start);
                SetOutputValue(OutputId.P2_Lmp_SeatPuck, ((OutputData)_OutputData).P2_Lmp_SeatPuck);
                SetOutputValue(OutputId.P2_Lmp_SeatMarquee, ((OutputData)_OutputData).P2_Lmp_SeatMarquee);
                SetOutputValue(OutputId.P2_Lmp_SeatSpeaker_R, ((OutputData)_OutputData).P2_Lmp_SeatRear_R);
                SetOutputValue(OutputId.P2_Lmp_SeatSpeaker_O, ((OutputData)_OutputData).P2_Lmp_SeatRear_O);
                SetOutputValue(OutputId.P2_Lmp_SeatSpeaker_B, ((OutputData)_OutputData).P2_Lmp_SeatRear_B);
                SetOutputValue(OutputId.Lmp_TMolding_R, ((OutputData)_OutputData).Cab_Lmp_R);
                SetOutputValue(OutputId.Lmp_TMolding_G, ((OutputData)_OutputData).Cab_Lmp_G);
                SetOutputValue(OutputId.Lmp_TMolding_B, ((OutputData)_OutputData).Cab_Lmp_B);
                SetOutputValue(OutputId.Lmp_SeatDownLight, ((OutputData)_OutputData).Cab_Lmp_RearSeat);
            }
        }

        #endregion

    }
}

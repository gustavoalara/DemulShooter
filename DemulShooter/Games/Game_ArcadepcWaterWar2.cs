using System;
using System.Collections.Generic;
using DsCore;
using DsCore.Config;
using DsCore.IPC;
using DsCore.MameOutput;

namespace DemulShooter.Games
{
    public class Game_ArcadepcWaterWar2 : Game__Unity
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
            public byte[] WaterFire = null;
            public byte WaterPump = 0;
            public byte[] StartLed = null;
            public byte[] TicketFeeder = null;
            public byte[] BigGun = null;
            public int[] Credits = null;

            public OutputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }
        
        private const int MAX_PLAYERS = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        public Game_ArcadepcWaterWar2(String RomName)
            : base(RomName, "Water2_EN", "Water2_EN")
        {
            _KnownMd5Prints.Add("Happy Water War 2 - v1.1", "f3fb78a0aa85562caba6ffbc6f295e13");

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
            _Outputs.Add(new GameOutput(OutputId.P1_WaterFire));
            _Outputs.Add(new GameOutput(OutputId.P2_WaterFire));
            _Outputs.Add(new GameOutput(OutputId.WaterPump));
            _Outputs.Add(new GameOutput(OutputId.P1_BigGun));
            _Outputs.Add(new GameOutput(OutputId.P2_BigGun));
            _Outputs.Add(new GameOutput(OutputId.P1_TicketFeeder));
            _Outputs.Add(new GameOutput(OutputId.P2_TicketFeeder));
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

                SetOutputValue(OutputId.P1_WaterFire, ((OutputData)_OutputData).WaterFire[0]);
                SetOutputValue(OutputId.P2_WaterFire, ((OutputData)_OutputData).WaterFire[1]);
                SetOutputValue(OutputId.WaterPump, ((OutputData)_OutputData).WaterPump);

                SetOutputValue(OutputId.P1_BigGun, ((OutputData)_OutputData).BigGun[0]);
                SetOutputValue(OutputId.P2_BigGun, ((OutputData)_OutputData).BigGun[1]);

                SetOutputValue(OutputId.P1_TicketFeeder, ((OutputData)_OutputData).TicketFeeder[0]);
                SetOutputValue(OutputId.P2_TicketFeeder, ((OutputData)_OutputData).TicketFeeder[1]);

                SetOutputValue(OutputId.P1_Credits, (int)((OutputData)_OutputData).Credits[0]);
                SetOutputValue(OutputId.P2_Credits, (int)((OutputData)_OutputData).Credits[1]);
            }
        }

        #endregion

    }
}

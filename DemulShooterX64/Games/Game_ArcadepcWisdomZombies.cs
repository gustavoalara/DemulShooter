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
    public class Game_ArcadepcWisdomZombies : Game__Unity
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
            public byte[] StartLamp = null;
            public byte[] SmallWater = null;
            public byte[] BigWater = null;
            public byte[] GunMotor = null;
            public byte[] TicketFeeder = null;
            public byte BonusWeaponLamp = 0;
            public byte SeatVibrationMotor = 0;
            public byte WaterLevelLamp = 0;
            public byte[] Life = null;
            public byte[] Damaged = null;
            public int[] Credits = null;

            public OutputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }

        private const int MAX_PLAYERS = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        public Game_ArcadepcWisdomZombies(String RomName)
            : base(RomName, "ShootZombies", "ShotZembies")
        {
            _KnownMd5Prints.Add("ShootZombies EN v1.2.8 - Original", "f0288515dd04d49c85dedacaaf922edd");

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
            _Outputs.Add(new SyncBlinkingGameOutput(OutputId.P1_LmpStart, 500));
            _Outputs.Add(new SyncBlinkingGameOutput(OutputId.P2_LmpStart, 500));
            _Outputs.Add(new SyncBlinkingGameOutput(OutputId.P3_LmpStart, 500));
            _Outputs.Add(new SyncBlinkingGameOutput(OutputId.P4_LmpStart, 500));
            _Outputs.Add(new GameOutput(OutputId.P1_WaterFire));
            _Outputs.Add(new GameOutput(OutputId.P2_WaterFire));
            _Outputs.Add(new GameOutput(OutputId.P3_WaterFire));
            _Outputs.Add(new GameOutput(OutputId.P4_WaterFire));
            _Outputs.Add(new GameOutput(OutputId.P1_BigGun));
            _Outputs.Add(new GameOutput(OutputId.P2_BigGun));
            _Outputs.Add(new GameOutput(OutputId.P3_BigGun));
            _Outputs.Add(new GameOutput(OutputId.P4_BigGun));
            _Outputs.Add(new GameOutput(OutputId.P1_GunMotor));
            _Outputs.Add(new GameOutput(OutputId.P2_GunMotor));
            _Outputs.Add(new GameOutput(OutputId.P3_GunMotor));
            _Outputs.Add(new GameOutput(OutputId.P4_GunMotor));
            /*_Outputs.Add(new GameOutput(OutputId.P1_TicketFeeder));
            _Outputs.Add(new GameOutput(OutputId.P2_TicketFeeder));
            _Outputs.Add(new GameOutput(OutputId.P3_TicketFeeder));
            _Outputs.Add(new GameOutput(OutputId.P4_TicketFeeder));*/
            _Outputs.Add(new GameOutput(OutputId.BonusWeaponLamp));
            _Outputs.Add(new GameOutput(OutputId.SeatVibrationLamp));
            _Outputs.Add(new GameOutput(OutputId.WaterLevelLamp));
            _Outputs.Add(new GameOutput(OutputId.P1_Life));
            _Outputs.Add(new GameOutput(OutputId.P2_Life));
            _Outputs.Add(new GameOutput(OutputId.P3_Life));
            _Outputs.Add(new GameOutput(OutputId.P4_Life));
            _Outputs.Add(new AsyncGameOutput(OutputId.P1_Damaged, Configurator.GetInstance().OutputCustomDamagedDelay, 100, 0));
            _Outputs.Add(new AsyncGameOutput(OutputId.P2_Damaged, Configurator.GetInstance().OutputCustomDamagedDelay, 100, 0));
            _Outputs.Add(new AsyncGameOutput(OutputId.P3_Damaged, Configurator.GetInstance().OutputCustomDamagedDelay, 100, 0));
            _Outputs.Add(new AsyncGameOutput(OutputId.P4_Damaged, Configurator.GetInstance().OutputCustomDamagedDelay, 100, 0));
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

                if (((OutputData)_OutputData).StartLamp[0] == 1)
                    SetOutputValue(OutputId.P1_LmpStart, -1);
                else
                    SetOutputValue(OutputId.P1_LmpStart, 0);

                if (((OutputData)_OutputData).StartLamp[1] == 1)
                    SetOutputValue(OutputId.P2_LmpStart, -1);
                else
                    SetOutputValue(OutputId.P2_LmpStart, 0);

                if (((OutputData)_OutputData).StartLamp[2] == 1)
                    SetOutputValue(OutputId.P3_LmpStart, -1);
                else
                    SetOutputValue(OutputId.P3_LmpStart, 0);

                if (((OutputData)_OutputData).StartLamp[3] == 1)
                    SetOutputValue(OutputId.P4_LmpStart, -1);
                else
                    SetOutputValue(OutputId.P4_LmpStart, 0);

                SetOutputValue(OutputId.P1_WaterFire, ((OutputData)_OutputData).SmallWater[0]);
                SetOutputValue(OutputId.P2_WaterFire, ((OutputData)_OutputData).SmallWater[1]);
                SetOutputValue(OutputId.P3_WaterFire, ((OutputData)_OutputData).SmallWater[2]);
                SetOutputValue(OutputId.P4_WaterFire, ((OutputData)_OutputData).SmallWater[3]);

                SetOutputValue(OutputId.P1_BigGun, ((OutputData)_OutputData).BigWater[0]);
                SetOutputValue(OutputId.P2_BigGun, ((OutputData)_OutputData).BigWater[1]);
                SetOutputValue(OutputId.P3_BigGun, ((OutputData)_OutputData).BigWater[2]);
                SetOutputValue(OutputId.P4_BigGun, ((OutputData)_OutputData).BigWater[3]);

                SetOutputValue(OutputId.P1_GunMotor, ((OutputData)_OutputData).GunMotor[0]);
                SetOutputValue(OutputId.P2_GunMotor, ((OutputData)_OutputData).GunMotor[1]);
                SetOutputValue(OutputId.P3_GunMotor, ((OutputData)_OutputData).GunMotor[2]);
                SetOutputValue(OutputId.P4_GunMotor, ((OutputData)_OutputData).GunMotor[3]);

                /*SetOutputValue(OutputId.P1_TicketFeeder, ((OutputData)_OutputData).TicketFeeder[0]);
                SetOutputValue(OutputId.P2_TicketFeeder, ((OutputData)_OutputData).TicketFeeder[1]);
                SetOutputValue(OutputId.P3_TicketFeeder, ((OutputData)_OutputData).TicketFeeder[2]);
                SetOutputValue(OutputId.P4_TicketFeeder, ((OutputData)_OutputData).TicketFeeder[3]);*/

                SetOutputValue(OutputId.BonusWeaponLamp, ((OutputData)_OutputData).BonusWeaponLamp);
                SetOutputValue(OutputId.SeatVibrationLamp, ((OutputData)_OutputData).SeatVibrationMotor);
                SetOutputValue(OutputId.WaterLevelLamp, ((OutputData)_OutputData).WaterLevelLamp);

                SetOutputValue(OutputId.P1_Damaged, ((OutputData)_OutputData).Damaged[0]);
                SetOutputValue(OutputId.P2_Damaged, ((OutputData)_OutputData).Damaged[1]);
                SetOutputValue(OutputId.P3_Damaged, ((OutputData)_OutputData).Damaged[2]);
                SetOutputValue(OutputId.P4_Damaged, ((OutputData)_OutputData).Damaged[3]);

                if (((OutputData)_OutputData).IsPlaying[0] == 1)
                    SetOutputValue(OutputId.P1_Life, (int)((OutputData)_OutputData).Life[0]);
                else
                    SetOutputValue(OutputId.P1_Life, 0);
                if (((OutputData)_OutputData).IsPlaying[1] == 1)
                    SetOutputValue(OutputId.P2_Life, (int)((OutputData)_OutputData).Life[1]);
                else
                    SetOutputValue(OutputId.P2_Life, 0);
                if (((OutputData)_OutputData).IsPlaying[2] == 1)
                    SetOutputValue(OutputId.P3_Life, (int)((OutputData)_OutputData).Life[2]);
                else
                    SetOutputValue(OutputId.P4_Life, 0);
                if (((OutputData)_OutputData).IsPlaying[3] == 1)
                    SetOutputValue(OutputId.P4_Life, (int)((OutputData)_OutputData).Life[3]);
                else
                    SetOutputValue(OutputId.P4_Life, 0);

                SetOutputValue(OutputId.P1_Credits, (int)((OutputData)_OutputData).Credits[0]);
                SetOutputValue(OutputId.P2_Credits, (int)((OutputData)_OutputData).Credits[1]);
                SetOutputValue(OutputId.P3_Credits, (int)((OutputData)_OutputData).Credits[2]);
                SetOutputValue(OutputId.P4_Credits, (int)((OutputData)_OutputData).Credits[3]);
            }
        }

        #endregion
    }
}

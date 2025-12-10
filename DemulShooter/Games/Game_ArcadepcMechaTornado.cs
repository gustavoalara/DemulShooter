using System;
using System.Collections.Generic;
using DsCore;
using DsCore.Config;
using DsCore.IPC;
using DsCore.MameOutput;
using DsCore.RawInput;

namespace DemulShooter.Games
{
    public class Game_ArcadepcMechaTornado : Game__Unity
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
            public byte Atomizer = 0;
            public byte[] IsPlaying = null;
            public byte[] Shake = null;
            public byte[] RotatingMotor = null;
            public byte[] WaterPower = null;
            public byte[] StartLight = null;
            public byte[] PlayerLight = null;
            public byte[] LightBelt = null;
            public byte[] Damaged = null;
            public byte[] Recoil = null;
            public float[] Life = null;
            public int[] Credits = null;

            public OutputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }

        private const int MAX_PLAYERS = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        public Game_ArcadepcMechaTornado(String RomName)
            : base(RomName, "mecha", "jixiesheqiu")
        {
            _KnownMd5Prints.Add("Mecha Tornado Arcade - v1.5", "32458270101d83dd6e0f08d0c617bf7e");

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
            _Outputs.Add(new GameOutput(OutputId.P2_LmpStart));
            _Outputs.Add(new GameOutput(OutputId.P3_LmpStart));
            _Outputs.Add(new GameOutput(OutputId.P4_LmpStart));
            _Outputs.Add(new GameOutput(OutputId.P1_LmpFront));
            _Outputs.Add(new GameOutput(OutputId.P2_LmpFront));
            _Outputs.Add(new GameOutput(OutputId.P3_LmpFront));
            _Outputs.Add(new GameOutput(OutputId.P4_LmpFront));
            _Outputs.Add(new GameOutput(OutputId.P1_GunMotor));
            _Outputs.Add(new GameOutput(OutputId.P2_GunMotor));
            _Outputs.Add(new GameOutput(OutputId.P3_GunMotor));
            _Outputs.Add(new GameOutput(OutputId.P4_GunMotor));
            _Outputs.Add(new GameOutput(OutputId.P1_Shaker));
            _Outputs.Add(new GameOutput(OutputId.P2_Shaker));
            _Outputs.Add(new GameOutput(OutputId.P3_Shaker));
            _Outputs.Add(new GameOutput(OutputId.P4_Shaker));
            _Outputs.Add(new GameOutput(OutputId.P1_WaterFire));
            _Outputs.Add(new GameOutput(OutputId.P2_WaterFire));
            _Outputs.Add(new GameOutput(OutputId.P3_WaterFire));
            _Outputs.Add(new GameOutput(OutputId.P4_WaterFire));
            _Outputs.Add(new GameOutput(OutputId.P1_BigGun));
            _Outputs.Add(new GameOutput(OutputId.P2_BigGun));
            _Outputs.Add(new GameOutput(OutputId.P3_BigGun));
            _Outputs.Add(new GameOutput(OutputId.P4_BigGun));
            //_Outputs.Add(new GameOutput(OutputId.P1_TicketFeeder));
            //_Outputs.Add(new GameOutput(OutputId.P2_TicketFeeder));
            _Outputs.Add(new AsyncGameOutput(OutputId.P1_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
            _Outputs.Add(new AsyncGameOutput(OutputId.P2_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
            _Outputs.Add(new AsyncGameOutput(OutputId.P3_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
            _Outputs.Add(new AsyncGameOutput(OutputId.P4_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
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

                SetOutputValue(OutputId.P1_LmpStart, ((OutputData)_OutputData).StartLight[0]);
                SetOutputValue(OutputId.P2_LmpStart, ((OutputData)_OutputData).StartLight[1]);
                SetOutputValue(OutputId.P3_LmpStart, ((OutputData)_OutputData).StartLight[2]);
                SetOutputValue(OutputId.P4_LmpStart, ((OutputData)_OutputData).StartLight[3]);

                SetOutputValue(OutputId.P1_LmpFront, ((OutputData)_OutputData).PlayerLight[0]);
                SetOutputValue(OutputId.P2_LmpFront, ((OutputData)_OutputData).PlayerLight[1]);
                SetOutputValue(OutputId.P3_LmpFront, ((OutputData)_OutputData).PlayerLight[2]);
                SetOutputValue(OutputId.P4_LmpFront, ((OutputData)_OutputData).PlayerLight[3]);

                SetOutputValue(OutputId.P1_GunMotor, ((OutputData)_OutputData).RotatingMotor[0]);
                SetOutputValue(OutputId.P2_GunMotor, ((OutputData)_OutputData).RotatingMotor[1]);
                SetOutputValue(OutputId.P3_GunMotor, ((OutputData)_OutputData).RotatingMotor[2]);
                SetOutputValue(OutputId.P4_GunMotor, ((OutputData)_OutputData).RotatingMotor[3]);

                SetOutputValue(OutputId.P1_Shaker, ((OutputData)_OutputData).Shake[0]);
                SetOutputValue(OutputId.P2_Shaker, ((OutputData)_OutputData).Shake[1]);
                SetOutputValue(OutputId.P3_Shaker, ((OutputData)_OutputData).Shake[2]);
                SetOutputValue(OutputId.P4_Shaker, ((OutputData)_OutputData).Shake[3]);

                SetOutputValue(OutputId.P1_WaterFire, ((OutputData)_OutputData).WaterPower[0]);
                SetOutputValue(OutputId.P2_WaterFire, ((OutputData)_OutputData).WaterPower[1]);
                SetOutputValue(OutputId.P3_WaterFire, ((OutputData)_OutputData).WaterPower[2]);
                SetOutputValue(OutputId.P4_WaterFire, ((OutputData)_OutputData).WaterPower[3]);

                //SetOutputValue(OutputId.WaterPump, _OutputData.WaterPump);

                if (((OutputData)_OutputData).IsPlaying[0] == 1)
                {
                    SetOutputValue(OutputId.P1_Life, (int)((OutputData)_OutputData).Life[0]);
                    SetOutputValue(OutputId.P1_CtmRecoil, (int)((OutputData)_OutputData).Recoil[0]);
                    SetOutputValue(OutputId.P1_Damaged, (int)((OutputData)_OutputData).Damaged[0]);
                }
                else
                {
                    SetOutputValue(OutputId.P1_Life, 0);
                }

                if (((OutputData)_OutputData).IsPlaying[1] == 1)
                {
                    SetOutputValue(OutputId.P2_Life, (int)((OutputData)_OutputData).Life[1]);
                    SetOutputValue(OutputId.P2_CtmRecoil, (int)((OutputData)_OutputData).Recoil[1]);
                    SetOutputValue(OutputId.P2_Damaged, (int)((OutputData)_OutputData).Damaged[1]);
                }
                else
                {
                    SetOutputValue(OutputId.P2_Life, 0);
                }

                if (((OutputData)_OutputData).IsPlaying[2] == 1)
                {
                    SetOutputValue(OutputId.P3_Life, (int)((OutputData)_OutputData).Life[2]);
                    SetOutputValue(OutputId.P3_CtmRecoil, (int)((OutputData)_OutputData).Recoil[2]);
                    SetOutputValue(OutputId.P3_Damaged, (int)((OutputData)_OutputData).Damaged[2]);
                }
                else
                {
                    SetOutputValue(OutputId.P3_Life, 0);
                }

                if (((OutputData)_OutputData).IsPlaying[3] == 1)
                {
                    SetOutputValue(OutputId.P4_Life, (int)((OutputData)_OutputData).Life[3]);
                    SetOutputValue(OutputId.P4_CtmRecoil, (int)((OutputData)_OutputData).Recoil[3]);
                    SetOutputValue(OutputId.P4_Damaged, (int)((OutputData)_OutputData).Damaged[3]);
                }
                else
                {
                    SetOutputValue(OutputId.P4_Life, 0);
                }

                SetOutputValue(OutputId.P1_Credits, (int)((OutputData)_OutputData).Credits[0]);
                SetOutputValue(OutputId.P2_Credits, (int)((OutputData)_OutputData).Credits[1]);
                SetOutputValue(OutputId.P3_Credits, (int)((OutputData)_OutputData).Credits[2]);
                SetOutputValue(OutputId.P4_Credits, (int)((OutputData)_OutputData).Credits[3]);
            }
        }

        #endregion
    }
}

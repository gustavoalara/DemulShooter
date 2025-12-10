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
    public class Game_ArcadepcRha : Game__Unity
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
            public byte[] StartLamp = null;
            public byte[] Recoil = null;
            public byte[] Damaged = null;
            public float[] Life = null;
            public int[] Ammo = null;
            public int[] Credits = null;

            public OutputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }

        private const int MAX_PLAYERS = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        public Game_ArcadepcRha(String RomName)
            : base(RomName, "Game", "RabbidsShooter")
        {
            _KnownMd5Prints.Add("Rabbids Hollywood Arcade - Clean Dump", "2dac74521cd3bb08b61f93830bf2660d");
            _KnownMd5Prints.Add("Rabbids Hollywood Arcade - Patch by Ducon v2 (dongle+operator)", "72b58266f2d1311b2ba2e7c96ca774fa");
            _KnownMd5Prints.Add("Rabbids Hollywood Arcade - Patch by Ducon v3 (dongle+operator+attract mode)", "7edf14803ae7d43d14e7459b2baa651e");
            _KnownMd5Prints.Add("Rabbids Hollywood Arcade - Patch by Argon (dongle)", "1e74554181161f8a83084e02beeec5fc");

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
                SetOutputValue(OutputId.P1_LmpStart, ((OutputData)_OutputData).Recoil[0]);
                SetOutputValue(OutputId.P2_LmpStart, ((OutputData)_OutputData).Recoil[1]);
                SetOutputValue(OutputId.P3_LmpStart, ((OutputData)_OutputData).Recoil[2]);
                SetOutputValue(OutputId.P4_LmpStart, ((OutputData)_OutputData).Recoil[3]);
                SetOutputValue(OutputId.P1_CtmRecoil, ((OutputData)_OutputData).Recoil[0]);
                SetOutputValue(OutputId.P2_CtmRecoil, ((OutputData)_OutputData).Recoil[1]);
                SetOutputValue(OutputId.P3_CtmRecoil, ((OutputData)_OutputData).Recoil[2]);
                SetOutputValue(OutputId.P4_CtmRecoil, ((OutputData)_OutputData).Recoil[3]);
                SetOutputValue(OutputId.P1_Life, (int)((OutputData)_OutputData).Life[0]);
                SetOutputValue(OutputId.P2_Life, (int)((OutputData)_OutputData).Life[1]);
                SetOutputValue(OutputId.P3_Life, (int)((OutputData)_OutputData).Life[2]);
                SetOutputValue(OutputId.P4_Life, (int)((OutputData)_OutputData).Life[3]);
                SetOutputValue(OutputId.P1_Damaged, ((OutputData)_OutputData).Damaged[0]);
                SetOutputValue(OutputId.P2_Damaged, ((OutputData)_OutputData).Damaged[1]);
                SetOutputValue(OutputId.P3_Damaged, ((OutputData)_OutputData).Damaged[2]);
                SetOutputValue(OutputId.P4_Damaged, ((OutputData)_OutputData).Damaged[3]);
                SetOutputValue(OutputId.P1_Credits, ((OutputData)_OutputData).Credits[0]);
                SetOutputValue(OutputId.P2_Credits, ((OutputData)_OutputData).Credits[1]);
                SetOutputValue(OutputId.P3_Credits, ((OutputData)_OutputData).Credits[2]);
                SetOutputValue(OutputId.P4_Credits, ((OutputData)_OutputData).Credits[3]);
            }
        }

        #endregion
    }
}

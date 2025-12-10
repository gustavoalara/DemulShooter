using System;
using System.Collections.Generic;
using DsCore;
using DsCore.Config;
using DsCore.IPC;
using DsCore.MameOutput;

namespace DemulShooterX64.Games
{
    internal class Game_WndDcop : Game__Unity
    {
        private class InputData : Base_InputData
        {          
            public InputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }

        private class OutputData : Base_OutputData
        {
            public byte GunRecoil = 0;
            public byte DirectHit = 0;
            public byte Police_LightBar = 0;
            public byte GreenTestLight = 0;
            public byte RedLight = 0;
            public byte WhiteStrobe = 0;
            public byte GunLight = 0;
            public byte P1_Life = 0;
            public byte P1_Ammo = 0;

            public OutputData(int PlayerNumber) : base(PlayerNumber)
            {
            }
        }

        private const int MAX_PLAYERS = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        public Game_WndDcop(String RomName)
            : base(RomName, "DCOP", "DCOP")
        {
            _KnownMd5Prints.Add("DCOP - Tenoke ISO - Original", "3940b478b0069635b579c8bd2a6729c1");

            _InputData = new InputData(MAX_PLAYERS);
            _OutputData = new OutputData(MAX_PLAYERS);

            _tProcess.Start();
            Logger.WriteLog("Waiting for " + _RomName + " game to hook.....");
        }

        #region Outputs

        /// <summary>
        /// Create the Output list that we will be looking for and forward to MameHooker
        /// </summary>
        protected override void CreateOutputList()
        {
            _Outputs = new List<GameOutput>();
            _Outputs.Add(new GameOutput(OutputId.P1_GunRecoil));
            _Outputs.Add(new GameOutput(OutputId.Lmp_DirectHit));
            _Outputs.Add(new GameOutput(OutputId.Lmp_PoliceBar));
            _Outputs.Add(new GameOutput(OutputId.Lmp_GreenTestLight));
            _Outputs.Add(new GameOutput(OutputId.Lmp_RedLight));
            _Outputs.Add(new GameOutput(OutputId.Lmp_WhiteStrobe));
            _Outputs.Add(new GameOutput(OutputId.P1_LmpGun));
            _Outputs.Add(new AsyncGameOutput(OutputId.P1_CtmRecoil, Configurator.GetInstance().OutputCustomRecoilOnDelay, Configurator.GetInstance().OutputCustomRecoilOffDelay, 0));
            _Outputs.Add(new AsyncGameOutput(OutputId.P1_Damaged, Configurator.GetInstance().OutputCustomDamagedDelay, 100, 0));
            _Outputs.Add(new GameOutput(OutputId.P1_Ammo));
            _Outputs.Add(new GameOutput(OutputId.P1_Life));
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

                SetOutputValue(OutputId.P1_GunRecoil, ((OutputData)_OutputData).GunRecoil);
                SetOutputValue(OutputId.Lmp_DirectHit, ((OutputData)_OutputData).DirectHit);
                SetOutputValue(OutputId.Lmp_PoliceBar, ((OutputData)_OutputData).Police_LightBar);
                SetOutputValue(OutputId.Lmp_GreenTestLight, ((OutputData)_OutputData).GreenTestLight);
                SetOutputValue(OutputId.Lmp_RedLight, ((OutputData)_OutputData).RedLight);
                SetOutputValue(OutputId.Lmp_WhiteStrobe, ((OutputData)_OutputData).WhiteStrobe);
                SetOutputValue(OutputId.P1_LmpGun, ((OutputData)_OutputData).GunLight);
                SetOutputValue(OutputId.P1_Ammo, ((OutputData)_OutputData).P1_Ammo);
                SetOutputValue(OutputId.P1_Life, ((OutputData)_OutputData).P1_Life);

                SetOutputValue(OutputId.P1_CtmRecoil, ((OutputData)_OutputData).GunRecoil);

                if (_P1_LastLife > ((OutputData)_OutputData).P1_Life)
                    SetOutputValue(OutputId.P1_Damaged, 1);

                _P1_LastLife = ((OutputData)_OutputData).P1_Life;
            }
        }

        #endregion
    }
}

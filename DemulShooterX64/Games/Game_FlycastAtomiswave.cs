using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using DsCore;
using DsCore.MameOutput;
using DsCore.Config;

namespace DemulShooterX64
{
    public class Game_FlycastAtomiswave : Game_Flycast  
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Game_FlycastAtomiswave(String RomName) : base(RomName)
        { }

        #region Outputs

        /// <summary>
        /// Create the Output list that we will be looking for and forward to MameHooker
        /// </summary>
        protected override void CreateOutputList()
        {
            _Outputs = new List<GameOutput>();
            _Outputs.Add(new GameOutput(OutputId.P1_LmpStart));
            _Outputs.Add(new GameOutput(OutputId.P2_LmpStart));
            _Outputs.Add(new GameOutput(OutputId.P1_Ammo));
            _Outputs.Add(new GameOutput(OutputId.P2_Ammo));
            _Outputs.Add(new GameOutput(OutputId.P1_Clip));
            _Outputs.Add(new GameOutput(OutputId.P2_Clip));
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
            /*if (_RomName.Equals("claychal"))
                Compute_Confmiss_Outputs();
            else if (_RomName.Equals("sprtshot"))
                Compute_Deathcox_Outputs();             
            else if (_RomName.Equals("xtrmhunt"))
                Compute_Hotd2_Outputs(0x00096FA0);
            else if (_RomName.Equals("xtrmhnt2"))      
                Compute_Hotd2_Outputs(0x00096FA0);*/
            
        }

        #endregion
    }
}

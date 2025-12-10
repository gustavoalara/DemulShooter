using System;
using System.Collections.Generic;
using HarmonyLib;
using static SBK.Attribute.TimeSpanAttribute;

namespace BepInEx_DemulShooter_Plugin
{
    class mLanguageLocalizer
    {
        /// <summary>
        /// Available Languages : EN / FR / JA / ZH
        /// </summary>
        [HarmonyPatch(typeof(SBK.Localization.LanguageLocalizer), "SwitchLanguage")]
        class SwitchLanguage
        {
            static bool Prefix(ref SBK.Localization.LanguageCode i_Code, List<String> ___m_AvailableLanguages)
            {
                DemulShooter_Plugin.MyLogger.LogMessage("mLanguageLocalizer.SwitchLanguage(), i_Code=" + i_Code);
                DemulShooter_Plugin.MyLogger.LogMessage("mLanguageLocalizer.SwitchLanguage(), Available Languages :");  
                foreach (String s in ___m_AvailableLanguages)
                {
                    DemulShooter_Plugin.MyLogger.LogMessage(s);
                }

                DemulShooter_Plugin.MyLogger.LogMessage("mLanguageLocalizer.SwitchLanguage() : Trying to read custom config file...");
                switch (DemulShooter_Plugin.GameLanguage)
                {
                    case "EN":
                        {
                            i_Code = SBK.Localization.LanguageCode.EN;
                        }
                        break;
                    case "FR":
                        {
                            i_Code = SBK.Localization.LanguageCode.FR;
                        }
                        break;
                    case "JA":
                        {
                            i_Code = SBK.Localization.LanguageCode.JA;
                        }
                        break;
                    case "ZH":
                        {
                            i_Code = SBK.Localization.LanguageCode.ZH;
                        }
                        break;
                    default:
                        {
                            i_Code = SBK.Localization.LanguageCode.EN;
                        }
                        break;
                }

                return true;
            }
        }

    }
}

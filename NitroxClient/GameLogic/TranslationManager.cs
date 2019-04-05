using System;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class TranslationManager
    {
        private enum Languages 
        {

            English,
            German
        }

        private Languages currentLanguage = Languages.English; //default

        public TranslationManager()
        {

#if TRACE && TRANSLATION
            NitroxModel.Logger.Log.Debug("TranslationManager - current: " + Language.main.GetCurrentLanguage());
#endif

            try
            {
                currentLanguage = (Languages)Enum.Parse(typeof(Languages), Language.main.GetCurrentLanguage());
            }
            catch
            {
                Log.Error("Could not map current language to translation enum: " + Language.main.GetCurrentLanguage());
            }

        }

        internal string GetTranslation(string txt)
        {
            switch (currentLanguage)
            {
                case Languages.English:
                    return TranslateToEnglish(txt);
                case Languages.German:
                    return TranslateToGerman(txt);
                default:
                    return TranslateToEnglish(txt);
            }
        }

        private string TranslateToEnglish(string txt)
        {
            //current translation in code > ToDo: change to use .json or .xml source file for translated strings
            switch (txt)
            {
                case "txtInUseBy":
                    return "In use by";
                case "txtInConstructionBy":
                    return "Under construction by";
                default:
                    //fallback for unknown translation in master language > show hint at UI for issue creation
                    return "No Translation found for "+ txt;
            }
        }

        private string TranslateToGerman(string txt)
        {
            //current translation in code > ToDo: change to use .json or .xml source file for translated strings
            switch (txt)
            {
                case "txtInUseBy":
                    return "Wird verwendet von";
                case "txtInConstructionBy":
                    return "Wird hergestellt von";
                default:
                    //fallback for unknown translation in other languages > use the english translation
                    return TranslateToEnglish(txt);
            }
        }
    }
}

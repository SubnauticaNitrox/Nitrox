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
            German,
            Polish
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
                case Languages.Polish:
                    return TranslateToPolish(txt);
                default:
                    return TranslateToEnglish(txt);
            }
        }

        

        private string TranslateToEnglish(string txt)
        {
            //current translation in code > ToDo: change to use .json or .xml source file for translated strings
            switch (txt)
            {
                case "txtCrafterInUseBy {0}":
                    return "Fabricator in use by {0}";
                case "txtInUseBy {0}":
                    return "In use by {0}";
                case "txtInConstructionBy {0}":
                    return "Under construction by {0}";
                default:
                    //fallback for unknown translation in master language > show hint at UI for issue creation
                    return "No Translation found for "+ txt;
            }
        }

        private string TranslateToGerman(string txt)
        {
            switch (txt)
            {
                case "txtCrafterInUseBy {0}":
                    return "Fabrikator wird verwendet von {0}";
                case "txtInUseBy {0}":
                    return "Wird verwendet von {0}";
                case "txtInConstructionBy {0}":
                    return "Wird bearbeitet von {0}";
                default:
                    //fallback for unknown translation in other languages > use the english translation
                    return TranslateToEnglish(txt);
            }
        }

        private string TranslateToPolish(string txt)
        {
            switch (txt)
            {
                case "txtCrafterInUseBy {0}":
                    return "{0} używa fabrykatora";
                case "txtInUseBy {0}":
                    return "{0} tego używa";
                case "txtInConstructionBy {0}":
                    return "{0} tego używa";
                default:
                    //fallback for unknown translation in other languages > use the english translation
                    return TranslateToEnglish(txt);
            }
        }
    }
}

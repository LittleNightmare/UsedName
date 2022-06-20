using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace UsedName
{
    class Localization
    {
        public string currentLanguage = "en";
        private Dictionary<string, string> languageDict = new Dictionary<string, string> { };
        public Localization(string language = "en")
        {
            currentLanguage = language;
            this.LoadLanguage(language);
        }

        public string Localize(string message)
        {
            if (this.currentLanguage == "en") return message;
            if (languageDict.ContainsKey(message))
            {
                if (!string.IsNullOrEmpty(languageDict[message]))
                {
                    return languageDict[message];
                }
                languageDict.Add(message, "");
            }
            languageDict.Add(message, "");
            return message;
        }
        private void LoadLanguage(string language = "en")
        {
            if (language == "en") return;
            this.currentLanguage = language;
            var str = language switch{
                _ => Translations.zh_CN
            };
            this.languageDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);


        }

#if DEBUG
        public void StoreLanguage()
        {
            var str = JsonConvert.SerializeObject(this.languageDict);
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), this.currentLanguage + ".json"), str);
        }
#endif
    }
}
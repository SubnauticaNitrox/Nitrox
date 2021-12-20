using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LitJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nitrox.Test.Patcher.Patches
{
    [TestClass]
    public class Language_LoadLanguageFile_PatchTest
    {
        private string languageFolder, defaultFile;
        private JsonData defaultLanguage;

        [TestInitialize]
        public void Sanity()
        {
            languageFolder = Path.Combine(".", "LanguageFiles");
            Assert.IsTrue(Directory.Exists(languageFolder));

            defaultFile = Path.Combine(languageFolder, "English.json");
            Assert.IsTrue(File.Exists(defaultFile));

            using StreamReader streamReader = new(defaultFile);

            try
            {
                defaultLanguage = JsonMapper.ToObject(streamReader);
                Assert.IsTrue(defaultLanguage.Keys.Count > 0);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Unable to map default json file : {ex.Message}");
            }
        }

        [TestMethod]
        public void CheckInvalidFiles()
        {
            string[] files = Directory.GetFiles(languageFolder, "*.json");
            Assert.IsTrue(files.Length > 0);
            Assert.IsTrue(defaultLanguage.Keys.Count > 0);

            foreach (string file in files)
            {
                using StreamReader streamReader = new(file);

                JsonData json = JsonMapper.ToObject(streamReader);

                if (json.Keys.Count != defaultLanguage.Keys.Count)
                {
                    List<string> missingdiff = defaultLanguage.Keys.Where(key => !json.ContainsKey(key)).ToList();
                    List<string> addeddiff = json.Keys.Where(key => !defaultLanguage.ContainsKey(key)).ToList();

                    Assert.Inconclusive(
                        $"{file} structure is incorrect:\n'{string.Join(", ", missingdiff)}' keys are missing;\n '{string.Join(", ", addeddiff)}' keys shouldn't be here"
                    );
                }
            }
        }
    }
}

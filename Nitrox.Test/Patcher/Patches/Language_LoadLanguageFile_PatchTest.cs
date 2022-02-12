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

        public TestContext TestContext { get; set; }
        
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

            List<string> failedLanguageFiles = new();
            Lazy<HashSet<string>> defaultKeySet = new(() => defaultLanguage.Keys.ToHashSet());
            foreach (string file in files)
            {
                using StreamReader streamReader = new(file);

                JsonData json = JsonMapper.ToObject(streamReader);

                if (json.Keys.Count != defaultLanguage.Keys.Count)
                {
                    IEnumerable<string> missingKeys = defaultKeySet.Value.Except(json.Keys).ToArray();
                    IEnumerable<string> overflowingKeys = json.Keys.ToHashSet().Except(defaultKeySet.Value).ToArray();

                    string fileName = Path.GetFileName(file);
                    TestContext.WriteLine($"{fileName} structure is incorrect:");
                    if (missingKeys.Any())
                    {
                        TestContext.WriteLine($"\t - Missing keys: {string.Join(", ", missingKeys)}");
                    }
                    if (overflowingKeys.Any())
                    {
                        TestContext.WriteLine($"\t - Keys not in default language: {string.Join(", ", overflowingKeys)}");                        
                    }
                    
                    failedLanguageFiles.Add(file);
                }
            }

            if (failedLanguageFiles.Any())
            {
                Assert.Inconclusive($"Language files with problems: {string.Join(", ", failedLanguageFiles.Select(Path.GetFileName))}");
            }
        }
    }
}

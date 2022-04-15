using System;
using System.IO;
using LitJson;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NitroxPatcher.Patches.Persistent;

[TestClass]
public class Language_LoadLanguageFile_PatchTest
{
    [TestInitialize]
    public void DefaultLanguageSanity()
    {
        string languageFolder = Path.Combine(".", "LanguageFiles");
        Assert.IsTrue(Directory.Exists(languageFolder), $"The language files folder does not exist at {languageFolder}.");

        string defaultLanguageFilePath = Path.Combine(languageFolder, "English.json");
        Assert.IsTrue(File.Exists(defaultLanguageFilePath), $"The english language file does not exist at {defaultLanguageFilePath}.");

        using StreamReader streamReader = new(defaultLanguageFilePath);
        try
        {
            JsonData defaultLanguage = JsonMapper.ToObject(streamReader);
            Assert.IsTrue(defaultLanguage.Keys.Count > 0, $"The english language file has no entries.");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Unable to map default json file : {ex.Message}");
        }
    }
}

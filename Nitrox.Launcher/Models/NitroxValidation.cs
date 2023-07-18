using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Nitrox.Launcher.ViewModels;
using ReactiveUI.Validation.Extensions;

namespace Nitrox.Launcher.Models;

public static class NitroxValidation
{
    private static readonly char[] invalidPathCharacters = Path.GetInvalidFileNameChars();

    // TODO: Figure out how to use Validation in MVVM toolkit
    // public static void BindValidation(this CreateServerViewModel viewModel)
    // {
    //     viewModel.ValidationRule(vm => vm.Name, IsNotNullAndWhiteSpace, $"{nameof(viewModel.Name)} shouldn't be empty.");
    //     viewModel.ValidationRule(vm => vm.Name, IsValidFileName, $"{nameof(viewModel.Name)} shouldn't contain invalid characters.");
    // }
    //
    // public static void BindValidation(this ManageServerViewModel viewModel)
    // {
    //     viewModel.ValidationRule(vm => vm.ServerName, IsNotNullAndWhiteSpace, $"{nameof(viewModel.ServerName)} shouldn't be empty.");
    //     viewModel.ValidationRule(vm => vm.ServerName, IsValidFileName, $"{nameof(viewModel.ServerName)} shouldn't contain invalid characters.");
    //     // TODO: Validate that the name isn't a duplicate of another save
    //     viewModel.ValidationRule(vm => vm.ServerSeed, ContainsNoWhiteSpace, $"{nameof(viewModel.ServerSeed)} shouldn't contain any spaces.");
    //     viewModel.ValidationRule(vm => vm.ServerSeed, IsProperSeed, $"{nameof(viewModel.ServerSeed)} should contain 10 alphabetical characters.");
    //     viewModel.ValidationRule(vm => vm.ServerAutoSaveInterval, IsValidSaveInterval, $"{nameof(viewModel.ServerAutoSaveInterval)} should be between 10s and 24 hours (86400s).");
    //     viewModel.ValidationRule(vm => vm.ServerMaxPlayers, IsValidPlayerLimit, $"{nameof(viewModel.ServerMaxPlayers)} should be greater than 0.");
    //     viewModel.ValidationRule(vm => vm.ServerPort, IsValidPort, $"{nameof(viewModel.ServerPort)} should be between 0 and 65535");
    // }

    private static bool IsValidFileName(string s) => s == null || s.All(c => !invalidPathCharacters.Contains(c));
    private static bool IsNotNullAndWhiteSpace(string s) => !string.IsNullOrWhiteSpace(s);
    private static bool ContainsNoWhiteSpace(string s) => s == null || !Regex.IsMatch(s, @"\s");
    private static bool IsProperSeed(string s) => s == null || string.IsNullOrEmpty(s) || s.Length == 10 && Regex.IsMatch(s, @"^[a-zA-Z]+$");
    private static bool IsValidSaveInterval(int i) => i is >= 10 and <= 86400;
    private static bool IsValidPlayerLimit(int i) => i > 0;
    private static bool IsValidPort(int i) => i is >= 0 and <= 65535;
}

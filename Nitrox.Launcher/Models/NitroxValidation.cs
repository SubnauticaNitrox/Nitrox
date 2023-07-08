using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Nitrox.Launcher.ViewModels;
using ReactiveUI.Validation.Extensions;

namespace Nitrox.Launcher.Models;

public static class NitroxValidation
{
    private static readonly char[] invalidPathCharacters = Path.GetInvalidFileNameChars();

    public static void BindValidation(this CreateServerViewModel viewModel)
    {
        viewModel.ValidationRule(vm => vm.Name, IsNotNullAndWhiteSpace, $"{nameof(viewModel.Name)} shouldn't be empty.");
        viewModel.ValidationRule(vm => vm.Name, IsValidFileName, $"{nameof(viewModel.Name)} shouldn't contain invalid characters.");
    }

    public static void BindValidation(this ManageServerViewModel viewModel)
    {
        viewModel.ValidationRule(vm => vm.ServerName, IsNotNullAndWhiteSpace, $"{nameof(viewModel.ServerName)} shouldn't be empty.");
        viewModel.ValidationRule(vm => vm.ServerName, IsValidFileName, $"{nameof(viewModel.ServerName)} shouldn't contain invalid characters.");
        viewModel.ValidationRule(vm => vm.ServerName, ContainsNoWhiteSpace, $"{nameof(viewModel.ServerSeed)} shouldn't contain any spaces.");
    }

    private static bool IsValidFileName(string s) => s == null || s.All(c => !invalidPathCharacters.Contains(c));
    private static bool IsNotNullAndWhiteSpace(string s) => !string.IsNullOrWhiteSpace(s);
    private static bool ContainsNoWhiteSpace(string s) => !Regex.IsMatch(s, @"\s");
}

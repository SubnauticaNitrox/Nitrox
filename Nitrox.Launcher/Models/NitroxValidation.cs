using System.IO;
using System.Linq;
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
        // TODO: Fix these (they stopped working)
        viewModel.ValidationRule(vm => vm.Server.Name, IsNotNullAndWhiteSpace, $"{nameof(viewModel.Server.Name)} shouldn't be empty.");
        viewModel.ValidationRule(vm => vm.Server.Name, IsValidFileName, $"{nameof(viewModel.Server.Name)} shouldn't contain invalid characters.");
    }

    private static bool IsValidFileName(string s) => s == null || s.All(c => !invalidPathCharacters.Contains(c));
    private static bool IsNotNullAndWhiteSpace(string s) => !string.IsNullOrWhiteSpace(s);
}

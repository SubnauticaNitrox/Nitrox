using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Nitrox.Launcher.Models;

public enum PlayerPermissions
{
    [Display(Name="Player")]
    PLAYER,
    [Display(Name="Moderator")]
    MODERATOR,
    [Display(Name="Admin")]
    ADMIN
}

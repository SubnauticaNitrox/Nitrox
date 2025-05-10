using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NitroxModel.DataStructures;

namespace Nitrox.Server.Subnautica.Database.Models;

[Comment("Parsed batch cells")]
[Table("BatchCells")]
[PrimaryKey(nameof(Position))]
public record BatchCell
{
    public NitroxInt3 Position { get; set; }
}

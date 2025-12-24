using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class AutoSaveCommand : Command
    {
        private readonly IOptions<SubnauticaServerOptions> options;

        public AutoSaveCommand(IOptions<SubnauticaServerOptions> options) : base("autosave", Perms.ADMIN, "Toggles the map autosave")
        {
            AddParameter(new TypeBoolean("on/off", true, "Whether autosave should be on or off"));

            this.options = options;
        }

        protected override void Execute(CallArgs args)
        {
            options.Value.AutoSave = args.Get<bool>(0);
        }
    }
}

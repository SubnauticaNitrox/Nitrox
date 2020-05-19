using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.ConsoleCommands
{
    internal class DevCommand : Command
    {
        private readonly VehicleManager vehicleManager;

        public DevCommand(VehicleManager vehicleManager) : base("dev", Perms.ADMIN, "Become a Nitrox Wizard")
        {
            this.vehicleManager = vehicleManager;
            AddParameter(new TypeString("magicword", true));
        }

        protected override void Execute(CallArgs args)
        {
            string command = args.Get(0);

            switch (command)
            {
                case "vehicles_list":
                    SendMessage(args.Sender, "Vehicles list :");
                    vehicleManager.GetVehicles()
                        .ToList()
                        .ForEach(vm => SendMessage(args.Sender, vm.ToString() + "\n"));
                    break;

                case "vehicles_delete_all":
                    vehicleManager.GetVehicles()
                        .ToList()
                        .ForEach(vm => vehicleManager.RemoveVehicle(vm.Id));
                    SendMessage(args.Sender, "Done");
                    break;

                default:
                    SendMessage(args.Sender, "Not enough mana to summon this kind of magic");
                    break;
            }
        }
    }
}

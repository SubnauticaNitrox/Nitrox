using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.ConsoleCommands
{
    internal class DevCommand : Command
    {
        private readonly VehicleManager vehicleManager;

        public DevCommand(VehicleManager vehicleManager) : base("dev", Perms.CONSOLE, "Become a Nitrox Wizard", true)
        {
            this.vehicleManager = vehicleManager;
            AddParameter(new TypeString("magicword", true));
            AddParameter(new TypeString("field", false));
            AddParameter(new TypeString("value", false));
        }

        protected override void Execute(CallArgs args)
        {
            string command = args.Get(0);

            switch (command)
            {
                case "vehicles_list":
                    vehicleManager.GetVehicles()
                        .ToList()
                        .ForEach(vm => SendMessage(args.Sender, vm.ToString() + "\n"));
                    break;

                case "vehicles_delete_all":
                    vehicleManager.GetVehicles()
                        .ToList()
                        .ForEach(vm => vehicleManager.RemoveVehicle(vm.Id));
                    break;

                case "vehicles_reflectionset":
                    if (args.IsValid(1) && args.IsValid(2))
                    {
                        string field = args.Get(1);
                        int intvalue;
                        bool boolvalue;

                        if (int.TryParse(args.Get(2), out intvalue))
                        {
                            vehicleManager.GetVehicles().ToList().ForEach(vm => {
                                    try
                                    {
                                        vm.ReflectionCall(field, true, false, intvalue);
                                        SendMessage(args.Sender, "Done");
                                    }
                                    catch
                                    { }
                                });
                        }
                        else if (bool.TryParse(args.Get(2), out boolvalue))
                        {
                            vehicleManager.GetVehicles().ToList().ForEach(vm => {
                                try
                                {
                                    vm.ReflectionCall(field, true, false, boolvalue);
                                    SendMessage(args.Sender, "Done");
                                }
                                catch
                                { }
                            });
                        }
                    }
                    break;

                default:
                    SendMessage(args.Sender, "No enough mana to summon this kind of magic");
                    break;
            }
        }
    }
}

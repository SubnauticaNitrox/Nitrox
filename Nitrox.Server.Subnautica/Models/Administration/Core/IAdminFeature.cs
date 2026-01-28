namespace Nitrox.Server.Subnautica.Models.Administration.Core;

internal interface IAdminFeature;

internal interface IAdminFeature<T> : IAdminFeature where T : IAdminFeature<T>;

namespace Nitrox.Server.Subnautica.Models.Administration.Core;

/// <summary>
///     Implementors handle an administrative action.
/// </summary>
internal interface IAdminFeature<T> where T : IAdminFeature<T>;

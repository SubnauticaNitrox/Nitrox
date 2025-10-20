namespace Nitrox.Server.Subnautica.Models.Exceptions
{
    [Serializable]
    internal class DuplicateRegistrationException : Exception
    {
        public DuplicateRegistrationException()
        {
        }

        public DuplicateRegistrationException(string message) : base(message)
        {
        }
    }
}

namespace Aarhusvandsportscenter.Api.Domain.Services
{
    public class HashSalt
    {
        public HashSalt(string hash, byte[] salt)
        {
            Hash = hash;
            Salt = salt;
        }

        public string Hash { get; set; }
        public byte[] Salt { get; set; }
    }
}
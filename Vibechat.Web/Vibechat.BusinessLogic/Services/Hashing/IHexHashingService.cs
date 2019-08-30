namespace Vibechat.BusinessLogic.Services.Hashing
{
    public interface IHexHashingService
    {
        string Hash(byte[] value);

        string Hash(string value);
    }
}
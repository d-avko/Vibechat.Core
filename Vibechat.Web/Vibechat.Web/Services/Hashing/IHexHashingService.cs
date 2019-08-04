namespace Vibechat.Web.Services.Hashing
{
    public interface IHexHashingService
    {
        string Hash(byte[] value);

        string Hash(string value);
    }
}
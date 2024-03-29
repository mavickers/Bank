namespace LightPath.Bank.Interfaces
{
    public interface IBankAssetContentProcessor
    {
        byte[] Process(byte[] content);
    }
}

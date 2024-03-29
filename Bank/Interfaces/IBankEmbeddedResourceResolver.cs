namespace LightPath.Bank.Interfaces
{
    public interface IBankEmbeddedResourceResolver
    {
        /// <summary>
        /// Interface for adding resolvers for embedded resources.
        /// </summary>
        /// <param name="locator">
        /// The locator provided to the virtual path provider for the FileExists call.
        /// The locator can be a key, url, file path, etc.
        /// </param>
        /// <param name="resource">
        /// The BankEmbeddedResource to be evaluated.
        /// </param>
        /// <returns>bool</returns>
        bool IsResolving(string locator, BankEmbeddedResource resource);
    }
}

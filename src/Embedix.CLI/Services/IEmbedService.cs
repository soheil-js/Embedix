namespace Embedix.CLI.Services
{
    public interface IEmbedService
    {
        Task ExecuteAsync(EmbedRequest request, CancellationToken cancellationToken);
    }
}

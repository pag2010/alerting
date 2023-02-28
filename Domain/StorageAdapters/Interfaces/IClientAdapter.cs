namespace Alerting.Domain.StorageAdapters.Interfaces
{
    public interface IClientAdapter<TOutputClient, UInputClient> where TOutputClient : class
                                                                 where UInputClient:class
    {
        public TOutputClient GetClient(UInputClient client);
    }
}

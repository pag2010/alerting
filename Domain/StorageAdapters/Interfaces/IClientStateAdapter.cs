namespace Alerting.Domain.StorageAdapters.Interfaces
{
    public interface IClientStateAdapter<TOutputClientState, UInputClientState> where TOutputClientState : class
                                                                                where UInputClientState : class
    {
        public TOutputClientState GetClientRule(UInputClientState clientState);
    }
}

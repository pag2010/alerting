namespace Alerting.Domain.StorageAdapters.Interfaces
{
    public interface IClientRuleAdapter<TOutputClientRule, UInputClientRule> where TOutputClientRule : class
                                                                             where UInputClientRule : class
    {
        public TOutputClientRule GetClientRule(UInputClientRule clientRule);
    }
}

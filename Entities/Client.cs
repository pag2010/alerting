namespace Entities
{
    public class Client
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<ClientRule> Rules { get; set; }
        public ClientState State { get; set; }
    }
}
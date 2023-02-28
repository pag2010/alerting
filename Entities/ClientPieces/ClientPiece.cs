namespace Entities.ClientPieces
{
    public class ClientPiece
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public ClientPiece(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
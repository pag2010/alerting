using Microsoft.EntityFrameworkCore;

namespace Alerting.Domain.DataBase
{
    public class DataContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<TelegramBot> TelegramBots { get; set; }
        public DbSet<Alerting> Alertings { get; set; }
        public DbSet<ClientState> ClientStates { get; set; }
        public DbSet<StateType> StateTypes { get; set; }
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
    }
}

﻿using Alerting.Domain.DTO.Clients;
using System.Linq;
using System.Threading.Tasks;

namespace Alerting.Domain.Repositories.Interfaces
{
    public interface IClientRepository
    {
        Task<IQueryable<Client>> GetClientsAsync();
    }
}

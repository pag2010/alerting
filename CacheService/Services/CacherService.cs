using CacheService;
using Grpc.Core;

namespace CacheService.Services
{
    public class CacherService : Cacher.CacherBase
    {
        private readonly ILogger<CacherService> _logger;
        public CacherService(ILogger<CacherService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
    }
}
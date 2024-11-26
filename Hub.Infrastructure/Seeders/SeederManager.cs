using Microsoft.Extensions.Logging;

namespace Hub.Infrastructure.Seeders
{
    public interface ISeeder
    {
        Task<bool> SeedAsync();  
    }

    public class SeederManager
    {
        private readonly IEnumerable<ISeeder> _seeders;
        private readonly ILogger<SeederManager> _logger;
        private readonly int _maxParallelism;

        public SeederManager(IEnumerable<ISeeder> seeders, ILogger<SeederManager> logger, int maxParallelism = 4)
        {
            _seeders = seeders;
            _logger = logger;
            _maxParallelism = maxParallelism; 
        }

        public async Task SeedAllAsync()
        {
            var tasks = new List<Task<bool>>();

            foreach (var seeder in _seeders)
            {
                tasks.Add(SeedWithLogging(seeder));
            }

            var completedTasks = new List<Task<bool>>();
            var semaphore = new SemaphoreSlim(_maxParallelism);

            foreach (var task in tasks)
            {
                await semaphore.WaitAsync();  

                completedTasks.Add(ExecuteWithConcurrencyLimit(task, semaphore));
            }

            // Aguarda a conclusão de todas as tasks
            var results = await Task.WhenAll(completedTasks);

            // Se houver alguma falha, podemos tratar ou logar
            if (results.Contains(false))
            {
                _logger.LogError("Alguns seeders falharam durante a execução.");
            }
            else
            {
                _logger.LogInformation("Todos os seeders foram executados com sucesso.");
            }
        }

        private async Task<bool> SeedWithLogging(ISeeder seeder)
        {
            try
            {
                _logger.LogInformation($"Iniciando seeding de {seeder.GetType().Name}");
                var result = await seeder.SeedAsync();
                _logger.LogInformation(result ? $"Seeding de {seeder.GetType().Name} concluído com sucesso." : $"Seeding de {seeder.GetType().Name} falhou.");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao executar seeder {seeder.GetType().Name}");
                return false;
            }
        }

        private async Task<bool> ExecuteWithConcurrencyLimit(Task<bool> task, SemaphoreSlim semaphore)
        {
            try
            {
                var result = await task;
                return result;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}

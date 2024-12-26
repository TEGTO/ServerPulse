using AnalyzerApi.Command.BackgroundServices.ProcessLoadEvents;
using AnalyzerApi.Infrastructure;
using Confluent.Kafka;
using EventCommunication;
using MediatR;
using MessageBus.Interfaces;
using MessageBus.Models;
using Polly;
using Polly.Registry;
using Shared;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnalyzerApi.BackgroundServices
{
    public class LoadEventStatisticsProcessor : BackgroundService
    {
        private readonly IMessageConsumer messageConsumer;
        private readonly IMediator mediator;
        private readonly ILogger<LoadEventStatisticsProcessor> logger;

        private readonly ResiliencePipeline resiliencePipeline;
        private readonly string processTopic;
        private readonly int timeoutInMilliseconds;
        private readonly JsonSerializerOptions options;
        private readonly int batchSize;
        private readonly TimeSpan batchInterval;
        private readonly ConcurrentQueue<LoadEvent> batchQueue = new();

        public LoadEventStatisticsProcessor(
            IMessageConsumer messageConsumer,
            IMediator mediator,
            IConfiguration configuration,
            ResiliencePipelineProvider<string> resiliencePipelineProvider,
            ILogger<LoadEventStatisticsProcessor> logger)
        {
            this.messageConsumer = messageConsumer;
            this.mediator = mediator;
            this.logger = logger;

            processTopic = configuration[Configuration.KAFKA_LOAD_TOPIC_PROCESS]!;
            timeoutInMilliseconds = int.Parse(configuration[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]!);

            batchSize = int.Parse(configuration[Configuration.LOAD_EVENT_PROCESSING_BATCH_SIZE]!);
            batchInterval = TimeSpan.FromMilliseconds(int.Parse(configuration[Configuration.LOAD_EVENT_PROCESSING_BATCH_INTERVAL_IN_MILLISECONDS]!));

            options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                AllowTrailingCommas = false,
                ReadCommentHandling = JsonCommentHandling.Disallow,
            };

            resiliencePipeline = resiliencePipelineProvider.GetPipeline(Configuration.LOAD_EVENT_PROCESSING_RESILLIENCE);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("LoadEventStatisticsProcessor started.");

            using var batchTimer = new PeriodicTimer(batchInterval);

            var processingTask = Task.Run(async () =>
            {
                try
                {
                    await StartConsuming(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in message consumption loop.");
                }
            }, stoppingToken);

            try
            {
                while (await batchTimer.WaitForNextTickAsync(stoppingToken))
                {
                    await ProcessQueueAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException ex)
            {
                logger.LogInformation(ex, "Batch timer cancelled.");
            }

            await processingTask;

            logger.LogInformation("LoadEventStatisticsProcessor stopped.");
        }

        private async Task StartConsuming(CancellationToken stoppingToken)
        {
            await foreach (var response in messageConsumer.ConsumeAsync(processTopic, timeoutInMilliseconds, Offset.Stored, stoppingToken))
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                if (TryGetLoadEvent(response, out var ev))
                {
                    logger.LogInformation("Added LoadEvent to queue: {Key}", ev!.Key);
                    batchQueue.Enqueue(ev);

                    if (batchQueue.Count >= batchSize)
                    {
                        await ProcessQueueAsync(stoppingToken);
                    }
                }
                else
                {
                    logger.LogWarning("Failed to deserialize message: {Message}", response.Message);
                }
            }
        }

        private async Task ProcessQueueAsync(CancellationToken cancellationToken)
        {
            var batchSnapshot = new List<LoadEvent>();
            lock (batchQueue)
            {
                batchSnapshot = new List<LoadEvent>();
                while (batchQueue.TryDequeue(out var ev))
                {
                    batchSnapshot.Add(ev);
                    if (batchSnapshot.Count >= batchSize)
                        break;
                }
            }

            if (batchSnapshot.Any())
            {
                logger.LogInformation("Processing batch of size {BatchCount}.", batchSnapshot.Count);
                await ProcessBatchAsync(batchSnapshot, cancellationToken);
            }
        }

        private async Task ProcessBatchAsync(List<LoadEvent> batch, CancellationToken stoppingToken)
        {
            try
            {
                await resiliencePipeline.ExecuteAsync(async token =>
                {
                    logger.LogInformation("Processing batch of {BatchCount} LoadEvents...", batch.Count);

                    await mediator.Send(new ProcessLoadEventsCommand(batch.ToArray()), token);

                    logger.LogInformation("Successfully processed batch of {BatchCount} LoadEvents.", batch.Count);
                }, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing batch of LoadEvents.");
            }
        }

        private bool TryGetLoadEvent(ConsumeResponse response, out LoadEvent? loadEvent)
        {
            loadEvent = null;
            if (response.Message.TryToDeserialize(out LoadEvent? ev, options) && ev != null)
            {
                if (string.IsNullOrEmpty(ev.Endpoint) || string.IsNullOrEmpty(ev.Method) || ev.TimestampUTC == default)
                {
                    return false;
                }

                loadEvent = ev;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

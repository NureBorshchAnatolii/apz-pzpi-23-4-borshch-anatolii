using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Services;
using CareLink.Domain.Entities;

namespace CareLink.Application.Implementations
{
    public class RelativeMetricsService : IRelativeMetricsService
    {
        private readonly IRelativeRepository _relativeRepo;
        private readonly ICognitiveResultRepository _cognitiveRepo;
        private readonly IIoTDeviceRepository _deviceRepo;
        private readonly IMessageRepository _messageRepo;
        private readonly IIoTReadingRepository _readingRepo;

        public RelativeMetricsService(
            IRelativeRepository relativeRepo,
            ICognitiveResultRepository cognitiveRepo,
            IIoTDeviceRepository deviceRepo,
            IMessageRepository messageRepo,
            IIoTReadingRepository readingRepo)
        {
            _relativeRepo = relativeRepo;
            _cognitiveRepo = cognitiveRepo;
            _deviceRepo = deviceRepo;
            _messageRepo = messageRepo;
            _readingRepo = readingRepo;
        }

        public async Task<bool> IsRelativeOfAsync(long guardianUserId, long relativeUserId)
        {
            if (guardianUserId == relativeUserId)
                return true;

            return await _relativeRepo.ExistItemAsync(r =>
                r.GuardianUserId == guardianUserId &&
                r.RelativeUserId == relativeUserId);
        }

        public async Task<double> CalculateCognitiveReserveAsync(long guardianUserId, long relativeUserId)
        {
            await EnsureIsRelativeAsync(guardianUserId, relativeUserId);

            var results = await _cognitiveRepo.GetByUserIdAsync(relativeUserId);

            var cognitiveResults = results as List<CognitiveResult> ?? results.ToList();
            if (!cognitiveResults.Any()) return 0;

            double totalScore = cognitiveResults.Sum(r => r.Score * r.CognitiveExercise.DifficultyId);
            return totalScore / cognitiveResults.Count;
        }

        public async Task<double> CalculatePhysicalActivityDeclineAsync(long guardianUserId, long relativeUserId)
        {
            await EnsureIsRelativeAsync(guardianUserId, relativeUserId);

            var readings = await _readingRepo.GetLatestReadingsAsync(relativeUserId, 10);
            var ioTReadings = readings.ToList();
            if (!ioTReadings.Any()) return 0;

            double baseline = ioTReadings.Take(7).Average(r => r.ActivityLevel);
            double recent = ioTReadings.Skip(Math.Max(0, ioTReadings.Count - 7)).Average(r => r.ActivityLevel);

            if (baseline == 0) return 0;

            return (baseline - recent) / baseline;
        }

        public async Task<double> CalculateRestingHeartRateAsync(long guardianUserId, long relativeUserId)
        {
            await EnsureIsRelativeAsync(guardianUserId, relativeUserId);

            var readings = await _readingRepo.GetByUserIdAsync(relativeUserId);
            var restingReadings = readings
                .ToList();

            if (!restingReadings.Any()) return 0;

            return restingReadings.Average(r => r.Pulse);
        }

        public async Task<double> CalculateHeartRateVariabilityAsync(long guardianUserId, long relativeUserId)
        {
            await EnsureIsRelativeAsync(guardianUserId, relativeUserId);

            var readings = await _readingRepo.GetByUserIdAsync(relativeUserId);
            var pulses = readings.Select(r => (double)r.Pulse).ToList();

            if (!pulses.Any()) return 0;

            double avg = pulses.Average();
            double sumSquares = pulses.Sum(p => Math.Pow(p - avg, 2));

            return Math.Sqrt(sumSquares / pulses.Count);
        }

        public async Task<double> CalculateSocialIsolationIndexAsync(long guardianUserId, long relativeUserId)
        {
            await EnsureIsRelativeAsync(guardianUserId, relativeUserId);

            var sent = await _messageRepo.GetSentMessagesByUserAsync(guardianUserId);
            var received = await _messageRepo.GetReceivedMessagesByUserAsync(relativeUserId);

            int total = sent.Count() + received.Count();
            if (total == 0) return 1;

            return 1.0 - ((double)received.Count() / total);
        }
        
        private async Task EnsureIsRelativeAsync(long guardianUserId, long relativeUserId)
        {
            bool exists = await _relativeRepo.ExistItemAsync(r =>
                r.GuardianUserId == guardianUserId && r.RelativeUserId == relativeUserId);

            if (!exists)
                throw new UnauthorizedAccessException("This user is not your relative.");
        }
    }
}
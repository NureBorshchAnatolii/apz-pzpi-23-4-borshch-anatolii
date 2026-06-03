using CareLink.Application.Contracts.Repositories;
using CareLink.Application.Contracts.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace CareLink.Application.Implementations
{
    public class PdfService : IPdfService
    {
        private readonly IRelativeMetricsService _metricsService;
        private readonly IRelativeRepository _relativeRepo;
        private readonly IUserRepository _userRepo;


        public PdfService(
            IUserRepository userRepo,
            IRelativeRepository relativeRepo,
            IRelativeMetricsService metricsService)
        {
            _userRepo = userRepo;
            _relativeRepo = relativeRepo;
            _metricsService = metricsService;
        }

        public async Task<byte[]> GenerateRelativeReportAsync(long guardianUserId, long relativeUserId)
        {
            var isRelative = await _relativeRepo.ExistItemAsync(r =>
                r.GuardianUserId == guardianUserId && r.RelativeUserId == relativeUserId);
            if (!isRelative) throw new UnauthorizedAccessException("Not a relative");

            var relative = await _userRepo.GetByIdAsync(relativeUserId);
            if (relative == null) throw new Exception("Relative not found");

            var cognitiveReserve = await _metricsService.CalculateCognitiveReserveAsync(guardianUserId, relativeUserId);
            var physicalActivityDecline =
                await _metricsService.CalculatePhysicalActivityDeclineAsync(guardianUserId, relativeUserId);
            var restingHeartRate = await _metricsService.CalculateRestingHeartRateAsync(guardianUserId, relativeUserId);
            var heartRateVariability =
                await _metricsService.CalculateHeartRateVariabilityAsync(guardianUserId, relativeUserId);
            var socialIsolationIndex =
                await _metricsService.CalculateSocialIsolationIndexAsync(guardianUserId, relativeUserId);

            var relatives = await _relativeRepo.GetAllIncludedRelatives();
            var userRelatives = relatives
                .Where(r => r.RelativeUserId == relativeUserId)
                .Select(r => new { r.RelativeUser.FirstName, r.RelativeUser.LastName, r.RelationType.Name })
                .ToList();

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .AlignCenter()
                        .Text("Relative Health Report")
                        .FontSize(22)
                        .Bold()
                        .FontColor(Colors.Blue.Medium);

                    page.Content().Column(col =>
                    {
                        col.Spacing(20);

                        col.Item().Border(1).Padding(15).Column(card =>
                        {
                            card.Item().Text("User Information").Bold().FontSize(16);

                            card.Item().Text($"Full name: {relative.FirstName} {relative.LastName}");
                            card.Item().Text($"Email: {relative.Email}");
                            card.Item().Text($"Phone: {relative.PhoneNumber}");
                            card.Item().Text($"Date of birth: {relative.DateOdBirth:dd.MM.yyyy}");
                        });

                        col.Item().Border(1).Padding(15).Column(metrics =>
                        {
                            metrics.Item().Text("Health Metrics").Bold().FontSize(16);

                            metrics.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn();
                                    c.ConstantColumn(120);
                                });

                                void Row(string name, string value)
                                {
                                    table.Cell().Text(name);
                                    table.Cell().AlignRight().Text(value).Bold();
                                }

                                Row("Cognitive Reserve", cognitiveReserve.ToString("F2"));
                                Row("Physical Activity Decline", physicalActivityDecline.ToString("F2"));
                                Row("Resting Heart Rate", restingHeartRate.ToString("F2"));
                                Row("Heart Rate Variability", heartRateVariability.ToString("F2"));
                                Row("Social Isolation Index", socialIsolationIndex.ToString("F2"));
                            });
                        });

                        col.Item().Border(1).Padding(15).Column(rel =>
                        {
                            rel.Item().Text("Relatives").Bold().FontSize(16);

                            foreach (var r in userRelatives)
                                rel.Item().Text($"{r.FirstName} {r.LastName}");
                        });
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
                });
            }).GeneratePdf();

            return pdfBytes;
        }
    }
}
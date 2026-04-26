using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Automotive.Marketplace.Infrastructure.Services;

public class ContractPdfService : IContractPdfService
{
    public byte[] Generate(
        ContractCard card,
        ContractSellerSubmission seller,
        ContractBuyerSubmission buyer)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20, Unit.Millimetre);
                page.DefaultTextStyle(t => t.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text("TRANSPORTO PRIEMONIŲ PIRKIMO–PARDAVIMO SUTARTIS")
                        .Bold().FontSize(12).AlignCenter();
                    col.Item().Text($"Sudaryta: {card.AcceptedAt?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd")}")
                        .AlignCenter();
                });

                page.Content().Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Text("TRANSPORTO PRIEMONĖS DUOMENYS").Bold();
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                        void Row(string label, string value)
                        {
                            table.Cell().Text(label);
                            table.Cell().Text(value);
                        }

                        Row("Markė:", seller.Make);
                        Row("Komercinis pavadinimas:", seller.CommercialName);
                        Row("Valstybinis numeris:", seller.RegistrationNumber);
                        Row("Rida (km):", seller.Mileage.ToString("N0"));
                        Row("VIN kodas:", seller.Vin ?? "–");
                        Row("TP registracijos liudijimas:", seller.RegistrationCertificate ?? "–");
                        Row("SDK kodas:", seller.SdkCode ?? "–");
                        Row("Techninė apžiūra galioja:", seller.TechnicalInspectionValid ? "Taip" : "Ne");
                        Row("Buvo sugadinta:", seller.WasDamaged ? "Taip" : "Ne");
                        if (seller.WasDamaged)
                            Row("Žala žinoma:", seller.DamageKnown == true ? "Taip" : "Ne");
                        Row("Kaina (EUR):", seller.Price.HasValue ? seller.Price.Value.ToString("N2") : "–");
                    });

                    var defects = new List<string>();
                    if (seller.DefectBrakes) defects.Add("Stabdžių sistema");
                    if (seller.DefectSafety) defects.Add("Saugos sistemos");
                    if (seller.DefectSteering) defects.Add("Vairo ir pakabos sistema");
                    if (seller.DefectExhaust) defects.Add("Išmetimo sistema");
                    if (seller.DefectLighting) defects.Add("Apšvietimo sistema");

                    col.Item().Text($"Defektai: {(defects.Count > 0 ? string.Join(", ", defects) : "Nėra")}");
                    if (!string.IsNullOrWhiteSpace(seller.DefectDetails))
                        col.Item().Text($"Papildoma informacija: {seller.DefectDetails}");

                    col.Item().LineHorizontal(0.5f);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(s =>
                        {
                            s.Item().Text("PARDAVĖJAS").Bold();
                            s.Item().Text($"Vardas, pavardė: {seller.FullName}");
                            s.Item().Text($"Asmens / įmonės kodas: {seller.PersonalIdCode}");
                            s.Item().Text($"Adresas: {seller.Address}");
                            s.Item().Text($"Šalis: {seller.Country}");
                            s.Item().Text($"Telefonas: {seller.Phone}");
                            s.Item().Text($"El. paštas: {seller.Email}");
                            s.Item().Text($"Pateikta: {seller.SubmittedAt:yyyy-MM-dd HH:mm}");
                        });

                        row.ConstantItem(10);

                        row.RelativeItem().Column(b =>
                        {
                            b.Item().Text("PIRKĖJAS").Bold();
                            b.Item().Text($"Vardas, pavardė: {buyer.FullName}");
                            b.Item().Text($"Asmens / įmonės kodas: {buyer.PersonalIdCode}");
                            b.Item().Text($"Adresas: {buyer.Address}");
                            b.Item().Text($"Telefonas: {buyer.Phone}");
                            b.Item().Text($"El. paštas: {buyer.Email}");
                            b.Item().Text($"Pateikta: {buyer.SubmittedAt:yyyy-MM-dd HH:mm}");
                        });
                    });

                    col.Item().LineHorizontal(0.5f);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(s =>
                        {
                            s.Item().Text("Pardavėjo parašas: ________________________");
                            s.Item().Text($"Data: {seller.SubmittedAt:yyyy-MM-dd}");
                        });
                        row.ConstantItem(10);
                        row.RelativeItem().Column(b =>
                        {
                            b.Item().Text("Pirkėjo parašas: ________________________");
                            b.Item().Text($"Data: {buyer.SubmittedAt:yyyy-MM-dd}");
                        });
                    });
                });

                page.Footer()
                    .AlignCenter()
                    .Text(t =>
                    {
                        t.Span("Puslapis ");
                        t.CurrentPageNumber();
                        t.Span(" iš ");
                        t.TotalPages();
                    });
            });
        }).GeneratePdf();
    }
}

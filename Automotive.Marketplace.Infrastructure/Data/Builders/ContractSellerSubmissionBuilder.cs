using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class ContractSellerSubmissionBuilder
{
    private readonly Faker<ContractSellerSubmission> _faker;

    public ContractSellerSubmissionBuilder()
    {
        _faker = new Faker<ContractSellerSubmission>()
            .RuleFor(s => s.Id, f => f.Random.Guid())
            .RuleFor(s => s.ContractCardId, f => f.Random.Guid())
            .RuleFor(s => s.Make, f => f.Vehicle.Manufacturer())
            .RuleFor(s => s.CommercialName, f => f.Vehicle.Model())
            .RuleFor(s => s.RegistrationNumber, f => f.Random.AlphaNumeric(6).ToUpper())
            .RuleFor(s => s.Mileage, f => f.Random.Int(1000, 300000))
            .RuleFor(s => s.Vin, f => f.Vehicle.Vin())
            .RuleFor(s => s.TechnicalInspectionValid, true)
            .RuleFor(s => s.WasDamaged, false)
            .RuleFor(s => s.DamageKnown, _ => null)
            .RuleFor(s => s.DefectBrakes, false)
            .RuleFor(s => s.DefectSafety, false)
            .RuleFor(s => s.DefectSteering, false)
            .RuleFor(s => s.DefectExhaust, false)
            .RuleFor(s => s.DefectLighting, false)
            .RuleFor(s => s.Price, f => f.Random.Decimal(2000, 80000))
            .RuleFor(s => s.PersonalIdCode, f => f.Random.String2(11, "0123456789"))
            .RuleFor(s => s.FullName, f => f.Name.FullName())
            .RuleFor(s => s.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(s => s.Email, f => f.Internet.Email())
            .RuleFor(s => s.Address, f => f.Address.FullAddress())
            .RuleFor(s => s.Country, "Lietuva")
            .RuleFor(s => s.SubmittedAt, _ => DateTime.UtcNow)
            .RuleFor(s => s.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(s => s.CreatedBy, f => f.Random.Guid().ToString());
    }

    public ContractSellerSubmissionBuilder WithContractCard(Guid contractCardId)
    {
        _faker.RuleFor(s => s.ContractCardId, contractCardId);
        return this;
    }

    public ContractSellerSubmissionBuilder With<T>(Expression<Func<ContractSellerSubmission, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public ContractSellerSubmission Build() => _faker.Generate();
}

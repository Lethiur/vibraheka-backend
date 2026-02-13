using VibraHeka.Domain.Entities;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.IntegrationTests.Services.PaymentServiceTest;

[TestFixture]
public class RegisterOrderTest
{
    [Test]
    public async Task ShouldGenerateCheckoutUrlProperly()
    {
        var service = new PaymentService(new()
        {
            SecretKey = "c2tfdGVzdF81MVN5eE1UOHAyb3BRQVBUWGJjQllGM1ZibGFoV3p1T0VtQnBMVnVYUGltc1lna0NiNkpRWWl2ckRmUFdNSUxwb1QzRkJtSmxmUVJTMTh3OHRsNUxpTGRtNzAwMXFkTnZUOGg="
        });
        string crap = await service.RegisterOrder(new UserEntity()
        {
            Email = "mtesqtsdlc2@gmail.com"
        }, new OrderEntity()
        {
            ItemID = "price_1SyxN98p2opQAPTX1XXPGLIA",
            Quantity = 1,
            UserID = Guid.NewGuid().ToString()
        });
        
        Console.WriteLine(crap);
    }
}

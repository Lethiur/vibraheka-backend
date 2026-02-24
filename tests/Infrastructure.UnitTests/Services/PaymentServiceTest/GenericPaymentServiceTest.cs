using Moq;
using VibraHeka.Domain.Common.Interfaces.Payments;
using VibraHeka.Domain.Common.Interfaces.User;
using VibraHeka.Infrastructure.Services;

namespace VibraHeka.Infrastructure.UnitTests.Services.PaymentServiceTest;

public abstract class GenericPaymentServiceTest
{
    protected Mock<IPaymentRepository> _paymentRepositoryMock;
    protected Mock<IUserRepository> _userRepositoryMock;
    protected PaymentService _service;

    [SetUp]
    public void SetUp()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _service = new PaymentService(_paymentRepositoryMock.Object, _userRepositoryMock.Object);
    }
}

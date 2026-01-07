using Moq;
using NUnit.Framework;
using VibraHeka.Application.Admin.Queries.GetAllTherapists;
using VibraHeka.Domain.Common.Interfaces;
using VibraHeka.Domain.Common.Interfaces.User;

namespace VibraHeka.Application.UnitTests.Users.Queries.AdminGetTherapists;

[TestFixture]
public class GetAllTherapistsTest
{
    private Mock<IUserRepository> UserRepository;
    private Mock<IPrivilegeService> PrivilegeService;
    private Mock<ICurrentUserService> CurrentUserService;
    
    private GetAllTherapistsQueryHandler Handler;
    
    [SetUp]
    public void SetUp()
    {
        UserRepository = new Mock<IUserRepository>();
        PrivilegeService = new Mock<IPrivilegeService>();
        CurrentUserService = new Mock<ICurrentUserService>();
        
        Handler = new GetAllTherapistsQueryHandler(CurrentUserService.Object, PrivilegeService.Object,UserRepository.Object);
    }
    
    
    
}

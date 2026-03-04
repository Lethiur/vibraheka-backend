using CSharpFunctionalExtensions;
using MediatR;
using Moq;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Entities;

namespace VibraHeka.Infrastructure.UnitTests.Services.UserServiceTest
{
    public class UpdateUserAsyncTests : GenericUserServiceTest
    {
        [Test]
        public async Task ShouldReturnSuccessWhenEverythingGoesGood()
        {
            // Given: user exists and repo saves successfully
            var existingUser = new UserEntity
            {
                Id = "user-1",
                FirstName = "OldFirst",
                MiddleName = "OldMid",
                LastName = "OldLast",
                Bio = "Old bio",
                PhoneNumber = "000",
                LastModifiedBy = "someone",
                LastModified = DateTime.UtcNow.AddDays(-1)
            };

            var newUserData = new UserEntity
            {
                Id = "user-1",
                FirstName = "NewFirst",
                MiddleName = "NewMid",
                LastName = "NewLast",
                Bio = "New bio",
                PhoneNumber = "999"
            };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(
                    It.Is<string>(id => id == "user-1"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(existingUser));

            _userRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<UserEntity>()))
                .ReturnsAsync(Result.Success("ok"));

            // When: service is invoked
            var before = DateTime.UtcNow;
            Result<Unit> result = await _service.UpdateUserAsync(newUserData, "updater-1", CancellationToken.None);
            var after = DateTime.UtcNow;

            // Then: should return success
            Assert.That(result.IsSuccess, Is.True);

            // And: GetByIdAsync called with correct id + token
            _userRepositoryMock.Verify(r => r.GetByIdAsync(
                It.Is<string>(id => id == "user-1"),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)
            ), Times.Once);

            // And: AddAsync called with entity having updated fields
            _userRepositoryMock.Verify(r => r.AddAsync(
                It.Is<UserEntity>(u =>
                    u.Id == "user-1" &&
                    u.FirstName == "NewFirst" &&
                    u.MiddleName == "NewMid" &&
                    u.LastName == "NewLast" &&
                    u.Bio == "New bio" &&
                    u.PhoneNumber == "999" &&
                    u.LastModifiedBy == "updater-1" &&
                    u.LastModified >= before.AddSeconds(-2) &&
                    u.LastModified <= after.AddSeconds(2)
                )
            ), Times.Once);
        }

        [Test]
        public async Task ShouldFailWhenUserIdIsNull()
        {
            // Given: invalid input (null id)
            var newUserData = new UserEntity { Id = null! };

            // When: service is invoked
            Result<Unit> result = await _service.UpdateUserAsync(newUserData, "updater", CancellationToken.None);

            // Then: should return failure and not hit repo
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidUserID));

            _userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UserEntity>()), Times.Never);
        }

        [Test]
        public async Task ShouldFailWhenUserIdIsWhitespace()
        {
            // Given: invalid input (whitespace id)
            var newUserData = new UserEntity { Id = "   " };

            // When: service is invoked
            Result<Unit> result = await _service.UpdateUserAsync(newUserData, "updater", CancellationToken.None);

            // Then: should return failure and not hit repo
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.InvalidUserID));

            _userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UserEntity>()), Times.Never);
        }

        [Test]
        public async Task ShouldFailWhenUserNotFound()
        {
            // Given: repo returns success but user is null -> Ensure(user != null) should fail
            var newUserData = new UserEntity { Id = "missing" };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(
                    It.Is<string>(id => id == "missing"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success<UserEntity>(null!));

            // When: service is invoked
            Result<Unit> result = await _service.UpdateUserAsync(newUserData, "updater", CancellationToken.None);

            // Then: should fail with UserNotFound and not save
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo(UserErrors.UserNotFound));

            _userRepositoryMock.Verify(r => r.GetByIdAsync(
                It.Is<string>(id => id == "missing"),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)
            ), Times.Once);

            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UserEntity>()), Times.Never);
        }

        [Test]
        public async Task ShouldFailWhenGetByIdAsyncFails()
        {
            // Given: repo fails fetching user
            var newUserData = new UserEntity { Id = "user-1" };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(
                    It.Is<string>(id => id == "user-1"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<UserEntity>("DB read error"));

            // When: service is invoked
            Result<Unit> result = await _service.UpdateUserAsync(newUserData, "updater", CancellationToken.None);

            // Then: should fail and not save
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("DB read error"));           

            _userRepositoryMock.Verify(r => r.GetByIdAsync(
                It.Is<string>(id => id == "user-1"),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)
            ), Times.Once);

            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UserEntity>()), Times.Never);
        }

        [Test]
        public async Task ShouldFailWhenAddAsyncFails()
        {
            // Given: user exists but save fails
            var existingUser = new UserEntity { Id = "user-1" };
            var newUserData = new UserEntity { Id = "user-1", FirstName = "NewFirst" };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(
                    It.Is<string>(id => id == "user-1"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(existingUser));

            _userRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<UserEntity>()))
                .ReturnsAsync(Result.Failure<string>("DB write error"));

            // When: service is invoked
            Result<Unit> result = await _service.UpdateUserAsync(newUserData, "updater", CancellationToken.None);

            // Then: should fail
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("DB write error"));
            
            _userRepositoryMock.Verify(r => r.GetByIdAsync(
                It.Is<string>(id => id == "user-1"),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)
            ), Times.Once);

            _userRepositoryMock.Verify(r => r.AddAsync(It.Is<UserEntity>(u =>
                u.Id == "user-1" &&
                u.FirstName == "NewFirst" &&
                u.LastModifiedBy == "updater" &&
                u.LastModified != default
            )), Times.Once);
        }

        [Test]
        public async Task ShouldFailWhenGetByIdAsyncThrowsException()
        {
            // Given: repo throws exception -> BindTry should convert to failure
            var newUserData = new UserEntity { Id = "user-1" };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(
                    It.Is<string>(id => id == "user-1"),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Boom"));

            // When: service is invoked
            Result<Unit> result = await _service.UpdateUserAsync(newUserData, "updater", CancellationToken.None);

            // Then: should fail and not save
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Boom"));
            
            _userRepositoryMock.Verify(r => r.GetByIdAsync(
                It.Is<string>(id => id == "user-1"),
                It.Is<CancellationToken>(ct => ct == CancellationToken.None)
            ), Times.Once);

            _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UserEntity>()), Times.Never);
        }

        [Test]
        public async Task ShouldFailWhenAddAsyncThrowsException()
        {
            // Given: user exists but save throws -> BindTry should convert to failure
            var existingUser = new UserEntity { Id = "user-1" };
            var newUserData = new UserEntity { Id = "user-1", FirstName = "NewFirst" };

            _userRepositoryMock
                .Setup(r => r.GetByIdAsync(
                    It.Is<string>(id => id == "user-1"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(existingUser));

            _userRepositoryMock
                .Setup(r => r.AddAsync(It.IsAny<UserEntity>()))
                .ThrowsAsync(new Exception("Boom"));

            // When: service is invoked
            Result<Unit> result = await _service.UpdateUserAsync(newUserData, "updater", CancellationToken.None);

            // Then: should fail
            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("Boom"));

            _userRepositoryMock.Verify(r => r.AddAsync(It.Is<UserEntity>(u =>
                u.Id == "user-1" &&
                u.FirstName == "NewFirst" &&
                u.LastModifiedBy == "updater" &&
                u.LastModified != default
            )), Times.Once);
        }
    }
}

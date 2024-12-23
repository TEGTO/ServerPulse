﻿using Authentication.Models;
using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure;
using AutoMapper;

namespace AuthenticationApi.Tests
{
    [TestFixture]
    internal class AutoMapperProfileTests
    {
        private IMapper mapper;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());

            mapper = config.CreateMapper();
        }

        [Test]
        public void UserToUserRegistrationRequest_UserMappedCorrectly()
        {
            //Arrange
            var user = new User
            {
                UserName = "testuser",
                Email = "testuser@example.com",
                RefreshToken = "some-refresh-token",
                RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(1)
            };

            //Act
            var result = mapper.Map<UserRegistrationRequest>(user);

            //Assert
            Assert.That(result.Email, Is.EqualTo(user.Email));
        }

        [Test]
        public void UserRegistrationRequestToUser_UserRegistrationRequestMappedCorrectly()
        {
            //Arrange
            var request = new UserRegistrationRequest
            {
                Email = "testuser@example.com",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            //Act
            var result = mapper.Map<User>(request);

            //Assert
            Assert.That(result.Email, Is.EqualTo(request.Email));
        }

        [Test]
        public void AccessTokenDataToAuthToken_AccessTokenDataMappedCorrectly()
        {
            //Arrange
            var accessTokenData = new AccessTokenData
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            };

            //Act
            var result = mapper.Map<AccessTokenDataDto>(accessTokenData);

            //Assert
            Assert.That(result.AccessToken, Is.EqualTo(accessTokenData.AccessToken));
            Assert.That(result.RefreshToken, Is.EqualTo(accessTokenData.RefreshToken));
        }

        [Test]
        public void AuthTokenToAccessTokenData_AuthTokenMappedCorrectly()
        {
            //Arrange
            var authToken = new AccessTokenDataDto
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(1)
            };

            //Act
            var result = mapper.Map<AccessTokenData>(authToken);

            //Assert
            Assert.That(result.AccessToken, Is.EqualTo(authToken.AccessToken));
            Assert.That(result.RefreshToken, Is.EqualTo(authToken.RefreshToken));
        }

        [Test]
        public void UserUpdateDataRequestToUserUpdateData_UserUpdateDataRequestMappedCorrectly()
        {
            //Arrange
            var request = new UserUpdateDataRequest
            {
                Email = "new@example.com",
                OldPassword = "OldPassword123",
                Password = "NewPassword123"
            };

            //Act
            var result = mapper.Map<UserUpdateModel>(request);

            //Assert
            Assert.That(result.Email, Is.EqualTo(request.Email));
            Assert.That(result.OldPassword, Is.EqualTo(request.OldPassword));
            Assert.That(result.Password, Is.EqualTo(request.Password));
        }
    }
}

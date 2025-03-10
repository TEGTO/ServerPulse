﻿using Authentication.Models;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.ConfirmEmail;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.Login;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.RefreshToken;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.Register;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.UserUpdate;
using AuthenticationApi.Core.Dtos.Endpoints.OAuth.LoginOAuth;
using AuthenticationApi.Core.Entities;
using AuthenticationApi.Core.Models;
using AutoMapper;

namespace AuthenticationApi.Application.Validators.Tests
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
        public void UserToRegisterRequest_UserMappedCorrectly()
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
            var result = mapper.Map<RegisterRequest>(user);

            //Assert
            Assert.That(result.Email, Is.EqualTo(user.Email));
        }

        [Test]
        public void RegisterRequestToUser_RegisterRequestMappedCorrectly()
        {
            //Arrange
            var request = new RegisterRequest
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
        public void AccessTokenDataToRefreshTokenResponse_AccessTokenDataMappedCorrectly()
        {
            //Arrange
            var accessTokenData = new AccessTokenData
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            };

            //Act
            var result = mapper.Map<RefreshTokenResponse>(accessTokenData);

            //Assert
            Assert.That(result.AccessToken, Is.EqualTo(accessTokenData.AccessToken));
            Assert.That(result.RefreshToken, Is.EqualTo(accessTokenData.RefreshToken));
        }

        [Test]
        public void AccessTokenDataToLoginAccessTokenData_AccessTokenDataMappedCorrectly()
        {
            //Arrange
            var accessTokenData = new AccessTokenData
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            };

            //Act
            var result = mapper.Map<LoginAccessTokenData>(accessTokenData);

            //Assert
            Assert.That(result.AccessToken, Is.EqualTo(accessTokenData.AccessToken));
            Assert.That(result.RefreshToken, Is.EqualTo(accessTokenData.RefreshToken));
        }

        [Test]
        public void AccessTokenDataToLoginOAuthAccessTokenData_AccessTokenDataMappedCorrectly()
        {
            //Arrange
            var accessTokenData = new AccessTokenData
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            };

            //Act
            var result = mapper.Map<LoginOAuthAccessTokenData>(accessTokenData);

            //Assert
            Assert.That(result.AccessToken, Is.EqualTo(accessTokenData.AccessToken));
            Assert.That(result.RefreshToken, Is.EqualTo(accessTokenData.RefreshToken));
        }

        [Test]
        public void AccessTokenDataToRefreshTokenResponse_RefreshTokenResponseMappedCorrectly()
        {
            //Arrange
            var accessTokenData = new AccessTokenData
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(1)
            };

            //Act
            var result = mapper.Map<RefreshTokenResponse>(accessTokenData);

            //Assert
            Assert.That(result.AccessToken, Is.EqualTo(accessTokenData.AccessToken));
            Assert.That(result.RefreshToken, Is.EqualTo(accessTokenData.RefreshToken));
        }

        [Test]
        public void AccessTokenDataToConfirmEmailAccessTokenData_AccessTokenDataMappedCorrectly()
        {
            //Arrange
            var accessTokenData = new AccessTokenData
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            };

            //Act
            var result = mapper.Map<ConfirmEmailAccessTokenData>(accessTokenData);

            //Assert
            Assert.That(result.AccessToken, Is.EqualTo(accessTokenData.AccessToken));
            Assert.That(result.RefreshToken, Is.EqualTo(accessTokenData.RefreshToken));
        }

        [Test]
        public void RefreshTokenRequestToAccessTokenData_RefreshTokenRequestMappedCorrectly()
        {
            //Arrange
            var request = new RefreshTokenRequest
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(1)
            };

            //Act
            var result = mapper.Map<AccessTokenData>(request);

            //Assert
            Assert.That(result.AccessToken, Is.EqualTo(request.AccessToken));
            Assert.That(result.RefreshToken, Is.EqualTo(request.RefreshToken));
        }

        [Test]
        public void UserUpdateRequestToUserUpdateData_UserUpdateRequestMappedCorrectly()
        {
            //Arrange
            var request = new UserUpdateRequest
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

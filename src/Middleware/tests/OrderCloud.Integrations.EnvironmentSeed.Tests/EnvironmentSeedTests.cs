﻿using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using OrderCloud.Integrations.EnvironmentSeed.Models;

namespace Headstart.Tests
{
    public class EnvironmentSeedTests
    {
        [Test]
        [TestCase("Aa@101234")] // less than 10 characters
        [TestCase("Aa@10123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456")] // greater than 100 characters
        [TestCase("a@10123456")] // missing upper case
        [TestCase("A@10123456")] // missing lower case
        [TestCase("Aa@abcdefg")] // missing number
        [TestCase("Aa10123456")] // missing special character
        public void InitialAdminPassword_DoesNotMeetMinimumRequirements_FailsValidation(string password)
        {
            // Arrange
            var seed = new EnvironmentSeedRequest
            {
                Marketplace = new MarketplaceSettings
                {
                    InitialAdmin = new AdminAccountCredentials
                    {
                        Password = password,
                    },
                },
            };

            var ctx = new ValidationContext(seed.Marketplace.InitialAdmin)
            {
                MemberName = nameof(seed.Marketplace.InitialAdmin.Password),
            };

            // Act
            var result = Validator.TryValidateProperty(seed.Marketplace.InitialAdmin.Password, ctx, null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        [TestCase("Aa@1012345")]
        public void InitialAdminPassword_MeetsMinimumRequirements_PassesValidation(string password)
        {
            // Arrange
            var seed = new EnvironmentSeedRequest
            {
                Marketplace = new MarketplaceSettings
                {
                    InitialAdmin = new AdminAccountCredentials
                    {
                        Password = password,
                    },
                },
            };

            var ctx = new ValidationContext(seed.Marketplace.InitialAdmin)
            {
                MemberName = nameof(seed.Marketplace.InitialAdmin.Password),
            };

            // Act
            var result = Validator.TryValidateProperty(seed.Marketplace.InitialAdmin.Password, ctx, null);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        [TestCase("other")]
        public void Region_NotInValueRange_FailsValidation(string region)
        {
            // Arrange
            var seed = new EnvironmentSeedRequest
            {
                Marketplace = new MarketplaceSettings
                {
                    Region = region,
                },
            };

            var ctx = new ValidationContext(seed.Marketplace)
            {
                MemberName = nameof(seed.Marketplace.Region),
            };

            // Act
            var result = Validator.TryValidateProperty(seed.Marketplace.Region, ctx, null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        [TestCase("US-East")]
        [TestCase("Australia-East")]
        [TestCase("Europe-West")]
        [TestCase("Japan-East")]
        [TestCase("US-West")]
        public void Region_InValueRange_PassesValidation(string region)
        {
            // Arrange
            var seed = new EnvironmentSeedRequest
            {
                Marketplace = new MarketplaceSettings
                {
                    Region = region,
                },
            };

            var ctx = new ValidationContext(seed.Marketplace)
            {
                MemberName = nameof(seed.Marketplace.Region),
            };

            // Act
            var result = Validator.TryValidateProperty(seed.Marketplace.Region, ctx, null);

            // Assert
            Assert.IsTrue(result);
        }
    }
}

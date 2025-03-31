using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Xunit;


namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class SetPartnerPromoCodeLimitAsyncTests
    {
        //TODO: Add Unit Tests
        private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
        private readonly PartnersController _partnersController;
        private readonly Fixture _fixture;

        public SetPartnerPromoCodeLimitAsyncTests()
        {
            _fixture = (Fixture) new Fixture().Customize(new AutoMoqCustomization());
            _partnersRepositoryMock = _fixture.Freeze<Mock<IRepository<Partner>>>();
            _partnersController = _fixture.Build<PartnersController>().OmitAutoProperties().Create();
        }
        public static Partner CreateBasePartner()
        {
            var partner = new Partner()
            {
                Id = Guid.Parse("A3DB571C-A765-41F5-A49F-4FE50972DCDC"),
                Name = "СуперПуперПартнер",
                NumberIssuedPromoCodes = 100,
                IsActive = true,
                PartnerLimits = new List<PartnerPromoCodeLimit>()
                {
                    new PartnerPromoCodeLimit()
                    {
                        Id = Guid.Parse("e00633a5-978a-420e-a7d6-3e1dab116393"),
                        CreateDate = DateTime.Now.AddMonths(-1),
                        EndDate = DateTime.Now.AddMonths(1),
                        Limit = 100
                    }
                }
            };

            return partner;
        }

        public static IEnumerable<object[]> TestData()
        {
            yield return new object[] {
                CreateBasePartner(),
                DateTime.Now.AddMonths(2), // Дата окончания лимита    
                true // Ожидаем обнуление промокодов
            };
            yield return new object[] { 
                CreateBasePartner(),
                DateTime.Now.AddDays(-5), 
                false // Не ожидаем обнуления
            };
        }
        // Если партнер не найден, то также нужно выдать ошибку 404; 
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var partnerId = Guid.Parse("7d994823-8226-4273-b063-1a95f3cc1df8");
            Partner partner = null;
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);
            var request = _fixture.Create<SetPartnerPromoCodeLimitRequest>();
            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);
            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        //        Если партнер заблокирован, то есть поле IsActive = false в классе Partner, то также нужно выдать ошибку 400;
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotActive_ReturnsBadRequest()
        {
            // Arrange
            var partner = CreateBasePartner();
            partner.IsActive = false;
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);
            var request = _fixture.Create<SetPartnerPromoCodeLimitRequest>();
            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, request);
            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
        //Если партнеру выставляется лимит, то мы должны обнулить количество промокодов, которые партнер выдал NumberIssuedPromoCodes, если лимит закончился, то количество не обнуляется;
        [Theory]
        [MemberData(nameof(TestData))]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerLimitIsSet_NumberIssuedPromoCodesIsReset(
            Partner partner, DateTime endDate, bool expectReset)
        {
            // Arrange
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);
            // Act
            var request = _fixture.Build<SetPartnerPromoCodeLimitRequest>()
                                  .With(r => r.Limit, 100)
                                  .With(r => r.EndDate, endDate)
                                  .Create();
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, request);
            // Assert
            partner.NumberIssuedPromoCodes.Should().Be(expectReset?0:100);
        }
        //При установке лимита нужно отключить предыдущий лимит;
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerLimitIsSet_PreviousLimitIsDisabled()
        {
            // Arrange
            var partner = CreateBasePartner();
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);
            // Act
            var request = _fixture.Build<SetPartnerPromoCodeLimitRequest>().With(p =>p.EndDate,DateTime.Now.AddMonths(1)).Create();
            var previousLimit = partner.PartnerLimits.FirstOrDefault(x =>
                !x.CancelDate.HasValue);
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, request);
            // Assert
            previousLimit.CancelDate.Value.Date.Should().Be(DateTime.Now.Date);
        }
        //Лимит должен быть больше 0;
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_LimitIsLessThanZero_ReturnsBadRequest()
        {
            // Arrange
            var partner = CreateBasePartner();
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);
            // Act
            var request = _fixture.Build<SetPartnerPromoCodeLimitRequest>()
                .With(r => r.Limit,-1)
                .Create();
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, request);
            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
        //Нужно убедиться, что сохранили новый лимит в базу данных(это нужно проверить Unit-тестом);
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerLimitIsSet_SaveNewLimitToDatabase()
        {
            // Arrange
            var partner = CreateBasePartner();
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id))
                .ReturnsAsync(partner);
            // Act
            var request = _fixture.Create<SetPartnerPromoCodeLimitRequest>();
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, request);
            // Assert
            _partnersRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Partner>()), Times.Once);
        }

    }
}
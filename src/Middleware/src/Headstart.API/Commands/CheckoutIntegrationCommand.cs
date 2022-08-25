using System.Threading.Tasks;
using Headstart.Common.Commands;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using Headstart.Common.Services;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using ITaxCalculator = Headstart.Common.Services.ITaxCalculator;

namespace Headstart.API.Commands
{
    public interface ICheckoutIntegrationCommand
    {
        Task<ShipEstimateResponse> GetRatesAsync(HSOrderCalculatePayload orderCalculatePayload);

        Task<HSOrderCalculateResponse> CalculateOrder(HSOrderCalculatePayload orderCalculatePayload);

        Task<HSOrderCalculateResponse> CalculateOrder(string orderID, DecodedToken decodedToken);

        Task<ShipEstimateResponse> GetRatesAsync(string orderID);
    }

    public class CheckoutIntegrationCommand : ICheckoutIntegrationCommand
    {
        private readonly IShippingCommand shippingCommand;
        private readonly ITaxCalculator taxCalculator;
        private readonly ICurrencyConversionService currencyConversionService;
        private readonly IOrderCloudClient orderCloudClient;
        private readonly IDiscountDistributionService discountDistribution;

        public CheckoutIntegrationCommand(
            IDiscountDistributionService discountDistribution,
            ITaxCalculator taxCalculator,
            ICurrencyConversionService currencyConversionService,
            IOrderCloudClient orderCloudClient,
            IShippingCommand shippingCommand)
        {
            this.taxCalculator = taxCalculator;
            this.currencyConversionService = currencyConversionService;
            this.orderCloudClient = orderCloudClient;
            this.shippingCommand = shippingCommand;
            this.discountDistribution = discountDistribution;
        }

        public async Task<ShipEstimateResponse> GetRatesAsync(HSOrderCalculatePayload orderCalculatePayload)
        {
            var shipEstimateResponse = await shippingCommand.GetRatesAsync(orderCalculatePayload.OrderWorksheet, orderCalculatePayload.ConfigData);
            var buyerCurrency = orderCalculatePayload.OrderWorksheet.Order.xp.Currency ?? CurrencyCode.USD;
            await shipEstimateResponse.ShipEstimates.ConvertCurrency(CurrencyCode.USD, buyerCurrency, currencyConversionService);

            return shipEstimateResponse;
        }

        public async Task<ShipEstimateResponse> GetRatesAsync(string orderID)
        {
            var orderWorksheet = await orderCloudClient.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);
            var shipEstimateResponse = await shippingCommand.GetRatesAsync(orderWorksheet);
            var buyerCurrency = orderWorksheet.Order.xp.Currency ?? CurrencyCode.USD;
            await shipEstimateResponse.ShipEstimates.ConvertCurrency(CurrencyCode.USD, buyerCurrency, currencyConversionService);

            return shipEstimateResponse;
        }

        public async Task<HSOrderCalculateResponse> CalculateOrder(string orderID, DecodedToken decodedToken)
        {
            var worksheet = await orderCloudClient.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID, decodedToken.AccessToken);
            return await this.CalculateOrder(new HSOrderCalculatePayload()
            {
                ConfigData = null,
                OrderWorksheet = worksheet,
            });
        }

        public async Task<HSOrderCalculateResponse> CalculateOrder(HSOrderCalculatePayload orderCalculatePayload)
        {
            if (orderCalculatePayload.OrderWorksheet.Order.xp != null && orderCalculatePayload.OrderWorksheet.Order.xp.OrderType == OrderType.Quote)
            {
                // quote orders do not have tax cost associated with them
                return new HSOrderCalculateResponse();
            }
            else
            {
                var promotions = await orderCloudClient.Orders.ListAllPromotionsAsync(OrderDirection.All, orderCalculatePayload.OrderWorksheet.Order.ID);
                var promoCalculationTask = discountDistribution.SetLineItemProportionalDiscount(orderCalculatePayload.OrderWorksheet, promotions);
                var taxCalculationTask = taxCalculator.CalculateEstimateAsync(orderCalculatePayload.OrderWorksheet.Reserialize<HSOrderWorksheet>(), promotions);
                var taxCalculation = await taxCalculationTask;
                await promoCalculationTask;

                return new HSOrderCalculateResponse
                {
                    TaxTotal = taxCalculation.TotalTax,
                    xp = new OrderCalculateResponseXp()
                    {
                        TaxCalculation = taxCalculation,
                    },
                };
            }
        }
    }
}

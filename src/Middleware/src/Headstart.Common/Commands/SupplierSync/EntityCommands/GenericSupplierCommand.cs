using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Attributes;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    [SupplierSync("Generic")]
    public class GenericSupplierCommand : ISupplierSyncCommand
    {
        private readonly IOrderCloudClient ocSeller;

        public GenericSupplierCommand(IOrderCloudClient ocClient)
        {
            ocSeller = ocClient;
        }

        public async Task<JObject> GetOrderAsync(string id, OrderType orderType, DecodedToken decodedToken)
        {
            // TODO: BaseUrl cannot be found here
            var ocAuth = await ocSeller.AuthenticateAsync();
            HSShipEstimate estimate;
            HSShipMethod ship_method = null;
            HSOrderWorksheet supplierWorksheet = null;

            // Supplier worksheet will not exist on quote orders.
            try
            {
                supplierWorksheet = await ocSeller.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Outgoing, id, ocAuth.AccessToken);
            }
            catch (OrderCloudException)
            {
            }

            var salesOrderID = orderType == OrderType.Standard ? id.Split('-')[0] : id;
            var buyerWorksheet = await ocSeller.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, salesOrderID, ocAuth.AccessToken);
            var supplierID = supplierWorksheet?.Order?.ToCompanyID;
            if (buyerWorksheet.Order.xp?.OrderType == OrderType.Quote)
            {
                supplierID = buyerWorksheet.LineItems[0].SupplierID;
            }

            var buyerLineItems = buyerWorksheet.GetBuyerLineItemsBySupplierID(supplierID);
            if (buyerWorksheet?.ShipEstimateResponse != null && buyerWorksheet?.ShipEstimateResponse?.ShipEstimates.Count > 0)
            {
                estimate = buyerWorksheet.GetMatchingShipEstimate(supplierWorksheet?.LineItems?.FirstOrDefault()?.ShipFromAddressID);
                ship_method = estimate?.ShipMethods?.FirstOrDefault(m => m.ID == estimate.SelectedShipMethodID);
            }

            var returnObject = new JObject { };

            if (supplierWorksheet?.Order != null)
            {
                returnObject.Add(new JProperty("SupplierOrder", new JObject
                    {
                        { "Order", JToken.FromObject(supplierWorksheet?.Order) },
                        new JProperty("LineItems", JToken.FromObject(supplierWorksheet?.LineItems)),
                    }));
            }

            if (buyerWorksheet.Order != null)
            {
                returnObject.Add(new JProperty("BuyerOrder", new JObject
                    {
                        { "Order", JToken.FromObject(buyerWorksheet?.Order) },
                        new JProperty("LineItems", JToken.FromObject(buyerLineItems)),
                    }));

                // No supplier worksheet exists in these scenarios, just treat buyer order as supplier order.
                if (buyerWorksheet.Order.xp?.OrderType == OrderType.Quote)
                {
                    returnObject.Add(new JProperty("SupplierOrder", new JObject
                        {
                            { "Order", JToken.FromObject(buyerWorksheet?.Order) },
                            new JProperty("LineItems", JToken.FromObject(buyerWorksheet?.LineItems)),
                        }));
                }
            }

            if (ship_method != null)
            {
                returnObject.Add(new JProperty("ShipMethod", JToken.FromObject(ship_method)));
            }

            return JObject.FromObject(returnObject);
        }

        public Task<List<TemplateHydratedProduct>> ParseProductTemplate(IFormFile file, DecodedToken decodedToken)
        {
            throw new System.NotImplementedException();
        }
    }
}

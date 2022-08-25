using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Headstart.Common.Attributes;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    public class SupplierSyncCommand : ISupplierSyncCommand
    {
        private readonly IOrderCloudClient oc;

        public SupplierSyncCommand(IOrderCloudClient oc)
        {
            this.oc = oc;
        }

        public async Task<JObject> GetOrderAsync(string id, OrderType orderType, DecodedToken decodedToken)
        {
            var me = await oc.Me.GetAsync(accessToken: decodedToken.AccessToken);

            // Quote orders often won't have a hyphen in their order IDs, so allowing ID to be a fallback. This value is determined subsequently for quotes.
            var supplierID = id.Split("-").Length > 1 ? id.Split("-")[1] : id;
            if (orderType != OrderType.Quote)
            {
                Require.That(decodedToken.CommerceRole == CommerceRole.Seller || supplierID == me.Supplier.ID, new ErrorCode("Unauthorized", $"You are not authorized view this order", HttpStatusCode.Unauthorized));
            }

            try
            {
                var type =
                    Assembly.GetExecutingAssembly().GetTypeByAttribute<SupplierSyncAttribute>(attribute => attribute.SupplierID == supplierID) ??
                    Assembly.GetExecutingAssembly().GetTypeByAttribute<SupplierSyncAttribute>(attribute => attribute.SupplierID == "Generic");
                if (type == null)
                {
                    throw new MissingMethodException($"Command for {supplierID} is unavailable");
                }

                var command = (ISupplierSyncCommand)Activator.CreateInstance(type);
                var method = command.GetType().GetMethod($"GetOrderAsync", BindingFlags.Public | BindingFlags.Instance);
                if (method == null)
                {
                    throw new MissingMethodException($"Get Order Method for {supplierID} is unavailable");
                }

                return await (Task<JObject>)method.Invoke(command, new object[] { id, orderType, decodedToken });
            }
            catch (MissingMethodException mex)
            {
                throw new Exception(JsonConvert.SerializeObject(new ApiError()
                {
                    Data = new { decodedToken, OrderID = id, OrderType = orderType },
                    ErrorCode = mex.Message,
                    Message = $"Missing Method for: {supplierID ?? "Invalid Supplier"}",
                }));
            }
        }
    }
}

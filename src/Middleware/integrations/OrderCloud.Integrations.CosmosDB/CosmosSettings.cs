﻿namespace OrderCloud.Integrations.CosmosDB
{
    public class CosmosSettings
    {
        public string DatabaseName { get; set; }

        public string EndpointUri { get; set; }

        public string PrimaryKey { get; set; }

        public int RequestTimeoutInSeconds { get; set; }
    }
}

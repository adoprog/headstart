﻿namespace OrderCloud.Integrations.Vertex.Models
{
    public enum VertexDiscountType
    {
        DiscountAmount,
        DiscountPercent,
    }

    public class VertexDiscount
    {
        public double discountValue { get; set; }

        public VertexDiscountType discountType { get; set; }

        public string userDefinedDiscountCode { get; set; }
    }
}

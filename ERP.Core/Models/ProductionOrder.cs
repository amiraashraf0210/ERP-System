namespace ERP.Core.Models
{
    public class ProductionOrder
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;

        // العميل والموديل
        public int? CustomerId { get; set; }
        public string? ModelName { get; set; }      // الموديل
        public double? RequestedQty { get; set; }   // الكمية المطلوبة (اختياري)

        // التواريخ
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? StartDate { get; set; }
        public DateTime? DeliveryDate { get; set; } // تاريخ التسليم المتوقع

        // نتائج الإنتاج
        public double ProducedWeight { get; set; }  // الوزن المنتج (كجم)
        public double WasteWeight { get; set; }     // الهالك (كجم)

        // الحالة
        public string Status { get; set; } = "InProgress"; // InProgress, Done, Delivered

        public string? Notes { get; set; }
        public int? CreatedByUserId { get; set; }

        // Navigation
        public Customer? Customer { get; set; }
        public User? CreatedBy { get; set; }

        // المواد الخام المستخدمة (بدون كميات إجبارية)
        public ICollection<ProductionMaterial> Materials { get; set; } = new List<ProductionMaterial>();
    }

    // مواد خام مرتبطة بأمر الإنتاج (Checklist)
    public class ProductionMaterial
    {
        public int Id { get; set; }
        public int ProductionOrderId { get; set; }
        public int RawMaterialId { get; set; }
        public double? Quantity { get; set; }  // اختياري
        public string? Notes { get; set; }

        // Navigation
        public ProductionOrder ProductionOrder { get; set; } = null!;
        public RawMaterial RawMaterial { get; set; } = null!;
    }
}

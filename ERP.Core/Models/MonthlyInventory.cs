using System;

namespace ERP.Core.Models
{
    public class MonthlyInventory
    {
        public int Id { get; set; }
        public int Month { get; set; } // e.g. 6
        public int Year { get; set; }  // e.g. 2026
        public DateTime Date { get; set; }
        
        public int GoodId { get; set; }
        public double SystemStock { get; set; } // رصيد النظام الدفتري
        public double ActualStock { get; set; } // رصيد الجرد الفعلي
        
        public double BeginningStock { get; set; } // رصيد أول الشهر
        public double Purchases { get; set; } // المشتريات خلال الشهر
        public double Sales { get; set; } // المبيعات خلال الشهر
        public double Consumption { get; set; } // الاستهلاك = أول الشهر + المشتريات - الفعلي
        
        public bool IsClosed { get; set; } // هل تم الإقفال؟
        
        // Navigation
        public Good? Good { get; set; }
    }
}

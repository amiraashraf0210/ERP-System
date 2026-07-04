namespace ERP.Core.Models
{
    // سجل الراتب الشهري لموظف
    public class PayrollRecord
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public double BaseSalary { get; set; }    // الراتب الأساسي
        public double WorkedDays { get; set; }    // أيام العمل الفعلية
        public double NetSalary { get; set; }     // الراتب الصافي المصروف
        public DateTime? PaidDate { get; set; }   // تاريخ الصرف (null = لم يُصرف بعد)
        public int? BoxTransactionId { get; set; } // ربط بحركة الخزينة
        public string? Notes { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Paid

        // Navigation
        public Employee? Employee { get; set; }
        public BoxTransaction? BoxTransaction { get; set; }
    }

    // سجل دوام الموظف (حضور/غياب/إجازة)
    public class EmployeeAttendance
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string Status { get; set; } = "Active"; // Active = شغّال, Left = مشي, Rejoined = رجع
        public DateTime? StatusDate { get; set; }      // تاريخ المغادرة أو الرجوع
        public string? Notes { get; set; }

        // Navigation
        public Employee? Employee { get; set; }
    }
}

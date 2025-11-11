namespace HospitalPortalWasm.Models;

public class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Document { get; set; } = "";
    public string FullName { get; set; } = "";
    public DateTime? BirthDate { get; set; }
}

public class Medication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public int Stock { get; set; }
}

public class Dispense
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public Guid MedicationId { get; set; }
    public int Quantity { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
}

public class LabTestType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}

public enum LabOrderStatus { Pending, Resulted }

public class LabOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public Guid TestTypeId { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public LabOrderStatus Status { get; set; } = LabOrderStatus.Pending;
    public string? ResultText { get; set; }
}

public class InvoiceItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Kind { get; set; } = ""; // "LAB" | "FARM"
    public Guid RefId { get; set; }
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
}

public class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public List<InvoiceItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Amount);
}

public class HospitalData
{
    public List<Patient> Patients { get; set; } = new();
    public List<Medication> Medications { get; set; } = new();
    public List<Dispense> Dispenses { get; set; } = new();
    public List<LabTestType> LabTestTypes { get; set; } = new();
    public List<LabOrder> LabOrders { get; set; } = new();
    public List<Invoice> Invoices { get; set; } = new();
}

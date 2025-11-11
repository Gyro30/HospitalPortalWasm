using Blazored.LocalStorage;
using HospitalPortalWasm.Models;

namespace HospitalPortalWasm.Services;

public class HospitalService : IHospitalService
{
    private const string Key = "hospital-data-v1";
    private readonly ILocalStorageService _storage;
    private HospitalData _db = new();

    public HospitalService(ILocalStorageService storage) { _storage = storage; }

    public async Task EnsureSeedAsync()
    {
        _db = await _storage.GetItemAsync<HospitalData>(Key) ?? new HospitalData();

        if (!_db.Patients.Any())
        {
            _db.Patients.AddRange(new[]
            {
                new Patient { Document = "12345678", FullName = "Ana Ruiz", BirthDate = new DateTime(1990,5,2) },
                new Patient { Document = "87654321", FullName = "Carlos Pérez", BirthDate = new DateTime(1987,11,21) },
            });
        }
        if (!_db.Medications.Any())
        {
            _db.Medications.AddRange(new[]
            {
                new Medication { Name = "Paracetamol 500 mg", Stock = 40 },
                new Medication { Name = "Amoxicilina 500 mg", Stock = 25 },
            });
        }
        if (!_db.LabTestTypes.Any())
        {
            _db.LabTestTypes.AddRange(new[]
            {
                new LabTestType { Code = "HB",  Name = "Hemoglobina",            Price = 12m },
                new LabTestType { Code = "GLU", Name = "Glucosa",                Price = 10m },
                new LabTestType { Code = "PCR", Name = "Proteína C Reactiva",    Price = 30m },
            });
        }

        await SaveAsync();
    }

    public Task SaveAsync() => _storage.SetItemAsync(Key, _db).AsTask();


    // Pacientes
    public Task<List<Patient>> GetPatientsAsync() =>
        Task.FromResult(_db.Patients.OrderBy(p => p.FullName).ToList());

    public async Task AddPatientAsync(Patient p)
    {
        _db.Patients.Add(p);
        await SaveAsync();
    }

    // Farmacia
    public Task<List<Medication>> GetMedicationsAsync() =>
        Task.FromResult(_db.Medications.OrderBy(m => m.Name).ToList());

    public async Task AddMedicationAsync(Medication m)
    {
        _db.Medications.Add(m);
        await SaveAsync();
    }

    public Task<List<Dispense>> GetDispensesAsync() =>
        Task.FromResult(_db.Dispenses.OrderByDescending(d => d.Date).ToList());

    public async Task DispenseAsync(Guid patientId, Guid medicationId, int qty)
    {
        var med = _db.Medications.First(x => x.Id == medicationId);
        if (qty <= 0 || qty > med.Stock) throw new InvalidOperationException("Cantidad inválida o sin stock.");

        med.Stock -= qty;
        _db.Dispenses.Add(new Dispense
        {
            PatientId = patientId,
            MedicationId = medicationId,
            Quantity = qty,
            Date = DateTime.Now
        });

        await SaveAsync();
    }

    // Laboratorio
    public Task<List<LabTestType>> GetTestTypesAsync() =>
        Task.FromResult(_db.LabTestTypes.OrderBy(t => t.Name).ToList());

    public async Task AddTestTypeAsync(LabTestType t)
    {
        _db.LabTestTypes.Add(t);
        await SaveAsync();
    }

    public Task<List<LabOrder>> GetLabOrdersAsync() =>
        Task.FromResult(_db.LabOrders.OrderByDescending(o => o.Date).ToList());

    public async Task CreateLabOrderAsync(Guid patientId, Guid testTypeId)
    {
        _db.LabOrders.Add(new LabOrder { PatientId = patientId, TestTypeId = testTypeId, Date = DateTime.Now });
        await SaveAsync();
    }

    public async Task ResultLabOrderAsync(Guid orderId, string resultText)
    {
        var o = _db.LabOrders.First(x => x.Id == orderId);
        o.Status = LabOrderStatus.Resulted;
        o.ResultText = resultText;
        await SaveAsync();
    }

    // Facturación
    public Task<List<Invoice>> GetInvoicesAsync() =>
        Task.FromResult(_db.Invoices.OrderByDescending(i => i.Date).ToList());

    public async Task<Invoice> CreateInvoiceAsync(Guid patientId, IEnumerable<Guid> labOrderIds, IEnumerable<Guid> dispenseIds)
    {
        var inv = new Invoice { PatientId = patientId, Date = DateTime.Now };

        foreach (var id in labOrderIds)
        {
            var o = _db.LabOrders.First(x => x.Id == id);
            var tt = _db.LabTestTypes.First(x => x.Id == o.TestTypeId);
            inv.Items.Add(new InvoiceItem
            {
                Kind = "LAB",
                RefId = o.Id,
                Description = $"Lab: {tt.Code} {tt.Name}",
                Amount = tt.Price
            });
        }

        foreach (var id in dispenseIds)
        {
            var d = _db.Dispenses.First(x => x.Id == id);
            var med = _db.Medications.First(x => x.Id == d.MedicationId);
            inv.Items.Add(new InvoiceItem
            {
                Kind = "FARM",
                RefId = d.Id,
                Description = $"Farmacia: {med.Name} x{d.Quantity}",
                Amount = 2.5m * d.Quantity // precio demo
            });
        }

        _db.Invoices.Add(inv);
        await SaveAsync();
        return inv;
    }

    // Historia
    public Task<List<(DateTime when, string text)>> GetHistoryAsync(Guid patientId)
    {
        var items = new List<(DateTime, string)>();

        items.AddRange(_db.LabOrders.Where(x => x.PatientId == patientId).Select(x =>
        {
            var tt = _db.LabTestTypes.First(t => t.Id == x.TestTypeId);
            var estado = x.Status == LabOrderStatus.Pending ? "pendiente" : $"resultado: {x.ResultText}";
            return (x.Date, $"Laboratorio: {tt.Code} {tt.Name} ({estado})");
        }));

        items.AddRange(_db.Dispenses.Where(x => x.PatientId == patientId).Select(x =>
        {
            var m = _db.Medications.First(m => m.Id == x.MedicationId);
            return (x.Date, $"Farmacia: {m.Name} x{x.Quantity}");
        }));

        items.AddRange(_db.Invoices.Where(x => x.PatientId == patientId).Select(x =>
            (x.Date, $"Factura: {x.Id.ToString()[..8]} Total S/. {x.Total:0.00}")));

        return Task.FromResult(items.OrderByDescending(i => i.Item1).ToList());
    }
}

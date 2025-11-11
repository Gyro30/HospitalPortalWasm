using HospitalPortalWasm.Models;

namespace HospitalPortalWasm.Services;

public interface IHospitalService
{
    Task EnsureSeedAsync();
    Task SaveAsync();

    // Pacientes
    Task<List<Patient>> GetPatientsAsync();
    Task AddPatientAsync(Patient p);

    // Farmacia
    Task<List<Medication>> GetMedicationsAsync();
    Task AddMedicationAsync(Medication m);
    Task<List<Dispense>> GetDispensesAsync();
    Task DispenseAsync(Guid patientId, Guid medicationId, int qty);

    // Laboratorio
    Task<List<LabTestType>> GetTestTypesAsync();
    Task AddTestTypeAsync(LabTestType t);
    Task<List<LabOrder>> GetLabOrdersAsync();
    Task CreateLabOrderAsync(Guid patientId, Guid testTypeId);
    Task ResultLabOrderAsync(Guid orderId, string resultText);

    // Facturación
    Task<List<Invoice>> GetInvoicesAsync();
    Task<Invoice> CreateInvoiceAsync(Guid patientId, IEnumerable<Guid> labOrderIds, IEnumerable<Guid> dispenseIds);

    // Historia
    Task<List<(DateTime when, string text)>> GetHistoryAsync(Guid patientId);
}

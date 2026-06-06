public class EnrollmentWorker
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentWorker(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    public void ProcessBatch()
    {
        Console.WriteLine("Processing enrollment batch...");
    }
}
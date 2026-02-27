namespace Scheduly.Domain.Exceptions;

public class AppointmentOverlapException : DomainException
{
    public AppointmentOverlapException()
        : base("APPOINTMENT_OVERLAP", "An appointment already exists for this time slot.")
    {
    }
}

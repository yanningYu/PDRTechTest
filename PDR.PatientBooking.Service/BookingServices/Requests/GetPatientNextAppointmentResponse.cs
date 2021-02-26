using System;

namespace PDR.PatientBooking.Service.BookingServices.Requests
{
    public class GetPatientNextAppointmentResponse
    {
        public Guid Id {get;set;}
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string DoctorFirstName { get; set; }
        public string DoctorLastName { get; set; }
        public string DoctorEmail{get;set;}
    }
}

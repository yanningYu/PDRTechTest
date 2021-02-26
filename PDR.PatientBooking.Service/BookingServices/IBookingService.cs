using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PDR.PatientBooking.Service.BookingServices.Requests;

namespace PDR.PatientBooking.Service.BookingServices
{
    public interface IBookingService
    {
        void AddBooking(AddBookingRequest request);

        GetPatientNextAppointmentResponse CancelBookingAppointment(Guid id);
    }
}

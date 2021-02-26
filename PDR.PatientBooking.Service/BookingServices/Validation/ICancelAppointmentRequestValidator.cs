using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PDR.PatientBooking.Service.Validation;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public interface ICancelAppointmentRequestValidator
    {
        PdrValidationResult ValidateRequest(Guid id);
    }
}

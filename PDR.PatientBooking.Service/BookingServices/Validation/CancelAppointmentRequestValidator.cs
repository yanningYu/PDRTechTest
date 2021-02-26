using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.Validation;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class CancelAppointmentRequestValidator: ICancelAppointmentRequestValidator
    {
        private readonly PatientBookingContext _context;

        public CancelAppointmentRequestValidator(PatientBookingContext context)
        {
            _context = context;
        }
        public PdrValidationResult ValidateRequest(Guid id)
        {
            var result = new PdrValidationResult(true);

            if (NoFoundAppointment(id, ref result))
                return result;

            if (PastAppointment(id, ref result))
                return result;

            return result;
        }

        private bool NoFoundAppointment(Guid id, ref PdrValidationResult result)
        {
            if (!_context.Order.Any(order => order.Id == id))
            {
                result.PassedValidation = false;
                result.Errors.Add("There is no the appointment in our system.");
                return true;
            }

            return false;
        }

        private bool PastAppointment(Guid id, ref PdrValidationResult result)
        {
            if (!_context.Order.Any(order => order.Id == id && order.StartTime > DateTime.Now))
            {
                result.PassedValidation = false;
                result.Errors.Add("The patient can not cancel the past appointment.");
                return true;
            }

            return false;
        }
    }
}

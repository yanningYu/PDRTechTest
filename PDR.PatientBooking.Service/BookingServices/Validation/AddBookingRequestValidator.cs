using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Validation;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class AddBookingRequestValidator: IAddBookingRequestValidator
    {
        private readonly PatientBookingContext _context;

        public AddBookingRequestValidator(PatientBookingContext context)
        {
            _context = context;
        }
        public PdrValidationResult ValidateRequest(AddBookingRequest request)
        {
            var result = new PdrValidationResult(true);

            if (DoctorAlreadyBusy(request, ref result))
                return result;

            return result;
        }

        public bool DoctorAlreadyBusy(AddBookingRequest request, ref PdrValidationResult result)
        {
            if (_context.Order.Any(
                order => order.DoctorId == request.DoctorId 
                    && 
                    (
                        request.EndTime > order.StartTime && request.EndTime <= order.EndTime
                        ||  request.StartTime >= order.StartTime && request.StartTime < order.EndTime
                    ) ))
            {
                result.PassedValidation = false;
                result.Errors.Add("The doctor is really busy at the moment.");
                return true;
            }

            return false;
        }
    }
}

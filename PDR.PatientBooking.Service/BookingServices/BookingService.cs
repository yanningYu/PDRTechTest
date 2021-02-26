using System;
using System.Collections.Generic;
using System.Linq;

using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Validation;

namespace PDR.PatientBooking.Service.BookingServices
{
    public class BookingService: IBookingService
    {
        private readonly PatientBookingContext _context;

        private readonly IAddBookingRequestValidator _validator;

        private readonly ICancelAppointmentRequestValidator _cancelValidator;

        public BookingService(
            PatientBookingContext context, 
            IAddBookingRequestValidator validator,
            ICancelAppointmentRequestValidator cancelValidator)
        {
            this._context = context;
            this._validator = validator;
            _cancelValidator = cancelValidator;
        }

        public void AddBooking(AddBookingRequest request)
        {
            var validationResult = _validator.ValidateRequest(request);

            if (!validationResult.PassedValidation)
            {
                throw new ArgumentException(validationResult.Errors.First());
            }

            var bookingId = new Guid();
            var bookingStartTime = request.StartTime;
            var bookingEndTime = request.EndTime;
            var bookingPatientId = request.PatientId;
            var bookingPatient = this._context.Patient.FirstOrDefault(x => x.Id == request.PatientId);
            var bookingDoctorId = request.DoctorId;
            var bookingDoctor = this._context.Doctor.FirstOrDefault(x => x.Id == request.DoctorId);
            var bookingSurgeryType = this._context.Patient.FirstOrDefault(x => x.Id == bookingPatientId).Clinic.SurgeryType;

            var myBooking = new Order
            {
                Id = bookingId,
                StartTime = bookingStartTime,
                EndTime = bookingEndTime,
                PatientId = bookingPatientId,
                DoctorId = bookingDoctorId,
                Patient = bookingPatient,
                Doctor = bookingDoctor,
                SurgeryType = (int)bookingSurgeryType
            };

            this._context.Order.AddRange(new List<Order> { myBooking });
            this._context.SaveChanges();
        }

        public GetPatientNextAppointmentResponse CancelBookingAppointment(Guid id)
        {
            var validationResult = _cancelValidator.ValidateRequest(id);

            if (!validationResult.PassedValidation)
            {
                throw new ArgumentException(validationResult.Errors.First());
            }

            var canceledOrder = _context.Order.FirstOrDefault(order => order.Id == id);
            var patientId = canceledOrder.PatientId;

            _context.Remove(canceledOrder);
            _context.SaveChanges();

            var nextOrders = _context.Order.Where(order => order.PatientId == patientId && order.StartTime >= DateTime.Now);

            if (!nextOrders.Any())
            {
                return null;
            }

            var nextOrder = nextOrders.OrderBy(order => order.StartTime).FirstOrDefault();
            var response = new GetPatientNextAppointmentResponse
            {
                Id = nextOrder.Id,
                DoctorEmail = nextOrder.Doctor.Email,
                DoctorFirstName = nextOrder.Doctor.FirstName,
                DoctorLastName = nextOrder.Doctor.LastName,
                StartTime = nextOrder.StartTime,
                EndTime = nextOrder.EndTime
            };
            
            return response;
        }
    }
}

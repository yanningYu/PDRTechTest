using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoFixture;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using NUnit.Framework;

using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Validation;
using PDR.PatientBooking.Service.ClinicServices.Requests;
using PDR.PatientBooking.Service.PatientServices.Validation;

namespace PDR.PatientBooking.Service.Tests.BookingService.Validation
{
    public class AddBookingRequestValidatorTests
    {
        private IFixture _fixture;

        private PatientBookingContext _context;

        private AddBookingRequestValidator _addBookingRequestValidator;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _fixture = new Fixture();

            //Prevent fixture from generating from entity circular references 
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);


            // Sut instantiation
            _addBookingRequestValidator = new AddBookingRequestValidator(
                _context
            );
        }

        [Test]
        public void ValidateRequest_AllCheckPass_ReturnPassValidationResult()
        {
            //arrange
            var request = GetValidRequest();

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeTrue();
        }

        [Test]
        public void ValidateRequest_RequestStartTimeInDoctorWorkingTime_RequestFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();

            var existingOrder = _fixture
                .Build<Order>()
                .With(x => x.StartTime, request.StartTime.AddMinutes(30))
                .With(x => x.EndTime, request.EndTime.AddMinutes(30))
                .Create();

            _context.Add(existingOrder);
            _context.SaveChanges();

            request.DoctorId = existingOrder.DoctorId;

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("The doctor will be busy at the moment.");
        }

        [Test]
        public void ValidateRequest_RequestEndTimeInDoctorWorkingTime_RequestFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();

            var existingOrder = _fixture
                .Build<Order>()
                .With(x => x.StartTime, request.StartTime.AddMinutes(-30))
                .With(x => x.EndTime, request.EndTime.AddMinutes(-30))
                .Create();

            _context.Add(existingOrder);
            _context.SaveChanges();

            request.DoctorId = existingOrder.DoctorId;

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("The doctor will be busy at the moment.");
        }

        [Test]
        public void ValidateRequest_RequestTimeContainDoctorWorkingTime_RequestFailedValidationResult()
        {
            //arrange
            var request = GetValidRequest();

            var existingOrder = _fixture
                .Build<Order>()
                .With(x => x.StartTime, request.StartTime.AddMinutes(-30))
                .With(x => x.EndTime, request.EndTime.AddMinutes(30))
                .Create();

            _context.Add(existingOrder);
            _context.SaveChanges();

            request.DoctorId = existingOrder.DoctorId;

            //act
            var res = _addBookingRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("The doctor will be busy at the moment.");
        }

        [Test]
        public void ValidateRequest_AppointmentInPast_RequestFailedValidationResult()
        {
            //arrange
            var pastRequest = 
                _fixture.Build<AddBookingRequest>()
                    .With(booking => booking.StartTime, DateTime.Now.AddDays(-1))
                    .With(booking => booking.EndTime, DateTime.Now.AddDays(-1).AddHours(1))
                    .Create();

            //act
            var res = _addBookingRequestValidator.ValidateRequest(pastRequest);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("Patients can't book appointments in the past.");
        }

        private AddBookingRequest GetValidRequest()
        {
            var request = _fixture.Build<AddBookingRequest>()
                .With(booking => booking.StartTime, DateTime.Now.AddDays(1))
                .With(booking => booking.EndTime, DateTime.Now.AddDays(1).AddHours(1))
                .Create();
            return request;
        }
    }
}

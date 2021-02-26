using System;

using AutoFixture;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using NUnit.Framework;

using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Validation;

namespace PDR.PatientBooking.Service.Tests.BookingService.Validation
{
    [TestFixture]
    public class CancelAppointmentRequestValidatorTest
    {
        private IFixture _fixture;

        private PatientBookingContext _context;

        private CancelAppointmentRequestValidator _cancelAppointmentRequestValidator;

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
            _cancelAppointmentRequestValidator = new CancelAppointmentRequestValidator(
                _context
            );
        }

        [Test]
        public void ValidateRequest_AllCheckPass_ReturnPassValidationResult()
        {
            //arrange
            var request = Guid.NewGuid();

            var existingOrder = _fixture
                .Build<Order>()
                .With(x => x.StartTime, DateTime.Now.AddMinutes(30))
                .With(x => x.EndTime, DateTime.Now.AddMinutes(60))
                .With(x => x.Id, request)
                .Create();

            _context.Add(existingOrder);
            _context.SaveChanges();

            //act
            var res = _cancelAppointmentRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeTrue();
        }

        [Test]
        public void ValidateRequest_NotFoundOrder_RequestFailedValidationResult()
        {
            //arrange
            var request = Guid.NewGuid();

            var existingOrder = _fixture
                .Build<Order>()
                .With(x => x.StartTime, DateTime.Now.AddMinutes(30))
                .With(x => x.EndTime, DateTime.Now.AddMinutes(60))
                .Create();

            _context.Add(existingOrder);
            _context.SaveChanges();

            //act
            var res = _cancelAppointmentRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("There is no the appointment in our system.");
        }

        [Test]
        public void ValidateRequest_CancelPastOrder_RequestFailedValidationResult()
        {
            //arrange
            var request = Guid.NewGuid();

            var existingOrder = _fixture
                .Build<Order>()
                .With(x => x.StartTime, DateTime.Now.AddMinutes(-60))
                .With(x => x.EndTime, DateTime.Now.AddMinutes(-30))
                .With(x => x.Id, request)
                .Create();

            _context.Add(existingOrder);
            _context.SaveChanges();

            //act
            var res = _cancelAppointmentRequestValidator.ValidateRequest(request);

            //assert
            res.PassedValidation.Should().BeFalse();
            res.Errors.Should().Contain("The patient can not cancel the past appointment.");
        }
    }
}

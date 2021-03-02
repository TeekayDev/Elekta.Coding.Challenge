using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Elekta.Patient.Appointment.Services;
using Elekta.Patient.Appointment.Services.Models;
using System.Net;

namespace Elekta.Patient.Appointment
{
    public static class CreatePatientAppointment
    {
        private static PatientAppointmentInput input;

        [FunctionName("CreatePatientAppointment")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Create new patient appointment request received.");
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                if ((input  = GetInputParameters(requestBody, log)) == null)
                    return new BadRequestResult();

                var PatientAppointment = new PatientAppointmentBookingCreate(input.PatientID, input.AppointmentTime);
                if (!PatientAppointment.Save())
                    return new BadRequestObjectResult(PatientAppointment.Message);
                
                return new OkObjectResult("New appointment created");

            }
            catch (Exception ex)
            {
                log.LogError("Create new patient appointment request errored. " + ex);
                return new ObjectResult(500);
            }
            finally
            {
                log.LogInformation("Create new patient appointment request finished.");
            }
        }

        private static PatientAppointmentInput GetInputParameters(string requestBody, ILogger log)
        {
            try
            {
                return JsonConvert.DeserializeObject<PatientAppointmentInput>(requestBody);
            }
            catch (Exception ex)
            {
                log.LogError($"Invalid input: {requestBody}", ex);
            }

            return null;
        }
    }
}

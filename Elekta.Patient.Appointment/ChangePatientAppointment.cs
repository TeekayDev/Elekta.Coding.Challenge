using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Elekta.Patient.Appointment.Services.Models;
using Elekta.Patient.Appointment.Services;

namespace Elekta.Patient.Appointment
{
    public static class ChangePatientAppointment
    {
        private static PatientAppointmentInput input;

        [FunctionName("ChangePatientAppointment")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Change patient appointment request received.");
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                if ((input = GetInputParameters(requestBody, log)) == null)
                    return new BadRequestResult();

                var PatientAppointment = new PatientAppointmentBookingChange(input.PatientID, input.AppointmentTime, input.NewAppointmentTime);
                if (!PatientAppointment.Save())
                    return new BadRequestObjectResult(PatientAppointment.Message);

                return new OkObjectResult("Appointment changed.");

            }
            catch (Exception ex)
            {
                log.LogError("Change patient appointment request errored. " + ex);
                return new BadRequestResult();
            }
            finally
            {
                log.LogInformation("Change patient appointment request finished.");
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

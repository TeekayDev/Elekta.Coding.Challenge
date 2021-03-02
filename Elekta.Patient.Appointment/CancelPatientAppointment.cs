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
    public static class CancelPatientAppointment
    {
        private static PatientAppointmentInput input;

        [FunctionName("CancelPatientAppointment")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function,"post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Cancel patient appointment request received.");
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                if ((input = GetInputParameters(requestBody, log)) == null)
                    return new BadRequestResult();

                var PatientAppointment = new PatientAppointmentBookingCancel(input.PatientID, input.AppointmentTime);
                if (!PatientAppointment.Save())
                    return new BadRequestObjectResult(PatientAppointment.Message);

                return new OkObjectResult("Appointment Cancel.");

            }
            catch (Exception ex)
            {
                log.LogError("Cancel patient appointment request errored. " + ex);
                return new ObjectResult(500);
            }
            finally
            {
                log.LogInformation("Cancel patient appointment request finished.");
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

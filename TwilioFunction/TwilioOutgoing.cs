using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace TwilioFunctions
{
    public static class TwilioOutgoing
    {
        /// <summary>
        /// Initiates a simple outgoing phone call to a number.
        /// </summary>
        [FunctionName("SimpleOutgoing")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            //Acquire number to call from querystring/body
            string number = req.Query["number"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            number = number ?? data?.number;

            log.LogInformation(number);

            //Get Twilio account specific info from Environment Variables
            string accountSid = Environment.GetEnvironmentVariable("TwilioAccountSID");
            string authToken = Environment.GetEnvironmentVariable("TwilioAuthToken");
            string fromNumber = Environment.GetEnvironmentVariable("TwilioFromNumber");

            //Init Twilio
            TwilioClient.Init(accountSid, authToken);

            PhoneNumber to = new PhoneNumber(fromNumber);
            PhoneNumber from = new PhoneNumber(number);
            var call = CallResource.Create(to, from,
                url: new Uri("http://demo.twilio.com/docs/voice.xml"));

            log.LogInformation(call.Sid);

            return number != null
                ? (ActionResult)new OkObjectResult(call.Sid)
                : new BadRequestObjectResult("Please pass a number to call on the query string or in the request body");
        }
    }
}

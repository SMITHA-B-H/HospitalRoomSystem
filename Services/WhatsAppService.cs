using RestSharp;

namespace HospitalRoomAPI.Services
{
    public class WhatsAppService
    {
        private const string InstanceId = "instance176091";

        private const string Token = "f7owlkc98feize9t";

        public async Task SendTokenMessage(
            string phone,
            string patientName,
            int token,
            string doctor)
        {
            try
            {
                var client = new RestClient(
                    $"https://api.ultramsg.com/{InstanceId}/messages/chat"
                );

                var request = new RestRequest();

                request.Method = Method.Post;

                request.AddHeader(
                    "content-type",
                    "application/x-www-form-urlencoded"
                );

                request.AddParameter(
                    "token",
                    Token
                );

                request.AddParameter(
                    "to",
                    $"91{phone}@c.us"
                );

                request.AddParameter(
                    "body",
                    $"Hello {patientName}, Your token number is {token} for {doctor}. Please wait for your turn."
                );

                var response =
                    await client.ExecuteAsync(request);

                Console.WriteLine(response.Content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"WhatsApp Error: {ex.Message}"
                );
            }
        }
    }
}
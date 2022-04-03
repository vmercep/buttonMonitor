using System;
using System.Device.Gpio;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace buttonMonitor
{
    class Program
    {


        static int buttonPin = 10;

        static int litInMs = 3000;
        static GpioController controller = null;

        static string uri = "http://localhost:8000/api/StavkeRacuna/insertRacunFiskal";


        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {

            buttonPin = Convert.ToInt32( Environment.GetEnvironmentVariable("BUTTON"));

            litInMs= Convert.ToInt32(Environment.GetEnvironmentVariable("TIME_OUT"));

            uri = Environment.GetEnvironmentVariable("URI");

            //MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            //ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            //ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            //await ioTHubModuleClient.OpenAsync();
            //Console.WriteLine("IoT Hub module client initialized.");

            // Construct GPIO controller
            controller = new GpioController(PinNumberingScheme.Board);

            // Sets the button pin to input mode so we can read a value 
            controller.OpenPin(buttonPin, PinMode.InputPullDown);

            Console.WriteLine("Pins opened");
            Console.WriteLine($"Initial Button State: {controller.Read(buttonPin)}");
            var thread = new Thread(() => ThreadBody());
            thread.Start();
        }

        private static async void ThreadBody()
        {
            while (true)
            {


                //Console.WriteLine($"Button: {controller.Read(buttonPin)}");
                //await Task.Delay(1000);
                
                if (controller.Read(buttonPin)== true)
                {
                    Console.WriteLine($"Button: {controller.Read(buttonPin)}");

                    _ = Task.Run(async () =>
                      {
                          try
                          {
                              await PostJsonContent(uri);
                          }
                          catch (Exception e)
                          {
                              Console.WriteLine("Error in API call" + e.Message);
                          }

                      });
                    
                    
                    await Task.Delay(litInMs);
                    Console.WriteLine($"End!");
                }
                





            }
        }

        private static async Task PostJsonContent(string uri)
        {
            try
            {
                var client = new HttpClient();
                var postRequest = new HttpRequestMessage(HttpMethod.Post, uri)
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(uri),
                    Headers = {
                            { HttpRequestHeader.ContentType.ToString(), "application/json" }
                            },
                    Content = new StringContent("{ 'nacinPlacanja': 1,'iznos': 15,'userId': 1,'stavkeRacuna': [{ 'kolicina': 1, 'cijena': 15,'artiklId': 1 }]}", Encoding.UTF8, "application/json")
                };

                var postResponse = await client.SendAsync(postRequest);
                Console.WriteLine($"Response from WS: {postResponse}");
                postResponse.EnsureSuccessStatusCode();
            }
            catch(Exception e)
            {
                Console.WriteLine("Error in API request "+e.Message);
                throw new Exception(e.Message);
            }
        }
    }
}

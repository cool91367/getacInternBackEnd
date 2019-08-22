using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Confluent.Kafka;
using WebApplication.Models;
using WebApplication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.ProjectOxford.Face;




namespace SignalRChat.Hubs
{
    public class Image
    {
        public string Url { get; set; }
    }

    public class ChatHub : Hub
    {
        const string subscriptionKey = "20d99f21eb054633b2abb4890c9b90c4";

        const string uriBase ="https://japaneast.api.cognitive.microsoft.com/face/v1.0/detect";

        const string enpointUrl = "https://japaneast.api.cognitive.microsoft.com/face/v1.0";


        public async Task SendMessage(string message)
        {
            if (Clients != null)
            {
                await Clients.All.SendAsync("ReceiveMessage", message);
            }
        }

         public async Task UploadMessage(List<Chat> chats)
        {
            var config = new ProducerConfig { BootstrapServers = Constant.BrokerIP };
            foreach (Chat chat in chats)
            {
                using (var producer = new ProducerBuilder<string, string>(config).Build())
                {
                    try
                    {
                        var deliveryReport = await producer.ProduceAsync(
                            chat.Topic, new Message<string, string> { Key = chat.Id, Value = JsonConvert.SerializeObject(chat.Content)});
                    }
                    catch (ProduceException<string, string> e)
                    {
                        Console.WriteLine($"failed to deliver message: {e.Message} [{e.Error.Code}]");
                    }
                }
            }
        }

        /* One way to connect to the face api,sometimes it can't get the
           required faceid because the default model recognition_01
           and detection_01 is older and hence weaker.
           The api it called is defined in Microsoft.ProjectOxford.Face,
           however it isn't the way the tutorial shows.
           Not sure what's the difference between this and
           Microsoft.Azure.CognitiveServices.Vision.Face 2.2.0-preview
        */
        public async Task TestFaceVerifyUrl(string url1, string url2)
        {
            Console.WriteLine("Detect faces:");
            Console.WriteLine(url1);
            Console.WriteLine(url2);

            var uri = "https://japaneast.api.cognitive.microsoft.com/face/v1.0";

            var faceAttr = new [] {
            FaceAttributeType.Age, FaceAttributeType.Gender,
            FaceAttributeType.HeadPose, FaceAttributeType.Smile,
            //FaceAttributeType.Emotion,
            };

            var client = new FaceServiceClient(subscriptionKey, uri);

            var faces = await client.DetectAsync(url1, returnFaceAttributes: faceAttr);
            var faces2 = await client.DetectAsync(url2, returnFaceAttributes: faceAttr);

            /* The way to get the required FaceAttribute
            Console.WriteLine(faces.FirstOrDefault().FaceId);
            Console.WriteLine(faces.FirstOrDefault().FaceAttributes.Emotion.Happiness);
            */

            var verifyResult = await client.VerifyAsync(faces.FirstOrDefault().FaceId, faces2.FirstOrDefault().FaceId);

            string message = $"The verification result is：" +
                $"\n Same person：{verifyResult.IsIdentical} " +
                $"\n Confidence：{verifyResult.Confidence}";

            Console.WriteLine(message);

            await Clients.All.SendAsync("ReceiveFaceVerifyMessage", message);
        }

        public HttpClient HttpClientSetup()
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            return client;
        }

        public string UrlSetup(int mode)
        {
            string requestParameters = "";

            if (mode == 0)
            {
                requestParameters += "returnFaceId=true&returnFaceLandmarks=false" +
                "&recognitionModel=recognition_02" + "&detectionModel=detection_02";
            }
            else if (mode == 1)
            {
                requestParameters += "returnFaceId=true&returnFaceLandmarks=false" +
                "&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," +
                "emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";
            }

            string uri = uriBase + "?" + requestParameters;

            return uri;
        }

        public HttpContent HttpContentSetup(string url)
        {
            Image image = new Image() { Url = url };

            var json = JsonConvert.SerializeObject(image);
            Console.WriteLine(json);

            HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");

            return contentPost;
        }

        private async Task<string> FaceVerify(string url)
        {
            HttpClient client = HttpClientSetup();

            string uri = UrlSetup(0);

            HttpResponseMessage response;

            response = await client.PostAsync(uri, HttpContentSetup(url));

            string contentString = await response.Content.ReadAsStringAsync();

            return contentString;
        }

        public async Task SendFaceVerifyUrl(string url1, string url2)
        {
            Console.WriteLine(url1);
            Console.WriteLine(url2);

            string contentString1 = await FaceVerify(url1);

            Console.WriteLine("Response1:\n");
            Console.WriteLine(contentString1);

            string contentString2 = await FaceVerify(url2);

            Console.WriteLine("Response2:\n");
            Console.WriteLine(contentString2);

            string[] words1 = contentString1.Split('"');
            string[] words2 = contentString2.Split('"');

            var client2 = new FaceServiceClient(subscriptionKey, enpointUrl);
            var verifyResult = await client2.VerifyAsync(new Guid(words1[3]),new Guid(words2[3]));

            string message = $"Two Picture belongs to ：";

            if (verifyResult.IsIdentical)
            {
                message += "the same person\n";
                message +=  $"Confidence：{verifyResult.Confidence}";
            }
            else
            {
                message += "different person";
                message += $"Confidence：{verifyResult.Confidence}";
            }

            Console.WriteLine(message);
            await Clients.All.SendAsync("ReceiveFaceVerifyMessage", message);
        }
        
        private async Task<string> FaceCompare(string imageFilePath,int mode)
        {
            HttpClient client = HttpClientSetup();

            string uri = UrlSetup(0);

            HttpResponseMessage response;

            byte[] byteData = null;

            if (mode == 0)
            {
                // Request body. Posts a locally stored JPEG image.
                byteData = GetImageAsByteArray(imageFilePath);
            }
            else if (mode == 1)
            {
                byteData = Convert.FromBase64String(imageFilePath);

            }

            ByteArrayContent content = new ByteArrayContent(byteData);

            // This example uses content type "application/octet-stream".
            // The other content types you can use are "application/json"
            // and "multipart/form-data".
            content.Headers.ContentType =
                new MediaTypeHeaderValue("application/octet-stream");

            // Execute the REST API call.
            response = await client.PostAsync(uri, content);

            string contentString = await response.Content.ReadAsStringAsync();

            return contentString;
        }

        public async Task SendFaceCompareName(string name)
        {
            //Path dependent
            string imageFilePath1 = "/Users/schume/Desktop/";

            imageFilePath1 += name;

            string contentString1 = await FaceCompare(imageFilePath1,0);

            // Display the JSON response.
            Console.WriteLine("Response1:\n");
            Console.WriteLine(contentString1);

            string[] words1 = contentString1.Split('"');

            string MostMatchName  = "";

            double Match = 0;

            string PicFolder = "/Users/schume/Desktop/Picture";
            foreach (string PictureName in Directory.GetFiles(PicFolder))
            {
                Console.WriteLine(PictureName);

                if ( (!PictureName.EndsWith(".jpg")) && (!PictureName.EndsWith(".jpeg"))
                   &&(!PictureName.EndsWith(".png")) )
                {
                    Console.WriteLine("!!!!!!!" + PictureName);
                    continue;
                }

                string contentString2 = await FaceCompare(PictureName,0);

                Console.WriteLine("Response2:\n");
                Console.WriteLine(contentString2);

                string[] words2 = contentString2.Split('"');

                var client2 = new FaceServiceClient(subscriptionKey, enpointUrl);

                var verifyResult = await client2.VerifyAsync(new Guid(words1[3]),new Guid(words2[3]));

                Console.WriteLine("The verification result is：" +
                $"\n Same person：{verifyResult.IsIdentical}" +
                $"\n Confidence：{verifyResult.Confidence}");

                if (verifyResult.Confidence > Match)
                {
                    MostMatchName = "";
                    MostMatchName += PictureName;
                    Match = verifyResult.Confidence;
                }
            }

            string message = $"After searching the Database, the most matching picture is " + MostMatchName + "\n";

            if (Match > 0.5)
            {
            message += "They are the same person\n";
            message +=  $"Confidence：{Match}\n";
            }
            else
            {
            message += "They are different person\n";
            message += $"Confidence：{Match}\n";
            }

            Console.WriteLine(message);

            await Clients.All.SendAsync("ReceiveFaceCompareMessage", message,MostMatchName);
        }
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
        public async Task SendFaceVerifyByLocalImage(string url1 , string url2)
        {
            url1 = url1.Split(',')[1];
            string contentString1 = await FaceCompare(url1,1);
            Console.WriteLine("Response1:\n");
            Console.WriteLine(contentString1);

            string[] words1 = contentString1.Split('"');

            url2 = url2.Split(',')[1];
            string contentString2 = await FaceCompare(url2,1);
            Console.WriteLine("Response2:\n");
            Console.WriteLine(contentString2);

            string[] words2 = contentString2.Split('"');

            var client2 = new FaceServiceClient(subscriptionKey, enpointUrl);
            var verifyResult = await client2.VerifyAsync(new Guid(words1[3]),new Guid(words2[3]));

            string message = $"Two Picture belongs to ：";

            if (verifyResult.IsIdentical)
            {
                message += "the same person\n";
                message +=  $"Confidence：{verifyResult.Confidence}";
            }
            else
            {
                message += "different person";
                message += $"Confidence：{verifyResult.Confidence}";
            }

            Console.WriteLine(message);
            await Clients.All.SendAsync("ReceiveFaceVerifyMessage", message);
        }
    }
}
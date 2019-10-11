using Android.App;
using Android.Runtime;
using Android.InputMethodServices;
using Android.Views;
using Java.Lang;
using System;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Google.Cloud.Vision.V1;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.IO;
using Exception = System.Exception;
using System.Net.Http;
using System.Net.Http.Headers;

namespace XamarinKeyboard1
{
    [Service(Permission = "android.permission.BIND_INPUT_METHOD", Label = "EDMTKeyboard")]
	[MetaData("android.view.im", Resource = "@xml/method")]
	[IntentFilter(new string[] { "android.view.InputMethod" })]
	public class MainActivity : InputMethodService, KeyboardView.IOnKeyboardActionListener
	{
		private KeyboardView kv;
		private Keyboard keyboard;
        //static string subscriptionKey = "87e4754ee7274df3bfc170f76b779dc6";
        //static string endpoint = "https://westus.api.cognitive.microsoft.com/vision/v2.0/";
        //string _mode = "Printed";
        static string subscriptionKey = "87e4754ee7274df3bfc170f76b779dc6";
        static string endpoint = "https://westus.api.cognitive.microsoft.com/";
        static string uriBase = endpoint + "vision/v2.1/ocr";

        public override View OnCreateInputView()
		{
            kv = (KeyboardView)LayoutInflater.Inflate(Resource.Layout.Keyboard, null);
			keyboard = new Keyboard(this, Resource.Xml.Qwerty);
			kv.Keyboard = keyboard;
			
			kv.OnKeyboardActionListener = this;
			return kv;
		}
		
		public void OnKey([GeneratedEnum] Android.Views.Keycode primaryCode, [GeneratedEnum] Android.Views.Keycode[] keyCodes)
		{
			long eventTime = JavaSystem.CurrentTimeMillis();
			var ev = new KeyEvent(eventTime, eventTime, KeyEventActions.Down, primaryCode, 0, 0, 0, 0,
								  KeyEventFlags.SoftKeyboard | KeyEventFlags.KeepTouchMode);				
		}

        public void OnPress([GeneratedEnum] Android.Views.Keycode primaryCode)
        {
            string code = primaryCode.GetHashCode().ToString(); //Get hashcode of button pressed on keyboard
            switch(code)
            {
                case "0": //Back button
                    break;

                case "1": //Camera button
                    camera_onbuttonClick();
                    break;

                case "2": //Gallery button
                    gallery_onbuttonClick();
                    break;
            }
        }


        //private TextRecognitionMode RecognitionMode => (TextRecognitionMode)System.Enum.Parse(typeof(TextRecognitionMode), _mode);

        //Method to open camera on button click
        public async void camera_onbuttonClick() 
        {
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported) //check camera access on device
            {
                Console.Write("No Camera");
                return;
             }

            //Open camera and capture photo
             var photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions()
             {
                });

            //check if photo is captured or not
            if (photo != null)
            {
                try
                {
                    // CallGoogleVisionAPI(); //Call google cloud vision service

                    //1st approach
                    //// Create a client
                    //ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);
                    //using (Stream imageFileStream = File.OpenRead(photo.Path))
                    //{
                    //    var ocrResult = await client.RecognizeTextAsync(imageFileStream.ToString(), RecognitionMode);
                    //    string text = ocrResult.ToString();
                    
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception::" + ex);
                }
            }
        }

        //Method to pick image from gallery on button click
        public async void gallery_onbuttonClick()
        { 
            if (!CrossMedia.Current.IsPickPhotoSupported)//check gallery access 
            {
                Console.Write("Photos Not Supported");
                return;
            }
            var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
            {
                PhotoSize = PhotoSize.Medium
            });
            if (file == null)
                return;
            else
            {
                try
                {
                    //1st approach
                    //ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);
                    //using (var photoStream = file.GetStream())
                    //{
                    //    //var ocrResult = await client.RecognizeTextAsync(imageFileStream, RecognitionMode);
                    //    var ocrResult = await client.RecognizeTextInStreamAsync(photoStream, RecognitionMode);
                    //    string text = ocrResult.ToString();
                    //}

                    //2nd approach
                    HttpClient client = new HttpClient();
                    // Request headers.
                    client.DefaultRequestHeaders.Add(
                        "Ocp-Apim-Subscription-Key", subscriptionKey);

                    // Request parameters. 
                    // The language parameter doesn't specify a language, so the 
                    // method detects it automatically.
                    // The detectOrientation parameter is set to true, so the method detects and
                    // and corrects text orientation before detecting text.
                    string requestParameters = "language=unk&detectOrientation=true";

                    // Assemble the URI for the REST API method.
                    string uri = uriBase + "?" + requestParameters;

                    HttpResponseMessage response;

                    // Read the contents of the specified local image
                    // into a byte array.
                    byte[] byteData = GetImageAsByteArray(file.Path);

                    // Add the byte array as an octet stream to the request body.
                    using (ByteArrayContent content = new ByteArrayContent(byteData))
                    {
                        // This example uses the "application/octet-stream" content type.
                        // The other content types you can use are "application/json"
                        // and "multipart/form-data".
                        content.Headers.ContentType =
                            new MediaTypeHeaderValue("application/octet-stream");

                        // Asynchronously call the REST API method.
                        response = await client.PostAsync(uri, content);
                    }

                    // Asynchronously get the JSON response.
                    string contentString = await response.Content.ReadAsStringAsync();
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Exception::" + ex);
                }
            }
        }

        //Method to extract text from image using Google Cloud vision API
        public void CallGoogleVisionAPI() 
        {
            //var certificate = new X509Certificate2("//Users//apurva//Downloads//snapio-228915-3e71201609c7.p12", "notasecret", X509KeyStorageFlags.Exportable);
            var client = ImageAnnotatorClient.Create();
            var image = Image.FromFile("//Users//apurva//Desktop//photo");
            var response = client.DetectText(image);
            foreach (var annotation in response)
            {
                if (annotation.Description != null)
                {
                    string text = (annotation.Description);
                }
            }
        }
    
      //Creates a Computer Vision client 
       public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
                { Endpoint = endpoint };
            return client;
        }

        //Method to extract text from image using Azure-ComputerVision API
        public void CallAzureAPI()
        {

        }
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            // Open a read-only file stream for the specified file.
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                // Read the file's contents into a byte array.
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }

        //Default methods created by KeyboardActionListener
        public void OnRelease([GeneratedEnum] Android.Views.Keycode primaryCode)
		{
			
		}

		public void OnText(ICharSequence text)
		{
			
		}

		public void SwipeDown()
		{
			
		}

		public void SwipeLeft()
		{
			
		}

		public void SwipeRight()
		{
			
		}

		public void SwipeUp()
		{
			
		}
	}
}
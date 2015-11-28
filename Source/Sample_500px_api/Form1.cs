using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CCSWE.FiveHundredPx;

namespace Sample_500px_api
{
    public partial class Form1 : Form
    {
        private static HttpListener _listener;
        private OAuthToken requestToken;

        //private FiveHundredPxService _service = new FiveHundredPxService("<CONSUMER KEY>", "<CONSUMER SECRET>", "http://localhost:1234/OAuth/");


        private FiveHundredPxService MyService = new FiveHundredPxService("<CONSUMER KEY>", "<CONSUMER SECRET>", "http://localhost:1234/OAuth/");

        

        public Form1()
        {
            InitializeComponent();

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:1234/OAuth/");
            _listener.Start();

            var mythread = new Thread(Listen);
            mythread.Start();
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            requestToken = await MyService.GetRequestToken();
           
            string url = MyService.GetAuthorizeRequestTokenUrl(requestToken);
            
            //External browser
            //System.Diagnostics.Process.Start(url);

            //Internal Browser
            webBrowser1.Navigate(url);
        }

        private async void Listen()
        {
            while (true)
            {
                var test = _listener.GetContext();
                var response = test.Response;

                var test2 = test.Request;
                string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();

                foreach (var key in test2.QueryString.AllKeys)
                {
                    switch (key)
                    {
                        case OAuthParameter.Token:
                            {
                                requestToken.Token = test2.QueryString[key];
                                break;
                            }
                        case OAuthParameter.Verifier:
                            {
                                requestToken.Verifier = test2.QueryString[key];
                                break;
                            }
                    }
                }


                var accessToken = await MyService.GetAccessToken(requestToken);
                MyService.AccessToken = accessToken;


                // The GetCurrentUser() method is an authenticated request and retrieves the details of the authenticated user
                var response3 = await MyService.GetCurrentUser();
                string myuser = response3.User.ToString();


                string texto = "Successfully authenticated as: " + myuser;
                MessageBox.Show(texto);

            }

        }
           
        

        private async void button2_Click(object sender, EventArgs e)
        {
            //Get popular and choose the first
            var photoFilter = new PhotoFilter(Feature.Popular, Categories.Nude);

            var response_pop = await MyService.GetPhotos(photoFilter, 1, 100);
            List<CCSWE.FiveHundredPx.Models.Photo> lista_fotos_500px = new List<CCSWE.FiveHundredPx.Models.Photo>();
            foreach (var photo in response_pop.Photos)
            {
                // Store the photos for the current page in a List<Photo> or something
                lista_fotos_500px.Add(photo);
            }
            CCSWE.FiveHundredPx.Models.Photo chosen_one = (CCSWE.FiveHundredPx.Models.Photo)lista_fotos_500px[0];




           
            var response1 = await MyService.LikePhoto(chosen_one.Id);

            if (response1.IsSuccessStatusCode)
            {
                MessageBox.Show("Liked photo!!");
            }
            else
            {
                MessageBox.Show("Issue liking!!");
            }


          
            var response2 = await MyService.FavoritePhoto(chosen_one.Id);

            if (response2.IsSuccessStatusCode)
            {
                MessageBox.Show("Favorited photo!!");
            }
            else
            {
                MessageBox.Show("Issue Favoriting!!");
            }

        }
    }
}

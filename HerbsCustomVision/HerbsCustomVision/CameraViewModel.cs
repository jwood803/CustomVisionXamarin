using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HerbsCustomVision
{
    public class CameraViewModel
    {
        public Command AccessCamera { get; set; }

        public CameraViewModel()
        {
            AccessCamera = new Command(GetCameraAccess);
        }

        void GetCameraAccess()
        {
            if (CrossMedia.Current.Initialize().Result && CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported)
            {
                var mediaOptions = new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    SaveToAlbum = true
                };

                var file = CrossMedia.Current.TakePhotoAsync(mediaOptions).Result;
            }
        }
    }
}

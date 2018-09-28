using Plugin.Media;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Cognitive.CustomVision.Prediction.Models;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace HerbsCustomVision
{
    public class CameraViewModel : INotifyPropertyChanged
    {
        public CameraViewModel()
        {
            _herbVisionService = new HerbVisionService();

            TagPredictionModels = new ObservableCollection<ImageTagPredictionModel>();

            CameraCommand = new Command(GetCameraAccess);
            ClearCommand = new Command(ClearPredictions);
        }

        private ImageSource _cameraSource;
        private HerbVisionService _herbVisionService;

        public Command CameraCommand { get; private set; }
        public Command ClearCommand { get; private set; }

        public ImageSource CameraSource
        {
            get { return _cameraSource; }

            set
            {
                if (value != _cameraSource)
                {
                    _cameraSource = value;

                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<ImageTagPredictionModel> TagPredictionModels { get; set; }

        private void ClearPredictions()
        {
            TagPredictionModels.Clear();
            CameraSource = null;
        }

        private async void GetCameraAccess()
        {
            if (CrossMedia.Current.Initialize().Result && CrossMedia.Current.IsCameraAvailable &&
                CrossMedia.Current.IsTakePhotoSupported)
            {
                var mediaOptions = new Plugin.Media.Abstractions.StoreCameraMediaOptions
                {
                    SaveToAlbum = true
                };

                var file = await CrossMedia.Current.TakePhotoAsync(mediaOptions);

                if (file == null)
                    return;

                CameraSource = ImageSource.FromStream(() =>
                {
                    var stream = file.GetStream();
                    return stream;
                });

                var predictionModels = _herbVisionService.Predict(file.GetStream());
                TagPredictionModels.Clear();
                predictionModels.ForEach(TagPredictionModels.Add);
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged END
    }
}
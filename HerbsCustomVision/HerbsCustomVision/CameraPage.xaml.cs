using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HerbsCustomVision
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CameraPage : ContentPage
	{
        CameraViewModel _cameraViewModel;

		public CameraPage ()
		{
			InitializeComponent ();

            _cameraViewModel = new CameraViewModel();

            BindingContext = _cameraViewModel;
		}
	}
}
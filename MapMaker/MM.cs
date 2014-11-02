

using System;
using System.Collections.Generic;
using System.Windows;
namespace MapMaker
{
    public class MM
    {
        public static MainWindow mainWindow;

        MapImageResource mapImageResourcePending;
        Func<bool> imageIdEnteredCallback;
        public List<MapImageResource> mapImageResources
        {
            get
            {
                return mainWindow.mapImageResources;
            }
        }

        public void mapImageIdEntered(string id)
        {
            mapImageResourcePending.Id = id;
            imageIdEnteredCallback.Invoke();
        }

        public string getNowTimestamp()
        {
            return DateTime.Now.Ticks.ToString();
        }

        public void mapImageAdd(EnterNameUserControl userControl)
        {
            string openAdress = openImageResourceDialog();
            if (openAdress != String.Empty)
            {
                mapImageResourcePending = new MapImageResource();

                mapImageResourcePending.Location = openAdress;

                System.Drawing.Image image = System.Drawing.Image.FromFile(openAdress);
                mapImageResourcePending.Dimensions = new Point();
                mapImageResourcePending.Dimensions.X = image.Width;
                mapImageResourcePending.Dimensions.Y = image.Height;

                userControl.Visibility = System.Windows.Visibility.Visible;
                imageIdEnteredCallback = () =>
                {
                    userControl.Visibility = System.Windows.Visibility.Hidden;

                    //mapImageResourcePending.Dimensions = new Point(50, 50);

                    mapImageResources.Add(mapImageResourcePending);
                    mapImageResourcePending = null;

                    MM.mainWindow.refreshDataContextOutliner();
                    return true;
                };
            }


        }



        public string openImageResourceDialog()
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();


            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpeg)|*.jpeg|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                return filename;
            }

            else return "";
        }

















        public void mapDimensionsChosen(int width, int height)
        {
            mainWindow.setMapDimensions(width, height);
        }


























        public string getNowDateNicelyFormatted()
        {
            return DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss");
        }









        public MM()
        {
            //mapImageResources = new List<MapImageResource>();
        }


        static readonly MM _instance = new MM();

        public static MM Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}

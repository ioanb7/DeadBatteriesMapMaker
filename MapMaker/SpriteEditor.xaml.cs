using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MapMaker
{
    /// <summary>
    /// Interaction logic for EnterMapDimensionsUserControl.xaml
    /// </summary>
    public class Rect
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
    public partial class SpriteEditor : UserControl
    {
        public List<Rect> Rects;
        Point mousePoint;
        Point mousePointClicked; // when he started pressing and then moving..
        bool isPrinting = false;
        ToolType tool;

        public SpriteEditor()
        {
            InitializeComponent();
            mousePoint = new Point();

            currentDrawerMakerRectangle.Visibility = Visibility.Hidden;
            AutoRectGeneratorGrid.Visibility = Visibility.Hidden;

            tool = ToolType.None;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AutoToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            AutoRectGeneratorGrid.Visibility = Visibility.Visible;
        }

        private void AutoToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            AutoRectGeneratorGrid.Visibility = Visibility.Hidden;
        }

        private void AddSpriteRectButton_Click(object sender, RoutedEventArgs e)
        {
            tool = ToolType.Collision;
        }


        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPrinting = true;
            mousePointClicked = mousePoint;

            if (tool == ToolType.Collision)
            {
                currentDrawerMakerRectangle.Visibility = Visibility.Hidden;
            }
        }

        private void currentDrawerMakerRectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseReleased();
        }

        private void processMouseInput()
        {
            mousePoint = Point.ConvertPoint(Mouse.GetPosition((Grid)drawer));
            mousePoint.X += (int)MapScrollViewer.HorizontalOffset;
            mousePoint.Y += (int)MapScrollViewer.VerticalOffset;

            PreviewXTextBox.Text = ((int)mousePoint.X).ToString();
            PreviewYTextBox.Text = ((int)mousePoint.Y).ToString();
        }

        private void drawer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseReleased();
        }
        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            processMouseInput();

            if (isPrinting)
            {
                if(tool == ToolType.Collision)
                {
                    printCollision();
                }
            }
        }

        /// <summary>
        /// Certain things should happen when the mouse is released
        /// </summary>
        private void mouseReleased()
        {
            isPrinting = false;

            if (tool == ToolType.Collision)
            {
                mouseReleasedCollision();
            }
        }


        #region Collision functionalities

        private void printCollision()
        {
            // if in between canvas...
            int width = 0;
            int height = 0;
            int x = (int)currentDrawerMakerRectangle.Margin.Left;
            int y = (int)currentDrawerMakerRectangle.Margin.Top;
            width = (int)(mousePoint.X - mousePointClicked.X);
            height = (int)(mousePoint.Y - mousePointClicked.Y);


            if (width < 0)
            {
                x = (int)mousePoint.X;
                width = (int)mousePointClicked.X - (int)mousePoint.X;
            }
            else
            {
                x = (int)mousePointClicked.X;
            }
            if (height < 0)
            {
                y = (int)mousePoint.Y;
                height = (int)mousePointClicked.Y - (int)mousePoint.Y;
            }
            else
            {
                y = (int)mousePointClicked.Y;
            }

            if (width < 2 || height < 2)
                currentDrawerMakerRectangle.Visibility = Visibility.Hidden;
            else
                currentDrawerMakerRectangle.Visibility = Visibility.Visible;

            x -= (int)MapScrollViewer.HorizontalOffset;
            y -= (int)MapScrollViewer.VerticalOffset;

            PreviewWidthTextBox.Text = width.ToString();
            PreviewHeightTextBox.Text = height.ToString();

            currentDrawerMakerRectangle.Width = width;
            currentDrawerMakerRectangle.Height = height;
            currentDrawerMakerRectangle.Margin = new Thickness(x, y, 0, 0);
        }
        private void mouseReleasedCollision()
        {
            currentDrawerMakerRectangle.Visibility = Visibility.Hidden;

            MapRectangle mapRectangle = new MapRectangle
            {
                X = (int)currentDrawerMakerRectangle.Margin.Left,
                Y = (int)currentDrawerMakerRectangle.Margin.Top
                ,
                Width = (int)currentDrawerMakerRectangle.Width,
                Height = (int)currentDrawerMakerRectangle.Height
            };

            Rectangle visualRectangle = new Rectangle();
            visualRectangle.Width = currentDrawerMakerRectangle.Width;
            visualRectangle.Height = currentDrawerMakerRectangle.Height;
            visualRectangle.Margin = currentDrawerMakerRectangle.Margin;

            visualRectangle.HorizontalAlignment = HorizontalAlignment.Left;
            visualRectangle.VerticalAlignment = VerticalAlignment.Top;

            visualRectangle.Fill = new SolidColorBrush(Colors.Red);

            Canvas.SetZIndex(visualRectangle, 120);

            drawer.Children.Add(visualRectangle);

            PreviewXTextBox.Text = "";
            PreviewYTextBox.Text = "";
        }

        #endregion
    }
}

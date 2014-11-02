using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Properties
        public List<MapImageResource> mapImageResources;
        public List<MapImage> mapImages;
        public List<MapImage> mapImagesLastMove;
        public List<GameObject> gameObjects;
        public Map map;
        Point mousePoint;
        Point mousePointClicked; // when he started pressing and then moving..
        bool isPrinting = false;
        CollisionType collisionType;
        List<MapRectangle> collisionLevel1;
        List<MapRectangle> collisionLevel2;
        List<MapRectangle> collisionLevel3;
        Brush ReplacementColorInCaseThereIsNoBackgroundImage;
        ToolType _tool;
        ToolType tool {
            get
            {
                return _tool;
            }
            set
            {
                if (_tool == ToolType.MapImageRectRevise && value != ToolType.MapImageRectRevise)
                    currentMapRepositionerIsOver();

                _tool = value;
                currentToolTextBox.Text = _tool.ToString();

                if(_tool == ToolType.ImageProperties)
                {
                    propertiesGrid.Visibility = Visibility.Visible;
                    sourceBoxGrid.Visibility = Visibility.Hidden;
                }
                else
                {
                    propertiesGrid.Visibility = Visibility.Hidden;
                    sourceBoxGrid.Visibility = Visibility.Visible;
                }

                
            }
        }
        #endregion

        #region Initialize
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
            MM.mainWindow = this;

            MapImage.ZindexTotal = 50;

            mousePoint = new Point(0, 0);
            mousePointClicked = new Point(0, 0);
            mapImageResources = new List<MapImageResource>();
            mapImages = new List<MapImage>();
            mapImagesLastMove = new List<MapImage>();
            gameObjects = new List<GameObject>();
            collisionLevel1 = new List<MapRectangle>();
            collisionLevel2 = new List<MapRectangle>();
            collisionLevel3 = new List<MapRectangle>();

            ReplacementColorInCaseThereIsNoBackgroundImage = null;

            InitializeDefaultGridVisibility();

            //initialize map
            map = new Map();
            map.BackgroundImage = null;

            //initialize tools
            tool = ToolType.None;
            collisionType = CollisionType.Level1;


            AddGameObjectTypesToComboBox();


#if DEBUG
            //easier ways to debug the application
            MM.Instance.mapDimensionsChosen(500, 500);
            //showSpriteUserControl();
#endif
        }

        #endregion

        #region Initialize helpers
        private void InitializeDefaultGridVisibility()
        {
            collisionDrawerLevel1.Visibility = Visibility.Hidden;
            collisionDrawerLevel2.Visibility = Visibility.Hidden;
            collisionDrawerLevel3.Visibility = Visibility.Hidden;
            enterMapDimensionsUserControl.Visibility = Visibility.Visible;
            currentCollisionMakerRectangle.Visibility = Visibility.Hidden;
            currentMapPositionerRectangle.Visibility = Visibility.Hidden;
            currentMapRepositionerRectangle.Visibility = Visibility.Hidden;
            currentMapRepositionerPointRectangle.Visibility = Visibility.Hidden;
            collisionDrawer.Visibility = Visibility.Hidden;

            hideAllCollsionRectanglesAndTools();
            closeBackgroundOptions();
        }



        private void AddGameObjectTypesToComboBox()
        {
            List<string> gameObjectTypes = new List<string>();
            foreach (GameObjectType gameObjectType in (GameObjectType[])Enum.GetValues(typeof(GameObjectType)))
            {
                gameObjectTypes.Add(gameObjectType.ToString());
            }
            gameObjectTypePropertiesComboBox.ItemsSource = gameObjectTypes;
        }

        #endregion

        #region Data binding for the map image resources (listbox)

        private readonly ObservableCollection<MapImageResource> _collection = new ObservableCollection<MapImageResource>();


        public ObservableCollection<MapImageResource> Collection
        {
            get { return _collection; }
        }


        public void refreshDataContextOutliner()
        {
            _collection.Add(mapImageResources.Last());
        }

        #endregion

        #region design flow elements

        private void addImageResourceButton_Click(object sender, RoutedEventArgs e)
        {
            MM.Instance.mapImageAdd(enterNameUserControl);
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: warn the user if there is no background image.
            // TODO: start a thread. and display saving... :)
            string folderName = "";

            do
            {
                folderName = MM.Instance.getNowTimestamp(); // get a name
            }
            while (System.IO.Directory.Exists(folderName));

            System.IO.Directory.CreateDirectory(folderName);

            MapExporter.SaveToFolder(folderName, "Exported with MapMaker made by Ioan-Catalin B." + "\r\n" + MM.Instance.getNowDateNicelyFormatted(),
                mapImages, mapImageResources,
                collisionLevel1, collisionLevel2, collisionLevel3,
                gameObjects,
                map);
        }

        public void setMapDimensions(int width, int height)
        {
            drawer.Width = width;
            drawer.Height = height;

            map.Width = width;
            map.Height = height;

            enterMapDimensionsUserControl.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Map Image functionalities

        private void printMapImage()
        {
            MapImageResource mapImageSelected = (MapImageResource)mapImageSourceListBox.SelectedItem;

            MapImage mapImage = new MapImage();

            mapImage.Id = mapImageSelected.Id;
            mapImage.Location = mapImageSelected.Location;
            mapImage.Dimensions = mapImageSelected.Dimensions;

            mapImage.pos = mousePoint;
            //mapImage.MapDimensions = mapImage.Dimensions;
            mapImage.MapDimensions = new Point(int.Parse(SpriteDimensionsOnTheMapWidth.Text), int.Parse(SpriteDimensionsOnTheMapHeight.Text));

            //scale it
            mapImage.MapDimensions.X = (int)((float)mapImage.MapDimensions.X * float.Parse(SpriteScaleTextBox.Text));
            mapImage.MapDimensions.Y = (int)((float)mapImage.MapDimensions.Y * float.Parse(SpriteScaleTextBox.Text));

            mapImage.MapId = mapImages.Count;

            mapImage.IsPartOfTheBackground = IsPartOfTheBackgroundCheckbox.IsChecked == true;

            mapImage.Zindex = ++MapImage.ZindexTotal;

            bool addIt = false;
            if (mapImages.Count > 0)
            {
                if (IsItACertainDistanceBetweenTwoPoints(mapImage.pos, mapImages.Last().pos, int.Parse(SpaceBetweenDrawingsTextBox.Text)))
                    addIt = true;
            }
            else
            {
                addIt = true;
            }

            int roomForError = 10;
            if (addIt == true)
            {
                if (mapImage.pos.X + (MapScrollViewer.HorizontalOffset) >= 0 - roomForError
                    && mapImage.pos.Y + (MapScrollViewer.VerticalOffset) >= 0 - roomForError
                    && mapImage.pos.X + (MapScrollViewer.HorizontalOffset) + mapImage.MapDimensions.X <= map.Width + roomForError
                    && mapImage.pos.Y + (MapScrollViewer.VerticalOffset) + mapImage.MapDimensions.Y <= map.Height + roomForError
                    )
                {
                    bool isStretched = StretchSpriteCheckbox.IsChecked == true;
                    mapImage.isStretched = isStretched;
                    mapImage.pos.X -= (int)MapScrollViewer.HorizontalOffset;
                    mapImage.pos.Y -= (int)MapScrollViewer.VerticalOffset;
                    mapImages.Add(mapImage);
                    mapImagesLastMove.Add(mapImage);
                    addMapImageToDrawer(mapImage);
                }
            }
        }

        #endregion

        #region High level events

        private void processMouseInput()
        {
            mousePoint = Point.ConvertPoint(Mouse.GetPosition((Grid)drawer));
            mousePoint.X += (int)MapScrollViewer.HorizontalOffset;
            mousePoint.Y += (int)MapScrollViewer.VerticalOffset;

            CursorCoordinateMapX.Text = ((int)mousePoint.X).ToString();
            CursorCoordinateMapY.Text = ((int)mousePoint.Y).ToString();
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            processMouseInput();

            if (isPrinting)
            {
                if (tool == ToolType.MapImage && mapImageSourceListBox.SelectedItem != null)
                {
                    printMapImage();
                }
                else if(tool == ToolType.Collision)
                {
                    printCollision();
                }
                else if (tool == ToolType.MapImageRect)
                {
                    printMapImageRect();
                }
                else if (tool == ToolType.MapImageRectRevise)
                {
                    adjustASD();
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

            if (tool == ToolType.MapImageRect && mapImageSourceListBox.SelectedItem != null)
            {
                mouseReleasedMapImageRepositionerRect();
            }
            if (tool == ToolType.MapImageRectRevise)
            {
                mouseReleasedMapImageRectRevise();
            }
        }


        #endregion

        #region helpers
        private bool IsItACertainDistanceBetweenTwoPoints(Point p1, Point p2, double distance)
        {
            return (Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)) > distance * distance;
        }

        #endregion

        #region View helpers

        private Image addMapImageToDrawer(MapImage mapImage)
        {
            Image image = new Image();

            image.Source = new BitmapImage(new Uri(mapImage.Location, UriKind.Absolute));
            image.Margin = new Thickness(mapImage.pos.X, mapImage.pos.Y, 0, 0);
            image.Width = mapImage.MapDimensions.X;
            image.Height = mapImage.MapDimensions.Y;
            image.HorizontalAlignment = HorizontalAlignment.Left;
            image.VerticalAlignment = VerticalAlignment.Top;
            Canvas.SetZIndex(image, mapImage.Zindex);

            if (mapImage.isStretched)
            {
                //image.Stretch = Stretch.UniformToFill;
                image.Stretch = Stretch.Fill;
            }

            drawer.Children.Add(image);

            return image;
        }

        #endregion

        #region Collision functionalities

        private void printCollision()
        {
            // if in between canvas...
            int width = 0;
            int height = 0;
            int x = (int)currentCollisionMakerRectangle.Margin.Left;
            int y = (int)currentCollisionMakerRectangle.Margin.Top;
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
                currentCollisionMakerRectangle.Visibility = Visibility.Hidden;
            else
                currentCollisionMakerRectangle.Visibility = Visibility.Visible;

            x -= (int)MapScrollViewer.HorizontalOffset;
            y -= (int)MapScrollViewer.VerticalOffset;

            currentCollisionMakerRectangle.Width = width;
            currentCollisionMakerRectangle.Height = height;
            currentCollisionMakerRectangle.Margin = new Thickness(x, y, 0, 0);
        }
        private void mouseReleasedCollision()
        {
            currentCollisionMakerRectangle.Visibility = Visibility.Hidden;

            MapRectangle mapRectangle = new MapRectangle
            {
                X = (int)currentCollisionMakerRectangle.Margin.Left,
                Y = (int)currentCollisionMakerRectangle.Margin.Top
                ,
                Width = (int)currentCollisionMakerRectangle.Width,
                Height = (int)currentCollisionMakerRectangle.Height
            };

            Rectangle visualRectangle = new Rectangle();
            visualRectangle.Width = currentCollisionMakerRectangle.Width;
            visualRectangle.Height = currentCollisionMakerRectangle.Height;
            visualRectangle.Margin = currentCollisionMakerRectangle.Margin;

            visualRectangle.HorizontalAlignment = HorizontalAlignment.Left;
            visualRectangle.VerticalAlignment = VerticalAlignment.Top;

            Color color = Colors.Black;
            if (collisionType == CollisionType.Level1)
                color = Colors.Red;
            if (collisionType == CollisionType.Level2)
                color = Colors.Green;
            if (collisionType == CollisionType.Level3)
                color = Colors.Blue;

            visualRectangle.Fill = new SolidColorBrush(color);

            //add it
            if (collisionType == CollisionType.Level1)
            {
                collisionDrawerLevel1.Children.Add(visualRectangle);
                mapRectangle.Id = collisionLevel1.Count;
                mapRectangle.ParentId = -1;
                collisionLevel1.Add(mapRectangle);
            }
            if (collisionType == CollisionType.Level2)
            {
                collisionDrawerLevel2.Children.Add(visualRectangle);
                mapRectangle.Id = collisionLevel2.Count;
                collisionLevel2.Add(mapRectangle);
            }
            if (collisionType == CollisionType.Level3)
            {
                collisionDrawerLevel3.Children.Add(visualRectangle);
                mapRectangle.Id = collisionLevel3.Count;
                collisionLevel3.Add(mapRectangle);
            }
        }

        #endregion

        #region Collision show/hide events
        private void CollisionsPanelCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            hideAllCollsionRectanglesAndTools();
        }

        private void hideAllCollsionRectanglesAndTools()
        {
            collisionDrawer.Visibility = Visibility.Hidden;
            CollisionGrid.Visibility = Visibility.Hidden;
            tool = ToolType.None;
        }

        private void showAllCollsionRectanglesAndTools()
        {
            collisionDrawer.Visibility = Visibility.Visible;
            CollisionGrid.Visibility = Visibility.Visible;
        }
        private void CollisionsPanelCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            showAllCollsionRectanglesAndTools();
        }
        #endregion

        #region Collision rectangle show/hide based on levels

        private void CollisionLevel1CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            collisionDrawerLevel1.Visibility = Visibility.Visible;
        }

        private void CollisionLevel1CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            collisionDrawerLevel1.Visibility = Visibility.Hidden;
        }
        private void CollisionLevel2CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            collisionDrawerLevel2.Visibility = Visibility.Visible;
        }

        private void CollisionLevel2CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            collisionDrawerLevel2.Visibility = Visibility.Hidden;
        }
        private void CollisionLevel3CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            collisionDrawerLevel3.Visibility = Visibility.Visible;
        }

        private void CollisionLevel3CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            collisionDrawerLevel3.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Collision set which level the collision belongs in
        private void collisionLevelXChecked(int x)
        {
            collisionType = (CollisionType)x;
        }
        private void CollisionLevel1Button_Click(object sender, RoutedEventArgs e)
        {
            collisionLevelXChecked(1);
        }

        private void CollisionLevel2Button_Click(object sender, RoutedEventArgs e)
        {
            collisionLevelXChecked(2);
        }
        private void CollisionLevel3Button_Click(object sender, RoutedEventArgs e)
        {
            collisionLevelXChecked(3);
        }

        #endregion

        #region Map editor specific tools - they help in the development flow - like a Undo button
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();

            MapImage mapImage = mapImages.Last();
            for (int i = drawer.Children.Count - 1; i >= 0; i--)
            {
                FrameworkElement control = (FrameworkElement)drawer.Children[i];
                if (//image.Margin.Left == mapImage.pos.X && image.Margin.Left == mapImage.pos.X&& 
                    mapImage.Zindex == Canvas.GetZIndex(control))
                {
                    drawer.Children.RemoveAt(i);
                    mapImages.Remove(mapImage);
                }
            }
            /*
            return;
            
            foreach(MapImage mapImage in mapImagesLastMove)
            {
                for (int i = drawer.Children.Count - 1; i >= 0; i--)
                {
                    FrameworkElement control = (FrameworkElement)drawer.Children[i];
                    if (//image.Margin.Left == mapImage.pos.X && image.Margin.Left == mapImage.pos.X&& 
                        mapImage.Zindex == Canvas.GetZIndex(control))
                    {
                        drawer.Children.RemoveAt(i);
                        mapImages.Remove(mapImage);
                    }
                }
            }
            mapImagesLastMove.Clear();
             */
        }
        #endregion

        #region Background
        /// <summary>
        /// pick image for the background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundPickImageButton_Click(object sender, RoutedEventArgs e)
        {
            string filename = MM.Instance.openImageResourceDialog();
            if(filename != "")
            {
                BackgroundImage.Source = new BitmapImage(new Uri(filename, UriKind.Absolute));
                BackgroundImage.Visibility = Visibility.Visible;
                BackgroundImage.Width = map.Width;
                BackgroundImage.Height = map.Height;
                ReplacementColorInCaseThereIsNoBackgroundImage = DrawerRectangle.Fill;
                DrawerRectangle.Fill = new SolidColorBrush(Colors.Transparent);
                map.BackgroundImage = new MapImage();
                map.BackgroundImage.Location = filename;
                
                System.Drawing.Image image = System.Drawing.Image.FromFile(filename);
                map.BackgroundImage.MapDimensions = new Point();
                map.BackgroundImage.MapDimensions.X = image.Width;
                map.BackgroundImage.MapDimensions.Y = image.Height;
            }
        }

        private void BackgroundDeleteImageButton_Click(object sender, RoutedEventArgs e)
        {
            BackgroundImage.Source = null;
            BackgroundImage.Visibility = Visibility.Visible;
            BackgroundImage.Width = 0;
            BackgroundImage.Height = 0;
            DrawerRectangle.Fill = ReplacementColorInCaseThereIsNoBackgroundImage;
            map.BackgroundImage = null;
        }

        private void AdjustMapSizeToBackgroundSizeButton_Click(object sender, RoutedEventArgs e)
        {
            if(map.BackgroundImage != null)
            {
                setMapDimensions(map.BackgroundImage.MapDimensions.X, map.BackgroundImage.MapDimensions.Y);
                BackgroundImage.Width = map.BackgroundImage.MapDimensions.X;
                BackgroundImage.Height = map.BackgroundImage.MapDimensions.Y;
            }
        }

        private void BackgroundCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            BackgroundGrid.Visibility = Visibility.Visible;
        }

        private void BackgroundCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            closeBackgroundOptions();
        }

        private void closeBackgroundOptions()
        {
            BackgroundGrid.Visibility = Visibility.Hidden;
        }
#endregion

        #region Sprite functionalities
        private void AdjustImageBasedOnSprite_Click(object sender, RoutedEventArgs e)
        {
            if (mapImageSourceListBox.SelectedItem != null)
            {
                MapImageResource mapImageSelected = (MapImageResource)mapImageSourceListBox.SelectedItem;
                SpriteDimensionsOnTheMapWidth.Text = ((int)mapImageSelected.Dimensions.X).ToString();
                SpriteDimensionsOnTheMapHeight.Text = ((int)mapImageSelected.Dimensions.Y).ToString();
            }
        }
        #endregion

        #region Tool changers

        private void ChangeToolCollisionButton_Click(object sender, RoutedEventArgs e)
        {
            tool = ToolType.Collision;
        }

        private void mapImageSourceListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            tool = ToolType.MapImage;
        }
        private void ChangeToolToImageDrawWIthRectangleButton_Click(object sender, RoutedEventArgs e)
        {
            if (mapImageSourceListBox.SelectedItem == null)
                MessageBox.Show("select an image");
            else
                tool = ToolType.MapImageRect;
        }

        private void ChangeToolToImageDrawButton_Click(object sender, RoutedEventArgs e)
        {
            tool = ToolType.MapImage;
        }

        private void ChangeToolToImagePropertiesButton_Click(object sender, RoutedEventArgs e)
        {
            tool = ToolType.ImageProperties;
        }

        #endregion

        #region Sprite add properties and make it a game object that the game can access.
        private void gameObjectTypePropertiesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //depends!

            ComboBox comboBox = (ComboBox)sender;
            string selected = (string)comboBox.SelectedValue;

            DatabaseTable dt = new DatabaseTable();
            if (selected != "")
            {
                GameObjectType gameObjectType = (GameObjectType)Enum.Parse(typeof(GameObjectType), selected);

                if (gameObjectType == GameObjectType.BonusHP)
                {
                    dt = (new Serializer<GameObjectBonusHPBase>()).Run();
                }
                else if(gameObjectType == GameObjectType.Teleport)
                {
                    dt = (new Serializer<GameObjectTeleportBase>()).Run();
                }
                else if (gameObjectType == GameObjectType.Spawner)
                {
                    dt = (new Serializer<GameObjectSpawnerBase>()).Run();
                }
                else if (gameObjectType == GameObjectType.Key)
                {
                    dt = (new Serializer<GameObjectKeyBase>()).Run();
                }

                foreach(DatabaseRow row in dt)
                {
                    if(row.Key == "Id")
                    {
                        row.Value = gameObjects.Count.ToString();
                    }
                }
            }

            DG.ItemsSource = dt;
        }
        private void mapImagePropertiesOKButton_Click(object sender, RoutedEventArgs e)
        {
            string selected = (string)gameObjectTypePropertiesComboBox.SelectedValue;

            DatabaseTable dt = new DatabaseTable();
            if (selected != "")
            {
                GameObjectType gameObjectType = (GameObjectType)Enum.Parse(typeof(GameObjectType), selected);

                GameObject go = null;
                if (gameObjectType == GameObjectType.BonusHP)
                {
                    go = (new Deserializer<GameObjectBonusHPBase>()).Run((DatabaseTable)DG.ItemsSource);
                }
                else if (gameObjectType == GameObjectType.Teleport)
                {
                    go = (new Deserializer<GameObjectTeleportBase>()).Run((DatabaseTable)DG.ItemsSource);
                }
                else if (gameObjectType == GameObjectType.Spawner)
                {
                    go = (new Deserializer<GameObjectSpawnerBase>()).Run((DatabaseTable)DG.ItemsSource);
                }
                else if (gameObjectType == GameObjectType.Key)
                {
                    go = (new Deserializer<GameObjectKeyBase>()).Run((DatabaseTable)DG.ItemsSource);
                }

                go.mapImage = mapImages.Last();
                //go.Id = gameObjects.Count;
                gameObjects.Add(go);
            }
        }
        #endregion

        #region MapImageRect functionalities
        private void printMapImageRect()
        {
            // if in between canvas...
            int width = 0;
            int height = 0;
            int x = (int)currentMapRepositionerRectangle.Margin.Left;
            int y = (int)currentMapRepositionerRectangle.Margin.Top;
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
                currentMapRepositionerRectangle.Visibility = Visibility.Hidden;
            else
                currentMapRepositionerRectangle.Visibility = Visibility.Visible;

            x -= (int)MapScrollViewer.HorizontalOffset;
            y -= (int)MapScrollViewer.VerticalOffset;

            currentMapRepositionerRectangle.Width = width;
            currentMapRepositionerRectangle.Height = height;
            currentMapRepositionerRectangle.Margin = new Thickness(x, y, 0, 0);
        }

        private void mouseReleasedMapImageRepositionerRect()
        {
            currentMapRepositionerRectangle.Visibility = Visibility.Hidden;
            MapImageResource mapImageSelected = (MapImageResource)mapImageSourceListBox.SelectedItem;

            MapImage mapImage = new MapImage();

            mapImage.Id = mapImageSelected.Id;
            mapImage.Location = mapImageSelected.Location;
            mapImage.Dimensions = mapImageSelected.Dimensions;

            mapImage.pos = new Point();
            mapImage.pos.X = (int)currentMapRepositionerRectangle.Margin.Left;
            mapImage.pos.Y = (int)currentMapRepositionerRectangle.Margin.Top;
            mapImage.MapDimensions = new Point();
            mapImage.MapDimensions.X = (int)currentMapRepositionerRectangle.Width;
            mapImage.MapDimensions.Y = (int)currentMapRepositionerRectangle.Height;

            mapImage.MapId = mapImages.Count;
            mapImage.IsPartOfTheBackground = IsPartOfTheBackgroundCheckbox.IsChecked == true;
            mapImage.Zindex = ++MapImage.ZindexTotal;

            bool isStretched = StretchSpriteCheckbox.IsChecked == true;
            mapImage.isStretched = isStretched;

            mapImages.Add(mapImage);
            Image image = addMapImageToDrawer(mapImage);



            currentMapRepositionerRectangle.Width = mapImage.MapDimensions.X;
            currentMapRepositionerRectangle.Height = mapImage.MapDimensions.Y;
            currentMapRepositionerRectangle.Margin = new Thickness(mapImage.pos.X, mapImage.pos.Y, 0, 0);

            //currentMapRepositionerPointRectangle.Margin = new Thickness(mapImage.MapDimensions.X, mapImage.MapDimensions.Y, 0, 0);
            currentMapRepositionerPointRectangle.Margin = new Thickness(mousePoint.X - 2, mousePoint.Y - 2, 0, 0);

            tool = ToolType.MapImageRectRevise;
            currentMapRepositionerRectangle.Visibility = Visibility.Visible;
            currentMapRepositionerPointRectangle.Visibility = Visibility.Visible;
        }
        
        
        
        private void mouseReleasedMapImageRectRevise()
        {
            currentMapRepositionerRectangle.Visibility = Visibility.Hidden;
            currentMapRepositionerPointRectangle.Visibility = Visibility.Hidden;

            //tool = ToolType.None;
        }
        private void adjustASD()
        {
            //get current Rect that is redimensioned

            // if in between canvas...
            int width = 0;
            int height = 0;

            width = (int)(mousePoint.X - currentMapRepositionerRectangle.Margin.Left);
            height = (int)(mousePoint.Y - currentMapRepositionerRectangle.Margin.Top);

            width = Math.Max(0, width);
            height = Math.Max(0, height);

            currentMapRepositionerRectangle.Width = width;
            currentMapRepositionerRectangle.Height = height;

            currentMapRepositionerPointRectangle.Margin = new Thickness(mousePoint.X - 2, mousePoint.Y - 2, 0, 0);

            //add it.
        }
        private void currentMapRepositionerRectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //currentMapRepositionerRectangle.Visibility = Visibility.Hidden;

            if (mapImageSourceListBox.SelectedItem != null) // TODO: pay more attention to this..
            {
                //adjustASD();

                //mouseReleasedMapImageRect();
                mouseReleasedMapImageRepositionerRect();
            }
        }
        private void currentMapRepositionerPointRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPrinting = true;
            //e.Handled = true;
            currentMapRepositionerRectangle.Visibility = Visibility.Visible;
            if (tool == ToolType.MapImageRectRevise)
            {
                //adjustASD();
            }
        }

        private void currentMapRepositionerIsOver()
        {
            mouseReleasedMapImageRectRevise();
        }

        private void currentMapRepositionerPointRectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //redimensionate the last image
            isPrinting = false;

            //remove it
            MapImage mapImage = mapImages.Last();
            for (int i = drawer.Children.Count - 1; i >= 0; i--)
            {
                FrameworkElement control = (FrameworkElement)drawer.Children[i];
                if (//image.Margin.Left == mapImage.pos.X && image.Margin.Left == mapImage.pos.X&& 
                    mapImage.Zindex == Canvas.GetZIndex(control))
                {
                    if (currentMapRepositionerPointRectangle.Margin.Left + 2 - control.Margin.Left > 0
                        &&
                        currentMapRepositionerPointRectangle.Margin.Top + 2 - control.Margin.Top > 0)
                    {


                        //control.Margin = new Thickness(control.Margin.Left, control.Margin.Top, currentMapRepositionerPointRectangle.Margin.Left + 2, currentMapRepositionerPointRectangle.Margin.Top + 2);
                        control.Width = currentMapRepositionerPointRectangle.Margin.Left + 2 - control.Margin.Left;
                        control.Height = currentMapRepositionerPointRectangle.Margin.Top + 2 - control.Margin.Top;
                    }
                    break;
                }
            }
            //re add it
            mapImages.Remove(mapImage);
            mapImages.Add(mapImage);
        }

        private void setLastImageStretched(bool isIt)
        {
            MapImage mapImage = mapImages.Last();
            for (int i = drawer.Children.Count - 1; i >= 0; i--)
            {
                FrameworkElement control = (FrameworkElement)drawer.Children[i];
                if (//image.Margin.Left == mapImage.pos.X && image.Margin.Left == mapImage.pos.X&& 
                    mapImage.Zindex == Canvas.GetZIndex(control))
                {
                    ((Image)control).Stretch = isIt == true ? Stretch.Fill : Stretch.None;
                    break;
                }
            }
            mapImage.isStretched = isIt;
            mapImages.Remove(mapImage);
            mapImages.Add(mapImage);
        }

        #endregion


        private void ScrollViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPrinting = true;
            mousePointClicked = mousePoint;

            if (tool == ToolType.Collision)
            {
                currentCollisionMakerRectangle.Visibility = Visibility.Hidden;
            }
            else if (tool == ToolType.MapImage)
            {
                mapImagesLastMove.Clear();
            }
        }

        private void drawer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseReleased();
        }
        private void currentCollisionMakerRectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseReleased();
        }
        private void currentMapPositionerRectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseReleased();
        }

        private void StretchSpriteCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            setLastImageStretched(true);
        }

        private void StretchSpriteCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            setLastImageStretched(false);
        }

        #region Sprites

        private void showSpriteUserControl()
        {
            spriteEditorUserControl.Visibility = Visibility.Visible;
        }

        #endregion

    }
}

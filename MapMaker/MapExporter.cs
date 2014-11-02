using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MapMaker
{
    public class MapExporter
    {
        public static void SaveToFolder(string folder, string metadata,
            List<MapImage> mapImages, List<MapImageResource> mapImageResources,
            List<MapRectangle> collisionLevel1, List<MapRectangle> collisionLevel2, List<MapRectangle> collisionLevel3,
            List<GameObject> gameObjects
            ,Map map)
        {
            DataObject data = new DataObject();
            data.collisionHolder = new CollisionHolder();

            string dataFile = folder + "/map.data";

            //export the metadata
            data.metadata = metadata;


            //process mapImageResources
            foreach(MapImageResource mapImageResource in mapImageResources)
            {
                string[] Path = mapImageResource.Location.Split('/');
                mapImageResource.Location = Path[Path.Count() - 1];
            }

            //export all map resources
            data.mapImageResources = mapImageResources;

            //export all game objects
            data.gameObjects = gameObjects;

            data.map = map;


            //background ......
            bool isABackgroundImage = map.BackgroundImage != null;
            //put the background image in the folder, if there is one.
            if (isABackgroundImage)
            {
                string[] Path = map.BackgroundImage.Location.Split('.');
                //export the background file
                string exportBackgroundFileName = "backgroundImage." + Path[Path.Count() - 1];
                string exportBackgroundLocation = folder + "/" + exportBackgroundFileName;
                System.IO.File.Copy(map.BackgroundImage.Location, exportBackgroundLocation); // TODO: is it png?
                //change this for the export to look nice
                map.BackgroundImage.Location = exportBackgroundFileName;

                //export it's metadata
                data.BackgroundImage = map.BackgroundImage;

                //relink the exported file
                map.BackgroundImage.Location = exportBackgroundLocation;
            }

            //put all `background` images in one file with transparency
            Bitmap bitmap = new Bitmap(map.Width, map.Height);
            try
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    //clear the screen
                    graphics.Clear(System.Drawing.Color.Transparent);

                    //process
                    foreach (MapImage mapImage in mapImages)
                    {
                        if (mapImage.IsPartOfTheBackground == false)
                            continue;

                        //export it
                        System.Drawing.Image image = System.Drawing.Image.FromFile(mapImage.Location);
                        if (mapImage.isStretched == false)
                            graphics.DrawImage(image, new PointF(mapImage.pos.X, mapImage.pos.Y));
                        else
                            graphics.DrawImage(image, mapImage.pos.X, mapImage.pos.Y, mapImage.Dimensions.X, mapImage.Dimensions.Y);
                    }
                }

                string exportMapLocation = folder + "/background.png";
                bitmap.Save(exportMapLocation, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("There was an error." +
                    "Make sure the image file path is valid." + 
                    "Unable to export background map images");
            }

            //export a variable saying if there is a background image.
            bool asd = isABackgroundImage;
            data.isABackgroundImage = isABackgroundImage;



            //create map objects folder
            string mapObjectsFolder = folder + "/objects";
            Directory.CreateDirectory(mapObjectsFolder);

            data.mapObjects = new List<MapImage>();
            //export all objects that are not from the background
            foreach (MapImage mapImage in mapImages)
            {
                if (mapImage.IsPartOfTheBackground == true)
                    continue;

                data.mapObjects.Add(mapImage);

                //copy them.

                string[] Path = mapImage.Location.Split('/');

                File.Copy(mapImage.Location, mapObjectsFolder + "/" + Path[Path.Count() - 1]);
            }







            //export a variable saying if there are collisions or not
            bool areCollisionRectanglesAvailable = collisionLevel3.Count != 0;
            data.areCollisionRectanglesAvailable = areCollisionRectanglesAvailable;

            //export all collisions       //if there are no collisions. don't export anything regarding collisions
            if(areCollisionRectanglesAvailable)
            {
                //process all collision levels and make some if there are none
                if (collisionLevel2.Count == 0)
                {
                    Rectangle rectangleWrapper = GetRectangleWrapper(collisionLevel3);

                    collisionLevel2.Add(new MapRectangle
                    {
                        Id = 0,
                        X = rectangleWrapper.X,
                        Y = rectangleWrapper.Y,
                        Width = rectangleWrapper.Width,
                        Height = rectangleWrapper.Height
                    });
                }

                if (collisionLevel1.Count == 0)
                {
                    Rectangle rectangleWrapper = GetRectangleWrapper(collisionLevel2);

                    collisionLevel1.Add(new MapRectangle
                    {
                        Id = 0,
                        X = rectangleWrapper.X,
                        Y = rectangleWrapper.Y,
                        Width = rectangleWrapper.Width,
                        Height = rectangleWrapper.Height
                    });
                }

                //process the parentId - id
                foreach(MapRectangle level1MapRectangle in collisionLevel1)
                {
                    foreach(MapRectangle level2MapRectangle in collisionLevel2)
                    {
                        if(level1MapRectangle.Intersects(level2MapRectangle))
                        {
                            level2MapRectangle.ParentId = level1MapRectangle.Id;
                        }
                    }
                }

                foreach (MapRectangle level2MapRectangle in collisionLevel2)
                {
                    foreach (MapRectangle level3MapRectangle in collisionLevel3)
                    {
                        if (level2MapRectangle.Intersects(level3MapRectangle))
                        {
                            level3MapRectangle.ParentId = level2MapRectangle.Id;
                        }
                    }
                }

                //TODO export the collision levels
                foreach(MapRectangle level1MapRectangle in collisionLevel1)
                {
                    CollisionHolder collisionHolderLevel1 = new CollisionHolder();
                    CopyMapRectangleToCollisionHolder(level1MapRectangle, ref collisionHolderLevel1);

                    //check for more childs:
                    foreach(MapRectangle level2MapRectangle in collisionLevel2)
                    {
                        if(level2MapRectangle.ParentId == level1MapRectangle.Id)
                        {
                            //add this as a child
                            CollisionHolder collisionHolderLevel2 = new CollisionHolder();
                            CopyMapRectangleToCollisionHolder(level2MapRectangle, ref collisionHolderLevel2);

                            //check for more childs:
                            foreach (MapRectangle level3MapRectangle in collisionLevel3)
                            {
                                if (level3MapRectangle.ParentId == level2MapRectangle.Id)
                                {
                                    //add this as a child of level2
                                    CollisionHolder collisionHolderLevel3 = new CollisionHolder();
                                    CopyMapRectangleToCollisionHolder(level3MapRectangle, ref collisionHolderLevel3);

                                    //last leaves (from tree) should have a null Children
                                    collisionHolderLevel3.Children = null;

                                    collisionHolderLevel2.Children.Add(collisionHolderLevel3);
                                }
                            }

                            //add it
                            collisionHolderLevel1.Children.Add(collisionHolderLevel2);
                        }
                    }

                    //export it
                    data.collisionHolder.Children.Add(collisionHolderLevel1);
                }
            }


            string json = JsonConvert.SerializeObject(data);






            // write/flush the file.
            using (var w = new StreamWriter(dataFile))
            {
                w.Write(json);
            }

            // TODO: compress the file?



            //save all objects that are saveable and not part of the map (e.g. they can't move or change pictures.... ) - just the background
            //delete them from the visual part, screenshot a picture of how the visual field looks like without them and save it into background.jpg

            //save things like: player spawn, key spawner, teleport spawner, next map or FINISH

            //new project -> set coordinates
            //make the user control big again

            // TODO: add background
            // TODO: add collision rectangle-s ... blank rectangles, why not?

        }

        private static void CopyMapRectangleToCollisionHolder(MapRectangle level3MapRectangle, ref CollisionHolder collisionHolderLevel3)
        {
            collisionHolderLevel3.Id = level3MapRectangle.Id;
            collisionHolderLevel3.ParentId = level3MapRectangle.ParentId;
            collisionHolderLevel3.X = level3MapRectangle.X;
            collisionHolderLevel3.Y = level3MapRectangle.Y;
            collisionHolderLevel3.Height = level3MapRectangle.Height;
            collisionHolderLevel3.Width = level3MapRectangle.Width;
        }

        private static Rectangle GetRectangleWrapper(List<MapRectangle> mapRectangleList)
        {
            Rectangle rectangleWrapper = new Rectangle(mapRectangleList[0].X, mapRectangleList[0].Y,
                mapRectangleList[0].Width, mapRectangleList[0].Height);

            //get a rectangle that puts every collisionLevel2 in it
            foreach (MapRectangle mapRectangle in mapRectangleList)
            {
                if (mapRectangle.X < rectangleWrapper.X)
                    rectangleWrapper.X = mapRectangle.X;
                if (mapRectangle.Y < rectangleWrapper.Y)
                    rectangleWrapper.Y = mapRectangle.Y;
                if (mapRectangle.Width > rectangleWrapper.Width)
                    rectangleWrapper.Width = mapRectangle.Width;
                if (mapRectangle.Height > rectangleWrapper.Height)
                    rectangleWrapper.Height = mapRectangle.Height;
            }
            return rectangleWrapper;
        }
    }
}
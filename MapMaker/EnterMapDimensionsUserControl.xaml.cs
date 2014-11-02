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
    public partial class EnterMapDimensionsUserControl : UserControl
    {
        public EnterMapDimensionsUserControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //@TODO: try parse.
            MM.Instance.mapDimensionsChosen(int.Parse(MapDimensionsXTextBox.Text.ToString()), int.Parse(MapDimensionsYTextBox.Text.ToString()));
        }
    }
}

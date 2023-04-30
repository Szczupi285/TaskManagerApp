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
using System.Windows.Shapes;

namespace TaskManagerApp
{
    /// <summary>
    /// Logika interakcji dla klasy TaskWindow.xaml
    /// </summary>
    public partial class TaskWindow : Window
    {
        public TaskWindow()
        {
            InitializeComponent();
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < 20; i++)
            {
                // Sets the rowDefinition heigth automaticly
                RowDefinition rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Auto);
                CurrentNotes.RowDefinitions.Add(rowDef);

                TextBlock Note = new TextBlock();
                // read value from database
                Note.Text = $"";
                Note.Margin = new Thickness(10);
                
                Note.TextWrapping = TextWrapping.Wrap;
                // creates the frame and assign the properties
                Frame frame = new Frame();
                frame.Content = Note;
                frame.Margin = new Thickness(10);
                frame.BorderThickness = new Thickness(2);
                frame.BorderBrush = Brushes.Black;
                Grid.SetRow(frame, i);

                
                CurrentNotes.Children.Add(frame);
                
            }
            
        }
    }
}

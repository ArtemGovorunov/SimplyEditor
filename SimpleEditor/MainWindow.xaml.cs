using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace SimpleEditor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FontTypeCombobox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            FontSizeCombobox.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            object temp;
            temp = RichTextBox.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            FontTypeCombobox.SelectedItem = temp;
            temp = RichTextBox.Selection.GetPropertyValue(Inline.FontSizeProperty);
            FontSizeCombobox.Text = temp.ToString();
            RichTextBox.Focus();
        }

        private void ButtonImage_Click(object sender, RoutedEventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "insert image";
            fileDialog.Filter = "Файлы рисунков (*.bmp, *.jpg)|*.bmp;*.jpg|Все файлы (*.*)|*.*";
            if (fileDialog.ShowDialog() == true)
            {
                //    var clipBoardData = Clipboard.GetDataObject();
                //    BitmapImage bitmapImage = new BitmapImage(new Uri(fileDialog.FileName, UriKind.Absolute));
                //    Clipboard.SetImage(bitmapImage);
                //    RichTextBox.Paste();
                //    Clipboard.SetDataObject(clipBoardData);
                //}
                Uri uri = new Uri(fileDialog.FileName, UriKind.Absolute);
                BitmapImage bitmapImg = new BitmapImage(uri);
                Image image = new Image();
                image.Stretch = Stretch.Fill;
                image.Width = 50;
                image.Height = 50;
                image.Source = bitmapImg;

                BlockUIContainer container = new BlockUIContainer(image);
                RichTextBox.Document.Blocks.Add(container);

                image.Loaded += delegate
                {
                    AdornerLayer al = AdornerLayer.GetAdornerLayer(image);
                    if (al != null)
                    {
                        al.Add(new ResizingAdorner(image));
                    }
                };
            }
        }


        private void RichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            object temp;

            temp = RichTextBox.Selection.GetPropertyValue(Inline.FontWeightProperty);
            btnBold.IsChecked = (temp != null) && (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
            temp = RichTextBox.Selection.GetPropertyValue(Inline.FontStyleProperty);
            btnItalic.IsChecked = (temp != null) && (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
            temp = RichTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            btnUnderline.IsChecked = (temp != null) && (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));


            temp = RichTextBox.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            if (temp != null)
                FontTypeCombobox.SelectedItem = temp;
            temp = RichTextBox.Selection.GetPropertyValue(Inline.FontSizeProperty);
            if (temp != null)
                FontSizeCombobox.Text = temp.ToString();
        }

        private void FontTypeCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontTypeCombobox.SelectedItem != null)
            {
                //if (RichTextBox.Selection.Text != "")
                    RichTextBox.Selection.ApplyPropertyValue(FontFamilyProperty, FontTypeCombobox.SelectedItem);
                RichTextBox.Focus();
            }
        }
        private void FontSizeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeCombobox.SelectedItem != null)
            {
                //if (RichTextBox.Selection.Text != "")//Костыль от бага. При неполном выделении текста - меняет самопроизвольно размер шрифта
                    RichTextBox.Selection.ApplyPropertyValue(RichTextBox.FontSizeProperty, FontSizeCombobox.SelectedItem);
                RichTextBox.Focus();
            }
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text Files (*.txt)|*.txt|RichText Files (*.rtf)|*.rtf|XAML Files (*.xaml)|*.xaml|All files (*.*)|*.*";
            if (sfd.ShowDialog() == true)
            {
                TextRange doc = new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd);
                using (FileStream fs = File.Create(sfd.FileName))
                {
                    if (System.IO.Path.GetExtension(sfd.FileName).ToLower() == ".rtf")
                        doc.Save(fs, DataFormats.Rtf);
                    else if (System.IO.Path.GetExtension(sfd.FileName).ToLower() == ".xaml")
                        doc.Save(fs, DataFormats.Xaml);
                    else
                        doc.Save(fs, DataFormats.Text);
                }
            }
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text Files (*.txt)|*.txt|RichText Files (*.rtf)|*.rtf|XAML Files (*.xaml)|*.xaml|All files (*.*)|*.*";

            if (ofd.ShowDialog() == true)
            {
                TextRange doc = new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd);
                using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open))
                {
                    if (System.IO.Path.GetExtension(ofd.FileName).ToLower() == ".rtf")
                        doc.Load(fs, DataFormats.Rtf);
                    else if (System.IO.Path.GetExtension(ofd.FileName).ToLower() == ".xaml")
                        doc.Load(fs, DataFormats.Xaml);
                    else
                        doc.Load(fs, DataFormats.Text);
                }
            }
        }

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd).Text.Length != 0)
                if (MessageBox.Show("Создать новый документ? Все несохраненные изменения будут потеряны!", "Новый докуммент", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                    this.RichTextBox.Document.Blocks.Clear();
        }

        private void Print_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintDocument((((IDocumentPaginatorSource)RichTextBox.Document).DocumentPaginator), "Print from SimpleEditor");

            }
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region SearchText

        string searchKeY = string.Empty;
        TextRange searchRange;
        TextRange foundRange;
        private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(SearchTextBox.Text != searchKeY)
            {
                searchKeY = SearchTextBox.Text;
                searchRange = new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd);
                foundRange = FindTextInRange(searchRange, SearchTextBox.Text);
            }
            else
            {
                if(foundRange!=null)
                    searchRange = new TextRange(foundRange.End, RichTextBox.Document.ContentEnd);
                else
                    searchRange = new TextRange(RichTextBox.Document.ContentStart, RichTextBox.Document.ContentEnd);
                foundRange = FindTextInRange(searchRange, SearchTextBox.Text);
            }
            if (foundRange != null)
                RichTextBox.Selection.Select(foundRange.Start, foundRange.End);
            else
                MessageBox.Show("Поиск достиг конца доккумента", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public TextRange FindTextInRange(TextRange searchRange, string searchText)
        {
            int offset = searchRange.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);
            if (offset < 0)
                return null;  // Not found

            var start = GetTextPositionAtOffset(searchRange.Start, offset);
            TextRange result = new TextRange(start, GetTextPositionAtOffset(start, searchText.Length));

            return result;
        }

        TextPointer GetTextPositionAtOffset(TextPointer position, int characterCount)
        {
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    int count = position.GetTextRunLength(LogicalDirection.Forward);
                    if (characterCount <= count)
                    {
                        return position.GetPositionAtOffset(characterCount);
                    }

                    characterCount -= count;
                }

                TextPointer nextContextPosition = position.GetNextContextPosition(LogicalDirection.Forward);
                if (nextContextPosition == null)
                    return position;

                position = nextContextPosition;
            }

            return position;
        }
        #endregion
    }
}

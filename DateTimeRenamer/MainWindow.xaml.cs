using Microsoft.WindowsAPICodePack.Dialogs;
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

namespace DateTimeRenamer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //string tmp = "금 8 26 15:27:12 +09:00 2022";
            //var tmp2 = Convert.ToDateTime(tmp);
            //Console.WriteLine(tmp2);
        }


        public void SetAllowDrop(bool isAllowDrop)
        {
            this.AllowDrop = isAllowDrop;
        }


        private void Window_Drop(object sender, DragEventArgs e)
        {
            List<string> fileList = new List<string>();

            string[] fileDrops = (string[])e.Data.GetData(DataFormats.FileDrop);

            fileList = GetFileList(fileDrops, out string mainPath);

            if ((fileList.Count > 0) && !string.IsNullOrWhiteSpace(mainPath))
            {
                if ((ModeComboBox.SelectedIndex == 1) && (string.IsNullOrWhiteSpace(PathTextBox.Text) || !Directory.Exists(PathTextBox.Text)))
                {
                    CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                    dialog.IsFolderPicker = true;
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        PathTextBox.Text = dialog.FileName;
                        RenameFileList(fileList, mainPath, PathTextBox.Text, ModeComboBox.SelectedIndex, UseFileWriteTimeCheckBox.IsChecked.Value);
                    }
                }
                else
                {
                    RenameFileList(fileList, mainPath, PathTextBox.Text, ModeComboBox.SelectedIndex, UseFileWriteTimeCheckBox.IsChecked.Value);
                }
            }
        }

        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((PathTextBox != null) && (BrowseButton != null))
            {
                switch ((sender as ComboBox).SelectedIndex)
                {
                    case 1:
                        PathTextBox.IsEnabled  = true;
                        BrowseButton.IsEnabled = true;
                        break;
                    default:
                        PathTextBox.IsEnabled  = false;
                        BrowseButton.IsEnabled = false;
                        break;
                }
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                PathTextBox.Text = dialog.FileName;
            }
        }


        private List<string> GetFileList(string[] fileDrops, out string mainPath)
        {
            mainPath = string.Empty;

            List<string> fileList = new List<string>();

            foreach (var fileDrop in fileDrops)
            {
                FileAttributes attr = File.GetAttributes(fileDrop);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    string[] files = Directory.GetFiles(fileDrop, "*.*", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        //string ext = System.IO.Path.GetExtension(file).ToLower();

                        //if ((ext == ".jpg") || (ext == ".png") || (ext == ".mov") || (ext == ".mp4"))
                        //{
                        //    fileList.Add(file);
                        //}

                        fileList.Add(file);
                    }
                }
                else
                {
                    //string ext = System.IO.Path.GetExtension(fileDrop).ToLower();

                    //if ((ext == ".jpg") || (ext == ".png") || (ext == ".mov") || (ext == ".mp4"))
                    //{
                    //    fileList.Add(fileDrop);
                    //}

                    fileList.Add(fileDrop);
                }

                if (string.IsNullOrWhiteSpace(mainPath))
                {
                    mainPath = fileDrop.Replace("\\" + fileDrop.Split('\\').Last(), string.Empty);
                }
            }

            return fileList;
        }

        private void RenameFileList(List<string> fileList, string srcPath, string dstPath, int mode, bool useFileWriteTime)
        {
            if (fileList.Count > 0)
            {
                this.AllowDrop = false;

                ProgressWindow progressWindow = new ProgressWindow();
                progressWindow.Show();
                progressWindow.Rename(fileList, srcPath, dstPath, mode, useFileWriteTime);

                this.AllowDrop = true;
            }
        }
    }
}

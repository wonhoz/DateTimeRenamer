using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DateTimeRenamer
{
    /// <summary>
    /// ProgressWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProgressWindow : Window
    {
        public bool StopRename = false;


        public ProgressWindow()
        {
            InitializeComponent();
        }


        public void Rename(List<string> fileList)
        {
            Task.Run(() =>
            {
                int i       = 1;
                int success = 0;
                int fail    = 0;

                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    RenameProgressBar.Value = 0;
                    RenameTextBlock.Text = String.Empty;
                }));

                foreach (string file in fileList)
                {
                    if (StopRename)
                    {
                        break;
                    }

                    try
                    {
                        string ext = System.IO.Path.GetExtension(file);

                        //using (StreamWriter streamWriter = new StreamWriter(file.Replace(ext, ".txt"), false))
                        //{
                        //    IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(file);
                        //    foreach (var directory in directories)
                        //    {
                        //        foreach (var tag in directory.Tags)
                        //        {
                        //            if (tag.Name.ToLower().Contains("date") || tag.Name.ToLower().Contains("creat"))
                        //            {
                        //                streamWriter.WriteLine($"{directory.Name} - {tag.Name} = {tag.Description}");
                        //            }
                        //        }
                        //    }
                        //}

                        string dateTime = string.Empty;

                        switch (ext.ToLower())
                        {
                            // 그림 파일
                            case ".jpg":
                            case ".png":
                                dateTime = GetMetaData(file, "Exif SubIFD", "Date/Time Original");
                                if (!string.IsNullOrWhiteSpace(dateTime))
                                {
                                    dateTime = dateTime.Replace(":", "-");
                                    dateTime = dateTime.Remove(13, 1);
                                    dateTime = dateTime.Insert(13, ".");
                                    dateTime = dateTime.Remove(16, 1);
                                    dateTime = dateTime.Insert(16, ".");
                                }
                                break;
                            // 동영상 파일
                            case ".mov":
                            case ".mp4":
                                dateTime = GetMetaData(file, "QuickTime Metadata Header", "Creation Date");
                                if (!string.IsNullOrWhiteSpace(dateTime))
                                {
                                    string[] dateTimeSplit = dateTime.Split(' ');
                                    string month = dateTimeSplit[1];
                                    string day   = dateTimeSplit[2];
                                    if (month.Length == 1) month = "0" + month;
                                    if (day.Length   == 1) day   = "0" + day;
                                    dateTime = $"{dateTimeSplit[5]}-{month}-{day} {dateTimeSplit[3].Replace(":", ".")}";
                                }
                                if (string.IsNullOrWhiteSpace(dateTime))
                                {
                                    dateTime = GetMetaData(file, "QuickTime Movie Header", "Created");
                                    if (!string.IsNullOrWhiteSpace(dateTime))
                                    {
                                        string[] dateTimeSplit = dateTime.Split(' ');
                                        string month = dateTimeSplit[1];
                                        string day   = dateTimeSplit[2];
                                        if (month.Length == 1) month = "0" + month;
                                        if (day.Length   == 1) day   = "0" + day;
                                        //dateTime = $"{dateTimeSplit[4]}-{month}-{day} {dateTimeSplit[3].Replace(":", ".")}";
                                        dateTime = $"{dateTimeSplit[4]}-{month}-{day} {dateTimeSplit[3]}";

                                        var tmpDateTime = Convert.ToDateTime(dateTime);
                                        tmpDateTime = tmpDateTime.AddHours(9);
                                        dateTime = tmpDateTime.ToString("yyyy-MM-dd HH.mm.ss");
                                    }
                                }
                                break;
                        }

                        if (string.IsNullOrWhiteSpace(dateTime))
                        {
                            dateTime = GetMetaData(file, "File", "File Modified Date");
                            if (!string.IsNullOrWhiteSpace(dateTime))
                            {
                                string[] dateTimeSplit = dateTime.Split(' ');
                                string month = dateTimeSplit[1];
                                string day   = dateTimeSplit[2];
                                if (month.Length == 1) month = "0" + month;
                                if (day.Length   == 1) day   = "0" + day;
                                dateTime = $"{dateTimeSplit[5]}-{month}-{day} {dateTimeSplit[3].Replace(":", ".")}";
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(dateTime))
                        {
                            for (int j = 0; j < 1000; j++)
                            {
                                string filePath = file.Replace(file.Split('\\').Last(), dateTime + $"_{j}" + ext);
                                if (j == 0)
                                {
                                    filePath = file.Replace(file.Split('\\').Last(), dateTime + ext);
                                }

                                if (File.Exists(filePath))
                                {
                                    continue;
                                }
                                else
                                {
                                    File.Move(file, filePath);
                                    break;
                                }
                            }
                        }

                        //var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                        //var dateTime = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);
                        //Console.WriteLine($"{dateTime}");

                        success++;
                    }
                    catch
                    {
                        try
                        {
                            FileInfo fileInfo = new FileInfo(file);
                            string ext      = System.IO.Path.GetExtension(file);
                            string dateTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH.mm.ss");
                            for (int j = 0; j < 1000; j++)
                            {
                                string filePath = file.Replace(file.Split('\\').Last(), dateTime + $"_{j}" + ext);
                                if (j == 0)
                                {
                                    filePath = file.Replace(file.Split('\\').Last(), dateTime + ext);
                                }

                                if (File.Exists(filePath))
                                {
                                    continue;
                                }
                                else
                                {
                                    File.Move(file, filePath);
                                    break;
                                }
                            }

                            success++;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + "\n" + ex.StackTrace, "Date Time Renamer");

                            fail++;
                        }
                    }

                    if (!StopRename)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            RenameProgressBar.Value = i++ * 100 / fileList.Count;
                            RenameTextBlock.Text = file;
                        }));
                    }
                }

                System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    if (StopRename)
                    {
                        MessageBox.Show($"Upload Stopped! Success: {success}, Failure: {fail}", "Date Time Renamer");
                    }
                    else
                    {
                        MessageBox.Show($"Upload Done! Success: {success}, Failure: {fail}", "Date Time Renamer");
                    }

                    ((MainWindow)System.Windows.Application.Current.MainWindow).SetAllowDrop(true);
                    this.Close();
                }));
            });
        }

        private string GetMetaData(string filePath, string directoryName, string tagName)
        {
            IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(filePath);
            MetadataExtractor.Directory directory = directories.Where(s => string.Equals(s.Name, directoryName)).FirstOrDefault();

            if (directory == null) return string.Empty;

            MetadataExtractor.Tag tag = directory.Tags.Where(s => string.Equals(s.Name, tagName)).FirstOrDefault();

            if (tag == null) return string.Empty;

            return tag.Description;
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            StopRename = true;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopRename = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            StopRename = true;
        }
    }
}

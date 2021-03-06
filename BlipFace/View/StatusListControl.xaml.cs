﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using BlipFace.Model;
using BlipFace.Presenter;


namespace BlipFace.View
{
    public partial class StatusListControl : IStatusesView
    {
        private const int BlipSize = 160;
        private int charLeft = BlipSize;

        object  statusListLock = new object();

        private string pictureFilePath = string.Empty;

        private readonly StatusesPresenter presenter;

        public StatusListControl()
        {
            this.InitializeComponent();
        }

        public StatusListControl(StatusesPresenter _presenter)
        {
            this.InitializeComponent();

            // Insert code required on object creation below this point.

            presenter = _presenter;

            SetTextBoxFocus();
        }

        #region EventHandlers

        /// <summary>
        /// Służy tylko do wyliczania ilości znaków pozostałych do wpisania
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbMessage_TextChanged(object sender, TextChangedEventArgs e)
        {
            charLeft = BlipSize - tbMessage.Text.Length;
            if (charLeft < 0)
            {
                tblCharLeft.Foreground = new SolidColorBrush(Color.FromRgb(200, 100, 100));
                btnSendBlip.IsEnabled = false;
            }
            else if (charLeft == 0)
            {
                tblCharLeft.Foreground = new SolidColorBrush(Color.FromRgb(120, 180, 120));
                btnSendBlip.IsEnabled = true;
            }
            else
            {
                tblCharLeft.Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160));
                btnSendBlip.IsEnabled = true;
            }

            tblCharLeft.Text = charLeft.ToString();
        }

        /// <summary>
        /// Zdarzenie gdy naciśnięty zostanie w kontrolce tbMessage klawisz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Return)
            //{
            //    //gdy naciśnięto enter to wysyłamy tekst
            //    SendStatus();
            //}

            //e.Handled = true;
        }

        private void TbMessage_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SendStatus();

                e.Handled = true;
            }
            //gdy naciśnięto enter to wysyłamy tekst
        }

        private void SendStatus()
        {
            if (!string.IsNullOrEmpty(tbMessage.Text) && (tbMessage.Text.Length <= 160))
            {
                EnableContrlsForSendMessage(false);

                if (!string.IsNullOrEmpty(pictureFilePath))
                {
                    presenter.AddStatus(tbMessage.Text, pictureFilePath);
                }
                else
                {
                    presenter.AddStatus(tbMessage.Text);
                }
            }
            else if (tbMessage.Text.Length > 160)
            {
                MessageBox.Show("Status jest za długi");
                EnableContrlsForSendMessage(true);
            }
        }

        /// <summary>
        /// Handler dla kliknięca przycisku wysyłania tekstu dla blipa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSendBlip_Click(object sender, RoutedEventArgs e)
        {
            SendStatus();
        }


        /// <summary>
        /// Otwiera przeglądarkę gdy klikniemy na linka
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Hyperlink hl = (Hyperlink) sender;
            string navigateUri = hl.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;
        }

        private void btnAddPicture_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog opf = new System.Windows.Forms.OpenFileDialog();
            opf.Filter = "Obrazy (*.bmp, *.jpg, *.gif, *.png)|*.bmp; *.jpg; *.gif; *.png|All Files|*.*";
            opf.Title = "Wybierz obraz do załączenia do statusu";
            opf.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            opf.AutoUpgradeEnabled = true;


            if (opf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //todo: ustaw obrazek
                //opf.FileName

                PicturePath = opf.FileName;
            }
        }


        private void btnDeletePic_Click(object sender, RoutedEventArgs e)
        {
            PicturePath = string.Empty;
        }

        #endregion

        //todo: błysk okna testowo
        [DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        public static void FlashMainWindow(Window window, bool invert)
        {
            IntPtr handle = (new WindowInteropHelper(window)).Handle;
            FlashWindow(handle, invert);
        }

        #region IStatusesView

        public IList<StatusViewModel> Statuses
        {
            get { return lstbStatusList.ItemsSource as IList<StatusViewModel>; }
            set
            {
                //statusy będą ustawiane asynchronicznie przez prezetnera
                //więc potrzeba obiektu Dispatcher
                Dispatcher.Invoke(
                    new Action<IList<StatusViewModel>>(
                        delegate(IList<StatusViewModel> statusesCollection)
                            {
                                lstbStatusList.ItemsSource = statusesCollection;

                                //todo:to tak testowo
                                FlashMainWindow(Window.GetWindow(this.Parent), true);
                            }), value);
            }
        }

        //todo: to może powinno być jako StatusViewModel

        private StatusViewModel mainStatus;
        public StatusViewModel MainStatus
        {
            get
            {

                return mainStatus;
            }
            set
            {
                //status będzie ustawiany asynchronicznie przez prezetnera
                //więc potrzeba obiektu Dispatcher
                Dispatcher.Invoke(new Action<StatusViewModel>(delegate(StatusViewModel status)
                                                                  {
                                                                      // lbUserLogin.Text = status.UserLogin;
                                                                      // lbContent.Text = status.Content;

                                                                      mainStatus = status;

                                                                      status.UserLogin = string.Empty;
                                                                      statusContent.BoundStatus = status;

                                                                      if (imgUserAvatar.Source == null)
                                                                      {
                                                                          BitmapImage imgAvatar = new BitmapImage();
                                                                          imgAvatar.BeginInit();
                                                                          imgAvatar.UriSource =
                                                                              new Uri(status.UserAvatar50);
                                                                          imgAvatar.EndInit();
                                                                          imgUserAvatar.Source = imgAvatar;
                                                                      }
                                                                  }), value);
            }
        }

        public string TextMessage
        {
            get { return tbMessage.Text; }
            set
            {
                //tekst wiadomości ustawiany asynchronicznie przez prezetnera
                //więc potrzeba obiektu Dispatcher
                Dispatcher.Invoke(new Action<string>(delegate(string textMessage)
                                                         {
                                                             tbMessage.Text = textMessage;
                                                             tbMessage.IsEnabled = true;
                                                             tbShowSave.Visibility = Visibility.Hidden;

                                                             SetTextBoxFocus();
                                                         }), value);
            }
        }


        public Exception Error
        {
            get { throw new NotImplementedException(); }
            set
            {
                Dispatcher.Invoke(
                    new Action<Exception>(delegate(Exception err)
                                              {
                                                  tbError.Visibility = Visibility.Visible;
                                                  tbError.ToolTip = "Błąd: " + err.Message;
                                                  EnableContrlsForSendMessage(true);
                                              }), System.Windows.Threading.DispatcherPriority.Normal, value);
            }
        }

        public TitleMessageViewModel ConnectivityStatus
        {
            get { throw new NotImplementedException(); }
            set
            {
                Dispatcher.Invoke(
                    new Action<TitleMessageViewModel>(delegate(TitleMessageViewModel status)
                                                          {
                                                              //chowamy błędy 
                                                              tbError.Visibility = Visibility.Collapsed;

                                                              tbOffline.Text = status.Title;
                                                              tbOffline.ToolTip = status.Message;
                                                              EnableContrlsForSendMessage(true);
                                                          }), value);
            }
        }

        public void UpdateStatuses(IList<StatusViewModel> statuses)
        {
            Dispatcher.Invoke(
                new Action<IList<StatusViewModel>>(
                    delegate(IList<StatusViewModel> statusesList)
                        {

                               // var currentList = lstbStatusList.ItemsSource as ObservableCollection<StatusViewModel>;
                            var currentList = lstbStatusList.ItemsSource as IList<StatusViewModel>;
                                for (int i = statusesList.Count - 1; i >= 0; i--)
                                {
                                    currentList.Insert(0, statusesList[i]);
                                }

                                //for (int i =0 ; i <statusesList.Count; i++)
                                //{
                                //    currentList.Insert(0, statusesList[i]);
                                //}
                           
                            //todo:to tak testowo
                            FlashMainWindow(Window.GetWindow(this.Parent), true);
                        }), statuses);
        }

        public void AddStatus(StatusViewModel statusView, bool insertAtBeginning)
        {
            Dispatcher.Invoke(
                new Action<StatusViewModel>(
                    delegate(StatusViewModel status)
                    {

                        var currentList = lstbStatusList.ItemsSource as IList<StatusViewModel>;
                        //currentList.Insert(0, status);

                        if(insertAtBeginning)
                        {
                            currentList.Insert(0, status);
                        }
                        else
                        {

                        currentList.Add(status);
                        }
                        //todo:to tak testowo
                        FlashMainWindow(Window.GetWindow(this.Parent), true);
                    }), statusView);
        }


        public string PicturePath
        {
            get { return pictureFilePath; }
            set
            {
                Dispatcher.Invoke(new Action<string>(delegate(string picturePath)
                                                         {
                                                             if (string.IsNullOrEmpty(picturePath))
                                                             {
                                                                 btnDeletePic.Visibility = Visibility.Collapsed;
                                                                 imgAttachPic.Source = null;
                                                                 pictureFilePath = string.Empty;
                                                             }
                                                             else
                                                             {
                                                                 pictureFilePath = picturePath;

                                                                 Uri iconUri = new Uri(picturePath,
                                                                                       UriKind.RelativeOrAbsolute);

                                                                 imgAttachPic.Source = BitmapFrame.Create(iconUri);
                                                                 btnDeletePic.Visibility = Visibility.Visible;
                                                                 btnDeletePic.ToolTip = string.Format("Usuń obraz {0}",
                                                                                                      picturePath);
                                                             }
                                                         }), value);
            }
        }

        #endregion

        #region handlery dla kommend

        /// <summary>
        /// Komenda wywoływana gdy naciśniemy cytuj
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CiteUser_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StatusViewModel status = (StatusViewModel) e.Parameter;
            int position = tbMessage.SelectionStart;

            presenter.MakeCitation(status, tbMessage.Text, position);
            SetTextBoxFocus();
        }


        /// <summary>
        /// Komenda wywoływana gdy naciśniemy przycisk Odpowiedz przy statusie
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DirectMessage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StatusViewModel status = (StatusViewModel) e.Parameter;


            presenter.MakeDirectMessage(status, tbMessage.Text);

            SetTextBoxFocus();
        }


        /// <summary>
        /// Komenda wywoływana gdy naciśniemy przycisk Prywatna przy statusie,
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrivateMessage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StatusViewModel status = (StatusViewModel) e.Parameter;


            presenter.MakePrivateMessage(status, tbMessage.Text);

            SetTextBoxFocus();
        }

        private void ShowPicture_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //throw new NotImplementedException();


            //Image img = new Image();
            //img.Source = ((Image) e.Parameter).Source;
            
            //BitmapImage imgAvatar = new BitmapImage();
            //imgAvatar.BeginInit();

            //string picUrl = e.Parameter.ToString();
            
            //imgAvatar.UriSource = new Uri(picUrl);
            //imgAvatar.EndInit();
            //img.Source = imgAvatar;


            BigPictureWindow w = new BigPictureWindow();

            //w.WindowStyle = WindowStyle.ToolWindow;
            //w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //w.Background = new SolidColorBrush(Colors.Black);

            //w.Width = SystemParameters.WorkArea.Width*0.9;
            //w.Height = SystemParameters.WorkArea.Height * 0.9;
            //w.Content = img;

            
            w.PictureSource = ((Image)e.Parameter).Source;
            w.PictureUrl = (string)((Image)e.Parameter).Tag;
            w.Show();
        }

        #endregion

        #region metody prywatne

        /// <summary>
        /// Pomocnicza metoda zawierająca w sobie logikę widoku
        /// przy dodawaniu, wiadomości
        /// true - oznacza że można pokazać i aktywować poszczególene części widoku
        /// zaangażowane w wizualizację wysyłania widomości
        /// </summary>
        /// <param name="show"></param>
        private void EnableContrlsForSendMessage(bool enable)
        {
            tbShowSave.Visibility = enable ? Visibility.Hidden : Visibility.Visible;
            tbMessage.IsEnabled = enable;
            btnSendBlip.IsEnabled = enable;

            //SetTextBoxFocus();
        }

        private void SetTextBoxFocus()
        {
            if (tbMessage.Text.Length > 0)
            {
                tbMessage.Select(tbMessage.Text.Length, 0);
            }
            tbMessage.Focus();
        }

        #endregion

        #region IStatusesView Members

        #endregion
    }
}
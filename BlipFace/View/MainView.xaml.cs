using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BlipFace.Service.Communication;
using BlipFace.Service.Entities;
using BlipFace.View;
using BlipFace.Presenter;
using BlipFace.Model;
using System.Windows.Threading;

namespace BlipFace.View
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainView : Window, IMainView
    {

        private const int BlipSize = 160;
        int charLeft = BlipSize;

        private MainPresenter preseneter;

        public MainView()
        {
            InitializeComponent();

            preseneter = new MainPresenter(this);
        }


        /// <summary>
        /// S�u�y tylko do wyliczania ilo�ci znak�w pozosta�ych do wpisania
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
        /// Zdarzenie gdy naci�ni�ty zostanie w kontrolce tbMessage klawisz
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                //gdy naci�ni�to enter to wysy�amy tekst

                EnableContrlsForSendMessage(false);
                
                preseneter.AddStatus(tbMessage.Text);
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            

            // preseneter.LoadStatuses();

            preseneter.LoadUserDashboard("blipface");

            preseneter.StartListeningForUpdates(90);

        }

        /// <summary>
        /// Handler dla klikni�ca przycisku wysy�ania tekstu dla blipa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSendBlip_Click(object sender, RoutedEventArgs e)
        {
            EnableContrlsForSendMessage(false);
           
            preseneter.AddStatus(tbMessage.Text);
        }


        #region IMainView Members

        public IList<StatusViewModel> Statuses
        {
            get
            {
                return lstbStatusList.ItemsSource as IList<StatusViewModel>;
            }
            set
            {

                //statusy b�d� ustawiane asynchronicznie przez prezetnera
                //wi�c potrzeba obiektu Dispatcher
                Dispatcher.Invoke(new Action<IList<StatusViewModel>>(delegate(IList<StatusViewModel> statusesCollection)
                {
                    lstbStatusList.ItemsSource = statusesCollection;

                }), value);
            }
        }

        //todo: to mo�e powinno by� jako StatusViewModel
        public StatusViewModel MainStatus
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string TextMessage
        {
            get
            {
                return tbMessage.Text;
            }
            set
            {
                
                //tekst wiadomo�ci ustawiany asynchronicznie przez prezetnera
                //wi�c potrzeba obiektu Dispatcher
                Dispatcher.Invoke(new Action<string>(delegate(string textMessage)
                {
                    tbMessage.Text = textMessage;
                    tbMessage.IsEnabled = true;
                    lbShowSave.Visibility = Visibility.Hidden;
                }), value);
            }
        }




       
        public Exception Error
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                //Application.Current.Dispatcher.Invoke(DispatcherPriority.Send,
                //    (DispatcherOperationCallback)delegate(object arg)
                //    {

                //        Exception ex = (Exception)arg;
                //        //throw new Exception(ex.Message,ex);
                //        MessageBox.Show(ex.Message);
                //    }
                //    , value);

                Dispatcher.Invoke(
                    new Action<Exception>(delegate(Exception _err)
                {
                    //throw new Exception(_err.Message, _err);
                    MessageBox.Show(_err.Message);

                    EnableContrlsForSendMessage(true);

                }), System.Windows.Threading.DispatcherPriority.Normal, value);
            }
        }


        /// <summary>
        /// Pomocnicza metoda zawieraj�ca w sobie logik� widoku
        /// przy dodawaniu, wiadomo�ci
        /// true - oznacza �e mo�na pokaza� i aktywowa� poszczeg�lene cz�ci widoku
        /// zaanga�owane w wizualizacj� wysy�ania widomo�ci
        /// </summary>
        /// <param name="show"></param>
        private void EnableContrlsForSendMessage(bool enable)
        {

            lbShowSave.Visibility = enable ? Visibility.Hidden: Visibility.Visible;
            tbMessage.IsEnabled = enable;
            btnSendBlip.IsEnabled = enable;
        }

        #endregion


        
    }
}

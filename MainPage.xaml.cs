using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Devices.Radio;
using System.Windows.Threading;
using Windows.Phone.Media.Devices;

namespace WP_FMRadio
{
    public partial class MainPage : PhoneApplicationPage
    {
        FMRadio wpRadio;
        DispatcherTimer dispatcherTimer;
        bool HeadphonePlugged = false;
        public MainPage()
        {
            InitializeComponent();
            AudioRoutingManager.GetDefault().AudioEndpointChanged += AudioEndpointChanged;

            var AudioEndPoint = AudioRoutingManager.GetDefault().GetAudioEndpoint();
            switch (AudioEndPoint)
            {
                case AudioRoutingEndpoint.WiredHeadset:
                    HeadphonePlugged = true;
                    break;
                case AudioRoutingEndpoint.WiredHeadsetSpeakerOnly:
                    HeadphonePlugged = false;
                    break;
            }

            wpRadio = FMRadio.Instance;
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            Double FrequenciaAnterior = 87.9;
            PanoramaItem ItemAnterior = null;
            FrequenciaAnterior = wpRadio.Frequency;
            for (double d = 87.9; d < 107.9; d = d + 0.2)
            {
                PanoramaItem item = new PanoramaItem();
                item.Header = d.ToString("0.0");
                item.Margin = new Thickness(0, 270, 0, 0);
                item.HeaderTemplate = Resources["MyItemHeaderTemplate"] as DataTemplate;
                item.Orientation = System.Windows.Controls.Orientation.Horizontal;
                Alternador.Items.Add(item);
                if (d.ToString("0.0") == FrequenciaAnterior.ToString("0.0"))
                    ItemAnterior = item;
            }
            if (null != ItemAnterior)
                Alternador.DefaultItem = ItemAnterior;
        }

        private void setFrequencia(double Frequencia)
        {
            FrequenciaSelecionada.Text = Frequencia.ToString();
            trocaStatusBotao();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            double FrequenciaAnterior = wpRadio.Frequency;
            setFrequencia(FrequenciaAnterior);
            double AlternadorAnterior = Convert.ToDouble(((PanoramaItem)Alternador.SelectedItem).Header);
            if (AlternadorAnterior.ToString("0.0") != FrequenciaAnterior.ToString("0.0"))
            {
                foreach (PanoramaItem i in Alternador.Items)
                {
                    double convertido = Convert.ToDouble(i.Header);
                    if (convertido.ToString("0.0") == FrequenciaAnterior.ToString("0.0"))
                        Alternador.DefaultItem = i;
                }
            }
            if (!HeadphonePlugged)
            {
                wpRadio.PowerMode = RadioPowerMode.Off;
            }
        }

        private void Alternador_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((null != e.AddedItems[0]) && (wpRadio.PowerMode == RadioPowerMode.On))
            {
                double frequencia = Convert.ToDouble(((PanoramaItem)e.AddedItems[0]).Header);
                wpRadio.Frequency = frequencia;
                setFrequencia(frequencia);
            }
        }

        private void btnLigaDesliga_Click(object sender, RoutedEventArgs e)
        {
            if (!HeadphonePlugged)
            {
                wpRadio.PowerMode = RadioPowerMode.Off;
                Dispatcher.BeginInvoke(new Action(() => MessageBox.Show("Ligue o seu Fone de Ouvidos.")));
            }
            else
            {
                if (wpRadio.PowerMode == RadioPowerMode.On)
                    wpRadio.PowerMode = RadioPowerMode.Off;
                else
                    wpRadio.PowerMode = RadioPowerMode.On;
                trocaStatusBotao();
            }
        }

        private void btnProximaRadio_Click(object sender, RoutedEventArgs e)
        {
            SintonizaFM(true);
        }

        private void btnVoltaRadio_Click(object sender, RoutedEventArgs e)
        {
            SintonizaFM(false);
        }

        private void SintonizaFM(bool Avanca)
        {
            Boolean Achou = false;
            while (!Achou)
            {
                if (wpRadio.PowerMode == RadioPowerMode.On)
                {
                    double frequencia = wpRadio.Frequency;
                    if (Avanca)
                    {
                        frequencia += 0.2;
                        if (frequencia >= 107.9)
                            frequencia = 87.9;
                    }
                    else
                    {
                        frequencia -= 0.2;
                        if (frequencia <= 87.9)
                            frequencia = 107.9;
                    }
                    wpRadio.Frequency = Convert.ToDouble(frequencia.ToString("0.00"));
                    if (wpRadio.SignalStrength >= 2.1)
                        Achou = true;
                }
                else
                    Achou = true;
            }
        }

        private void trocaStatusBotao()
        {
            if (wpRadio.PowerMode == RadioPowerMode.On) {
                btnLiga.Visibility = Visibility.Collapsed;
                btnDesliga.Visibility = Visibility.Visible;
            }
            else {
                btnLiga.Visibility = Visibility.Visible;
                btnDesliga.Visibility = Visibility.Collapsed; 
            }
        }

        public void AudioEndpointChanged(AudioRoutingManager sender, object args)
        {
            var AudioEndPoint = sender.GetAudioEndpoint();
            switch (AudioEndPoint)
            {
                case AudioRoutingEndpoint.WiredHeadset:
                    HeadphonePlugged = true;
                    break;
                case AudioRoutingEndpoint.WiredHeadsetSpeakerOnly:
                    HeadphonePlugged = false;
                    break;
            }
        }
    }
}
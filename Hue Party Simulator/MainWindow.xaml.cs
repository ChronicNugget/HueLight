using System;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Windows.Controls;

namespace Hue_Party_Simulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Used to control lights
        public LightControlConsumer LightController;

        // Used for canceling tasks that are running.
        public CancellationTokenSource tokenSource;
        public CancellationToken token;

        public MainWindow()
        {
            // Used to cancel tasks.
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;

            // Initialize LightController
            LightController = new LightControlConsumer();

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {            
            // Convert sending button to a button and get the text in the button.
            Button ButtonClicked = (Button)sender;
            bool DoLoop = ButtonClicked.Content.ToString().Contains("SendCommand");

            // IF we wanna do the loop   
            if (DoLoop)
            {
                // Make a new cancelation token now for the next loop iteration.
                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;

                // Change content of button so we know its running.
                ButtonClicked.Content = "Running...";

                // Start a new Task and begin it with a cancelation token to kill it later on.
                // The token is a threadsafe object that raises a cancelation event when we click the button and we
                // do not want a new loop to begin. Just think of it as like a guard at a prison.

                // They dont do anything but watch the inmates till the second one tries to get out and then they stop them.
                // In our case we watch the task that starts up but dont do anything till we try to stop the loop. 
                Task StartLoop = Task.Factory.StartNew(() =>
                {
                    // While we dont wanna cancel. This will be true till we click the button again.
                    while (!token.IsCancellationRequested)
                    {
                        LightController.CycleColorsOrReset();
                        Thread.Sleep(100);

                    }
                }, token);
            }

            // IF we dont wanna do the loop just reset back to default.

            if (!DoLoop)
            {
                // Cancel the loop of changing if it was currently running.
                tokenSource.Cancel();

                // Reset the light to default and change the button content back to what it was.
                // SendCommand is default content. This allows us to now restart the whole looping process over.

                // Reset Light Values here
                LightController.CycleColorsOrReset(true);

                // Reset Sender Cmd.
                ButtonClicked.Content = "SendCommmand";
                return;
            }
        }
    }
}
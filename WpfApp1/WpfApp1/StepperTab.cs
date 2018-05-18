using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.DeviceManagerUI;
using Thorlabs.MotionControl.KCube.DCServoCLI;
using Thorlabs.MotionControl.KCube.DCServoUI;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        //private GenericDeviceHolder.GenericDevice _genericDevice;

        private void Window_Closed(object sender, EventArgs e)
        {
            // disconnect any connected device
            DisconnectDevice();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            connectCCS();
            connectKDC();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            connectKDC();
        }

        private void connectKDC()
        {
            Cursor = Cursors.Wait;
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DeviceManager.RegisterLibrary(null, Path.Combine(path, "Thorlabs.MotionControl.KCube.DCServoUI.DLL"), "KCubeDCServoUI");

            // get list of devices
            DeviceManagerCLI.BuildDeviceList();
            // tell the device manager the Device Types we are interested in
            List<string> devices = DeviceManagerCLI.GetDeviceList();
            if (devices.Count == 0)
            {
                MessageBox.Show("No Devices");
                Cursor = Cursors.Arrow;
                return;
            }

            // populate the combo box
            _devices.ItemsSource = devices;
            _devices.SelectedIndex = 0;

            // get first serial number for example
            motorNO = devices[0];
            // create the device
            ConnectDevice(motorNO);
            Cursor = Cursors.Arrow;
        }

        private void _devices_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // get the serial number from the combobox
            string motorNO = _devices.SelectedItem as string;
            if (KDC101 != null && KDC101.DeviceID == motorNO)
            {
                // the device is already connected so leave it alone
                return;
            }

            // connect the device
            ConnectDevice(motorNO);
        }

        private void ConnectDevice(string motorNO)
        {
            // unload any currently connected device if not of the desired type
            if (KDC101 != null)
            {
                if (KDC101.DeviceID == motorNO)
                {
                    return;
                }
                DisconnectDevice();
            }

            // create the new device
            KDC101 = KCubeDCServo.CreateKCubeDCServo(motorNO);

            try
            {
                KDC101.Connect(motorNO);

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
                return;
            }

            // wait for settings to be initialized (on the channel)
            KDC101.WaitForSettingsInitialized(5000);
            
            // create user interface (WPF view) via the DeviceManager
            _contentControl.Content = KCubeDCServoUI.CreateLargeView(KDC101,DeviceConfiguration.DeviceSettingsUseOptionType.UseConfiguredSettings);

        }

        private void DisconnectDevice()
        {
            if (KDC101 !=null && KDC101.IsConnected)
            {
                KDC101.Disconnect(false);
                KDC101 = null;
            }
            // unregister devices before exit
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DeviceManager.UnregisterLibrary(null, Path.Combine(path, "Thorlabs.MotionControl.KCube.DCServoUI.DLL"), "KCubeDCServoUI");

        }
    }
}

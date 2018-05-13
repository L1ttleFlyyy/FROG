using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.DeviceManagerUI;


namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private GenericDeviceHolder.GenericDevice _genericDevice;

        private void Window_Closed(object sender, EventArgs e)
        {
            // disconnect any connected device
            DisconnectDevice();
            // unregister devices before exit
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DeviceManager.UnregisterLibrary(null, Path.Combine(path, "Thorlabs.MotionControl.KCube.DCServoUI.DLL"), "KCubeDCServoUI");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //TODO:window on loading
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DeviceManager.RegisterLibrary(null, Path.Combine(path, "Thorlabs.MotionControl.KCube.DCServoUI.DLL"), "KCubeDCServoUI");

            // get list of devices
            DeviceManagerCLI.BuildDeviceList();
            // tell the device manager the Device Types we are interested in
            List<string> devices = DeviceManagerCLI.GetDeviceList();
            if (devices.Count == 0)
            {
                MessageBox.Show("No Devices");
                return;
            }

            // populate the combo box
            _devices.ItemsSource = devices;
            _devices.SelectedIndex = 0;

            // get first serial number for example
            string serialNo = devices[0];
            // create the device
            ConnectDevice(serialNo);

            motorNO=serialNo;

        }

        private void _devices_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // get the serial number from the combobox
            string serialNo = _devices.SelectedItem as string;
            if (_genericDevice != null && _genericDevice.CoreDevice.DeviceID == serialNo)
            {
                // the device is already connected so leave it alone
                return;
            }

            // connect the device
            ConnectDevice(serialNo);
        }

        private void ConnectDevice(string serialNo)
        {
            // unload any currently connected device if not of the desired type
            if (_genericDevice != null)
            {
                if (_genericDevice.CoreDevice.DeviceID == serialNo)
                {
                    return;
                }
                DisconnectDevice();
            }

            // create the new device anonymously
            IGenericCoreDeviceCLI device = DeviceFactory.CreateDevice(serialNo);
            // create a generic device holder to hold the device
            GenericDeviceHolder devices = new GenericDeviceHolder(device);
            // NOTE channel 1 is always available as TCubes are treated as Single Channel devices
            // For Benchtops, check that the channel exists before accessing it;
            _genericDevice = devices[1];
            if (_genericDevice == null)
            {
                MessageBox.Show("Unknown Device Type");
                return;
            }

            // connect the device by accessing the core device functions
            try
            {
                _genericDevice.CoreDevice.Connect(serialNo);

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
                return;
            }

            // wait for settings to be initialized (on the channel)
            _genericDevice.Device.WaitForSettingsInitialized(5000);

            // create user interface (WPF view) via the DeviceManager
            IUIFactory factory = DeviceManager.GetUIFactory(_genericDevice.CoreDevice.DeviceID);
            IDeviceViewModel viewModel = factory.CreateViewModel(DisplayTypeEnum.Full, _genericDevice);
            viewModel.Initialize(DeviceConfiguration.DeviceSettingsUseOptionType.UseConfiguredSettings);

            // bind the view using the UI factory and attach it to our display
            _contentControl.Content = factory.CreateLargeView(viewModel);
        }

        private void DisconnectDevice()
        {
            if ((_genericDevice != null) && _genericDevice.CoreDevice.IsConnected)
            {
                _genericDevice.CoreDevice.Disconnect(false);
                _genericDevice = null;
            }

            if (KDC101 !=null && KDC101.IsConnected)
            {
                KDC101.Disconnect(false);
                KDC101 = null;
            }

        }
    }
}

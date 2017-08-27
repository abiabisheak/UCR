﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Providers;
using UCR.Models;
using UCR.Models.Devices;
using UCR.Models.Mapping;
using UCR.Models.Plugins;
using UCR.Utilities.Commands;
using UCR.ViewModels;

namespace UCR.Views.Controls
{
    /// <summary>
    /// Interaction logic for DeviceBindingControl.xaml
    /// </summary>
    public partial class DeviceBindingControl : UserControl
    {
        public static readonly DependencyProperty DeviceBindingProperty = DependencyProperty.Register("DeviceBinding", typeof(DeviceBinding), typeof(DeviceBindingControl), new PropertyMetadata(default(DeviceBinding)));
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(DeviceBindingControl), new PropertyMetadata(default(string)));

        // DDL Device number
        private ObservableCollection<ComboBoxItemViewModel> Devices { get; set; }

        // ContextMenu
        private ObservableCollection<ContextMenuItem> BindMenu { get; set; }

        
        
        public DeviceBindingControl()
        {
            LoadDeviceBinding();
            BindMenu = new ObservableCollection<ContextMenuItem>();
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DeviceBinding == null) return; // TODO Error logging
            DeviceBindingLabel.Content = Label;
            ReloadGui();
        }

        private void ReloadGui()
        {
            LoadDeviceList();
            LoadDeviceInputs();
            LoadContextMenu();
            LoadBindingName();
        }

        private void LoadBindingName()
        {
            if (DeviceBinding.IsBound)
            {
                BindButton.Content = DeviceBinding.BoundName();
            }
            else
            {
                BindButton.Content = "Click to enter bind mode";
            }
        }

        private void LoadDeviceList()
        {
            DeviceTypeBox.SelectedItem = DeviceBinding.DeviceType;
        }

        private void LoadDeviceInputs()
        {
            var devicelist = DeviceBinding.Plugin.GetDeviceList(DeviceBinding);
            Devices = new ObservableCollection<ComboBoxItemViewModel>();
            for(var i = 0; i < Math.Max(devicelist?.Count ?? 0, UCRConstants.MaxDevices); i++)
            {
                if (devicelist != null && i < devicelist.Count)
                {
                    Devices.Add(new ComboBoxItemViewModel(i + 1 + ". " + devicelist[i].Title, i));
                }
                else
                {
                    Devices.Add(new ComboBoxItemViewModel(i + 1+". N/A", i));
                }
                
            }

            ComboBoxItemViewModel selectedDevice = null;
            
            foreach (var comboBoxItem in Devices)
            {
                if (comboBoxItem.Value == DeviceBinding.DeviceNumber)
                {
                    selectedDevice = comboBoxItem;
                    break;
                }
            }
            if (selectedDevice == null)
            {
                selectedDevice = new ComboBoxItemViewModel(DeviceBinding.DeviceNumber+1+ ". N/A", DeviceBinding.DeviceNumber);
                Devices.Add(selectedDevice);
            }
            DeviceNumberBox.ItemsSource = Devices;
            DeviceNumberBox.SelectedItem = selectedDevice;
        }

        private void LoadContextMenu()
        {
            if (DeviceBinding == null) return;
            BuildContextMenu();
            Ddl.ItemsSource = BindMenu;
        }

        private void LoadDeviceBinding()
        {
            // TODO load device binding and update gui accordingly
        }

        private void BuildContextMenu()
        {
            BindMenu = new ObservableCollection<ContextMenuItem>();
            var device = DeviceBinding.Plugin.GetDevice(DeviceBinding);
            if (device == null) return;
            BindMenu = BuildMenu(device.Bindings);
        }

        private ObservableCollection<ContextMenuItem> BuildMenu(List<BindingInfo> bindingInfos)
        {
            var menuList = new ObservableCollection<ContextMenuItem>();
            if (bindingInfos == null) return menuList;
            foreach (var bindingInfo in bindingInfos)
            {
                RelayCommand cmd = null;
                if (bindingInfo.IsBinding)
                {
                    cmd = new RelayCommand(c =>
                    {
                        DeviceBinding.SetKeyTypeValue((int)bindingInfo.InputType, bindingInfo.InputIndex, bindingInfo.InputSubIndex);
                        LoadBindingName();
                    });
                }
                menuList.Add(new ContextMenuItem(bindingInfo.Title, BuildMenu(bindingInfo.SubBindings), cmd));
            }
            return menuList;
        }

        public DeviceBinding DeviceBinding
        {
            get { return (DeviceBinding)GetValue(DeviceBindingProperty); }
            set { SetValue(DeviceBindingProperty, value); }
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        private void DeviceNumberBox_OnSelected(object sender, RoutedEventArgs e)
        {
            if (DeviceNumberBox.SelectedItem == null) return;
            DeviceBinding.SetDeviceNumber(((ComboBoxItemViewModel)DeviceNumberBox.SelectedItem).Value);
            LoadContextMenu();
            LoadBindingName();
        }
    }
}
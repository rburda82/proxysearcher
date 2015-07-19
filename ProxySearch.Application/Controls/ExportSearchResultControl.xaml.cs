﻿using ProxySearch.Console.Code;
using ProxySearch.Console.Code.Settings;

namespace ProxySearch.Console.Controls
{
    /// <summary>
    /// Interaction logic for ExportDataControl.xaml
    /// </summary>
    public partial class ExportSearchResultControl : System.Windows.Controls.UserControl
    {
        public ExportSearchResultControl()
        {
            InitializeComponent();
        }

        public ExportSettings Settings
        {
            get
            {
                return Context.Get<AllSettings>().ExportSettings;
            }
        }
    }
}

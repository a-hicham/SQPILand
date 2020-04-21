using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;


namespace SqpiLand
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IConnectionObject dbConn;
        Model.DBModel myModel;

        public MainWindow()
        {
            if(!checkVisio())
            {
                MessageBox.Show("Visio wurde nicht gefunden. SQPILand wird geschlossen!");
                Application.Current.Shutdown();
            }
            //MessageBox.Show(Version);
            InitializeComponent();
            mainWindow.Title = "SQPILand " + getVersion();
        }

        private bool checkVisio()
        {
            Microsoft.Win32.RegistryKey regVersionString = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("Visio.Drawing\\CurVer");
            return null != regVersionString && null != Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(regVersionString.GetValue("") + "\\CLSID");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DBList.Items.Clear();
            bool msTrusted = (bool) MSDB.IsChecked && (bool) TrustedCheckBox.IsChecked;
            dbConn = ConnFactory.createConnection((bool)OracleDB.IsChecked ? "Oracle" : (bool)MSDB.IsChecked ? "MSSQL" : "PostgreSQL", ServerText.Text, (bool)OracleDB.IsChecked ? null : (bool)MSDB.IsChecked ? InitialDBText.Text : PostgreSQLDB.Text, msTrusted, !msTrusted ? UsernameText.Text : null, !msTrusted ? PasswordText.Password : null, (bool)OracleDB.IsChecked ? PortText.Text : (bool)MSDB.IsChecked ? "" : PortPostgreSQL.Text, SIDText.Text);

            //dbConn = OracleConn.GetInstance(ServerText.Text, PortText.Text, SIDText.Text, UsernameText.Text, PasswordText.Text);
            //DBList.Items.Add(ServerText.Text);
            //dbConn = DBConn.GetInstance(ServerText.Text, InitDbText.Text, (bool)TrustedCheckBox.IsChecked, !(bool)TrustedCheckBox.IsChecked ? UsernameText.Text : null, !(bool)TrustedCheckBox.IsChecked ? PasswordText.Text : null);
            IList<string> Tables = dbConn.GetMetaDatabases();
            foreach (string db in Tables)
                DBList.Items.Add(db);
        }

        private void ConnStringText_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            VisioDrawer.DrawModel(createModel(), (bool)ViewsCheckbox.IsChecked, (bool)ShowVisio.IsChecked, OutputFolder.Text, (bool)physNames.IsChecked, (bool)AllFieldsCheckbox.IsChecked);
        }

        private void TrustedCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private Version getVersion()
        {
                try
                {
                    return System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;
                }
                catch (Exception e)
                {
                    return null;
                }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private Model.DBModel createModel()
        {
            return dbConn.BuildModel(DBList.SelectedItem.ToString(), (bool)WithHistory.IsChecked);
        }

        private void XML_Export_Btn_Click(object sender, RoutedEventArgs e)
        {
            myModel = createModel();
            XElement elements = Tools.Utils.ModelToXML(myModel);
            elements.Save(@"C:\Visio\" + myModel.serverName + ".xml");
        }
    }
}

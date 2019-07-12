using System;
using System.Collections.Generic;
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
using System.Data.SQLite;
using Microsoft.Win32;
using System.IO;

namespace AndroidDBToNetDB
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string key = "password";
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            fileDialog.Filter = "所有文件(*.*)|*.*";
            if (fileDialog.ShowDialog().Value)
            {
                string[] names = fileDialog.FileNames;
                foreach(string name in names)
                {
                    FileInfo fileInfo = new FileInfo(name);
                    File.Delete(fileInfo.Directory + "\\" + fileInfo.Name + ".dectemp");
                    if (!encryptAndroid(name, key, fileInfo.Directory + "\\" + fileInfo.Name + ".dectemp"))
                    {
                        break;
                    }
                    File.Delete(name);
                    File.Move(fileInfo.DirectoryName + "\\" + fileInfo.Name + ".dectemp", name);

                    File.Delete(fileInfo.Directory + "\\" + fileInfo.Name + ".dectemp");
                    if(!decryptAndroid(name, key, fileInfo.Directory+"\\"+fileInfo.Name + ".dectemp"))
                    {
                        break;
                    }
                    File.Delete(name);
                    File.Move(fileInfo.DirectoryName + "\\" + fileInfo.Name + ".dectemp", name);

                    if (!encryptNet(name, key))
                    {
                        break;
                    }

                    if (!decryptNet(name, key))
                    {
                        break;
                    }
                }
            }

            
        }
        private bool encryptAndroid(string decryptDb, string key, string encryptDb)
        {
            bool result = true;
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(decryptDb);
                connection.Execute("ATTACH DATABASE '" + encryptDb + "' AS encrypted  KEY '"+key+"';");
                connection.ExecuteScalar<string>("SELECT sqlcipher_export('encrypted');");
                connection.Execute("DETACH DATABASE encrypted;");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                result = false;
            }
            if (connection != null)
                connection.Close();
            return result;
        }
        private bool decryptAndroid(string encryptDb, string key, string decryptDb)
        {
            bool result = true;
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(encryptDb, true, key);
                connection.Execute("ATTACH DATABASE '"+ decryptDb+"' AS plaintext KEY '';");
                connection.ExecuteScalar<string>("SELECT sqlcipher_export('plaintext');");
                connection.Execute("DETACH DATABASE plaintext;");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                result = false;
            }
            if(connection != null)
                connection.Close();
            return result;
        }
        private bool encryptNet(string db, string key)
        {
            bool result = true;
            SQLiteConnection connection = null;
            try
            {
                SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder();
                connectionStringBuilder.DataSource = db;
                connection = new SQLiteConnection(connectionStringBuilder.ToString());
                connection.Open();
                connection.ChangePassword(key);
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                result = false;
            }
            if (connection != null)
            {
                connection.Close();
            }
            return result;
        }
        private bool decryptNet(string db, string key)
        {
            bool result = true;
            SQLiteConnection connection = null;
            try
            {
                SQLiteConnectionStringBuilder connectionStringBuilder = new SQLiteConnectionStringBuilder();
                connectionStringBuilder.DataSource = db;
                connectionStringBuilder.Password = key;
                connection = new SQLiteConnection(connectionStringBuilder.ToString());
                connection.Open();
                connection.ChangePassword("");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                result = false;
            }
            if (connection != null)
            {
                connection.Close();
            }
            return result;
        }
    }
}

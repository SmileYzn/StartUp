using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace StartUp
{
    class DataMng
    {
        public bool SetRegSetting(string Setting, object Value)
        {
            try
            {
                RegistryKey keyHandle = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\MU_StartUp");

                keyHandle.SetValue(Setting, Value);
                keyHandle.Close();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return false;
        }

        public object GetRegSetting(string Setting)
        {
            try
            {
                RegistryKey keyHandle = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\MU_StartUp");

                var Result = keyHandle.GetValue(Setting);

                keyHandle.Close();

                return Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return null;
        }

        public object LoadRegSetting(string Setting, object DefaultValue)
        {
            object Result = GetRegSetting(Setting);

            if (Result == null)
            {
                SetRegSetting(Setting, DefaultValue);
                return DefaultValue;
            }

            return Result;
        }

        public bool SaveDataToXml(DataGridView dataGrid)
        {
            try
            {
                XDocument xmlDocument = new XDocument(new XElement("StartUp"));

                foreach (DataGridViewRow row in dataGrid.Rows)
                {
                    xmlDocument.Root.Add
                    (
                        new XElement
                        (
                            "Process",
                            new XAttribute("Run", row.Cells[1].Value),
                            new XAttribute("Delay", row.Cells[3].Value),
                            new XAttribute("Path", row.Cells[4].Value == null ? "" : row.Cells[4].Value),
                            new XAttribute("Parameters", row.Cells[5].Value == null ? "" : row.Cells[5].Value),
                            new XAttribute("WindowStyle", row.Cells[6].Value == null ? "" : row.Cells[6].Value)
                        )
                    );
                }

                xmlDocument.Save(Properties.Resources.Msg_FileName);

                return true;
            }
            catch
            {
                MessageBox.Show(Properties.Resources.Msg_FailSaveXML, Properties.Resources.Msg_Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return false;
        }

        public bool LoadDataFromXml()
        {
            FormMain Form = (FormMain)Application.OpenForms[0];

            try
            {
                if (File.Exists(Properties.Resources.Msg_FileName))
                {
                    XDocument xmlDocument = XDocument.Load(Properties.Resources.Msg_FileName);

                    foreach (XElement el in xmlDocument.Root.Elements())
                    {
                        if (el.Name.LocalName == "Process")
                        {
                            if (File.Exists(el.Attribute("Path").Value))
                            {
                                Form.dataGridViewMain.Rows.Add
                                (
                                    Form.dataGridViewMain.Rows.Count,
                                    el.Attribute("Run").Value,
                                    Properties.Resources.off,
                                    el.Attribute("Delay").Value,
                                    el.Attribute("Path").Value,
                                    el.Attribute("Parameters").Value,
                                    el.Attribute("WindowStyle").Value
                                );
                            }
                        }
                    }

                    return true;
                }
            }
            catch
            {
                MessageBox.Show(Properties.Resources.Msg_InvalidXML, Properties.Resources.Msg_Warning,MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            return false;
        }
    }
}

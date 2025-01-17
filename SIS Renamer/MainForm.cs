﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;


namespace SIS_Renamer
{
    public partial class MainForm : Form
    {
        String errorMsgNoSisFilesLoaded = "You must drag & drop SIS files to left listbox first!";
        String errorMsgSisNamesNotReady = "You must preview names before renaming!";
        String warningMsgTooManyFiles = "You loaded more than 20 files, this can cause the program to hang for a moment.\nDo you want to continue?";

        String sisNameWhenErrorHappens = "ERROR";

        public MainForm()
        {
            InitializeComponent();

            //sisPkgInfo x = new sisPkgInfo(@"E:\Windows\Documents\SISContents\Social_13143-480827\Social_Installer_O.pkg");
            //x.getData();

            listBoxSisFiles.AllowDrop = true;
        }

        public void showErrorMessage(String errorMsg)
        {
            MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void clearBothListboxes(object sender, EventArgs e)
        {
                listBoxSisFiles.Items.Clear();
                listBoxNewNames.Items.Clear();
        }

        private void listBoxSisFiles_DragDrop(object sender, DragEventArgs e)
        {
            //clear both listboxes if new dragdrop happened
            clearBothListboxes(null, null);

            //string to store file locations
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false); 
            
            for (int i = 0; i < s.Length; i++)
            {
                //add file locations to left listbox
                listBoxSisFiles.Items.Add(System.IO.Path.GetFullPath(s[i]));
            }
        }


        private void listBoxSisFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void buttonRenameFiles_Click(object sender, EventArgs e)
        {
            //no sis files loaded error
            if (listBoxSisFiles.Items.Count == 0)
            {
                showErrorMessage(errorMsgNoSisFilesLoaded);
                return;
            }

            //not previewed before renaming error
            if (listBoxNewNames.Items.Count == 0)
            {
                showErrorMessage(errorMsgSisNamesNotReady);
                return;
            }

            //should rename files if both listboxes have
            if (listBoxSisFiles.Items.Count == listBoxNewNames.Items.Count)
            {
                for (int a = 0; a < listBoxSisFiles.Items.Count; a++)
                {
                    //only rename sis file if it successfully parsed
                    if (listBoxNewNames.Items[a].ToString() != sisNameWhenErrorHappens)
                        Process.Start("CMD.exe", "/C ren \"" + listBoxSisFiles.Items[a].ToString() + "\" \"" + listBoxNewNames.Items[a].ToString().Replace(":", "") + "\"");
                }
            }
        }

        private void onlyFileName_CheckedChanged(object sender, EventArgs e)
        {
            if (onlyFileName.Checked)
            {
                showAppUID.Checked = false; showAppUID.Enabled = false;
                showVersionInfo.Checked = false; showVersionInfo.Enabled = false;
                showVendorName.Checked = false; showVendorName.Enabled = false;
                showInstallationType.Checked = false; showInstallationType.Enabled = false;
                showSupportedDevices.Checked = false; showSupportedDevices.Enabled = false;
                showAppType.Checked = false; showAppType.Enabled = false;
            }
            else
            {
                showAppUID.Checked = true; showAppUID.Enabled = true;
                showVersionInfo.Checked = true; showVersionInfo.Enabled = true;
                showVendorName.Checked = true; showVendorName.Enabled = true;
                showInstallationType.Checked = true; showInstallationType.Enabled = true;
                showSupportedDevices.Checked = true; showSupportedDevices.Enabled = true;
                showAppType.Checked = true; showAppType.Enabled = true;
            }
        }

        private void listBoxNewNames_DoubleClick(object sender, EventArgs e)
        {   
                string Renamed_SIS_Names = "";
                foreach (object item in listBoxNewNames.Items) Renamed_SIS_Names += item.ToString() + "\n";
                if (Renamed_SIS_Names != "") Clipboard.SetText(Renamed_SIS_Names);
                MessageBox.Show("New names for SIS files copied to clipboard");
        }

        private void disableNamingTemplateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (useNamingTemplateCheckBox.Checked)
            {
                onlyFileName.Checked = false;
                onlyFileName.Enabled = false;

                sisNamingTemplateTextBox.Enabled = true;

                showAppUID.Checked = false; showAppUID.Enabled = false;
                showVersionInfo.Checked = false; showVersionInfo.Enabled = false;
                showVendorName.Checked = false; showVendorName.Enabled = false;
                showInstallationType.Checked = false; showInstallationType.Enabled = false;
                showSupportedDevices.Checked = false; showSupportedDevices.Enabled = false;
                showAppType.Checked = false; showAppType.Enabled = false;

                fileNamePrefixTextBox.Enabled = false;
            }
            else
            {
                onlyFileName.Checked = false;
                onlyFileName.Enabled = true;

                sisNamingTemplateTextBox.Enabled = false;

                showAppUID.Checked = true; showAppUID.Enabled = true;
                showVersionInfo.Checked = true; showVersionInfo.Enabled = true;
                showVendorName.Checked = true; showVendorName.Enabled = true;
                showInstallationType.Checked = true; showInstallationType.Enabled = true;
                showSupportedDevices.Checked = true; showSupportedDevices.Enabled = true;
                showAppType.Checked = true; showAppType.Enabled = true;
               
                fileNamePrefixTextBox.Enabled = true;
            }
        }

        private void previewNames_Click(object sender, EventArgs e)
        {
            //warn when there is more than 20 sis files loaded
            if (listBoxSisFiles.Items.Count > 20)
            {
                DialogResult  userChoice = MessageBox.Show(warningMsgTooManyFiles, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (userChoice != DialogResult.Yes) return;
            }

            //error if sis list is empty
            if (listBoxSisFiles.Items.Count == 0)
            {
                showErrorMessage(errorMsgNoSisFilesLoaded);
                return;
            }

            listBoxNewNames.Items.Clear();
            foreach (string package in listBoxSisFiles.Items)
            {
                SISInfo sisInfo;
                //if any error happens while getting info
                try
                {
                    sisInfo = new SISInfo(package);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show("Exception: \n" + ex);
                    
                    //sis file cant be parsed, add error text instead
                    listBoxNewNames.Items.Add(sisNameWhenErrorHappens);
                    continue; //move to next sis in list
                }

                string temporaryNameString;

                if (useNamingTemplateCheckBox.Checked)
                {
                    temporaryNameString = sisNamingTemplateTextBox.Text;
                    temporaryNameString = temporaryNameString.Replace("{name}", sisInfo.appName);
                    temporaryNameString = temporaryNameString.Replace("{version}", sisInfo.appVersion);
                    temporaryNameString = temporaryNameString.Replace("{vendor}", sisInfo.vendorName);
                    temporaryNameString = temporaryNameString.Replace("{uid}", sisInfo.appUID);
                    temporaryNameString = temporaryNameString.Replace("{install_type}", sisInfo.installType);
                    temporaryNameString = temporaryNameString.Replace("{supported_devices}", sisInfo.supportedDevices);
                    temporaryNameString = temporaryNameString.Replace("{sis_type}", sisInfo.applicationType);

                    temporaryNameString += ".sis";
                }
                else
                {
                    string seperatorOfFileName = " - ";
                    string appVersionTmp, vendorNameTmp, appUIDTmp, installTypeTmp, supportedDevicesTmp, applicationTypeTmp;

                    //show version info check
                    if (!showVersionInfo.Checked) appVersionTmp = "";
                    else appVersionTmp = seperatorOfFileName + sisInfo.appVersion;

                    //show vendor name
                    if (!showVendorName.Checked) vendorNameTmp = "";
                    else vendorNameTmp = seperatorOfFileName + sisInfo.vendorName;

                    //show app UID
                    if (!showAppUID.Checked) appUIDTmp = "";
                    else appUIDTmp = seperatorOfFileName + sisInfo.appUID;

                    //show installation type (SA, PU, PP etc.)
                    if (!showInstallationType.Checked) installTypeTmp = "";
                    else installTypeTmp = seperatorOfFileName + sisInfo.installType;

                    //show supported devices
                    if (!showSupportedDevices.Checked) supportedDevicesTmp = "";
                    else supportedDevicesTmp = seperatorOfFileName + sisInfo.supportedDevices;

                    //show application type
                    if (!showAppType.Checked) applicationTypeTmp = "";
                    else applicationTypeTmp = " " + sisInfo.applicationType;

                    string prefixForFileName = fileNamePrefixTextBox.Text;
                    temporaryNameString = prefixForFileName + sisInfo.appName + appVersionTmp + vendorNameTmp + appUIDTmp + installTypeTmp + supportedDevicesTmp + applicationTypeTmp + ".sis";
                }

                //remove forbidden characters and double spaces
                temporaryNameString = temporaryNameString.Replace("  ", " ");
                temporaryNameString = temporaryNameString.Replace("<", "");
                temporaryNameString = temporaryNameString.Replace(">", "");
                temporaryNameString = temporaryNameString.Replace(":", "");
                temporaryNameString = temporaryNameString.Replace("\"", "");
                temporaryNameString = temporaryNameString.Replace("/", "");
                temporaryNameString = temporaryNameString.Replace("\\", "");
                temporaryNameString = temporaryNameString.Replace("|", "");
                temporaryNameString = temporaryNameString.Replace("?", "");
                temporaryNameString = temporaryNameString.Replace("*", "");
                temporaryNameString = temporaryNameString.Replace("\'", "");

                listBoxNewNames.Items.Add(temporaryNameString);
            }
        }
    }
}

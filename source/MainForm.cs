using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

namespace wib
{
    public partial class MainForm : Form
    {
        ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////
        /// DEFAULT VARIABLES FOR EXECUTION
        ////////////////////////////////////////////////////
            readonly String CurrentAppPath       = Directory.GetCurrentDirectory();
            readonly String DownloadPath         = Directory.GetCurrentDirectory() + "\\output";
            String OutputFileNameWithDate        = null;
            String CurrentFileStringMsg          = null;
            bool isRunAborted                    = false;
            BackgroundWorker bgworker            = null;
            Color CurrentTextColor               = Color.Black;
            readonly ImageDownloader IMObject    = new ImageDownloader();
            readonly Color CurrentTextColorError = Color.Red;
            readonly Color CurrentTextColorInfo  = Color.DarkSlateBlue;
            readonly Color CurrentTextColorOk    = Color.DarkGreen;
            int CurrentImageCount                = 0;
            bool SubLinkSearchBool               = false;
            String CurrentSearchURL              = null;
            bool SearchDoneVar                   = false;
            List<string> searchlinkarray       = null;
            List<string> imagelinkarray        = null;
            bool doOnlyHiRes                   = false;

        ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////
        /// BACKGROUND PROCESS FOR EXECUTION
        ////////////////////////////////////////////////////
            void bgw_DoWork(object sender, DoWorkEventArgs e)
                {
                ///////////////////////////////////////////////////
                ///////////////////////////////////////////////////
                /// Execute if not Pending
                ///////////////////////////////////////////////////
                 if (!bgworker.CancellationPending)
                    {
                        ///////////////////////////////////////////////////
                        ///////////////////////////////////////////////////
                        /// If not searched, search...
                        ///////////////////////////////////////////////////
                        if (!SearchDoneVar)
                            {
                                    string tmpsearchurlwithoutend;
                                    if (CurrentSearchURL.IndexOf("/", 9) == -1)
                                    {
                                        tmpsearchurlwithoutend = CurrentSearchURL;
                                    }
                                    else
                                    {
                                        tmpsearchurlwithoutend = CurrentSearchURL.Substring(0, CurrentSearchURL.IndexOf("/", 9));
                                    }

                                ///////////////////////////////////////////////////
                                ///////////////////////////////////////////////////
                                /// Create Image Array without Subsites
                                ///////////////////////////////////////////////////
                                    if (!SubLinkSearchBool) {
                                        IMObject.getHtmlUrlAsString(CurrentSearchURL);
                                            if (!IMObject.getContext())
                                            {
                                                IMObject.ResetVars();
                                                CurrentFileStringMsg = "Failed to load URL!";
                                                CurrentTextColor = CurrentTextColorError;
                                                bgworker.ReportProgress(0, 0);
                                            }
                                            else
                                            {
                                                imagelinkarray = IMObject.getImageLinks(tmpsearchurlwithoutend);
                                            }
                                     }

                                ///////////////////////////////////////////////////
                                ///////////////////////////////////////////////////
                                /// Create Image Array with Subsites
                                ///////////////////////////////////////////////////
                                    if (SubLinkSearchBool) {
                                        IMObject.getHtmlUrlAsString(CurrentSearchURL);
                                            if (!IMObject.getContext())
                                            {
                                                IMObject.ResetVars();
                                                CurrentFileStringMsg = "Failed to load URL!";
                                                CurrentTextColor = CurrentTextColorError;
                                                bgworker.ReportProgress(0, 0);
                                            }
                                            else
                                            {
                                                searchlinkarray = IMObject.getSiteLinks(CurrentSearchURL, tmpsearchurlwithoutend);
                                                IMObject.ResetVars();

                                                ///////////////////////////////////////////////////
                                                ///////////////////////////////////////////////////
                                                /// Image Links From All Sites
                                                ///////////////////////////////////////////////////
                                                    int CurrentLinkPosition = 0;
                                                    while(CurrentLinkPosition < searchlinkarray.Count && !isRunAborted)
                                                    {
                                                        if (bgworker.CancellationPending) { break; }
                                                        IMObject.getHtmlUrlAsString(searchlinkarray[CurrentLinkPosition].ToString());
                                                            if(!IMObject.getContext())
                                                            {
                                                                IMObject.ResetVars();
                                                                CurrentFileStringMsg = "Failed to load Page: " + searchlinkarray[CurrentLinkPosition].ToString();
                                                                CurrentTextColor = CurrentTextColorError;
                                                                bgworker.ReportProgress(0, 0);
                                                            } else {
                                                                CurrentFileStringMsg = "Found: " + searchlinkarray[CurrentLinkPosition].ToString();
                                                                CurrentTextColor = CurrentTextColorOk;
                                                                bgworker.ReportProgress(0, 0);
                                                                imagelinkarray.AddRange(IMObject.getImageLinks(tmpsearchurlwithoutend));        
                                                            }
                                                
                                                        IMObject.ResetVars();
                                                        CurrentLinkPosition = CurrentLinkPosition + 1;
                                                    }
                                            }
                                        }

                            ///////////////////////////////////////////////////
                            ///////////////////////////////////////////////////
                            /// Last Steps
                            ///////////////////////////////////////////////////
                                CurrentFileStringMsg = "Linklist fetched successfully with "+ imagelinkarray.Count.ToString() + " Images Found!";
                                CurrentTextColor = CurrentTextColorOk;
                                bgworker.ReportProgress(0, 0);
                                IMObject.ResetVars();
                                SearchDoneVar = true;
                            }

                    ///////////////////////////////////////////////////
                    ///////////////////////////////////////////////////
                    /// Download Images Loop From ArrayList
                    ///////////////////////////////////////////////////
                        while (CurrentImageCount < imagelinkarray.Count && !isRunAborted)
                            {
                                ///////////////////////////////////////////////////
                                ///////////////////////////////////////////////////
                                /// Break If Requested
                                //////////////////////////////////////////////////////////////////////////////////////////////////
                                    if (bgworker.CancellationPending) { break; }

                                ///////////////////////////////////////////////////
                                ///////////////////////////////////////////////////
                                /// Download Image From URL
                                ///////////////////////////////////////////////////                                
                                        if (!IMObject.downloadAnImage(DownloadPath + "\\" + CurrentImageCount + " " + OutputFileNameWithDate, imagelinkarray[CurrentImageCount].ToString(), doOnlyHiRes))
                                        {
                                            IMObject.ResetVars();
                                            CurrentFileStringMsg = CurrentImageCount + ": Image Failed! : " + imagelinkarray[CurrentImageCount].ToString();
                                            CurrentTextColor = CurrentTextColorError;
                                            CurrentImageCount += 1;
                                            bgworker.ReportProgress(0, 0);
                                        }
                                        else
                                        {
                                            IMObject.ResetVars();
                                            CurrentFileStringMsg = CurrentImageCount + ": Image Downloaded";
                                            CurrentTextColor = CurrentTextColorOk;
                                            CurrentImageCount += 1;
                                            bgworker.ReportProgress(0, 0);
                                        }
                            }
                    }

                ///////////////////////////////////////////////////
                ///////////////////////////////////////////////////
                /// Stop the Worker
                ///////////////////////////////////////////////////
                    if (bgworker.CancellationPending) { isRunAborted = true; e.Cancel = true; }
            }

        ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////
        /// WORKER - PROGRESS HAS CHANGE
        ////////////////////////////////////////////////////
            void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
            {
                ///////////////////////////////////////////////////
                ///////////////////////////////////////////////////
                /// Enable Stop Button On Start of Process
                ///////////////////////////////////////////////////
                    stop_button.Enabled = true;

                if (isRunAborted)
                    {
                        ///////////////////////////////////////////////////
                        ///////////////////////////////////////////////////
                        /// Has been stopped by user
                        /////////////////////////////////////////////////// 
                            AddTextToRTB(status_textbox, "\n\nOperation stopped by User!\n\n\n", CurrentTextColorError);
                            stop_button.Enabled = true;
                            start_button.Enabled = true;
                    }   else {
                        ///////////////////////////////////////////////////
                        ///////////////////////////////////////////////////
                        /// Refresh Text on Download OK
                        ///////////////////////////////////////////////////
                            AddTextToRTB(status_textbox, CurrentFileStringMsg + "!\n", CurrentTextColor);
                    }
            }

        ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////
        /// Stop the Execution as User
        ////////////////////////////////////////////////////
            private void button2_Click(object sender, EventArgs e)
            {
                ///////////////////////////////////////////////////
                ///////////////////////////////////////////////////
                /// Set the Form Elements on Stop
                ///////////////////////////////////////////////////
                    stop_button.Enabled = false;
                    start_button.Enabled = false;
                    text_url.ReadOnly = false;
                    radio_nosub.Enabled = true;
                    radio_yessub.Enabled = true;
                    checkBox1.Enabled = true;

                ///////////////////////////////////////////////////
                /// Stop Background Worker
                    bgworker.CancelAsync();
            }

        ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////
        /// Process Finished
        ////////////////////////////////////////////////////
            private void bwAsync_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                if (!isRunAborted)
                {
                    ///////////////////////////////////////////////////
                    ///////////////////////////////////////////////////
                    /// Display Message that Process is Finished
                    ///////////////////////////////////////////////////
                        AddTextToRTB(status_textbox, "\n\nAll Done\n\n\n\n", CurrentTextColorOk);

                    ///////////////////////////////////////////////////
                    ///////////////////////////////////////////////////
                    /// Set the Form Elements after Execution Complete
                    /////////////////////////////////////////////////// 
                        text_url.ReadOnly = false;
                        radio_nosub.Enabled = true;
                        radio_yessub.Enabled = true;
                        stop_button.Enabled = false;
                        start_button.Enabled = true;
                        checkBox1.Enabled = true;
                }

                ///////////////////////////////////////////////////
                ///////////////////////////////////////////////////
                /// Activate Start Button On Finish of Execution
                /////////////////////////////////////////////////// 
                    start_button.Enabled = true;
            }

        ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////
        /// Check if URL is OK
        ////////////////////////////////////////////////////
            public bool CheckURLFunc(string url)
            {
                var request = HttpWebRequest.Create(url);
                request.Method = "HEAD";
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response != null)
                    {
                        return true;
                    } else
                    {
                        return false;
                    }
                }
            }

        ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////
        /// Start the Process and check URL
        ////////////////////////////////////////////////////
        private void button1_Click(object sender, EventArgs e)
            {
                //if (CheckURLFunc(text_url.Text))
                if (true)
                    {
                    ///////////////////////////////////////////////////
                    ///////////////////////////////////////////////////
                    /// Setup Form Elements at Start
                    ///////////////////////////////////////////////////
                        radio_nosub.Enabled = false;
                        radio_yessub.Enabled = false;
                        text_url.ReadOnly = true;
                        stop_button.Enabled = false;
                        start_button.Enabled = false;
                        checkBox1.Enabled = false;

                    ///////////////////////////////////////////////////
                    ///////////////////////////////////////////////////
                    /// Display Settings and Change Vars
                    ///////////////////////////////////////////////////
                        CurrentSearchURL = text_url.Text;
                        AddTextToRTB(status_textbox, "Search URL: " + CurrentSearchURL + "\n", CurrentTextColorInfo);
                        if (radio_yessub.Checked)
                        {
                            SubLinkSearchBool = true;
                            AddTextToRTB(status_textbox, " Sublinks will be searched for images!\n", CurrentTextColorInfo);
                        } else
                        {
                            SubLinkSearchBool = false;
                            AddTextToRTB(status_textbox, " Sublink search disabled!\n", CurrentTextColorInfo);
                        }
                        AddTextToRTB(status_textbox, "Please wait while fetching linklist...\n", CurrentTextColorInfo);

                        if (checkBox1.Checked) {
                            doOnlyHiRes = true;
                        } else {
                            doOnlyHiRes = false;
                        } 

                    ///////////////////////////////////////////////////
                    ///////////////////////////////////////////////////
                    /// Prepare DateTime Output Folder Var
                    /////////////////////////////////////////////////// 
                        DateTime datetvar = DateTime.Now;
                        String[] tmpformatvar = { "d", "D", "f", "F", "g", "G"};
                        OutputFileNameWithDate = datetvar.ToString(tmpformatvar[5], DateTimeFormatInfo.InvariantInfo);
                        OutputFileNameWithDate = OutputFileNameWithDate.Replace("/", "-");
                        OutputFileNameWithDate = OutputFileNameWithDate.Replace(":", "-");

                    ///////////////////////////////////////////////////
                    ///////////////////////////////////////////////////
                    /// Add Background Worker Events for Execution
                    ///////////////////////////////////////////////////
                        bgworker = null;
                        bgworker = new BackgroundWorker();
                        bgworker.DoWork -= new DoWorkEventHandler(bgw_DoWork);
                        bgworker.DoWork += new DoWorkEventHandler(bgw_DoWork);
                        bgworker.ProgressChanged -= new ProgressChangedEventHandler(bgw_ProgressChanged);
                        bgworker.ProgressChanged += new ProgressChangedEventHandler(bgw_ProgressChanged);
                        bgworker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(bwAsync_RunWorkerCompleted);
                        bgworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwAsync_RunWorkerCompleted);
                        bgworker.WorkerReportsProgress = true;
                        bgworker.WorkerSupportsCancellation = true;
                        isRunAborted = false;
                        CurrentImageCount = 0;
                        SearchDoneVar = false;
                        searchlinkarray = null;
                        imagelinkarray = null;
                        searchlinkarray = new List<string>();
                        imagelinkarray = new List<string>();

                    ///////////////////////////////////////////////////
                    ///////////////////////////////////////////////////
                    /// Start the Background Worker at Start
                    /////////////////////////////////////////////////// 
                        bgworker.RunWorkerAsync();
                }
                else
                {
                    ///////////////////////////////////////////////////
                    ///////////////////////////////////////////////////
                    /// Link not available
                    ///////////////////////////////////////////////////
                        AddTextToRTB(status_textbox, "The URL is invalid!\n", CurrentTextColorError);
                }
            }


        ///////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////
        /// Check Write Permissions in Executable Folder
        ///////////////////////////////////////////////////////
            private Boolean checkWritingAcces()
            {
                try {
                    using (FileStream fs = File.Create(
                        Path.Combine(
                            CurrentAppPath,
                            Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose)) { }
                    String TmpDirPath = Path.Combine(
                            CurrentAppPath,
                            Path.GetRandomFileName() );
                    Directory.CreateDirectory(TmpDirPath);
                    if (!Directory.Exists(TmpDirPath)) { return false;}
                    else {Directory.Delete(TmpDirPath); }
                    return true;
                }
                catch {return false;}
            }

        ////////////////////////////////////////////////////
        ////////////////////////////////////////////////////
        /// RichTextBox Append Colored Text
        ////////////////////////////////////////////////////
            private void AddTextToRTB(RichTextBox richtb, string textstring, Color ColorVariable)
            {
                if (textstring != null) {
                    int pos = richtb.TextLength;
                    richtb.AppendText(textstring);
                    richtb.Select(pos, textstring.Length);
                    richtb.SelectionColor = ColorVariable;
                    richtb.Select();
                    richtb.DeselectAll(); }
            }

        ///////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////
        /// Construction of the Main Form
        ///////////////////////////////////////////////////////
            public MainForm()
            {
                ///////////////////////////////////////////////////
                ///////////////////////////////////////////////////
                /// Initialize Components for Form
                ///////////////////////////////////////////////////         
                    InitializeComponent();

                ///////////////////////////////////////////////////
                ///////////////////////////////////////////////////
                /// Check Writing Access for Downloads
                ///////////////////////////////////////////////////
                    if (!checkWritingAcces()) {
                            MessageBox.Show("No write access!\nPlease restart as administrator or move Executable...");
                            Environment.Exit(0);}

                ///////////////////////////////////////////////////
                ///////////////////////////////////////////////////
                /// Create Image Dir If Not Exists in Exe Folder
                /////////////////////////////////////////////////// 
                    if (!Directory.Exists(DownloadPath))  {
                            Directory.CreateDirectory(DownloadPath);
                            AddTextToRTB(status_textbox, "Image Directory has been created!\n", CurrentTextColorInfo);  }

                ///////////////////////////////////////////////////
                ///////////////////////////////////////////////////
                /// Disable Stop Button on Start
                ///////////////////////////////////////////////////      
                    stop_button.Enabled = false;

                ///////////////////////////////////////////////////
                ///////////////////////////////////////////////////
                /// SSL Preconfig
                ///////////////////////////////////////////////////      
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                    | SecurityProtocolType.Tls11
                                    | SecurityProtocolType.Tls12
                                    | SecurityProtocolType.Ssl3;
        }

        ///////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////
        /// Go To Website If Clicked On Link (Open Browser)
        ///////////////////////////////////////////////////////
        private void label2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http:/bugfishtm.de");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://bugfishtm.de/distributions/code/app/242");
        }
    }
}

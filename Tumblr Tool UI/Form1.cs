﻿/* 01010011 01101000 01101001 01101110 01101111  01000001 01101101 01100001 01101011 01110101 01110011 01100001
 *
 *  Project: Tumblr Tools - Image parser and downloader from Tumblr blog system
 *
 *  Author: Shino Amakusa
 *
 *  Created: 2013
 *
 *  Last Updated: December, 2014
 *
 * 01010011 01101000 01101001 01101110 01101111  01000001 01101101 01100001 01101011 01110101 01110011 01100001 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Tumblr_Tool.Common_Helpers;
using Tumblr_Tool.Enums;
using Tumblr_Tool.Image_Ripper;
using Tumblr_Tool.Managers;
using Tumblr_Tool.Properties;
using Tumblr_Tool.Tumblr_Objects;
using Tumblr_Tool.Tumblr_Stats;

namespace Tumblr_Tool
{
    public partial class mainForm : Form
    {
        public const string _CHECKINGCONNX = "Checking for Internet connection ...";

        public const string _DONE = "Done";
        public const string _DOWNLOADINGIMAGES = "Downloading images ...";
        public const string _DOWNLOADSTATUS = "Downloading: {0}";
        public const string _ERROR = "Error";

        public const string _FULLRESCAN = "Full Rescan";

        public const string _GETTINGBLOGINFO = "Getting Blog info ...";
        public const string _GETTINGSTATS = "Getting stats ...";

        public const string _INDEXINGPOSTS = "Indexing posts ...";
        public const string _INDEXINGSTATUS = "Indexing...";
        public const string _INITSTATUS = "Initializing";

        public const string _NEWESTONLY = "Newest Only";

        public const string _OPENSAVEFILE = "Opening save file ...";

        public const string _PERCENT = "{0}%";
        public const string _POSTCOUNT = "{0}/{1}";

        public const string _READINGLOG = "Reading log file ...";
        public const string _READYSTATUS = "Ready";

        public const string _SAVELOG = "Saving Log File ...";
        public const string _SIZE = "{0} {1}";

        public const string _UPDATINGLOG = "Updating Log File ...";

        public const string _VERSION = "1.1.4";

        public mainForm()
        {
            InitializeComponent();
            this.globalInit();
        }

        public mainForm(string file)
        {
            InitializeComponent();

            this.globalInit();

            this.txt_SaveLocation.Text = Path.GetDirectoryName(file);

            updateStatusText(_OPENSAVEFILE);

            openTumblrFile(file);
        }

        public AboutForm aboutForm { get; set; }

        public bool crawlDone { get; set; }

        public string currentImage { get; set; }

        public int currentPercent { get; set; }

        public int currentPostCount { get; set; }

        public int currentSelectedTab { get; set; }

        public decimal currentSize { get; set; }

        public bool disableOtherTabs { get; set; }

        public bool downloadDone { get; set; }

        public List<string> downloadedList { get; set; }

        public List<int> downloadedSizesList { get; set; }

        public string errorMessage { get; set; }

        public bool exit { get; set; }

        public bool fileDownloadDone { get; set; }

        public FileManager fileManager { get; set; }

        public bool isCancelled { get; set; }

        public List<string> notDownloadedList { get; set; }

        public ToolOptions options { get; set; }

        public OptionsForm optionsForm { get; set; }

        public Dictionary<string, parseModes> parseModesDict { get; set; }

        public bool readyDownload { get; set; }

        public ImageRipper ripper { get; set; }

        public string saveLocation { get; set; }

        public string status { get; set; }

        public TumblrBlog tumblrBlog { get; set; }

        public SaveFile tumblrLogFile { get; set; }

        public SaveFile tumblrSaveFile { get; set; }

        public TumblrStats tumblrStats { get; set; }

        public string tumblrURL { get; set; }

        public void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.aboutForm.ShowDialog();
        }

        public void addWorkStatusText(string str)
        {
            if (!this.txt_WorkStatus.Text.EndsWith(str))
            {
                this.txt_WorkStatus.Text += str;

                this.txt_WorkStatus.Update();
                this.txt_WorkStatus.Refresh();
            }
        }

        public void btn_Browse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog ofd = new FolderBrowserDialog())
            {
                DialogResult result = ofd.ShowDialog();
                if (result == DialogResult.OK)

                    this.txt_SaveLocation.Text = ofd.SelectedPath;
            }
        }

        public void btn_Crawl_Click(object sender, EventArgs e)
        {
            enableUI_Crawl(false);
            this.bar_Progress.Visible = true;
            this.lbl_PercentBar.Text = string.Empty;
            this.lbl_PercentBar.Visible = true;
            this.crawlDone = false;
            this.txt_WorkStatus.Clear();
            this.saveLocation = this.txt_SaveLocation.Text;
            this.tumblrURL = this.txt_TumblrURL.Text;

            this.lbl_PostCount.Visible = false;
            this.lbl_PostCount.ForeColor = Color.Black;
            this.bar_Progress.BarColor = Color.Black;
            this.lbl_PercentBar.ForeColor = Color.Black;
            this.lbl_PercentBar.Text = string.Empty;
            this.txt_WorkStatus.Visible = true;

            this.bar_Progress.Value = 0;

            this.lbl_Size.Text = string.Empty;
            this.lbl_Size.Visible = false;
            this.fileManager = new FileManager();
            this.ripper = new ImageRipper();

            if (checkFields())
            {
                if (!this.IsDisposed)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        updateStatusText(_INITSTATUS);
                        updateWorkStatusTextNewLine(_INITSTATUS);
                    });
                }

                this.crawl_Worker.RunWorkerAsync(ripper);

                this.crawl_UpdateUI_Worker.RunWorkerAsync(ripper);
            }
            else
            {
                this.btn_Start.Enabled = true;
                this.lbl_PostCount.Visible = false;
                this.lbl_Size.Visible = false;
                this.bar_Progress.Visible = false;
                this.img_DisplayImage.Visible = true;
                this.tab_TumblrStats.Enabled = true;
            }
        }

        public void btn_GetStats_Click(object sender, EventArgs e)
        {
            this.lbl_PostCount.Visible = false;
            this.bar_Progress.BarColor = Color.Black;
            this.lbl_PercentBar.ForeColor = Color.Black;
            this.lbl_PostCount.ForeColor = Color.Black;
            this.lbl_PercentBar.Text = string.Empty;
            this.tumblrURL = txt_Stats_TumblrURL.Text;

            updateStatusText(_INITSTATUS);
            if (isValidURL(this.tumblrURL))
            {
                enableUI_Stats(false);

                getStats_Worker.RunWorkerAsync();
                getStatsUI_Worker.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("Please enter valid url!", _ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                updateStatusText(_READYSTATUS);
            }
        }

        public void button_MouseEnter(object sender, EventArgs e)
        {
            Button button = sender as Button;

            button.UseVisualStyleBackColor = false;
            button.ForeColor = Color.Maroon;
            button.FlatAppearance.BorderColor = Color.Maroon;
            button.FlatAppearance.MouseOverBackColor = Color.White;
            button.FlatAppearance.BorderSize = 1;
        }

        public void button_MouseLeave(object sender, EventArgs e)
        {
            Button button = sender as Button;
            button.UseVisualStyleBackColor = true;
            button.ForeColor = Color.Black;
            button.FlatAppearance.BorderSize = 0;
        }

        public bool checkFields()
        {
            bool saveLocationEmpty = string.IsNullOrEmpty(this.saveLocation);
            bool urlValid = true;

            if (saveLocationEmpty)
            {
                MessageBox.Show("Save Location cannot be left empty! \r\nSelect a valid location on disk", _ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                enableUI_Crawl(true);
                this.btn_Browse.Focus();
            }
            else
            {
                if (!isValidURL(this.tumblrURL))
                {
                    MessageBox.Show("Please enter valid url!", _ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.txt_TumblrURL.Focus();
                    enableUI_Crawl(true);
                    urlValid = false;
                }
            }

            return (!saveLocationEmpty && urlValid);
        }

        public void colorizeProgressBar(int value)
        {
            switch (value / 10)
            {
                case 0:
                    this.bar_Progress.BarColor = Color.Gainsboro;

                    break;

                case 1:
                    this.bar_Progress.BarColor = Color.Gainsboro;
                    break;

                case 2:
                    this.bar_Progress.BarColor = Color.Red;
                    break;

                case 3:
                    this.bar_Progress.BarColor = Color.Green;
                    break;

                case 4:
                    this.bar_Progress.BarColor = Color.Silver;
                    break;

                case 5:
                    this.bar_Progress.BarColor = Color.Gray;
                    break;

                case 6:
                    this.bar_Progress.BarColor = Color.DimGray;
                    break;

                case 7:
                    this.bar_Progress.BarColor = Color.SlateGray;
                    break;

                case 8:
                    this.bar_Progress.BarColor = Color.DarkSlateGray;
                    break;

                case 9:
                    this.bar_Progress.BarColor = Color.Black;
                    break;

                default:
                    this.bar_Progress.BarColor = Color.YellowGreen;
                    break;
            }

            this.bar_Progress.Update();
            this.bar_Progress.Refresh();
        }

        public void crawlWorker_AfterDone(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (this.ripper != null)
                {
                    this.ripper.statusCode = processingCodes.Done;
                    this.crawlDone = true;

                    if (this.ripper.statusCode == processingCodes.Done)
                    {
                        this.tumblrSaveFile.blog = this.ripper.blog;
                        //tumblrLogFile = null;
                        //ripper.tumblrPostLog = null;

                        this.tumblrLogFile = this.ripper.tumblrPostLog;

                        this.crawlDone = true;

                        if (this.crawlDone && !this.optionsForm.parseOnly && !this.isCancelled)
                        {
                            this.fileManager.totalToDownload = this.ripper.imageList.Count;

                            this.download_UIUpdate_Worker.RunWorkerAsync();

                            this.download_Worker.RunWorkerAsync(ripper.imageList);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public void crawlWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Thread.Sleep(200);

                this.downloadedList = new List<string>();
                this.downloadedSizesList = new List<int>();
                this.readyDownload = false;

                lock (this.ripper)
                {
                    if (this.tumblrSaveFile != null && this.options.generateLog)
                    {
                        string file = this.saveLocation + @"\" + Path.GetFileNameWithoutExtension(this.tumblrSaveFile.fileName) + ".log";

                        if (File.Exists(file))
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                updateStatusText(_READINGLOG);
                                updateWorkStatusTextNewLine(_READINGLOG);
                            });

                            this.tumblrLogFile = fileManager.readTumblrFile(file);

                            this.Invoke((MethodInvoker)delegate
                            {
                                updateWorkStatusTextConcat(_READINGLOG, " ok");
                            });
                        }
                    }

                    this.tumblrBlog = new TumblrBlog();
                    this.tumblrBlog.url = this.tumblrURL;

                    this.ripper = new ImageRipper(this.tumblrBlog, this.saveLocation, this.optionsForm.generateLog, this.optionsForm.parsePhotoSets,
                        this.optionsForm.parseJPEG, this.optionsForm.parsePNG, this.optionsForm.parseGIF, 0);
                    ripper.tumblrPostLog = this.tumblrLogFile;
                    this.ripper.statusCode = processingCodes.Initializing;
                }

                lock (this.ripper)
                {
                    this.ripper.statusCode = processingCodes.checkingConnection;
                }

                if (WebHelper.checkForInternetConnection())
                {
                    lock (this.ripper)
                    {
                        this.ripper.statusCode = processingCodes.connectionOK;
                    }

                    if (this.ripper != null)
                    {
                        this.ripper.setAPIMode(options.apiMode);
                        this.ripper.setLogFile(tumblrLogFile);

                        if (this.ripper.isValidTumblr())
                        {
                            lock (this.ripper)
                            {
                                this.ripper.statusCode = processingCodes.gettingBlogInfo;
                            }

                            if (this.ripper.setBlogInfo())
                            {
                                lock (this.ripper)
                                {
                                    this.ripper.statusCode = processingCodes.blogInfoOK;
                                }

                                if (!saveTumblrFile(this.ripper.blog.name))
                                {
                                    lock (this.ripper)
                                    {
                                        this.ripper.statusCode = processingCodes.saveFileError;
                                    }
                                }
                                else
                                {
                                    lock (this.ripper)
                                    {
                                        this.ripper.statusCode = processingCodes.saveFileOK;
                                    }

                                    if (this.ripper != null)
                                    {
                                        parseModes mode = 0;
                                        this.Invoke((MethodInvoker)delegate
                                        {
                                            mode = this.parseModesDict[this.select_Mode.SelectedItem.ToString()];
                                        });

                                        lock (this.ripper)
                                        {
                                            this.ripper.statusCode = processingCodes.Crawling;
                                        }

                                        this.tumblrBlog = this.ripper.parseBlogPosts(mode);

                                        lock (this.ripper)
                                        {
                                            if (this.ripper.logUpdated)
                                            {
                                                this.ripper.statusCode = processingCodes.SavingLogFile;

                                                if (!this.IsDisposed)
                                                {
                                                    this.Invoke((MethodInvoker)delegate
                                                    {
                                                        updateStatusText(_SAVELOG);
                                                        updateWorkStatusTextNewLine(_SAVELOG);
                                                    });
                                                }
                                                this.tumblrLogFile = this.ripper.tumblrPostLog;
                                                saveLogFile();

                                                if (!this.IsDisposed)
                                                {
                                                    this.Invoke((MethodInvoker)delegate
                                                    {
                                                        updateStatusText("Log Saved");
                                                        updateWorkStatusTextConcat(_SAVELOG, " ok");
                                                    });

                                                    this.ripper.statusCode = processingCodes.Done;
                                                }
                                            }
                                        }
                                    }
                                }

                                lock (this.ripper)
                                {
                                    this.ripper.statusCode = processingCodes.Done;
                                }
                            }
                            else
                            {
                                lock (this.ripper)
                                {
                                    this.ripper.statusCode = processingCodes.blogInfoError;
                                }
                            }
                        }
                        else
                        {
                            lock (this.ripper)
                            {
                                this.ripper.statusCode = processingCodes.invalidURL;
                            }
                        }
                    }
                }
                else
                {
                    lock (this.ripper)
                    {
                        this.ripper.statusCode = processingCodes.connectionError;
                    }
                }
            }
            catch
            {
                return;
            }
        }

        public void crawlWorker_UI__AfterDone(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!this.IsDisposed)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (this.optionsForm.parseOnly)
                    {
                        enableUI_Crawl(true);
                        this.lbl_PostCount.Visible = false;
                    }
                    else if (this.ripper.statusCode != processingCodes.Done)
                    {
                        enableUI_Crawl(true);
                    }
                    else
                    {
                        updateStatusText(_DONE);
                        this.bar_Progress.Value = 0;
                        this.lbl_PercentBar.Text = string.Empty;


                        this.lbl_PostCount.Text = string.Format(_POSTCOUNT, "0", this.ripper.imageList.Count);
                    }
                });
            }
        }

        public void crawlWorker_UI__DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (this.ripper != null)
                {
                    if (!this.IsDisposed)
                    {
                        this.Invoke((MethodInvoker)delegate
                                {
                                    this.img_DisplayImage.Image = Resources.crawling;
                                });
                    }

                    int percent = 0;

                    while (!this.crawlDone)
                    {
                        if (this.ripper.statusCode == processingCodes.checkingConnection)
                        {
                            
                                if (!this.IsDisposed)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        updateWorkStatusTextNewLine(_CHECKINGCONNX);
                                        updateStatusText(_INITSTATUS);
                                    });
                                }
                            
                        }

                        if (this.ripper.statusCode == processingCodes.connectionOK)
                        {
                            
                                if (!this.IsDisposed)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        updateWorkStatusTextConcat(_CHECKINGCONNX, " ok");
                                        updateWorkStatusTextNewLine("Starting ...");
                                    });
                                }
                            
                        }
                        if (this.ripper.statusCode == processingCodes.connectionError)
                        {
                            
                                if (!this.IsDisposed)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        updateStatusText(_ERROR);
                                        updateWorkStatusTextConcat(_CHECKINGCONNX, " fail");
                                        this.btn_Start.Enabled = true;
                                        this.lbl_PostCount.Visible = false;
                                        this.img_DisplayImage.Visible = true;
                                        this.img_DisplayImage.Image = Resources.tumblrlogo;
                                        this.tab_TumblrStats.Enabled = true;
                                        this.crawlDone = true;
                                    });
                                }
                            
                        }

                        if (this.ripper.statusCode == processingCodes.gettingBlogInfo)
                        {
                            
                                if (!this.IsDisposed)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        updateWorkStatusTextNewLine(_GETTINGBLOGINFO);
                                    });
                                }
                            
                        }

                        if (this.ripper.statusCode == processingCodes.blogInfoOK)
                        {
                            
                                if (!this.IsDisposed)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        updateWorkStatusTextConcat(_GETTINGBLOGINFO, " ok");

                                        this.lbl_PostCount.Visible = false;
                                        this.lbl_PostCount.Text = string.Empty;
                                        this.txt_WorkStatus.Visible = true;

                                        this.txt_WorkStatus.SelectionStart = this.txt_WorkStatus.Text.Length;
                                    });
                                }
                            
                        }

                        if (this.ripper.statusCode == processingCodes.Starting)
                        {
                            
                                if (!this.IsDisposed)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        updateWorkStatusTextNewLine("Indexing " + "\"" + this.ripper.blog.title + "\" ... ");
                                    });
                                }
                            
                        }

                        if (this.ripper.statusCode == processingCodes.UnableDownload)
                        {
                            
                                if (!this.IsDisposed)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        updateWorkStatusTextNewLine("Unable to get post info from API (Offset " + this.ripper.parsedPosts.ToString() + " - " + (ripper.parsedPosts + (int)postStepEnum.JSON).ToString() + ") ... ");
                                    });
                                }
                                ripper.statusCode = processingCodes.Crawling;
                            
                        }

                        if (this.ripper.statusCode == processingCodes.Crawling)
                        {
                            
                                if (this.ripper.totalPosts != 0 && this.ripper.parsedPosts == 0)
                                {
                                    if (!this.IsDisposed)
                                    {
                                        this.Invoke((MethodInvoker)delegate
                                        {
                                            updateWorkStatusTextNewLine(this.ripper.totalPosts + " photo posts found.");
                                        });
                                    }
                                }

                                if (!this.IsDisposed && this.ripper.parsedPosts == 0)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        updateWorkStatusTextNewLine(_INDEXINGPOSTS);
                                        updateStatusText(_INDEXINGSTATUS);
                                        this.lbl_PostCount.Text = string.Format(_POSTCOUNT, "0", this.ripper.totalPosts.ToString());
                                        this.lbl_PercentBar.Text = string.Format(_PERCENT, "0");
                                        this.lbl_PostCount.Visible = true;

                                    });
                                }

                                percent = this.ripper.percentComplete;

                                if (percent > 100)
                                    percent = 100;

                                if (this.currentPercent != percent)
                                {
                                    this.currentPercent = percent;
                                    if (!this.IsDisposed)
                                    {
                                        this.Invoke((MethodInvoker)delegate
                                           {
                                               this.lbl_PercentBar.Text = string.Format(_PERCENT, percent.ToString());
                                               this.bar_Progress.Value = percent;
                                           });
                                    }
                                }

                                if (this.currentPostCount != this.ripper.parsedPosts)
                                {
                                    this.currentPostCount = this.ripper.parsedPosts;
                                    if (!this.IsDisposed)
                                    {
                                        this.Invoke((MethodInvoker)delegate
                                        {
                                            this.lbl_PostCount.Text = string.Format(_POSTCOUNT, this.ripper.parsedPosts.ToString(), this.ripper.totalPosts.ToString());
                                            
                                        });
                                    }
                                }

                                
                                
                            
                        }
                    }

                    if (this.ripper != null)
                    {
                        if (!this.IsDisposed)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                updateWorkStatusTextConcat(_INDEXINGPOSTS, " ok");

                                updateWorkStatusTextNewLine("Found " + (this.ripper.imageList.Count == 0 ? "no" : this.ripper.imageList.Count.ToString()) + " new image(s) to download");

                                this.bar_Progress.Value = 0;
                            });
                        }

                        if (this.ripper.statusCode == processingCodes.UnableDownload)
                        {
                            if (!this.IsDisposed)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    updateWorkStatusTextNewLine("Error downloading the blog post XML");
                                    updateStatusText(_ERROR);
                                    enableUI_Crawl(true);
                                });
                            }
                        }
                        else if (this.ripper.statusCode == processingCodes.invalidURL)
                        {
                            if (!this.IsDisposed)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    updateWorkStatusTextNewLine("Invalid Tumblr URL");
                                    updateStatusText(_ERROR);
                                    enableUI_Crawl(true);
                                });
                            }
                        }
                    }
                }
            }
            catch
            {
                //
            }
        }

        public void downloadWorker_AfterDone(object sender, RunWorkerCompletedEventArgs e)
        {
            lock (this.fileManager)
            {
                this.fileManager.statusCode = downloadStatusCodes.Done;
            }

            try
            {
                this.tumblrSaveFile.blog.posts = null;
                saveTumblrFile(this.ripper.blog.name);

                if (this.options.generateLog && this.downloadedList.Count > 0)
                {
                    if (!this.IsDisposed)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            updateWorkStatusTextNewLine(_UPDATINGLOG);
                            this.lbl_PercentBar.Visible = false;
                            this.bar_Progress.Visible = false;
                        });
                    }

                    saveLogFile();

                    if (!this.IsDisposed)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            updateWorkStatusTextConcat(_UPDATINGLOG, " done");
                        });
                    }

                    this.ripper.tumblrPostLog = null;
                    this.tumblrLogFile = null;
                }
                this.downloadDone = true;
            }
            catch
            {
            }
        }

        public void downloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(200);
            try
            {
                this.downloadedList = new List<string>();
                this.downloadedSizesList = new List<int>();
                this.downloadDone = false;
                this.fileDownloadDone = false;
                FileInfo file;
                HashSet<PhotoPostImage> imagesList = (HashSet<PhotoPostImage>)e.Argument;
                fileManager.totalToDownload = imagesList.Count;

                lock (this.fileManager)
                {
                    this.fileManager.statusCode = downloadStatusCodes.Preparing;
                }

                while (!readyDownload)
                {
                    //wait
                }

                if (imagesList != null && imagesList.Count != 0)
                {
                    int j = 0;

                    lock (this.fileManager)
                    {
                        this.fileManager.statusCode = downloadStatusCodes.Downloading;
                    }

                    foreach (PhotoPostImage photoImage in imagesList)
                    {
                        if (this.isCancelled)
                            break;

                        this.fileDownloadDone = false;

                        lock (this.fileManager)
                        {
                            this.fileManager.statusCode = downloadStatusCodes.Downloading;
                        }

                        bool downloaded = false;
                        string fullPath = "";

                        while (!this.fileDownloadDone && !this.isCancelled)
                        {
                            try
                            {
                                fullPath = FileHelper.getFullFilePath(photoImage.filename, this.saveLocation);

                                downloaded = fileManager.downloadFile(photoImage.url, this.saveLocation);

                                if (downloaded)
                                {
                                    j++;
                                    this.fileDownloadDone = true;
                                    photoImage.downloaded = true;
                                    fullPath = FileHelper.fixFileName(fullPath);

                                    file = new FileInfo(fullPath);

                                    lock (this.downloadedList)
                                    {
                                        this.downloadedList.Add(fullPath);
                                    }

                                    lock (this.downloadedSizesList)
                                    {
                                        this.downloadedSizesList.Add((int)new FileInfo(fullPath).Length);
                                    }
                                }
                                else if (this.fileManager.statusCode == downloadStatusCodes.UnableDownload)
                                {
                                    lock (this.notDownloadedList)
                                    {
                                        this.notDownloadedList.Add(photoImage.url);
                                    }
                                    photoImage.downloaded = false;

                                    if (FileHelper.fileExists(fullPath))
                                    {
                                        file = new FileInfo(fullPath);

                                        if (!FileHelper.IsFileLocked(file)) file.Delete();
                                    }
                                }
                            }
                            catch
                            {
                                lock (this.notDownloadedList)
                                {
                                    this.notDownloadedList.Add(photoImage.url);
                                }
                                photoImage.downloaded = false;
                                if (FileHelper.fileExists(fullPath))
                                {
                                    file = new FileInfo(fullPath);

                                    if (!FileHelper.IsFileLocked(file)) file.Delete();
                                }
                            }
                            finally
                            {
                                this.fileDownloadDone = true;
                            }
                        }

                        if (this.isCancelled)
                        {
                            if (FileHelper.fileExists(fullPath))
                            {
                                file = new FileInfo(fullPath);

                                if (!FileHelper.IsFileLocked(file)) file.Delete();
                            }
                        }
                    }

                    this.downloadDone = true;
                }
            }
            catch
            {
            }
        }

        public void downloadWorker_UI__AfterDone(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (!this.IsDisposed)
                {
                    try
                    {
                        if (this.downloadedList.Count == 0)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                this.img_DisplayImage.Image = Resources.tumblrlogo;
                            });
                        }
                        else
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                this.img_DisplayImage.ImageLocation = this.downloadedList[this.downloadedList.Count - 1];
                                this.lbl_PostCount.Text = string.Format(_POSTCOUNT, this.downloadedList.Count, this.fileManager.totalToDownload);
                            });
                        }
                    }
                    catch
                    {
                        //
                    }
                    this.Invoke((MethodInvoker)delegate
                    {
                        this.img_DisplayImage.Update();
                        this.img_DisplayImage.Refresh();
                    });

                    if (this.fileManager.statusCode == downloadStatusCodes.Done && this.downloadedList.Count > 0)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            updateWorkStatusTextConcat(_DOWNLOADINGIMAGES, " done");
                            updateWorkStatusTextNewLine("Downloaded " + this.downloadedList.Count.ToString() + " image(s).");
                            this.bar_Progress.Value = 0;
                            this.lbl_PercentBar.Text = string.Empty;
                        });
                    }
                    this.Invoke((MethodInvoker)delegate
                    {
                        updateStatusText("Done");
                        enableUI_Crawl(true);
                    });
                }
            }
            catch
            {
            }
        }

        public void downloadWorker_UI__DoWork(object sender, DoWorkEventArgs e)
        {
            this.currentPercent = 0;
            this.currentPostCount = 0;
            try
            {
                if (this.fileManager == null) this.fileManager = new FileManager();

                if (this.fileManager.totalToDownload == 0)
                {
                    if (!this.IsDisposed)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            this.bar_Progress.Visible = false;

                            this.lbl_PostCount.Visible = false;
                            updateStatusText(_DONE); ;
                        });
                    }
                }
                else
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (!this.bar_Progress.Visible)
                        {
                            this.bar_Progress.Visible = true;
                        }

                        if (!this.lbl_PercentBar.Visible)
                            this.lbl_PercentBar.Visible = true;

                        if (!this.lbl_PostCount.Visible)
                            this.lbl_PostCount.Visible = true;

                        if (!this.lbl_Size.Visible)
                            this.lbl_Size.Visible = true;
                    });

                    decimal totalLength = 0;

                    if (!this.IsDisposed && downloadedList.Count < 10)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            updateWorkStatusTextNewLine(_DOWNLOADINGIMAGES);
                        });

                        this.readyDownload = true;
                    }

                    while (!this.downloadDone && !this.isCancelled)
                    {
                        
                        int c = 0;
                        int f = 0;

                        if (this.notDownloadedList.Count != 0 && f != this.notDownloadedList.Count)
                        {
                            f = this.notDownloadedList.Count;

                            if (!this.IsDisposed)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    this.lbl_PostCount.ForeColor = Color.Maroon;
                                    this.bar_Progress.BarColor = Color.Maroon;
                                    this.bar_Progress.Update();
                                    this.bar_Progress.Refresh();
                                    this.lbl_PercentBar.ForeColor = Color.Maroon;

                                    this.errorMessage = "Error: Unable to download " + this.notDownloadedList[notDownloadedList.Count - 1];
                                    //updateWorkStatusTextNewLine(this.errorMessage);
                                });
                            }
                        }

                        if (this.downloadedList.Count != 0 && c != this.downloadedList.Count)
                        {
                            c = this.downloadedList.Count;

                            if (!this.IsDisposed)
                            {
                                if (this.currentImage != this.downloadedList[c - 1])
                                {
                                    this.currentImage = this.downloadedList[c - 1];
                                    try
                                    {
                                        this.Invoke((MethodInvoker)delegate
                                        {
                                           // this.img_DisplayImage.ImageLocation = this.downloadedList[c - 1];
                                            this.img_DisplayImage.Load(this.downloadedList[c - 1]);
                                            // this.img_DisplayImage.Refresh();
                                        });
                                    }
                                    catch (Exception)
                                    {
                                        this.img_DisplayImage.Image = Resources.tumblrlogo;
                                        this.img_DisplayImage.Update();
                                        this.img_DisplayImage.Refresh();
                                    }
                                }

                                int downloaded = this.downloadedList.Count;
                                int total = this.fileManager.totalToDownload;

                                if (downloaded > total)
                                    downloaded = total;

                                this.Invoke((MethodInvoker)delegate
                                {
                                    if (!this.bar_Progress.Visible)
                                    {
                                        this.bar_Progress.Visible = true;
                                    }

                                    if (!this.lbl_PercentBar.Visible)
                                        this.lbl_PercentBar.Visible = true;

                                    if (!this.lbl_PostCount.Visible)
                                        this.lbl_PostCount.Visible = true;

                                    if (!this.lbl_Size.Visible)
                                        this.lbl_Size.Visible = true;
                                });

                                int percent = total > 0 ? (int)(((double)downloaded / (double)total) * 100.00) : 0;

                                if (this.currentPercent != percent)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        this.bar_Progress.Value = percent;
                                        this.lbl_PercentBar.Text = string.Format(_PERCENT, percent.ToString());
                                    });
                                }

                                if (this.currentPostCount != downloaded)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        this.lbl_PostCount.Text = string.Format(_POSTCOUNT, downloaded.ToString(), total.ToString());
                                    });
                                }

                                try
                                {
                                    totalLength = (this.downloadedSizesList.Sum(x => Convert.ToInt32(x)) / (decimal)1024 / (decimal)1024);
                                    decimal totalLengthNum = totalLength > 1024 ? totalLength / 1024 : totalLength;
                                    string suffix = totalLength > 1024 ? "GB" : "MB";

                                    if (this.currentSize != totalLength)
                                    {
                                        this.Invoke((MethodInvoker)delegate
                                        {
                                            this.lbl_Size.Text = string.Format(_SIZE, (totalLengthNum).ToString("0.00"), suffix);
                                        });
                                    }
                                }
                                catch (Exception)
                                {
                                    //
                                }

                                if ((int)this.fileManager.percentDownloaded <= 0)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        updateStatusText(string.Format(_DOWNLOADSTATUS, "Connecting"));
                                    });
                                }
                                else if (percent != (int)this.fileManager.percentDownloaded)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        updateStatusText(string.Format(_DOWNLOADSTATUS, this.fileManager.percentDownloaded.ToString() + "%"));
                                    });
                                }

                                this.currentPercent = percent;
                                this.currentPostCount = downloaded;
                                this.currentSize = totalLength;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public void enableUI_Crawl(bool state)
        {
            this.btn_Browse.Enabled = state;
            this.btn_Start.Enabled = state;
            this.select_Mode.Enabled = state;
            this.fileToolStripMenuItem.Enabled = state;
            this.optionsToolStripMenuItem.Enabled = state;
            this.txt_TumblrURL.Enabled = state;
            this.txt_SaveLocation.Enabled = state;
            this.disableOtherTabs = !state;
        }

        public void enableUI_Stats(bool state)
        {
            this.btn_GetStats.Enabled = state;
            this.fileToolStripMenuItem.Enabled = state;
            this.optionsToolStripMenuItem.Enabled = state;
            this.txt_Stats_TumblrURL.Enabled = state;
            this.disableOtherTabs = !state;
        }

        public void fileBW_AfterDone(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        public void fileBW_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.Invoke((MethodInvoker)delegate
                {
                    updateStatusText(_OPENSAVEFILE);
                    openTumblrFile((string)e.Argument);
                });
            }
            catch
            {
            }
        }

        public void form_Closing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (!exit)
                {
                    this.exit = true;
                    DialogResult dialogResult = MessageBox.Show(this, "Are you sure you want to exit Tumblr Tools?", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.Yes)
                    {
                        //do something

                        this.ripper.isCancelled = true;
                        this.isCancelled = true;
                        this.downloadDone = true;
                        this.crawlDone = true;

                        if (this.tumblrLogFile == null) this.tumblrLogFile = this.ripper.tumblrPostLog;

                        if (!this.IsDisposed)
                        {
                            updateStatusText("Exiting...");
                            updateWorkStatusTextNewLine("Exiting ...");
                        }

                        if (this.options.generateLog && this.tumblrLogFile != null && this.ripper.logUpdated)
                        {
                            Thread thread = new Thread(saveLogFile);
                            thread.Start();
                        }
                    }
                    else if (dialogResult == DialogResult.No)
                    {
                        e.Cancel = true;
                        this.exit = false;
                    }
                }
                else
                {
                    //Application.Exit();
                }
            }
            catch
            {
                Application.Exit();
            }
        }

        public void getStatsWorker_AfterDone(object sender, RunWorkerCompletedEventArgs e)
        {
            this.tumblrStats.statusCode = processingCodes.Done;
        }

        public void getStatsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.tumblrStats.statusCode = processingCodes.Initializing;
            try
            {
                this.tumblrStats = new TumblrStats();

                if (WebHelper.checkForInternetConnection())
                {
                    this.Invoke((MethodInvoker)delegate
                            {
                                this.tumblrStats = new TumblrStats(this.tumblrBlog, this.tumblrURL, this.options.apiMode);
                                this.tumblrStats.statusCode = processingCodes.Initializing;
                            });

                    this.tumblrStats.parsePosts();
                }
                else
                {
                    this.tumblrStats.statusCode = processingCodes.connectionError;
                }
            }
            catch
            {
            }
        }

        public void getStatsWorker_UI__DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (!this.IsDisposed)
                {
                    this.Invoke((MethodInvoker)delegate
                        {
                            this.bar_Progress.Value = 0;
                            this.bar_Progress.Visible = true;
                            this.lbl_Size.Visible = false;
                            this.lbl_PercentBar.Visible = true;
                        });
                }

                while (this.tumblrStats.statusCode != processingCodes.Done && this.tumblrStats.statusCode != processingCodes.connectionError && this.tumblrStats.statusCode != processingCodes.invalidURL)
                {
                    if (this.tumblrStats.blog == null)
                    {
                        // wait till other worker created and populated blog info
                        this.Invoke((MethodInvoker)delegate
                        {
                            this.lbl_PercentBar.Text = "Getting initial blog info ... ";
                        });
                    }
                    else if (string.IsNullOrEmpty(this.tumblrStats.blog.title) && string.IsNullOrEmpty(this.tumblrStats.blog.description) && this.tumblrStats.totalPosts <= 0)
                    {
                        // wait till we got the blog title and desc and posts number
                    }
                    else
                    {
                        if (!this.IsDisposed)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                this.lbl_Stats_TotalCount.Visible = true;
                                this.lbl_Stats_BlogTitle.Text = tumblrStats.blog.title;
                                this.lbl_Stats_TotalCount.Text = tumblrStats.totalPosts.ToString();

                                this.lbl_PostCount.Text = string.Empty;
                                this.img_Stats_Avatar.LoadAsync(JSONHelper.getAvatarQueryString(tumblrStats.blog.url));
                            });
                        }

                        if (!this.IsDisposed)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                updateStatusText(_GETTINGSTATS);
                            });
                        }

                        int percent = 0;
                        while (percent < 100)
                        {
                            if (!this.IsDisposed)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    this.txt_Stats_BlogDescription.Visible = true;
                                    if (this.txt_Stats_BlogDescription.Text == string.Empty)
                                        this.txt_Stats_BlogDescription.Text = WebHelper.stripHTMLTags(this.tumblrStats.blog.description);
                                });
                            }

                            percent = (int)(((double)this.tumblrStats.parsed / (double)this.tumblrStats.totalPosts) * 100.00);
                            if (percent < 0)
                                percent = 0;

                            if (percent >= 100)
                                percent = 100;

                            if (this.currentPercent != percent)
                            {
                                if (!this.IsDisposed)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        this.bar_Progress.Value = percent;
                                    });
                                }
                            }

                            if (this.currentPostCount != this.tumblrStats.parsed)
                            {
                                if (!this.IsDisposed)
                                {
                                    this.Invoke((MethodInvoker)delegate
                                    {
                                        this.lbl_PercentBar.Visible = true;
                                        this.lbl_Stats_TotalCount.Text = tumblrStats.totalPosts.ToString();
                                        this.lbl_Stats_PhotoCount.Text = tumblrStats.photoPosts.ToString();
                                        this.lbl_Stats_TextCount.Text = tumblrStats.textPosts.ToString();
                                        this.lbl_Stats_QuoteStats.Text = tumblrStats.quotePosts.ToString();
                                        this.lbl_Stats_LinkCount.Text = tumblrStats.linkPosts.ToString();
                                        this.lbl_Stats_AudioCount.Text = tumblrStats.audioPosts.ToString();
                                        this.lbl_Stats_VideoCount.Text = tumblrStats.videoPosts.ToString();
                                        this.lbl_Stats_ChatCount.Text = tumblrStats.chatPosts.ToString();
                                        this.lbl_Stats_AnswerCount.Text = tumblrStats.answerPosts.ToString();
                                        this.lbl_PercentBar.Text = string.Format(_PERCENT, percent.ToString());
                                        this.lbl_PostCount.Visible = true;
                                        this.lbl_PostCount.Text = string.Format(_POSTCOUNT, tumblrStats.parsed.ToString(), tumblrStats.totalPosts.ToString());
                                    });
                                }
                            }

                            this.currentPostCount = tumblrStats.parsed;
                            this.currentPercent = percent;
                        }
                    }
                }

                if (this.tumblrStats.statusCode == processingCodes.invalidURL)
                {
                    if (!this.IsDisposed)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            updateStatusText(_ERROR);
                            this.lbl_PostCount.Visible = false;
                            this.bar_Progress.Visible = false;
                            this.lbl_Size.Visible = false;

                            MessageBox.Show("Invalid Tumblr URL", _ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        });
                    }
                }
                else if (this.tumblrStats.statusCode == processingCodes.connectionError)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show("No Internet connection detected!", _ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        updateStatusText(_ERROR);
                    });
                }
            }
            catch (Exception)
            {
                // throw ex;
            }
        }

        public void getStatsWorker_UI_AfterDone(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (!this.IsDisposed)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        enableUI_Stats(true);
                        updateStatusText(_DONE);
                        this.lbl_PostCount.Visible = false;
                        this.bar_Progress.Visible = false;
                        this.lbl_PercentBar.Visible = false;
                    });
                }
            }
            catch
            {
            }
        }

        public void globalInit()
        {
            this.parseModesDict = new Dictionary<string, parseModes>();
            this.downloadedList = new List<string>();
            this.downloadedSizesList = new List<int>();
            this.notDownloadedList = new List<string>();
            this.options = new ToolOptions();
            this.ripper = new ImageRipper();
            this.tumblrStats = new TumblrStats();

            this.select_Mode.Items.Add(_NEWESTONLY);
            this.select_Mode.Items.Add(_FULLRESCAN);

            this.parseModesDict.Add(_NEWESTONLY, parseModes.NewestOnly);
            this.parseModesDict.Add(_FULLRESCAN, parseModes.FullRescan);

            this.select_Mode.SelectItem(_NEWESTONLY);

            this.tumblrStats.blog = null;

            AdvancedMenuRenderer renderer = new AdvancedMenuRenderer();
            renderer.HighlightForeColor = Color.Maroon;
            renderer.HighlightBackColor = Color.White;
            renderer.ForeColor = Color.Black;
            renderer.BackColor = Color.White;

            this.menu_TopMenu.Renderer = renderer;
            this.txt_WorkStatus.Visible = false;
            this.txt_Stats_BlogDescription.Visible = false;
            this.lbl_Stats_BlogTitle.Text = string.Empty;
            this.lbl_PercentBar.Text = string.Empty;

            this.bar_Progress.Visible = true;
            this.bar_Progress.Minimum = 0;
            this.bar_Progress.Maximum = 100;
            this.bar_Progress.Step = 1;
            this.bar_Progress.Value = 0;

            this.fileManager = new FileManager();
            this.Text += " (" + _VERSION + ")";
            this.optionsForm = new OptionsForm();
            this.optionsForm.mainForm = this;
            this.aboutForm = new AboutForm();
            this.aboutForm.mainForm = this;
            this.aboutForm.version = "Version: " + _VERSION;

            this.optionsForm.apiMode = apiModeEnum.JSON.ToString();

            loadOptions();
            this.lbl_Size.Text = string.Empty;
            this.lbl_Size.Visible = false;
            this.lbl_PostCount.Text = string.Empty;
            this.lbl_PostCount.Visible = false;
            this.lbl_Status.Text = _READYSTATUS;
            this.lbl_Status.Visible = true;
            this.lbl_PercentBar.Text = string.Empty;
            this.lbl_PercentBar.Visible = true;

            setDoubleBuffering(this.bar_Progress, true);
            setDoubleBuffering(this.img_DisplayImage, true);
        }

        public bool isValidURL(string urlString)
        {
            try
            {
                return urlString.isValidUrl();
            }
            catch (Exception e)
            {
                string error = e.Message;
                return false;
            }
        }

        public void loadOptions()
        {
            this.optionsForm.setOptions();
            loadOptions(this.optionsForm.options);
        }

        public void loadOptions(ToolOptions _options)
        {
            this.options = _options;
        }

        public void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.btn_Start.Enabled = false;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.AddExtension = true;
                ofd.DefaultExt = ".tumblr";
                ofd.RestoreDirectory = true;
                ofd.Filter = "Tumblr Tools Files (.tumblr)|*.tumblr|All Files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    updateStatusText(_OPENSAVEFILE);

                    this.fileBackgroundWorker.RunWorkerAsync(ofd.FileName);
                }
                else
                {
                    this.btn_Start.Enabled = true;
                }
            }
        }

        public void openTumblrFile(string file)
        {
            try
            {
                if (!this.IsDisposed)
                {
                    this.tumblrSaveFile = !string.IsNullOrEmpty(file) ? this.fileManager.readTumblrFile(file) : null;

                    this.tumblrBlog = this.tumblrSaveFile != null ? this.tumblrSaveFile.blog : null;

                    this.txt_SaveLocation.Text = !string.IsNullOrEmpty(file) ? Path.GetDirectoryName(file) : string.Empty;


                    if (this.tumblrSaveFile != null && this.tumblrSaveFile.blog != null && !string.IsNullOrEmpty(this.tumblrSaveFile.blog.url))
                    {
                        this.txt_TumblrURL.Text = this.tumblrSaveFile.blog.url;
                    }
                    else if (this.tumblrSaveFile != null && this.tumblrSaveFile.blog != null && string.IsNullOrEmpty(this.tumblrSaveFile.blog.url) && !string.IsNullOrEmpty(this.tumblrSaveFile.blog.cname))
                    {
                        this.txt_TumblrURL.Text = this.tumblrSaveFile.blog.cname;
                    }
                    else
                    {
                        this.txt_TumblrURL.Text = "Error parsing save file...";
                    }

                    updateStatusText(_READYSTATUS);
                    this.btn_Start.Enabled = true;
                }
            }
            catch
            {
            }
        }

        public void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.optionsForm.ShowDialog();
        }

        public void saveLogFile()
        {
            this.fileManager.saveTumblrFile(this.saveLocation + @"\" + this.tumblrLogFile.getFileName(), this.tumblrLogFile);
        }

        public void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile saveFile = new SaveFile(this.ripper.blog.name + ".tumblr", this.ripper.blog);
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.AddExtension = true;
                sfd.DefaultExt = ".tumblr";
                sfd.Filter = "Tumblr Tools Files (.tumblr)|*.tumblr";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (this.fileManager.saveTumblrFile(sfd.FileName, saveFile))
                    {
                        MessageBox.Show("Saved", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        public bool saveTumblrFile(string name)
        {
            this.tumblrSaveFile = new SaveFile(name + ".tumblr", this.ripper.blog);

            return this.fileManager.saveTumblrFile(this.saveLocation + @"\" + this.tumblrSaveFile.getFileName(), this.tumblrSaveFile);
        }

        public void setDoubleBuffering(System.Windows.Forms.Control control, bool value)
        {
            System.Reflection.PropertyInfo controlProperty = typeof(System.Windows.Forms.Control)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            controlProperty.SetValue(control, value, null);
        }

        public void statsTumblrURLUpdate(object sender, EventArgs e)
        {
            this.txt_Stats_TumblrURL.Text = this.txt_TumblrURL.Text;
        }

        public void tabControl_Main_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (this.disableOtherTabs)
            {
                Dotnetrix.Controls.TabControl tabWizardControl = sender as Dotnetrix.Controls.TabControl;

                int selectedTab = tabWizardControl.SelectedIndex;

                //Disable the tab selection
                if (this.currentSelectedTab != selectedTab)
                {
                    //If selected tab is different than the current one, re-select the current tab.
                    //This disables the navigation using the tab selection.
                    tabWizardControl.SelectTab(this.currentSelectedTab);
                }
            }
        }

        public void tabMainTabSelect_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (!e.TabPage.Enabled)
            {
                e.Cancel = true;
            }
        }

        public void tabPage_Enter(object sender, EventArgs e)
        {
            this.currentSelectedTab = this.tabControl_Main.SelectedIndex;
        }

        public void toolStripMenuItem_Paint(object sender, PaintEventArgs e)
        {
            ToolStripMenuItem TSMI = sender as ToolStripMenuItem;

            AdvancedMenuRenderer renderer = TSMI.GetCurrentParent().Renderer as AdvancedMenuRenderer;

            renderer.changeTextForeColor(TSMI, e);
        }

        public void txt_StatsTumblrURL_TextChanged(object sender, EventArgs e)
        {
            this.txt_TumblrURL.Text = this.txt_Stats_TumblrURL.Text;
        }

        public void updateStatusText(string text)
        {
            if (!this.lbl_Status.Text.Contains(text))
            {
                this.lbl_Status.Text = text;
                this.lbl_Status.Invalidate();
                this.status_Strip.Update();
                this.status_Strip.Refresh();
            }
        }

        public void updateWorkStatusTextConcat(string strToReplace, string strToAdd = "")
        {
            if (this.txt_WorkStatus.Text.Contains(strToReplace) && !this.txt_WorkStatus.Text.Contains(string.Concat(strToReplace, strToAdd)))
            {
                this.txt_WorkStatus.Text = txt_WorkStatus.Text.Replace(strToReplace, string.Concat(strToReplace, strToAdd));

                this.txt_WorkStatus.Update();
                this.txt_WorkStatus.Refresh();
            }
        }

        public void updateWorkStatusTextNewLine(string text)
        {
            if (!this.txt_WorkStatus.Text.Contains(text))
            {
                this.txt_WorkStatus.Text += txt_WorkStatus.Text != "" ? "\r\n" : "";
                this.txt_WorkStatus.Text += text;
                this.txt_WorkStatus.Update();
                this.txt_WorkStatus.Refresh();
            }
        }

        public void updateWorkStatusTextReplace(string str, string replaceStr)
        {
            if (this.txt_WorkStatus.Text.Contains(str))
            {
                this.txt_WorkStatus.Text = txt_WorkStatus.Text.Replace(str, replaceStr);

                this.txt_WorkStatus.Update();
                this.txt_WorkStatus.Refresh();
            }
        }

        public void workStatusAutoScroll(object sender, EventArgs e)
        {
            this.txt_WorkStatus.SelectionStart = this.txt_WorkStatus.TextLength;
            this.txt_WorkStatus.ScrollToCaret();
        }
    }
}
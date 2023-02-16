using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Transactions;
using System.Threading;
using WindowsFormsApplication1;

namespace UpdateApp
{
    /// <summary>
    /// 桌面应用更新程序
    /// 启动时，根据主程序本地的版本文件（LocalVersion.xml），拿到远程更新地址，比较远程配置的版本文件（ServerVersion.xml）
    /// 如果有新版本，则判断更新程序是否位于系统盘，且是否为管理员身份运行
    /// 如果位于系统盘，且不是管理员身份运行，则重新以管理员身份运行更新重启，操作系统会弹出账号控制提示给客户
    /// 如果不是则打开主窗体，提示有新版本可以更新，是否下载更新
    /// （这个机制可以达到无论客户把程序装在系统盘还是其他盘都不会影响更新操作，并且如果客户不是装在系统盘，还可以免去每次检查更新系统都弹出"权限控制"的提示给客户）
    /// 下载更新完成后，会自动删除下载的更新包，并且重新启动主程序
    /// 
    /// 在主程序启动时，同时启动更新程序，即可检查更新了，如果客户选中暂时不更新，下次打开还是会提示更新，如果想跳过改版本，那就得在取消的时候做点操作
    /// 把远程最新版本号，修改到主程序本地的版本文件（LocalVersion.xml）
    /// </summary>
    public partial class Form1 : Form
    {
        //private bool mIsCancel = false;

        public Form1(dynamic param)
        {
            this.TopMost = true;
            List<string> list = new List<string> { "zh-CN", "zh-HK" };
            if (!list.Contains(Thread.CurrentThread.CurrentCulture.CompareInfo.Name))
            {
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("");
            }

            InitializeComponent();
            bool ifUpdate = Convert.ToBoolean(param.requiredUpdate);


            if (!ifUpdate)
            {

                //ifUpdate = MessageBox.Show(Properties.Resources.HasNewVersion, Properties.Resources.NewVersion, MessageBoxButtons.YesNo,MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification) == DialogResult.Yes;
                UpgradeTips tips = new UpgradeTips();
                //设置更新文字提示
                tips.setTips(param.remark.ToString());
                DialogResult ret = tips.ShowDialog();
                if (ret == DialogResult.OK)
                {
                    ifUpdate = true;
                }
                else
                {
                    MessagePopup popup = new MessagePopup();
                    ret = popup.ShowDialog();
                    if (ret == DialogResult.OK)
                    {
                        //UpgradeForm form = new UpgradeForm();
                        ifUpdate = true;
                        //form.ShowDialog();
                    }
                }
            }



            if (ifUpdate)
            {
                try
                {
                    Update update = new Update() { DownloadFileCompleted = DownloadFileCompleted, DownloadProgressChanged = DownloadProgressChanged };

                    this.backgroundWorker1.RunWorkerAsync();
                    update.Download(param);
                }
                catch (Exception ex)
                {
                    this.TopMost = false;
                    this.Hide();
                    ErrorMessage errorMessage = new ErrorMessage();
                    errorMessage.ShowDialog();
                    Environment.Exit(0);
                }
            }
            else
            {
                Environment.Exit(0);
            }
        }


        /// <summary>
        /// 更新包下载完成，开始更新操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            backgroundWorker1.CancelAsync();
            label1.Text = "50%";
            if (!e.Cancelled && e.UserState != null)
            {
                dynamic param = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(e.UserState));

                //文件大小校验
                FileInfo file = new FileInfo(Application.StartupPath + "\\" + param.packName.ToString() + ".zip");
                if (Convert.ToInt32(param.size) != Convert.ToInt32(file.Length.ToString()))
                {
                    var killList = param.killList.ToString().Split(',');
                    foreach (var item in killList)
                    {
                        UpdateApp.Update.CloseProcess(item);
                    }
                    //MessageBox.Show(this, "下载文件时出错，请检查网络并退出重试。", "下载出错", MessageBoxButtons.OK);
                    this.TopMost = false;
                    this.Hide();
                    
                    ErrorMessage errorMessage = new ErrorMessage();
                    errorMessage.ShowDialog();
                    Environment.Exit(0);
                }

                //MessageBox.Show(this,Properties.Resources.DownLoadSuccess, Properties.Resources.NewVersion, MessageBoxButtons.OK);
                CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
                //解压文件、复制文件、删除文件 做个假进度
                Task.Factory.StartNew(() =>
                {
                    BeginInvoke((Action)(() =>
                    {
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = 100;
                        progressBar1.Value = 50;
                        //label1.Text = "";
                    }));
                    Random r = new Random();
                    while (!cts.Token.IsCancellationRequested)
                    {

                        BeginInvoke(new Action(() => {  if (progressBar1.Value<99) progressBar1.Value += 1; }));
                        if (progressBar1.Value <= 100)
                        {
                            label1.Text = progressBar1.Value + "%";
                        }
                        Thread.Sleep(1200);

                    }
                    //进度加速到100
                    BeginInvoke(new Action(() =>
                    {
                        for (int i = progressBar1.Value; i < 100; i++)
                        {
                            progressBar1.Value += 1;
                            label1.Text = progressBar1.Value.ToString() + "%";
                            Application.DoEvents();
                            Thread.Sleep(30);
                        }

                    }));
                });
                Task.Factory.StartNew(() =>
                {
                    //dynamic temp = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(e.UserState));
                    //杀掉所有需要关闭的进程
                    var killList = param.killList.ToString().Split(',');
                    foreach (var item in killList)
                    {
                        UpdateApp.Update.CloseProcess(item);
                    }
                    //获取主程序的所在路径，这里因为是在主程序的安装根目录里放了一个文件夹，文件夹里面放更新程序的结构..\App\App.exe   ..\App\UpdateApp\UpdateApp.exe
                    var appPath = Directory.GetParent(Application.StartupPath).FullName;
                    //下载到的更新包的所在路径，与更新程序所在位置相同
                    var packPath = Application.StartupPath + "\\" + param.packName.ToString() + ".zip";


                    //更新包解压路径，与更新程序所在位置相同
                    var unpackPath = Application.StartupPath + "\\" + param.packName;
                    //开始解压
                    myZip.ZipHelper.UnZip(packPath, Application.StartupPath);
                    //复制新文件到主程序安装目录，至于文件复制失败之类的情况，没有处理，重新安装客户端吧
                    myZip.ZipHelper.CopyDirectory(unpackPath, appPath);

                    //操作完成，删除更新包
                    File.Delete(packPath);
                    //删除解压后的更新包
                    myZip.ZipHelper.DeleteFolder(unpackPath);
                    Directory.Delete(unpackPath, true);
                    //停止进度条
                    cts.Cancel();
                    //MessageBox.Show(Properties.Resources.UpdateSuccess, Properties.Resources.NewVersion, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);

                    //MessageBox.Show(this, temp.remark.ToString(), "本次更新内容", MessageBoxButtons.OK);

                    while (progressBar1.Value < 100)
                    {

                        Thread.Sleep(100);
                    }
                    this.TopMost = false;
                    this.Hide();

                    UpgradeSucceed succeed = new UpgradeSucceed();
                    succeed.ShowDialog();
                    //更新完成，重新打开主应用程序
                    Process p = new Process();
                    p.StartInfo.FileName = $@"{appPath}\{param.appName.ToString()}.exe";
                    p.Start();
                    //退出更新程序
                    Environment.Exit(0);
                });
            }
            else
            {
                //MessageBox.Show(Properties.Resources.DownLoadFail, Properties.Resources.NewVersion, MessageBoxButtons.OK);
                ErrorMessage errorMessage = new ErrorMessage();
                errorMessage.ShowDialog();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// 更新下载进度条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Minimum = 0;
            progressBar1.Maximum = (int)e.TotalBytesToReceive * 2;
            progressBar1.Value = (int)e.BytesReceived;
            label1.Text = e.ProgressPercentage / 2 + "%";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessagePopup popup = new MessagePopup();
            DialogResult ret = popup.ShowDialog();
            if (ret == DialogResult.Cancel)
            {
                // mIsCancel = true;
                //this.Close();
                Environment.Exit(0);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (backgroundWorker1.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                int progess1 = progressBar1.Value;
                Thread.Sleep(1000 * 10);
                int progess2 = progressBar1.Value;
                if (progess1 == progess2)
                {
                    Thread.Sleep(1000 * 10);
                    progess2 = progressBar1.Value;
                    if (progess1 == progess2)
                    {
                        this.Hide();

                        ErrorMessage errorMessage = new ErrorMessage();
                        errorMessage.ShowDialog();
                        Environment.Exit(0);
                        
                    }
                }
            }

        }

    }
}

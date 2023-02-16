using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;

namespace UpdateApp
{
    public class Update
    {

        WebClient _client;
        public Update()
        {
            _client = new WebClient();
            _client.DownloadFileCompleted += (sender, args) => { DownloadFileCompleted?.Invoke(sender, args); };
            _client.DownloadProgressChanged += (sender, args) => { DownloadProgressChanged?.Invoke(sender, args); };
        }
        /// <summary>
        /// 在异步文件下载操作完成时发生。
        /// </summary>
        public Action<object, AsyncCompletedEventArgs> DownloadFileCompleted { get; set; }
        /// <summary>
        /// 在异步下载操作成功转换部分或全部数据后发生
        /// </summary>
        public Action<object, DownloadProgressChangedEventArgs> DownloadProgressChanged { get; set; }
        /// <summary>
        /// 根据名字 关闭进程
        /// </summary>
        /// <param name="ProcessName"></param>
        /// <returns></returns>
        public static bool CloseProcess(string ProcessName)
        {
            bool result = false;
            var temp = System.Diagnostics.Process.GetProcessesByName(ProcessName);
            foreach (var item in temp)
            {
                try
                {
                    item.Kill();
                    result = true;
                }
                catch
                {
                }
            }
            return result;
        }

        /// <summary>
        /// 检查是否有新版本
        /// </summary>
        /// <returns></returns>
        public static dynamic HasNewVersion()
        {
            try
            {
                var appPath = Directory.GetParent(Application.StartupPath).FullName;
                
                XElement localxdoc = XElement.Load(appPath + "\\LocalVersion.xml");
               
                var localV = localxdoc.Element("version").Value;
                
                var localUrl = localxdoc.Element("url").Value;

                //应用程序名称
                var localAppName = localxdoc.Element("appName").Value;
                //更新过程必须关掉的进程列表
                var localKillList = localxdoc.Element("killList").Value;
                XElement serverxdoc = XElement.Load(localUrl);

                //服务端版本
                var serverV = serverxdoc.Element("version").Value;
                //更新包地址
                var serverU = serverxdoc.Element("url").Value;
                var serverRe = serverxdoc.Element("remark").Value.Replace("\\n", "\n");
                
                //是否必须更新
                var serverRU = serverxdoc.Element("requiredUpdate").Value;
                var temp2 = Convert.ToInt32(serverV.Replace(".", ""));
                var temp3 = Convert.ToInt32(localV.Replace(".", ""));
                var FileSize = serverxdoc.Element("size").Value;
                return new
                {
                    result = temp2 > temp3,
                    url = serverU,
                    appName = localAppName,
                    packName = Path.GetFileNameWithoutExtension(serverU),
                    killList = localKillList,
                    requiredUpdate = serverRU,
                    remark = serverRe,
                    size = FileSize,
                    local_version = localV,
                    server_version = serverV
                };
            }
            catch (Exception e){
                //MessageBox.Show(e.ToString());
                return new { result = false };
            }                      
        }
        /// <summary>
        /// 异步下载文件
        /// </summary>
        /// <param name="param"></param>
        public void Download(dynamic param)
        {
            var savePath = AppDomain.CurrentDomain.BaseDirectory;
            var downUrl = param.url.ToString();
            try
            {
                _client.DownloadFileAsync(new Uri(downUrl), savePath + param.packName.ToString() + ".zip", param);
            }
            catch 
            {
                ErrorMessage errorMessage = new ErrorMessage();
                errorMessage.ShowDialog();
            }
        }

        /// <summary>
        /// 判断本程序当前是否运行在系统盘
        /// </summary>
        /// <returns></returns>
        public static bool IsSystemPath()
        {
            //系统盘路径
            string path = Environment.GetEnvironmentVariable("systemdrive");
            return Application.StartupPath.StartsWith(path, StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// 判断本程序当前是否以管理员身份运行
        /// </summary>
        /// <returns></returns>
        public static bool IsAdministrator()
        {
            WindowsIdentity current = WindowsIdentity.GetCurrent();
            WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
            return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        /// <summary>
        /// 以管理员身份重新启动程序
        /// </summary>
        public static void ReStartAppWithAdministrator()
        {
            try
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.UseShellExecute = true;
                //startInfo.WorkingDirectory = $@"{AppDomain.CurrentDomain.BaseDirectory}";
                startInfo.FileName = $@"{Application.StartupPath}\{Process.GetCurrentProcess().ProcessName}.exe";
                //设置启动动作,确保以管理员身份运行
                startInfo.Verb = "runas";
                System.Diagnostics.Process.Start(startInfo);
            }
            catch
            {

            }
        }
    }
}

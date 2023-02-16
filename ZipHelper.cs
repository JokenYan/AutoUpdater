using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//ICSharpCode.SharpZipLib.dll
namespace myZip
{
    /// <summary>
    /// Zipѹ��������
    /// </summary>
    public class ZipHelper
    {
        /// <summary>
        /// ѹ���ļ��� �������ļ���
        /// </summary>
        /// <param name="filesPath"></param>
        /// <param name="zipFilePath"></param>
        /// <param name="compressionLevel"></param>
        public void CreateZipFile(string filesPath, string zipFilePath, int compressionLevel = 9)
        {
            if (!Directory.Exists(filesPath))
            {
                return;
            }
            if (Path.GetExtension(zipFilePath) != ".zip")
            {
                zipFilePath = zipFilePath + ".zip";
            }
            string[] filenames = Directory.GetFiles(filesPath, "*.*", SearchOption.AllDirectories);
            
            ZipOutputStream stream = new ZipOutputStream(File.Create(zipFilePath));
            stream.SetLevel(compressionLevel); // ѹ������ 0-9  
            byte[] buffer = new byte[4096]; //��������С  
            
            foreach (string file in filenames)
            {
                if(file != zipFilePath)
                {
                    ZipEntry entry = new ZipEntry(file.Replace(filesPath+"\\", ""));
                    entry.DateTime = DateTime.Now;
                    stream.PutNextEntry(entry);
                    using (FileStream fs = File.OpenRead(file))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                }    
            }
            stream.Finish();
            stream.Close();
        }

        ///<summary>
        /// ���ָ�����ļ��У�����ɾ���ļ���
        /// </summary>
        /// <param name="dir"></param>
        public static void DeleteFolder(string dir)
        {
            System.Threading.Tasks.Parallel.ForEach(Directory.GetFileSystemEntries(dir), (d) => {
                try
                {
                    if (File.Exists(d))
                    {
                        FileInfo fi = new FileInfo(d);
                        if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                            fi.Attributes = FileAttributes.Normal;
                        File.Delete(d);//ֱ��ɾ�����е��ļ�  
                    }
                    else
                    {
                        DirectoryInfo d1 = new DirectoryInfo(d);
                        if (d1.GetFiles().Length != 0)
                        {
                            DeleteFolder(d1.FullName);////�ݹ�ɾ�����ļ���
                        }
                        Directory.Delete(d,true);
                    }
                }
                catch
                {

                }
            });
            if (Directory.GetFileSystemEntries(dir).Length > 0)
            {
                DeleteFolder(dir);
            }
        }
        public static void CopyDirectory(string srcPath, string destPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //��ȡĿ¼�£���������Ŀ¼�����ļ�����Ŀ¼
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)     //�ж��Ƿ��ļ���
                    {
                        if (!Directory.Exists(destPath + "\\" + i.Name))
                        {
                            Directory.CreateDirectory(destPath + "\\" + i.Name);   //Ŀ��Ŀ¼�²����ڴ��ļ��м��������ļ���
                        }
                        CopyDirectory(i.FullName, destPath + "\\" + i.Name);    //�ݹ���ø������ļ���
                    }
                    else
                    {                       
                        File.Copy(i.FullName, destPath + "\\" + i.Name, true);      //�����ļ��м������ļ���true��ʾ���Ը���ͬ���ļ�
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>  
        /// ���ܣ���ѹzip��ʽ���ļ���  
        /// </summary>  
        /// <param name="zipFilePath">ѹ���ļ�·��</param>  
        /// <param name="unZipDir">��ѹ�ļ����·��,Ϊ��ʱĬ����ѹ���ļ�ͬһ��Ŀ¼�£���ѹ���ļ�ͬ�����ļ���</param>  
        /// <returns>��ѹ�Ƿ�ɹ�</returns>  
        public static void UnZip(string zipFilePath, string unZipDir="")
        {
            if (zipFilePath == string.Empty)
            {
                throw new Exception("ѹ���ļ�����Ϊ�գ�");
            }
            if (!File.Exists(zipFilePath))
            {
                throw new FileNotFoundException("ѹ���ļ������ڣ�");
            }
            //��ѹ�ļ���Ϊ��ʱĬ����ѹ���ļ�ͬһ��Ŀ¼�£���ѹ���ļ�ͬ�����ļ���  
            if (unZipDir == string.Empty)
                unZipDir = zipFilePath.Replace(Path.GetFileName(zipFilePath), Path.GetFileNameWithoutExtension(zipFilePath));
            if (!unZipDir.EndsWith("/"))
                unZipDir += "/";
            if (!Directory.Exists(unZipDir))
                Directory.CreateDirectory(unZipDir);

            using (var s = new ZipInputStream(File.OpenRead(zipFilePath)))
            {

                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);
                    if (!string.IsNullOrEmpty(directoryName))
                    {
                        Directory.CreateDirectory(unZipDir + directoryName);
                    }
                    if (directoryName != null && !directoryName.EndsWith("/"))
                    {
                    }
                    if (fileName != String.Empty)
                    {
                        using (FileStream streamWriter = File.Create(unZipDir + theEntry.Name))
                        {

                            int size;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

    }
    

}

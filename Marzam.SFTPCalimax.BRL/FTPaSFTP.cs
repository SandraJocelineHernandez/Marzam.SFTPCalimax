using EnterpriseDT.Net.Ftp;
using Renci.SshNet;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Timers;

namespace Marzam.SFTPCalimax.BRL
{
    public class FTPaSFTP
    {
        public static void AcarreoResp(object sender, ElapsedEventArgs e)
        {
            Log.Information("\n Proceso 2 Iniciado \n");

            string HostSftp = ConfigurationManager.AppSettings["HostSftp"];
            int PortSftp = int.Parse(ConfigurationManager.AppSettings["PortSftp"]);
            string UserSftp = ConfigurationManager.AppSettings["UserSftp"];
            string PasswordSftp = ConfigurationManager.AppSettings["PasswordSftp"];
            var sftpUploadPathResp = ConfigurationManager.AppSettings["SftpFileUploadResp"];


            string HostFtp = ConfigurationManager.AppSettings["HostFtp"];
            string UserFtp = ConfigurationManager.AppSettings["UserFtp"];
            string PasswordFtp = ConfigurationManager.AppSettings["PasswordFtp"];
            var ftpFilePathResp = ConfigurationManager.AppSettings["FtpFilePathResp"];
            var UriResp = ConfigurationManager.AppSettings["UriResp"];

            int Delete = int.Parse(ConfigurationManager.AppSettings["EliminarProceso2"]);
            string Extension = ConfigurationManager.AppSettings["ExtensionProceso2"];

            byte[] document = new byte[0];
            List<string> lista = new List<string>();
            List<string> list = new List<string>();
            List<object> tamaños = new List<object>();

            try
            {
                if (Extension == "")
                {
                    Log.Information("Se descargaran todos los archivos sin importar su extension\n\n");
                }
                else
                {
                    Log.Information($"Se descargaran unicamete los archivos con extension: {Extension} \n\n");
                }

                Log.Information("Estableciendo conexión a FTP...");
                FTPClient ftpClient = new FTPClient();
                ftpClient.RemoteHost = HostFtp;
                ftpClient.Connect();
                ftpClient.Login(UserFtp, PasswordFtp);
                ftpClient.ConnectMode = FTPConnectMode.PASV;
                ftpClient.TransferType = FTPTransferType.BINARY;

                if (ftpClient.IsConnected)
                {
                    Log.Information("Conexión establecida correctamente \n");

                    ftpClient.ChDir(ftpFilePathResp);

                    Uri uri = new Uri(UriResp);
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                    request.Method = WebRequestMethods.Ftp.ListDirectory;
                    request.Credentials = new NetworkCredential(UserFtp, PasswordFtp);

                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream);

                    string ultimoArchivo = "";
                    byte[] file = new byte[0];

                    if (reader != null)
                    {
                        if (Delete == 1)
                        {
                            Log.Information("Buscando archivos... \n");
                            Log.Information("Archivos encontrados: ");

                            while (!reader.EndOfStream)
                            {
                                var fileName = reader.ReadLine();
                                lista.Add(fileName);
                                Log.Information($"{fileName}");
                            }

                            reader.Close();
                            response.Close();

                            foreach (var data in lista)
                            {
                                if (Extension == "")
                                {
                                    ultimoArchivo = data;
                                    file = ftpClient.Get(ftpFilePathResp + ultimoArchivo);
                                    tamaños.Add(file);
                                    if (file.Length > 0)
                                    {
                                        Log.Information($"{ultimoArchivo} descargado");
                                        ftpClient.Delete(ultimoArchivo);
                                        Log.Information($"{ultimoArchivo} eliminado del FPT \n");
                                    }
                                    list.Add(ultimoArchivo);
                                }
                                else
                                {
                                    string exten = Path.GetExtension(data).ToString();
                                    if (Extension == exten)
                                    {
                                        ultimoArchivo = data;
                                        file = ftpClient.Get(ftpFilePathResp + ultimoArchivo);
                                        tamaños.Add(file);
                                        if (file.Length > 0)
                                        {
                                            Log.Information($"{ultimoArchivo} descargado");
                                            ftpClient.Delete(ultimoArchivo);
                                            Log.Information($"{ultimoArchivo} eliminado del FPT \n");
                                        }
                                        list.Add(ultimoArchivo);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.Information("Buscando archivos... \n");
                            Log.Information("Archivos encontrados: ");

                            while (!reader.EndOfStream)
                            {
                                var fileName = reader.ReadLine();
                                lista.Add(fileName);
                                Log.Information($"{fileName}");
                            }
                           
                            reader.Close();
                            response.Close();

                            Log.Information("Descargando archivo(s)...");

                            foreach (var data in lista)
                            {
                                if (Extension == "")
                                {
                                    ultimoArchivo = data;
                                    file = ftpClient.Get(ftpFilePathResp + ultimoArchivo);
                                    tamaños.Add(file);
                                    if (file.Length > 0)
                                    {
                                        Log.Information($"{ultimoArchivo} descargado");
                                    }
                                    list.Add(ultimoArchivo);
                                }
                                else
                                {
                                    string exten = Path.GetExtension(data).ToString();
                                    if (Extension == exten)
                                    {
                                        ultimoArchivo = data;
                                        file = ftpClient.Get(ftpFilePathResp + ultimoArchivo);
                                        tamaños.Add(file);
                                        if (file.Length > 0)
                                        {
                                            Log.Information($"{ultimoArchivo} descargado");
                                        }
                                        list.Add(ultimoArchivo);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Log.Information("Buscando archivos...\n");
                        Log.Information("No se encontroaron archivos");
                    }
                
                    Log.Information("\n");
                    Log.Information("Desconectando del FTP...");
                
                    ftpClient.Quit();
                
                    Log.Information("Desconectado");
                }
                else
                {
                    Log.Error("No se logro crear la conexión, verifica las credenciales\n\n");
                }

                if(list.Count > 0 && tamaños.Count > 0)
                {
                    Log.Information("\n");
                    Log.Information("Estableciendo conexión a SFTP...");

                    using (var sftpClient = new SftpClient(HostSftp, PortSftp, UserSftp, PasswordSftp))
                    {
                        sftpClient.Connect();

                        if (sftpClient.IsConnected)
                        {
                            Log.Information("Conexión establecida correctamente \n");

                            string archivoTmp = Path.GetTempPath();
                            string files = "";
                            
                            int i = 0;

                            if (list.Count == tamaños.Count)
                            {
                                foreach (var data in list)
                                {
                                    var bytes = tamaños[i];
                                    files = Path.Combine(archivoTmp, data);
                                    File.WriteAllBytes(files, (byte[])bytes);

                                    using (var stream = File.OpenRead(files))
                                    {
                                        Log.Information($"Cargando archivo {data} ...");
                                        sftpClient.UploadFile(stream, sftpUploadPathResp + data);
                                        Log.Information("Archivo cargado en SFTP");
                                        i++;
                                    }
                                }
                            }
                            else
                            {
                                Log.Information("No se pueden subir los archivos por un error en la descarga");
                            }
                            Log.Information("\n");
                            Log.Information("Desconectando del SFTP...");
                            sftpClient.Disconnect();
                            Log.Information("Desconectado");
                        }
                        else
                        {
                            Log.Error("No se logro establecer una conexión debido a un error desconocido");
                        }

                    }
                }
                else
                {
                    Log.Information("No hay archivos por subir");
                }
            }
            catch (Exception ex)
            {
                Log.Error("ERROR " + ex.Message);
            }
        }


        public static void AcarreoOut(object sender, ElapsedEventArgs e)
        {
            Log.Information("Proceso 3 Iniciado \n");

            string HostSftp = ConfigurationManager.AppSettings["HostSftp"];
            int PortSftp = int.Parse(ConfigurationManager.AppSettings["PortSftp"]);
            string UserSftp = ConfigurationManager.AppSettings["UserSftp"];
            string PasswordSftp = ConfigurationManager.AppSettings["PasswordSftp"];
            var sftpUploadPathOut = ConfigurationManager.AppSettings["SftpFileUploadOut"];

            string HostFtp = ConfigurationManager.AppSettings["HostFtp"];
            string UserFtp = ConfigurationManager.AppSettings["UserFtp"];
            string PasswordFtp = ConfigurationManager.AppSettings["PasswordFtp"];
            var ftpFilePathOut = ConfigurationManager.AppSettings["FtpFilePathOut"];
            var UriOut = ConfigurationManager.AppSettings["UriOut"];

            int Delete = int.Parse(ConfigurationManager.AppSettings["EliminarProceso3"]);
            string Extension = ConfigurationManager.AppSettings["ExtensionProceso3"];
        
            byte[] document = new byte[0];
            List<string> lista = new List<string>();
            List<string> list = new List<string>();
            List<object> tamaños = new List<object>();

            try
            {
                if (Extension == "")
                {
                    Log.Information("Se descargaran todos los archivos sin importar su extension\n\n");
                }
                else
                {
                    Log.Information($"Se descargaran unicamete los archivos con extension: {Extension} \n\n");
                }

                Log.Information("Estableciendo conexión a FTP...");
                FTPClient ftpClient = new FTPClient();
                ftpClient.RemoteHost = HostFtp;
                ftpClient.Connect();
                ftpClient.Login(UserFtp, PasswordFtp);
                ftpClient.ConnectMode = FTPConnectMode.PASV;
                ftpClient.TransferType = FTPTransferType.BINARY;

                ftpClient.ChDir(ftpFilePathOut);

                if (ftpClient.IsConnected)
                {
                    Log.Information("Conexión establecida correctamente \n");

                    Uri uri = new Uri(UriOut);
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                    request.Method = WebRequestMethods.Ftp.ListDirectory;
                    request.Credentials = new NetworkCredential(UserFtp, PasswordFtp);

                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream);

                    string ultimoArchivo = "";
                    byte[] file = new byte[0];

                    if (reader != null)
                    {
                        if (Delete == 1)
                        {
                            Log.Information("Buscando archivos... \n");
                            Log.Information("Archivos encontrados: ");

                            while (!reader.EndOfStream)
                            {
                                var fileName = reader.ReadLine();
                                lista.Add(fileName);
                                Log.Information($"{fileName}");
                            }

                            reader.Close();
                            response.Close();

                            foreach (var data in lista)
                            {
                                if (Extension == "")
                                {
                                    ultimoArchivo = data;
                                    file = ftpClient.Get(ftpFilePathOut + ultimoArchivo);
                                    tamaños.Add(file);
                                    if (file.Length > 0)
                                    {
                                        Log.Information($"{ultimoArchivo} descargado");
                                        ftpClient.Delete(ultimoArchivo);
                                        Log.Information($"{ultimoArchivo} eliminado del FPT \n");
                                    }
                                    list.Add(ultimoArchivo);
                                }
                                else
                                {
                                    ultimoArchivo = data;
                                    string exten = Path.GetExtension(data).ToString();
                                    if (Extension == exten)
                                    {
                                        file = ftpClient.Get(ftpFilePathOut + ultimoArchivo);
                                        tamaños.Add(file);
                                        if (file.Length > 0)
                                        {
                                            Log.Information($"{ultimoArchivo} descargado");
                                            ftpClient.Delete(ultimoArchivo);
                                            Log.Information($"{ultimoArchivo} eliminado del FPT \n");
                                        }
                                        list.Add(ultimoArchivo);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.Information("Buscando archivos... \n");
                            Log.Information("Archivos encontrados: ");
                            while (!reader.EndOfStream)
                            {
                                var fileName = reader.ReadLine();
                                lista.Add(fileName);
                                Log.Information($"{fileName}");
                            }

                            reader.Close();
                            response.Close();


                            foreach (var data in lista)
                            {
                                if (Extension == "")
                                {
                                    ultimoArchivo = data;
                                    file = ftpClient.Get(ftpFilePathOut + ultimoArchivo);
                                    tamaños.Add(file);
                                    if (file.Length > 0)
                                    {
                                        Log.Information($"{ultimoArchivo} descargado");
                                    }
                                    list.Add(ultimoArchivo);
                                }
                                else
                                {
                                    ultimoArchivo = data;
                                    string exten = Path.GetExtension(data).ToString();
                                    if (Extension == exten)
                                    {
                                        file = ftpClient.Get(ftpFilePathOut + ultimoArchivo);
                                        tamaños.Add(file);
                                        if (file.Length > 0)
                                        {
                                            Log.Information($"{ultimoArchivo} descargado");
                                        }
                                        list.Add(ultimoArchivo);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Log.Information("Buscando archivos...\n");
                        Log.Information("No se encontroaron archivos \n");
                    }
                    Log.Information("\n");
                    Log.Information("Desconectando del FTP...");

                    ftpClient.Quit();

                    Log.Information("Desconectado");
                }
                else
                {
                    Log.Error("No se logro crear la conexión, verifica las credenciales\n\n");
                }

                if(list.Count > 0 && tamaños.Count > 0)
                {
                    Log.Information("\n");
                    Log.Information("Estableciendo conexión a SFTP...");

                    using (var sftpClient = new SftpClient(HostSftp, PortSftp, UserSftp, PasswordSftp))
                    {
                        sftpClient.Connect();

                        if (sftpClient.IsConnected)
                        {
                            Log.Information("Conexión establecida correctamente \n");

                            string archivoTmp = Path.GetTempPath();
                            string files = "";

                            int i = 0;

                            if (list.Count == tamaños.Count)
                            {
                                foreach (var data in list)
                                {
                                    var bytes = tamaños[i];
                                    files = Path.Combine(archivoTmp, data);
                                    File.WriteAllBytes(files, (byte[])bytes);

                                    using (var stream = File.OpenRead(files))
                                    {
                                        Log.Information($"Cargando archivo {data} ...");
                                        sftpClient.UploadFile(stream, sftpUploadPathOut + data);
                                        Log.Information("Archivo cargado en SFTP");
                                        i++;
                                    }
                                }
                            }
                            else
                            {
                                Log.Information("No se pueden subir los archivos por un error en la descarga");
                            }
                            Log.Information("\n");
                            Log.Information("Desconectando del SFTP...");
                            sftpClient.Disconnect();
                            Log.Information("Desconectado");
                        }
                        else
                        {
                            Log.Error("No se logro establecer una conexión debido a un error desconocido");
                        }
                    }
                }
                else
                {
                    Log.Information("No hay archivos por subir");
                }
            }
            catch (Exception ex)
            {
                Log.Error("ERROR " + ex.Message);
            }
        }
    }
}

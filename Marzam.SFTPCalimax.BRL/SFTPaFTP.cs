using EnterpriseDT.Net.Ftp;
using Renci.SshNet;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Timers;
using System.Xml;

namespace Marzam.SFTPCalimax.BRL
{
    public class SFTPaFTP
    {
        public static void AcarreoIn(object sender, ElapsedEventArgs e)
        {
            Log.Information("Proceso 1 Iniciado \n");

            string HostSftp = ConfigurationManager.AppSettings["HostSftp"];
            int PortSftp = int.Parse(ConfigurationManager.AppSettings["PortSftp"]);
            string UserSftp = ConfigurationManager.AppSettings["UserSftp"];
            string PasswordSftp = ConfigurationManager.AppSettings["PasswordSftp"];
            string sftpFilePathIn = ConfigurationManager.AppSettings["SftpFilePathIn"];

            string HostFtp = ConfigurationManager.AppSettings["HostFtp"];
            string UserFtp = ConfigurationManager.AppSettings["UserFtp"];
            string PasswordFtp = ConfigurationManager.AppSettings["PasswordFtp"];
            string ftpUploadPathIn = ConfigurationManager.AppSettings["FtpUploadPathIn"];

            int Delete = int.Parse(ConfigurationManager.AppSettings["EliminarProceso1"]);
            string Extension = ConfigurationManager.AppSettings["ExtensionProceso1"];
            
            byte[] document = new byte[0];
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

                using (var sftpClient = new SftpClient(HostSftp, PortSftp, UserSftp, PasswordSftp))
                {
                    Log.Information("Estableciendo conexión a SFTP...");

                    sftpClient.Connect();
                    
                    if (sftpClient.IsConnected)
                    {
                        Log.Information("Conexión establecida correctamente \n");
                        
                        Log.Information("Buscando archivos... \n");
                        var archivo = sftpClient.ListDirectory(sftpFilePathIn).Where(x => !x.IsDirectory).OrderByDescending(x => x.LastWriteTime).ToList();
                       
                        if (archivo.Count > 0)
                        {
                            if(Delete == 1)
                            {
                                Log.Information($"{archivo.Count} archivo(s) encontrado(s)");
                                foreach (var listArchivos in archivo)
                                {
                                    
                                    Log.Information(listArchivos.Name + " " + listArchivos.LastAccessTime);

                                    if (listArchivos != null)
                                    {
                                        var ultimoArchivo = listArchivos;

                                        if (ultimoArchivo != null)
                                        {
                                            using (MemoryStream stream = new MemoryStream())
                                            {
                                                if (Extension == "")
                                                {
                                                    sftpClient.DownloadFile(ultimoArchivo.FullName, stream);
                                                    document = stream.ToArray();
                                                    Log.Information("Archivo descargado");
                                                    tamaños.Add(document);
                                                    sftpClient.DeleteFile(ultimoArchivo.FullName);
                                                    Log.Information($"{ultimoArchivo.Name} eliminado del SFPT \n");
                                                    list.Add(listArchivos.FullName);
                                                }
                                                else
                                                {
                                                    string exten = Path.GetExtension(listArchivos.Name).ToString();
                                                    if (Extension == exten)
                                                    {
                                                        sftpClient.DownloadFile(ultimoArchivo.FullName, stream);
                                                        document = stream.ToArray();
                                                        Log.Information("Archivo descargado");
                                                        tamaños.Add(document);
                                                        sftpClient.DeleteFile(ultimoArchivo.FullName);
                                                        Log.Information($"{ultimoArchivo.Name} eliminado del SFPT \n");
                                                        list.Add(listArchivos.FullName);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Log.Information($"{archivo.Count} archivo(s) encontrado(s)");
                                foreach (var listArchivos in archivo)
                                {
                                    Log.Information(listArchivos.Name + " " + listArchivos.LastAccessTime);

                                    if (listArchivos != null)
                                    {
                                        var ultimoArchivo = listArchivos;

                                        if (ultimoArchivo != null)
                                        {
                                            using (MemoryStream stream = new MemoryStream())
                                            {
                                                if (Extension == "") 
                                                {
                                                    sftpClient.DownloadFile(ultimoArchivo.FullName, stream);
                                                    document = stream.ToArray();
                                                    Log.Information("Archivo descargado");
                                                    tamaños.Add(document);
                                                    list.Add(listArchivos.FullName);
                                                }
                                                else
                                                {
                                                    string exten = Path.GetExtension(listArchivos.Name).ToString();
                                                    if (Extension == exten)
                                                    {
                                                        sftpClient.DownloadFile(ultimoArchivo.FullName, stream);
                                                        document = stream.ToArray();
                                                        Log.Information("Archivo descargado");
                                                        tamaños.Add(document);
                                                        list.Add(listArchivos.FullName);
                                                    }
                                                }
                                                
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Log.Error("ERROR, no se encontro un archivo");
                                    }
                                }
                            }
                        }
                        else
                        {
                            Log.Information("No se encontro ningun archivo");
                        }
                        Log.Information("\n");
                        Log.Information("Desconectando del SFTP...");

                        sftpClient.Disconnect();
                        Log.Information("Desconectado \n");
                    }
                    else
                    {
                        Log.Error("No se logro crear la conexión, verifica las credenciales");
                    }
                }
                
                if(list.Count > 0 && tamaños.Count > 0)
                {
                    Log.Information("Estableciendo conexión a FTP...");

                    FTPClient ftpConnection = new FTPClient();
                    ftpConnection.RemoteHost = HostFtp;
                    ftpConnection.Connect();
                    ftpConnection.Login(UserFtp, PasswordFtp);
                    ftpConnection.ConnectMode = FTPConnectMode.PASV;
                    ftpConnection.TransferType = FTPTransferType.BINARY;

                    if (ftpConnection.IsConnected)
                    {
                        Log.Information("Conexión establecida correctamente \n");

                        string archivoTmp = Path.GetTempPath();
                        string file = "";

                        int i = 0;
                        if (list.Count == tamaños.Count)
                        {
                            foreach (var data in list)
                            {
                                var bytes = tamaños[i];

                                string nom = data.Replace("/in/", "");
                                file = Path.Combine(archivoTmp, nom);

                                File.WriteAllBytes(file, (byte[])bytes);
                                Log.Information($"Cargando archivo {nom} ...");

                                ftpConnection.Put(file, ftpUploadPathIn + nom);
                                Log.Information("Archivo cargado en FTP");
                                i++;
                            }
                        }
                        else
                        {
                            Log.Information("No se pueden subir los archivos por un error en la descarga");
                        }

                        Log.Information("\n");
                        Log.Information("Desconectando del FTP...");
                        ftpConnection.Quit();
                        Log.Information("Desconectado");
                    }
                    else
                    {
                        Log.Information("No se logro establecer la conexión debido a un error desconocido");
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
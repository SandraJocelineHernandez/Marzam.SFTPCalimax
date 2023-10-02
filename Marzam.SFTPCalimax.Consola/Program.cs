using System;
using System.Configuration;
using Serilog;

namespace Marzam.SFTPCalimax.Consola
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.Console().WriteTo.File("ArchivoLogInicio-.txt", rollingInterval: RollingInterval.Day).CreateLogger();
                Log.Information("Programa ejecutandose \n\n");

                int TimeHours1 = int.Parse(ConfigurationManager.AppSettings["HorasProceso1"]);
                int TimeMinutes1 = int.Parse(ConfigurationManager.AppSettings["MinutosProceso1"]);
                int TimeHours2 = int.Parse(ConfigurationManager.AppSettings["HorasProceso2"]);
                int TimeMinutes2 = int.Parse(ConfigurationManager.AppSettings["MinutosProceso2"]);
                int TimeHours3 = int.Parse(ConfigurationManager.AppSettings["HorasProceso3"]);
                int TimeMinutes3 = int.Parse(ConfigurationManager.AppSettings["MinutosProceso3"]);

                int Time1 = 0;
                int Time2 = 0;
                int Time3 = 0;

                if (TimeHours1 == 0 && TimeMinutes1 == 0 && TimeHours2 == 0 && TimeMinutes2 == 0 && TimeHours3 == 0 && TimeMinutes3 == 0)
                {
                    Log.Warning("No se especificaron horas:minutos en ninguno de los procesos");
                    Log.Information("Los 3 procesos se realizaran 1 sola vez");

                    BRL.SFTPaFTP.AcarreoIn(null, null);
                    BRL.FTPaSFTP.AcarreoResp(null, null);
                    BRL.FTPaSFTP.AcarreoOut(null, null);
                }
                else 
                {
                    if (TimeHours1 == 0 && TimeMinutes1 == 0 && (TimeHours2 > 0 || TimeMinutes2 > 0) && (TimeHours3 > 0 || TimeMinutes3 > 0))
                    {
                        Log.Warning("No se especificaron horas:minutos para el proceso 1");
                        Log.Information("El proceso se realizara 1 sola vez");

                        Time2 = (TimeHours2 * 60) + TimeMinutes2;
                        Time3 = (TimeHours3 * 60) + TimeMinutes3;

                        var Timer2 = new System.Timers.Timer(TimeSpan.FromMinutes(Time2).TotalMilliseconds);
                        var Timer3 = new System.Timers.Timer(TimeSpan.FromMinutes(Time3).TotalMilliseconds);

                        BRL.SFTPaFTP.AcarreoIn(null, null);
                        Timer2.Elapsed += BRL.FTPaSFTP.AcarreoResp;
                        Timer3.Elapsed += BRL.FTPaSFTP.AcarreoOut;

                        Timer2.Start();
                        Timer3.Start();
                        while (true) ;
                    }
                    else if ((TimeHours1 > 0 || TimeMinutes1 > 0) && TimeHours2 == 0 && TimeMinutes2 == 0 && (TimeHours3 > 0 || TimeMinutes3 > 0))
                    {
                        Log.Warning("No se especificaron horas:minutos para el proceso 2");
                        Log.Information("El proceso se realizara 1 sola vez");

                        Time1 = (TimeHours1 * 60) + TimeMinutes1;
                        Time3 = (TimeHours3 * 60) + TimeMinutes3;

                        var Timer1 = new System.Timers.Timer(TimeSpan.FromMinutes(Time1).TotalMilliseconds);
                        var Timer3 = new System.Timers.Timer(TimeSpan.FromMinutes(Time3).TotalMilliseconds);

                        Timer1.Elapsed += BRL.SFTPaFTP.AcarreoIn;
                        BRL.FTPaSFTP.AcarreoResp(null, null);
                        Timer3.Elapsed += BRL.FTPaSFTP.AcarreoOut;

                        Timer1.Start();
                        Timer3.Start();

                        while (true) ;
                    }
                    else
                    {
                        if ((TimeHours1 > 0 || TimeMinutes1 > 0) && (TimeHours2 > 0 || TimeMinutes2 > 0) && TimeHours3 == 0 && TimeMinutes3 == 0)
                        {
                            Log.Warning("No se especificaron horas:minutos para el proceso 3");
                            Log.Information("El proceso se realizara 1 sola vez");

                            Time1 = (TimeHours1 * 60) + TimeMinutes1;
                            Time2 = (TimeHours2 * 60) + TimeMinutes2;

                            var Timer1 = new System.Timers.Timer(TimeSpan.FromMinutes(Time1).TotalMilliseconds);
                            var Timer2 = new System.Timers.Timer(TimeSpan.FromMinutes(Time2).TotalMilliseconds);

                            Timer1.Elapsed += BRL.SFTPaFTP.AcarreoIn;
                            Timer2.Elapsed += BRL.FTPaSFTP.AcarreoResp;
                            BRL.FTPaSFTP.AcarreoOut(null, null);

                            Timer1.Start();
                            Timer2.Start();
                            while (true) ;
                        }
                        else if (TimeHours1 > 0 || TimeMinutes1 > 0 && TimeHours2 > 0 || TimeMinutes2 > 0 && TimeHours3 > 0 || TimeMinutes3 > 0)
                        {
                            Time1 = (TimeHours1 * 60) + TimeMinutes1;
                            Time2 = (TimeHours2 * 60) + TimeMinutes2;
                            Time3 = (TimeHours3 * 60) + TimeMinutes3;

                            var Timer1 = new System.Timers.Timer(TimeSpan.FromMinutes(Time1).TotalMilliseconds);
                            var Timer2 = new System.Timers.Timer(TimeSpan.FromMinutes(Time2).TotalMilliseconds);
                            var Timer3 = new System.Timers.Timer(TimeSpan.FromMinutes(Time3).TotalMilliseconds);

                            Timer1.Elapsed += BRL.SFTPaFTP.AcarreoIn;
                            Timer2.Elapsed += BRL.FTPaSFTP.AcarreoResp;
                            Timer3.Elapsed += BRL.FTPaSFTP.AcarreoOut;

                            Timer1.Start();
                            Timer2.Start();
                            Timer3.Start();

                            while (true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write("ERROR " + ex.Message);
            }
        }
    }
}

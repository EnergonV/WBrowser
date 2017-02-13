/*
 * Создано в SharpDevelop.
 * Пользователь: Виталий
 * Дата: 21.12.2016
 * Время: 19:27
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */
using System;
using System.Windows.Forms;
using Medallion.Shell;
using System.Diagnostics;
using System.IO;




namespace WBrowser
{
	/// <summary>
	/// Класс служит для управления сервером nginx, который отображает документацию в браузере.
	/// 		
	///	    • Stop — быстрое завершение 
	///		• Quit — плавное завершение 
	///		• Reload — перезагрузка конфигурационного файла 
	///		• Reopen — переоткрытие лог-файлов 		
	/// </summary>
	public class DocServer
	{
		
		// disable once FieldCanBeMadeReadOnly.Local
		//Эта строка отключает в VS лшние примечания.
	
		
        string sGet, sPass;
        string progName, wdir, path;		
		bool useShell = false;
		bool redirSOut = false;
		bool redirSIn = false;
		

	/// <summary>	
	///
	/// </summary>		
	public DocServer(string dir ="DocServer", string progName  = "nginx.exe")			
		{	
		
		//Первый вариант, если указано всё
			this.progName = progName;            

            if (Directory.Exists(dir)){
				this.wdir = dir;
                this.path = (string)String.Format(this.wdir + "\\" + progName);
				return;
            }
			
		//Второй вариант, когда программа находится в подпапке той же родительской папки
		this.wdir = (string)String.Format( Environment.CurrentDirectory + "\\" + dir);
		if(Directory.Exists(this.wdir))
				{
				this.path = (string)String.Format(this.wdir + "\\" + progName);
				return;
				}
			
		//Третий вариант, если программа находится в подпапке проекта и там скомпилирована
		//Visual Studio или SharpDevelop
		this.wdir =(string) String.Format("..\\..\\..\\..\\" + dir);
		if (Directory.Exists(this.wdir))
			{
                         
                this.path = (string)String.Format(this.wdir + "\\" + progName);
			}


		}
		
	/// <summary>	
	///
	/// </summary>	
		
	public string Start()
		{			
	var command = Command.Run( path, new string[] { "" } );
           // command.StandardInput.WriteLine("<i>Привет, Сервер!</i>");
           // command.StandardInput.Close();
            command.StandardOutput.ReadToEnd();

     var outText = command.Result.StandardOutput;
     var errText = command.Result.StandardError;
     var exitCode = command.Result.ExitCode;
            //var results = command.StandardOutput.GetLines().ToArray();

            return sGet = String.Format(outText + "; "+ errText+ "; " + exitCode);
			
		}
		
	/// <summary>	
	///Быстрая остановка
	/// </summary>	
		
	public void Stop(){var cmd = Command.Run(path, new string[] { "-s", "stop" });}

	/// <summary>	
	///Перезагрузка сервера
	/// </summary>	
	public void Reload(){var cmd = Command.Run(path, new string[] { "-s", "reload" });}
		
	/// <summary>	
	///Переоткрыть конфигурацию в случае её изменения
	/// </summary>
	public void Reopen(){var cmd = Command.Run(path, new string[] { "-s", "reopen" });}
	
	/// <summary>	
	///Постепенное закрытие сервера
	/// </summary>
	public void Quit(){var cmd = Command.Run(path, new string[] { "-s", "quit" });}
	
	/// <summary>	
	///Деструктор класса
	/// </summary>			
	~DocServer()
        {
            try
            {


                if (File.Exists(path)) this.Quit();
            }
            catch { }

        }
		
		
	}
}

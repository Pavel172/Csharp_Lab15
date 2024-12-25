using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Newtonsoft.Json;
using System.Threading;

namespace FileWatcherLogger
{
    // Задание 1: Наблюдатель за файловой системой
    public interface IFileObserver
    {
        void OnFileChanged(string filePath);
    }

    public class FileSystemWatcherCustom
    {
        private readonly string _directoryPath;
        private readonly List<IFileObserver> _observers = new List<IFileObserver>();
        private HashSet<string> _previousState;
        private readonly System.Timers.Timer _timer;

        public FileSystemWatcherCustom(string directoryPath)
        {
            _directoryPath = directoryPath;
            _previousState = GetDirectoryState();
            _timer = new System.Timers.Timer(500);
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
        }

        private HashSet<string> GetDirectoryState()
        {
            var files = Directory.GetFiles(_directoryPath);
            return new HashSet<string>(files);
        }

        public void AddObserver(IFileObserver observer)
        {
            _observers.Add(observer);
        }

        public void RemoveObserver(IFileObserver observer)
        {
            _observers.Remove(observer);
        }

        private void NotifyObservers(string filePath)
        {
            foreach (var observer in _observers)
            {
                observer.OnFileChanged(filePath);
            }
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var currentState = GetDirectoryState();
            var newFiles = currentState.Except(_previousState);
            var deletedFiles = _previousState.Except(currentState);

            foreach (var file in newFiles)
            {
                NotifyObservers(file);
            }

            foreach (var file in deletedFiles)
            {
                NotifyObservers(file);
            }

            _previousState = currentState;
        }
    }

    public class FileObserver : IFileObserver
    {
        public void OnFileChanged(string filePath)
        {
            Console.WriteLine($"File changed: {filePath}");
        }
    }

    // Задание 2: Логгер с репозиторием
    public interface ILogRepository : IDisposable
    {
        void WriteToTextFile(string message);
        void WriteToJsonFile(string message);
        void Save();
    }

    public class TextLogRepository : ILogRepository
    {
        private readonly string _filePath;

        public TextLogRepository(string filePath)
        {
            _filePath = filePath;
        }

        public void WriteToTextFile(string message)
        {
            File.AppendAllText(_filePath, message + Environment.NewLine);
        }

        public void WriteToJsonFile(string message) { }

        public void Save() { }

        public void Dispose() { }
    }

    public class JsonLogRepository : ILogRepository
    {
        private readonly string _jsonFilePath;
        private readonly List<string> _logEntries;

        public JsonLogRepository(string jsonFilePath)
        {
            _jsonFilePath = jsonFilePath;
            _logEntries = new List<string>();
        }

        public void WriteToTextFile(string message) { }

        public void WriteToJsonFile(string message)
        {
            _logEntries.Add(message);
        }

        public void Save()
        {
            var logObject = new { Logs = _logEntries };
            string json = JsonConvert.SerializeObject(logObject, Formatting.Indented);
            File.WriteAllText(_jsonFilePath, json);
        }

        public void Dispose()
        {
            Save();
        }
    }

    public class Logger
    {
        private readonly ILogRepository _repository;

        public Logger(ILogRepository repository)
        {
            _repository = repository;
        }

        public void Log(string message)
        {
            _repository.WriteToTextFile(message);
            _repository.WriteToJsonFile(message);
            _repository.Save();
        }
    }

    // Задание 3: Синглтон генератор случайных чисел
    public class SingleRandomizer
    {
        private static SingleRandomizer _instance;
        private static readonly object _lock = new object();
        private readonly Random _random;

        private SingleRandomizer()
        {
            _random = new Random();
        }

        public static SingleRandomizer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SingleRandomizer();
                        }
                    }
                }
                return _instance;
            }
        }

        public int NextNumber()
        {
            return _random.Next(1, 101);
        }
    }

    class Program
    {
        static void Main()
        {
            // Демонстрация Задания 1: Наблюдатель за файловой системой
            var fileSystemWatcher = new FileSystemWatcherCustom(@"D:\My projects\C#_LAB15\bin\Debug\net8.0");
            var observer = new FileObserver();
            fileSystemWatcher.AddObserver(observer);

            // Демонстрация Задания 2: Логгер
            using (var textLogger = new TextLogRepository("log.txt"))
            {
                var logger = new Logger(textLogger);
                logger.Log("txt log message");
            }

            using (var jsonLogger = new JsonLogRepository("log.json"))
            {
                var logger = new Logger(jsonLogger);
                logger.Log("JSON log message");
            }

            // Демонстрация Задания 3: Единый генератор случайных чисел
            var randomizer1 = SingleRandomizer.Instance;
            var randomizer2 = SingleRandomizer.Instance;

            Console.WriteLine(randomizer1.NextNumber());
            Console.WriteLine(randomizer2.NextNumber());
            Console.WriteLine(randomizer1 == randomizer2);
        }
    }
}
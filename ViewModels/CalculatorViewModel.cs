using Calculator.Models;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Globalization;
using static Calculator.Models.Calculator;

namespace Calculator.ViewModels
{
    class CalculatorViewModel : INotifyPropertyChanged
    {
        // Модель калькулятора
        private Models.Calculator _calculator = new Models.Calculator();

        // Событие уведомления View об изменении свойств ViewModel
        public event PropertyChangedEventHandler? PropertyChanged;

        // Замена внутренних символов операций на символы интерфейса
        public static Dictionary<string, string> UIOperationKeys = new Dictionary<string, string>
        {
            { "*", "×" },
            { "/", "÷" }
        };

        // Подписка на изменения модели
        public CalculatorViewModel()
        {
            _calculator.PropertyChanged += CalculatorPropertyChanged;

            Debug.WriteLine("[VIEWMODEL][КОНСТРУКТОР] CalculatorViewModel создан");
        }

        // Обновляет свойства отображения при изменении операндов модели
        private void CalculatorPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"[VIEWMODEL][СОБЫТИЕ] Изменено свойство модели: {e.PropertyName}");

            if (e.PropertyName == nameof(_calculator.SecondOperand) || e.PropertyName == nameof(_calculator.MainOperand))
            {
                Debug.WriteLine("[VIEWMODEL][ОБНОВЛЕНИЕ] Выполняется обновление отображаемых данных");

                OnPropertyChanged(nameof(SecondInputString));
                OnPropertyChanged(nameof(MainInputString));
            }
        }

        // Вызывает обновление привязанных свойств ViewModel к View
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        // Преобразует строку с внутренними символами операций в строку с символами интерфейса
        private string ParseString(string str)
        {
            foreach (string key in UIOperationKeys.Keys)
            {
                str = str.Replace(key, UIOperationKeys[key]);
            }

            return str;
        }

        // Основная строка калькулятора
        public string? MainInputString
        {
            get
            {
                string result = ParseString(_calculator.MainOperand.ToString("#,##0.######"));

                if (_calculator.isBeginDot)
                    result += ",";

                switch (_calculator.Statement)
                {
                    case Statements.ERROR:
                        return "Error";

                    default:
                        return result;
                }
            }
        }

        // Строка истории вычислений
        public string SecondInputString
        {
            get
            {
                switch (_calculator.Statement)
                {
                    case Statements.ENTERING_SECOND:
                        return ParseString($"{_calculator.SecondOperand.ToString("#,##0.######")} {_calculator.symbol}");

                    case Statements.SHOWING_RESULT:
                        return ParseString($"{_calculator.SecondOperand.ToString("#,##0.######")} {_calculator.symbol} {_calculator.lastMainOperand.ToString("#,##0.######")} =");

                    default:
                        return "";
                }
            }
        }

        // Передает ввод символа в модель
        private void AddSymbol(string? symbol)
        {
            Debug.WriteLine($"[VIEWMODEL][КОМАНДА] Ввод символа '{symbol}'");

            _calculator.Add(symbol);
        }

        // Запускает вычисление выражения
        private void Evaluate()
        {
            Debug.WriteLine("[VIEWMODEL][КОМАНДА] Выполнение вычисления");

            _calculator.Evaluate();
        }

        // Передает выбранную операцию в модель
        private void Operation(string? symbol)
        {
            Debug.WriteLine($"[VIEWMODEL][КОМАНДА] Выбрана операция '{symbol}'");

            _calculator.Operation(symbol);
        }

        // Меняет знак текущего числа
        private void UnaryMinus()
        {
            Debug.WriteLine("[VIEWMODEL][КОМАНДА] Смена знака числа");

            _calculator.UnaryMinus();
        }

        // Выполняет команду очистки
        private void Clear(string type)
        {
            Debug.WriteLine($"[VIEWMODEL][КОМАНДА] Очистка '{type}'");

            _calculator.Clear(type);
        }

        // Команды интерфейса
        public ICommand AddCommand
        {
            get
            {
                return new RelayCommand<string?>(AddSymbol, SymbolLimit);
            }
        }

        public ICommand EvaluateCommand
        {
            get
            {
                return new RelayCommand(Evaluate);
            }
        }

        public ICommand OperationCommand
        {
            get
            {
                return new RelayCommand<string?>(Operation);
            }
        }

        public ICommand UnaryMinusCommand
        {
            get
            {
                return new RelayCommand(UnaryMinus);
            }
        }

        public ICommand ClearCommand
        {
            get
            {
                return new RelayCommand<string?>(Clear);
            }
        }

        // Ограничивает максимальную длину вводимого числа
        private bool SymbolLimit(string? symbol)
        {
            Debug.WriteLine($"[VIEWMODEL][ПРОВЕРКА] Длина строки: {MainInputString?.Length}");

            if (MainInputString?.Length >= 25)
            {
                Debug.WriteLine("[VIEWMODEL][ПРОВЕРКА] Достигнут лимит длины числа");
                return false;
            }

            return true;
        }
    }
}
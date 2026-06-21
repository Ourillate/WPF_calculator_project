using Calculator.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Calculator.Models
{
    public class Calculator : INotifyPropertyChanged
    {
        // Событие уведомления CalculatorViewModel об изменении свойств модели
        public event PropertyChangedEventHandler? PropertyChanged;

        // Вызывает обновление привязанных свойтв модели в CalculatorViewModel
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        // Определение таблицы соответствия операций и методов вычисления
        private Dictionary<Operations, Func<decimal>> OperationFuncs;

        public bool isBeginDot = false;

        public Calculator()
        {
            // Установка значений таблицы
            OperationFuncs = new()
            {
                { Operations.ADDITION, Addition },
                { Operations.SUBTRACTION, Substraction },
                { Operations.MULTIPLICATION, Multiplication },
                { Operations.DIVISION, Division }
            };

            Debug.WriteLine("[MODEL][КОНСТРУКТОР] Калькулятор создан");
        }

        // Перечисление арифметических операции калькулятора
        public enum Operations
        {
            NONE,
            ADDITION,
            SUBTRACTION,
            MULTIPLICATION,
            DIVISION,
            UNARY_MINUS
        };

        // Преобразование символа операции в элемент перечисления
        private Dictionary<string, Operations> SymbolToOperation = new()
        {
            { "+", Operations.ADDITION },
            { "-", Operations.SUBTRACTION },
            { "*", Operations.MULTIPLICATION },
            { "/", Operations.DIVISION }
        };

        // Текущее состояние калькулятора
        public enum Statements
        {
            ENTERING_FIRST,
            ENTERING_SECOND,
            SHOWING_RESULT,
            ERROR
        };

        public Statements Statement
        {
            get;
            private set;
        } = Statements.ENTERING_FIRST;

        public Operations operation = Operations.NONE;
        public string? symbol = "";

        // Первый операнд для выполняемой операции
        private decimal _secondOperand = 0;
        public decimal SecondOperand
        {
            get => _secondOperand;
            private set
            {
                _secondOperand = value;
                OnPropertyChanged(nameof(SecondOperand));
            }
        }

        // Предыдущий второй операнд для повторного нажатия "="
        public decimal lastMainOperand = 0;

        // Второй операнд для выполняемой операции
        private decimal _mainOperand = 0;
        public decimal MainOperand
        {
            get => _mainOperand;
            private set
            {
                _mainOperand = value;
                OnPropertyChanged(nameof(MainOperand));
            }
        }

        // Добавляет цифру или разделитель к текущему числу
        public void Add(string? symbol)
        {
            Debug.WriteLine($"[MODEL][ВВОД] Нажата кнопка '{symbol}'. Текущее число: {MainOperand}");

            string? str = "";

            if(Statement == Statements.SHOWING_RESULT)
            {
                Reset();
            }
            else if(Statement == Statements.ENTERING_SECOND && MainOperand != 0)
            {
                MainOperand = 0;
            }

            if (MainOperand != 0)
                str = MainOperand.ToString();
            if (isBeginDot && symbol == ",")
                return;
            else if (symbol == "," && !str.Contains(","))
            {
                isBeginDot = true;
                OnPropertyChanged(nameof(MainOperand));
            }

            if (isBeginDot && symbol != ",")
            {
                isBeginDot = false;
                MainOperand = decimal.Parse(str + "," + symbol);
            }
            else if (symbol != ",")
                MainOperand = decimal.Parse(str + symbol);

            Debug.WriteLine($"[MODEL][ВВОД] Новое значение числа: {MainOperand}");
        }

        // Переводит калькулятор в состояние ошибки
        private void ThrowError()
        {
            Debug.WriteLine("[MODEL][ОШИБКА] Калькулятор перешел в состояние ошибки");

            Statement = Statements.ERROR;
            SecondOperand = 0;
            MainOperand = 0;
        }

        // Полностью сбрасывает состояние калькулятора
        private void Reset()
        {
            Debug.WriteLine("[MODEL][СБРОС] Калькулятор полностью очищен");

            Statement = Statements.ENTERING_FIRST;
            SecondOperand = 0;
            MainOperand = 0;
            isBeginDot = false;
            operation = Operations.NONE;
        }

        // Выполняет выбранную арифметическую операцию
        public void Evaluate()
        {
            Debug.WriteLine($"[MODEL][ВЫЧИСЛЕНИЕ] Начало вычисления. Состояние: {Statement}, Операция: {operation}");

            if (Statement == Statements.ERROR)
            {
                Reset();
                return;
            }
            else if (Statement == Statements.SHOWING_RESULT)
            {
                SecondOperand = MainOperand;
                MainOperand = lastMainOperand;
                operation = SymbolToOperation[symbol!];
            }
            else if (operation == Operations.NONE)
                return;
            else if (Statement == Statements.SHOWING_RESULT)
            {
                Reset();
            }

            decimal result = 0;

            Statement = Statements.SHOWING_RESULT;

            result = OperationFuncs[operation]();

            Debug.WriteLine($"[MODEL][ВЫЧИСЛЕНИЕ] Получен результат: {result}");

            if (Statement == Statements.ERROR)
                return;

            operation = Operations.NONE;
            lastMainOperand = MainOperand;
            MainOperand = result;

            Debug.WriteLine("[MODEL][ВЫЧИСЛЕНИЕ] Вычисление успешно завершено");
            Debug.WriteLine($"[MODEL][СОСТОЯНИЕ] Второй операнд={MainOperand}, Первый операнд={SecondOperand}, Предыдущий второй операнд={lastMainOperand}\n" +
                $"[MODEL][СОСТОЯНИЕ] Выражение={SecondOperand}{symbol}{MainOperand}");
        }

        // Выбирает арифметическую операцию и сохраняет первый операнд
        public void Operation(string? symbol)
        {
            Debug.WriteLine($"[MODEL][ОПЕРАЦИЯ] Выбрана операция '{symbol}', Текущее число: {MainOperand}, Состояние: {Statement}");

            if (Statement == Statements.ERROR)
            {
                Reset();
                return;
            }
            else if (Statement == Statements.ENTERING_SECOND)
            {
                Evaluate();
                return;
            }

            this.symbol = symbol;
            operation = SymbolToOperation[symbol!];
            
            if (Statement == Statements.SHOWING_RESULT)
            {
                Statement = Statements.ENTERING_SECOND;
                SecondOperand = MainOperand;
            }
            else
            {
                Statement = Statements.ENTERING_SECOND;
                SecondOperand = MainOperand;
                MainOperand = 0;
            }

            Debug.WriteLine($"[MODEL][ОПЕРАЦИЯ] Тип операции: {operation}");
            Debug.WriteLine($"[MODEL][ОПЕРАЦИЯ] Первый операнд сохранен: {SecondOperand}");
        }

        // Арифметические операции
        private decimal Addition()
        {
            Debug.WriteLine($"[MODEL][СЛОЖЕНИЕ] {SecondOperand} + {MainOperand}");
            return SecondOperand + MainOperand;
        }

        private decimal Substraction()
        {
            Debug.WriteLine($"[MODEL][ВЫЧИТАНИЕ] {SecondOperand} - {MainOperand}");
            return SecondOperand - MainOperand;
        }

        private decimal Multiplication()
        {
            Debug.WriteLine($"[MODEL][УМНОЖЕНИЕ] {SecondOperand} * {MainOperand}");
            return SecondOperand * MainOperand;
        }

        private decimal Division()
        {
            Debug.WriteLine($"[MODEL][ДЕЛЕНИЕ] {SecondOperand} / {MainOperand}");

            if (MainOperand == 0)
            {
                Debug.WriteLine("[MODEL][ОШИБКА] Попытка деления на ноль");
                ThrowError();
                return 0;
            }

            return SecondOperand / MainOperand;
        }

        // Меняет знак текущего числа
        public void UnaryMinus()
        {
            Debug.WriteLine($"[MODEL][СМЕНА ЗНАКА] Было: {MainOperand}");

            MainOperand = 0 - MainOperand;

            Debug.WriteLine($"[MODEL][СМЕНА ЗНАКА] Стало: {MainOperand}");
        }

        // Выполняет команды очистки
        public void Clear(string type)
        {
            Debug.WriteLine($"[MODEL][ОЧИСТКА] Команда: {type}");

            switch (type)
            {
                case "C":
                    Reset();
                    Debug.WriteLine($"[MODEL][ОЧИСТКА] Новое значение: {MainOperand}");
                    break;

                case "CE":
                    MainOperand = 0;
                    Debug.WriteLine($"[MODEL][ОЧИСТКА] Новое значение: {MainOperand}");
                    break;

                case "BS":
                    string str = MainOperand.ToString();

                    if (str == "0")
                        return;
                    else if (str.Length == 1)
                        MainOperand = 0;
                    else
                        MainOperand = decimal.Parse(str.Remove(str.Length - 1));

                    Debug.WriteLine($"[MODEL][ОЧИСТКА] Новое значение: {MainOperand}");
                    break;
            }
        }
    }
}
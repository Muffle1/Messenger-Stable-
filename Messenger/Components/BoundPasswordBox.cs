using System.Windows;
using System.Windows.Controls;

namespace Messenger
{
    public class BoundPasswordBox
    {
        private static bool _updating = false;

        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword",
                typeof(string),
                typeof(BoundPasswordBox),
                new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static readonly DependencyProperty LengthPasswordProperty =
            DependencyProperty.RegisterAttached("PasswordLength",
                typeof(int),
                typeof(BoundPasswordBox));

        /// <summary>
        /// Возврат значения в PasswordBox
        /// </summary>
        /// <param name="d">объект PasswordBox</param>
        /// <returns>значение в PasswordBox</returns>
        public static string GetBoundPassword(DependencyObject d) =>
            (string)d.GetValue(BoundPasswordProperty);

        /// <summary>
        /// Возврат количества символов в PasswordBox
        /// </summary>
        /// <param name="d">объект PasswordBox</param>
        /// <returns>количество символов в PasswordBox</returns>
        public static int GetPasswordLength(DependencyObject d) =>
            (int)d.GetValue(LengthPasswordProperty);

        /// <summary>
        /// Установка значения в PasswordBox
        /// </summary>
        /// <param name="d">объект PasswordBox</param>
        /// <param name="value">значение</param>
        public static void SetBoundPassword(DependencyObject d, string value) =>
            d.SetValue(BoundPasswordProperty, value);

        /// <summary>
        /// Установка значения в длину строки PasswordBox
        /// </summary>
        /// <param name="d">объект PasswordBox</param>
        /// <param name="value">значение</param>
        public static void SetPasswordLength(DependencyObject d, int value) =>
            d.SetValue(LengthPasswordProperty, value);

        /// <summary>
        /// Обработка события при изменении значения в PasswordBox
        /// </summary>
        /// <param name="d">объект PasswordBox</param>
        /// <param name="e">аргументы события</param>
        private static void OnBoundPasswordChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            PasswordBox password = d as PasswordBox;
            if (password != null)
                password.PasswordChanged -= PasswordChanged;

            if (e.NewValue != null)
            {
                if (!_updating)
                    password.Password = e.NewValue.ToString();
            }
            else
                password.Password = string.Empty;

            password.PasswordChanged += new RoutedEventHandler(PasswordChanged);
        }

        /// <summary>
        /// Обработка события при изменении значения в PasswordBox
        /// </summary>
        /// <param name="sender">объект PasswordBox</param>
        /// <param name="e">аргументы события</param>
        static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox password = sender as PasswordBox;
            _updating = true;
            SetBoundPassword(password, password.Password);
            SetPasswordLength(password, password.Password.Length);
            _updating = false;
        }

    }
}

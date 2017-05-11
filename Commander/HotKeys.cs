using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Commander
{
    public sealed class HotKeys : IMessageFilter, IDisposable
    {
        public event KeyEventHandler HotKeyPressed;

        private readonly int _id;

        #region Native win32 API

        private const int WmHotKey = 0x0312;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, KeyModifiers fsModifiers, Keys vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [Flags]
        public enum KeyModifiers
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Windows = 8
        }

        #endregion

        public event EventHandler KeyChanged;
        public event EventHandler KeyModifierChanged;
        private IntPtr _handle;
        public IntPtr Handle
        {
            get { return _handle; }
            set { _handle = value; }
        }

        private Keys _key;
        private KeyModifiers _keyModifier;
        private bool _isKeyRegisterd;

        public HotKeys()
        {
            _id = GetHashCode();
            Application.AddMessageFilter(this);
        }

        ~HotKeys()
        {
            Dispose();
        }

        public void Dispose()
        {
            Application.RemoveMessageFilter(this);
            UnregisterHotKey(_handle, _id);
        }

        private void RegisterHotKey()
        {
            if (_key == Keys.None)
                return;
            if (_isKeyRegisterd)
                _isKeyRegisterd = !(UnregisterHotKey(_handle, _id));
            _isKeyRegisterd = RegisterHotKey(_handle, _id, _keyModifier, _key);
            if (!_isKeyRegisterd)
                throw new ApplicationException("Hotkey allready in use");
        }

        [Bindable(true), Category("HotKey")]
        public Keys Key
        {
            get { return _key; }
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnKeyChanged(new EventArgs());
                }
            }
        }

        [Bindable(true), Category("HotKey")]
        public KeyModifiers KeyModifier
        {
            get { return _keyModifier; }

            set
            {
                if (_keyModifier != value)
                {
                    _keyModifier = value;
                    OnKeyModifierChanged(new EventArgs());
                }
            }
        }

        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case WmHotKey:
                    if ((int)(m.WParam) == _id)
                    {
                        KeyEventArgs args = new KeyEventArgs((IsButtonDown(m.LParam, KeyModifiers.Alt) ? Keys.Alt : Keys.None)
                                | (IsButtonDown(m.LParam, KeyModifiers.Control) ? Keys.Control : Keys.None)
                                | (IsButtonDown(m.LParam, KeyModifiers.Shift) ? Keys.Shift : Keys.None) | Key);

                        OnHotKeyPressed(args);
                        return true;
                    }
                    break;
            }
            return false;
        }

        public static bool IsButtonDown(IntPtr ptr, KeyModifiers keyModifiers)
        {
            return Convert.ToBoolean(((long)ptr) & (long)keyModifiers);
        }

        private void OnHotKeyPressed(KeyEventArgs e)
        {
            if (HotKeyPressed != null)
                HotKeyPressed(this, e);
        }

        private void OnKeyChanged(EventArgs e)
        {
            RegisterHotKey();
            if (KeyChanged != null)
                KeyChanged(this, e);
        }

        private void OnKeyModifierChanged(EventArgs e)
        {
            RegisterHotKey();
            if (KeyModifierChanged != null)
                KeyModifierChanged(this, e);
        }
    }
}
/*
void perebor_updates(string begin_dir, string end_dir)
{
    //Берём нашу исходную папку
    DirectoryInfo dir_inf = new DirectoryInfo(begin_dir);
    //Перебираем все внутренние папки
    foreach (DirectoryInfo dir in dir_inf.GetDirectories())
    {
        //Проверяем - если директории не существует, то создаём;
        if (Directory.Exists(end_dir + "\\" + dir.Name) != true)
        {
            Directory.CreateDirectory(end_dir + "\\" + dir.Name);
        }

        //Рекурсия (перебираем вложенные папки и делаем для них то-же самое).
        perebor_updates(dir.FullName, end_dir + "\\" + dir.Name);
    }

    //Перебираем файлики в папке источнике.
    foreach (string file in Directory.GetFiles(begin_dir))
    {
        //Определяем (отделяем) имя файла с расширением - без пути (но с слешем "\").
        string filik = file.Substring(file.LastIndexOf('\\'), file.Length - file.LastIndexOf('\\'));
        //Копируем файлик с перезаписью из источника в приёмник.
        File.Copy(file, end_dir + "\\" + filik, true);
    }
}
*/

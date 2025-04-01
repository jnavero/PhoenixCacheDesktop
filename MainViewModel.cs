using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PhoenixCacheDesktop.Services;


namespace PhoenixCacheDesktop.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ApiClient _apiClient;
        private string _selectedKey;
        private string _value;
        private string _ttl;
        private string _host = "localhost";
        private string _port = "8080";

        public ObservableCollection<string> Keys { get; set; } = new ObservableCollection<string>();
        public List<CacheDto> Cache { get; set; } = new List<CacheDto>();

        public string SelectedKey
        {
            get => _selectedKey;
            set
            {
                _selectedKey = value;
                OnPropertyChanged();
                LoadKeyDetails();
            }
        }

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public string TTL
        {
            get => _ttl;
            set
            {
                _ttl = value;
                OnPropertyChanged();
            }
        }
    

        public string Host
        {
            get => _host;
            set { 
                _host = value; 
                OnPropertyChanged(); 
            }
        }

        public string Port
        {
            get => _port;
            set { 
                _port = value; 
                OnPropertyChanged(); 
            }
        }

        public ICommand ConnectCommand { get; }
        public ICommand FlushCommand { get; }
        public ICommand RemoveKeyCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            _apiClient = new ApiClient();
            ConnectCommand = new RelayCommand(async () => await ConnectAsync());
            FlushCommand = new RelayCommand(async () => await FlushCacheAsync());
            RemoveKeyCommand = new RelayCommand(async () => await RemoveKeyAsync());
        }

        private async Task ConnectAsync()
        {
           var result = await _apiClient.UpdateBaseUrl(Host, Port);
            if (!result)
            {
                return;
            }
            await LoadKeysAsync();
        }

        private async Task LoadKeysAsync()
        {
            Keys.Clear();
            Cache.Clear();

            var keys = await _apiClient.ListKeysAsync();
            if (keys == null)
                return;

            foreach (var key in keys)
            {
                Cache.Add(key);
                Keys.Add(key.key);
            }
        }

        private async void LoadKeyDetails()
        {
            if (string.IsNullOrEmpty(SelectedKey))
            {
                return;
            }

            var vAux = Cache.FirstOrDefault(t => t.key == SelectedKey);
            Value = vAux != null ? vAux.value : null;

            TTL = vAux != null ? vAux.expires_in : null;
        }

        private async Task FlushCacheAsync()
        {
            await _apiClient.FlushCacheAsync();
            await LoadKeysAsync();
        }

        private async Task RemoveKeyAsync()
        {
            if (string.IsNullOrEmpty(SelectedKey))
                return;
            await _apiClient.RemoveKeyAsync(SelectedKey);
            Value = string.Empty;
            TTL = string.Empty;
            Keys.Remove(SelectedKey);
            await LoadKeysAsync();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly bool _canExecute;

        public RelayCommand(Func<Task> execute, bool canExecute = true)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute;

        public async void Execute(object parameter) => await _execute();

        public event EventHandler CanExecuteChanged;
    }
}
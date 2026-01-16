using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public abstract class RealTimeViewModel : ObservableObject, IDisposable
    {
        private readonly DispatcherTimer _timer;

        public RealTimeViewModel()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += async (s, e) => await OnTimerTick();
            _timer.Start();
        }
        private async Task OnTimerTick()
        {
            if (CanAutoRefresh())
            {
                await PerformSilentUpdateAsync();
            }
        }
        protected abstract Task PerformSilentUpdateAsync();
        protected virtual bool CanAutoRefresh()
        {
            return true; 
        }
        public void Dispose()
        {
            _timer?.Stop();
        }
    }
}

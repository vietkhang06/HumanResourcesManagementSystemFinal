using CommunityToolkit.Mvvm.ComponentModel;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class ChangeHistoryViewModel : ObservableObject
    {
        private ObservableCollection<ChangeHistory> _histories = new();
        public ObservableCollection<ChangeHistory> Histories
        {
            get => _histories;
            set => SetProperty(ref _histories, value);
        }

        public ChangeHistoryViewModel()
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var context = new DataContext())
                {
                    var list = context.ChangeHistories
                        .AsNoTracking()
                        .Include(h => h.Account)
                        .ThenInclude(a => a.Employee)
                        .OrderByDescending(h => h.ChangeTime)
                        .Take(100)
                        .ToList();

                    Histories = new ObservableCollection<ChangeHistory>(list);
                }
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine(ex.Message);

                var inner = ex.InnerException;
                while (inner != null)
                {
                    sb.AppendLine(inner.Message);
                    inner = inner.InnerException;
                }

                MessageBox.Show("Không thể tải dữ liệu lịch sử.\nChi tiết lỗi:\n" + sb.ToString(), "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
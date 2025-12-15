using CommunityToolkit.Mvvm.ComponentModel;
using HumanResourcesManagementSystemFinal.Data;
using HumanResourcesManagementSystemFinal.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace HumanResourcesManagementSystemFinal.ViewModels
{
    public partial class ChangeHistoryViewModel : ObservableObject
    {
        public ObservableCollection<ChangeHistory> Histories { get; set; } = new();

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
                        .Include(h => h.Account)          // Load tài khoản thực hiện
                        .ThenInclude(a => a.Employee)     // Load thông tin nhân viên của tài khoản đó
                        .OrderByDescending(h => h.ChangeTime) // Mới nhất lên đầu
                        .Take(100) // Chỉ lấy 100 dòng mới nhất để đỡ lag
                        .ToList();

                    Histories.Clear();
                    foreach (var item in list)
                    {
                        Histories.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch sử: " + ex.Message);
            }
        }
    }
}
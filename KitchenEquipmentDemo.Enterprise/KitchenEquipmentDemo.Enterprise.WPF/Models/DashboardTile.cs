using System.Windows.Input;

namespace KitchenEquipmentDemo.Enterprise.WPF.Models
{
    public class DashboardTile
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public ICommand Command { get; set; }
        public bool IsVisible { get; set; } = true;
    }
}
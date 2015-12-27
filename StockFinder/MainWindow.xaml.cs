using StockFinder.viewmodel;
using System.Windows;

namespace StockFinder
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new ViewModel(stockChart);
            this.DataContext = vm;
            //stockChart.RenderableSeries[0].DataSeries = vm.StockGraphOHLC;
            //stockChart.RenderableSeries[1].DataSeries = vm.StockGraphMA;
        }
    }
}

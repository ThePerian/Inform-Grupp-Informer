using System.Windows.Controls;

namespace IGCInformer
{
    public class RssChannelStackPanel : StackPanel
    {
        public int Id { get; set; }
        public int ItemCount { get; set; }

        public RssChannelStackPanel(int id):base()
        {
            this.Id = id;
        }
    }
}

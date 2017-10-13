using System.ComponentModel;

namespace Hodlr
{
    public class WrappedCell<T> : INotifyPropertyChanged
    {
        public T Item { get; set; }
        bool isSelected = false;
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("SELECTED"));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

    }
}

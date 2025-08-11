using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace YAAL.Assets.Scripts
{
    public static class ListSorter
    {
        public static void AddSorted(ObservableCollection<string> collection, string newItem)
        {
            int index = 0;
            while (index < collection.Count && string.Compare(collection[index], newItem, StringComparison.OrdinalIgnoreCase) < 0)
            {
                index++;
            }
            collection.Insert(index, newItem);
        }
    }
}

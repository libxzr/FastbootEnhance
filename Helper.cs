using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace FastbootEnhance
{
    class Helper
    {
        public class ListHelper<T>
        {
            public delegate bool Filter(T t);

            ListView listView;
            List<T> items;
            Filter filter;

            public ListHelper(ListView listView, Filter filter)
            {
                this.listView = listView;
                items = new List<T>();
                this.filter = filter;
            }

            public void clear()
            {
                listView.SelectedItems.Clear();
                listView.Items.Clear();
                items = new List<T>();
            }

            public void addItem(T item)
            {
                items.Add(item);
            }

            public void render()
            {
                listView.Items.Clear();
                foreach (T t in items)
                {
                    listView.Items.Add(t);
                }
            }

            public void doFilter()
            {
                listView.Items.Clear();
                foreach (T t in items)
                {
                    if (filter(t))
                        listView.Items.Add(t);
                }
            }
        }

        public delegate void PathSelectCallback(string path);
        public static void fileSelect(PathSelectCallback callback)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                callback(openFileDialog.FileName);
            }
        }

        public static void pathSelect(PathSelectCallback callback)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.Description = Properties.Resources.select_save_path;

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                callback(folderBrowserDialog.SelectedPath);
            }
        }

        public static DateTime timeStamp2DataTime(Int64 timestamp)
        {
            Int64 begtime = timestamp * 10000000;
            DateTime dt_1970 = new DateTime(1970, 1, 1, 8, 0, 0);
            long tricks_1970 = dt_1970.Ticks;
            long time_tricks = tricks_1970 + begtime;
            DateTime dt = new DateTime(time_tricks);
            return dt;
        }

        public static string byte2AUnit(ulong size)
        {
            if (size > 1024 * 1024 * 1024)
                return (int)((double)size / 1024 / 1024 / 1024 * 100) / 100.0 + " GB";

            if (size > 1024 * 1024)
                return (int)((double)size / 1024 / 1024 * 100) / 100.0 + " MB";

            if (size > 1024)
                return (int)((double)size / 1024 * 100) / 100.0 + " KB";

            return size + " B";
        }
    }
}

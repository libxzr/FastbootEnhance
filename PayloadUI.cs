using ChromeosUpdateEngine;
using System;
using System.Threading;
using System.Windows;

namespace FastbootEnhance
{
    class PayloadUI
    {
        public const string PAYLOAD_TMP = ".\\payload.tmp.dumper";

        enum page_status
        {
            empty,
            loaded
        }

        static page_status cur_status;
        public static Payload payload;

        static void switchMainView()
        {
            switch (cur_status)
            {
                case page_status.empty:
                    MainWindow.THIS.payload_before_load.Visibility = Visibility.Visible;
                    MainWindow.THIS.payload_after_load.Visibility = Visibility.Hidden;
                    break;
                case page_status.loaded:
                    MainWindow.THIS.payload_before_load.Visibility = Visibility.Hidden;
                    MainWindow.THIS.payload_after_load.Visibility = Visibility.Visible;
                    break;
            }
        }

        static void onLoad(string filename)
        {
            Exception exception = null;

            MainWindow.THIS.payload_load_btn.Visibility = Visibility.Hidden;
            MainWindow.THIS.payload_opening.Visibility = Visibility.Visible;

            Action load = new Action(delegate
            {
                try
                {
                    payload = new Payload(filename, PAYLOAD_TMP);
                }
                catch (Exception e)
                {
                    exception = e;
                }
            });

            Action afterLoad = new Action(delegate
            {
                MainWindow.THIS.payload_load_btn.Visibility = Visibility.Visible;
                MainWindow.THIS.payload_opening.Visibility = Visibility.Hidden;

                if (exception != null)
                {
                    MessageBox.Show(exception.Message);
                    return;
                }

                Payload.PayloadInitException exc = payload.init();
                if (exc != null)
                {
                    payload.Dispose();
                    payload = null;
                    MessageBox.Show(Properties.Resources.payload_unsupported_format + "\n" + exc.Message);
                    return;
                }

                cur_status = page_status.loaded;
                MainWindow.THIS.payload_cur_open.Content = Properties.Resources.payload_current_file + filename;
                refreshData();
                switchMainView();
            });

            Helper.offloadAndRun(load, afterLoad);
        }

        static void actionInit()
        {
            listHelper = new Helper.ListHelper<payload_partition_info_row>(MainWindow.THIS.payload_partition_info,
                new Helper.ListHelper<payload_partition_info_row>.Filter(
                    delegate (payload_partition_info_row row)
                    {
                        if (MainWindow.THIS.payload_partition_name_textbox.Text == "")
                            return true;

                        if (row.name.Contains(MainWindow.THIS.payload_partition_name_textbox.Text))
                            return true;
                        return false;
                    }
            ));

            MainWindow.THIS.payload_partition_name_textbox.TextChanged += delegate
            {
                listHelper.doFilter();
            };

            MainWindow.THIS.payload_load_btn.Click += delegate
            {
                Helper.fileSelect(new Helper.PathSelectCallback(delegate (string ret)
                {
                    onLoad(ret);
                }), "Payload|*.bin;*.zip");
            };

            MainWindow.THIS.payload_remove.Click += delegate
            {
                payload.Dispose();
                payload = null;
                cur_status = page_status.empty;
                switchMainView();
            };

            MainWindow.THIS.payload_extract.Click += delegate
            {
                if (MainWindow.THIS.payload_partition_info.SelectedItems.Count == 0)
                {
                    MessageBox.Show(Properties.Resources.payload_target_partition_not_selected);
                    return;
                }

                if (payload.manifest.MinorVersion != 0 && !(bool)MainWindow.THIS.payload_ignore_delta.IsChecked)
                {
                    MessageBox.Show(Properties.Resources.payload_incremental_warning);
                    return;
                }

                bool ignore_unknown_op = (bool)MainWindow.THIS.payload_ignore_unknown_op.IsChecked;
                bool ignore_checks = (bool)MainWindow.THIS.payload_ignore_check.IsChecked;

                Helper.pathSelect(new Helper.PathSelectCallback(delegate (string path)
                {
                    MainWindow.THIS.payload_progress.Visibility = Visibility.Visible;
                    MainWindow.THIS.payload_action_bar.Visibility = Visibility.Hidden;
                    Helper.TaskbarItemHelper.start();
                    System.Collections.IList selected = MainWindow.THIS.payload_partition_info.SelectedItems;
                    MainWindow.THIS.payload_extract.IsEnabled = false;
                    MainWindow.THIS.payload_extract_options.IsEnabled = false;

                    new Thread(new ThreadStart(delegate
                    {
                        int full = selected.Count * 2;
                        int now = 0;
                        Payload.PayloadExtractionException exc = null;

                        foreach (payload_partition_info_row row in selected)
                        {
                            MainWindow.THIS.Dispatcher.BeginInvoke(new Action(delegate
                            {
                                MainWindow.THIS.payload_progress.Value = ++now * 100 / full;
                                Helper.TaskbarItemHelper.update(now * 100 / full);
                            }));
                            exc = payload.extract(row.name, path, ignore_unknown_op, ignore_checks);
                            if (exc != null)
                                break;
                            MainWindow.THIS.Dispatcher.BeginInvoke(new Action(delegate
                            {
                                MainWindow.THIS.payload_progress.Value = ++now * 100 / full;
                                Helper.TaskbarItemHelper.update(now * 100 / full);
                            }));
                        }

                        if (exc == null)
                            MessageBox.Show(Properties.Resources.operation_completed);
                        else
                            MessageBox.Show(Properties.Resources.payload_error_occur + "\n" + exc.Message);

                        MainWindow.THIS.Dispatcher.BeginInvoke(new Action(delegate
                        {
                            MainWindow.THIS.payload_progress.Visibility = Visibility.Hidden;
                            MainWindow.THIS.payload_action_bar.Visibility = Visibility.Visible;
                            Helper.TaskbarItemHelper.stop();
                            MainWindow.THIS.payload_extract.IsEnabled = true;
                            MainWindow.THIS.payload_extract_options.IsEnabled = true;
                        }));

                    })).Start();
                }));
            };

            MainWindow.THIS.payload_before_load.DragEnter += delegate (object sender, DragEventArgs e)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effects = DragDropEffects.All;
                else
                    e.Effects = DragDropEffects.None;
            };

            MainWindow.THIS.payload_before_load.Drop += delegate (object sender, DragEventArgs e)
            {
                Array files = (Array)e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 1)
                {
                    MessageBox.Show(Properties.Resources.payload_unable_drop_multifile);
                    return;
                }
                string filename = files.GetValue(0).ToString();
                if (!filename.EndsWith(".bin") || !filename.EndsWith(".zip"))
                {
                    MessageBox.Show(Properties.Resources.payload_unsupported_format);
                    return;
                }
                onLoad(files.GetValue(0).ToString());
            };
        }

        static void refreshData()
        {
            MainWindow.THIS.payload_progress.Visibility = Visibility.Hidden;
            MainWindow.THIS.payload_action_bar.Visibility = Visibility.Visible;
            MainWindow.THIS.payload_info.Items.Clear();
            listHelper.clear();
            MainWindow.THIS.payload_partition_name_textbox.Text = "";
            MainWindow.THIS.payload_dynamic_partition_meta.Items.Clear();

            payloadInfoListAppend(Properties.Resources.payload_version, payload.file_format_version.ToString());
            payloadInfoListAppend(Properties.Resources.payload_manifest_size, Helper.byte2AUnit(payload.manifest_size));
            payloadInfoListAppend(Properties.Resources.payload_metadata_signature_size, Helper.byte2AUnit(payload.metadata_signature_size));
            payloadInfoListAppend(Properties.Resources.payload_metadata_signature, payload.metadata_signature_message.Signatures_[0].Data.ToBase64());
            payloadInfoListAppend(Properties.Resources.payload_data_size, Helper.byte2AUnit(payload.data_size));
            payloadInfoListAppend(Properties.Resources.payload_signature_size, Helper.byte2AUnit(payload.payload_signatures_message_size));
            payloadInfoListAppend(Properties.Resources.payload_signature, payload.payload_signatures_message.Signatures_[0].Data.ToBase64());

            payloadInfoListAppend(Properties.Resources.payload_full_package, payload.manifest.MinorVersion == 0 ?
                Properties.Resources.yes : Properties.Resources.no + " (" + payload.manifest.MinorVersion + ")");
            if (payload.manifest.HasMaxTimestamp)
                payloadInfoListAppend(Properties.Resources.payload_timestamp, Helper.timeStamp2DataTime(payload.manifest.MaxTimestamp).ToString());
            payloadInfoListAppend(Properties.Resources.payload_blocksize, Helper.byte2AUnit(payload.manifest.BlockSize));

            if (payload.manifest.NewImageInfo != null)
            {
                if (payload.manifest.NewImageInfo.HasBoard)
                    payloadInfoListAppend("board", payload.manifest.NewImageInfo.Board);

                if (payload.manifest.NewImageInfo.HasKey)
                    payloadInfoListAppend("key", payload.manifest.NewImageInfo.Key);

                if (payload.manifest.NewImageInfo.HasChannel)
                    payloadInfoListAppend("channel", payload.manifest.NewImageInfo.Channel);

                if (payload.manifest.NewImageInfo.HasVersion)
                    payloadInfoListAppend("version", payload.manifest.NewImageInfo.Version);

                if (payload.manifest.NewImageInfo.HasBuildVersion)
                    payloadInfoListAppend("build_version", payload.manifest.NewImageInfo.BuildVersion);

                if (payload.manifest.NewImageInfo.HasBuildChannel)
                    payloadInfoListAppend("build_channel", payload.manifest.NewImageInfo.BuildChannel);
            }


            foreach (PartitionUpdate partitionUpdate in payload.manifest.Partitions)
            {
                payloadPartitionInfoListAppend(partitionUpdate.PartitionName,
                    partitionUpdate.NewPartitionInfo != null && partitionUpdate.NewPartitionInfo.HasSize ?
                    Helper.byte2AUnit(partitionUpdate.NewPartitionInfo.Size) : Properties.Resources.unknown,
                    partitionUpdate.NewPartitionInfo != null && partitionUpdate.NewPartitionInfo.HasHash ?
                    partitionUpdate.NewPartitionInfo.Hash.ToBase64() : Properties.Resources.unknown);
            }
            listHelper.render();

            if (payload.manifest.DynamicPartitionMetadata != null)
            {
                if (payload.manifest.DynamicPartitionMetadata.HasSnapshotEnabled)
                    dynamicPartitionMetaAppend(Properties.Resources.payload_snapshot_enabled +
                        (payload.manifest.DynamicPartitionMetadata.SnapshotEnabled ? Properties.Resources.yes :
                        Properties.Resources.no));

                if (payload.manifest.DynamicPartitionMetadata.Groups != null)
                    foreach (DynamicPartitionGroup dynamicPartitionGroup in payload.manifest.DynamicPartitionMetadata.Groups)
                    {
                        dynamicPartitionMetaAppend(Properties.Resources.payload_partition_group_name + dynamicPartitionGroup.Name);
                        dynamicPartitionMetaAppend(Properties.Resources.payload_partition_group_size + Helper.byte2AUnit(dynamicPartitionGroup.Size));
                        dynamicPartitionMetaAppend(Properties.Resources.payload_partition_group_include);
                        if (dynamicPartitionGroup.PartitionNames != null)
                            foreach (string parition in dynamicPartitionGroup.PartitionNames)
                                dynamicPartitionMetaAppend(parition);
                    }
            }
            else
            {
                dynamicPartitionMetaAppend(Properties.Resources.payload_no_dynamic_metadata);
            }

        }

        class payload_info_row
        {
            public string title { get; }
            public string value { get; }
            public payload_info_row(string title, string value)
            {
                this.title = title;
                this.value = value;
            }
        }

        static Helper.ListHelper<payload_partition_info_row> listHelper;
        static void payloadInfoListAppend(string title, string value)
        {
            MainWindow.THIS.payload_info.Items.Add(new payload_info_row(title, value));
        }

        class payload_partition_info_row
        {
            public string name { get; }
            public string size { get; }
            public string hash { get; }
            public payload_partition_info_row(string name, string size, string hash)
            {
                this.name = name;
                this.size = size;
                this.hash = hash;
            }
        }

        static void dynamicPartitionMetaAppend(string line)
        {
            MainWindow.THIS.payload_dynamic_partition_meta.Items.Add(new payload_dynamic_partition_meta_row(line));
        }

        class payload_dynamic_partition_meta_row
        {
            public string line { get; }
            public payload_dynamic_partition_meta_row(string line)
            {
                this.line = line;
            }
        }

        static void payloadPartitionInfoListAppend(string name, string size, string hash)
        {
            listHelper.addItem(new payload_partition_info_row(name, size, hash));
        }

        public static void init()
        {
            cur_status = page_status.empty;
            actionInit();
            switchMainView();
        }
    }
}

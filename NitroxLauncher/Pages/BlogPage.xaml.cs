using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NitroxLauncher.Models;

namespace NitroxLauncher.Pages
{
    public partial class BlogPage : PageBase
    {
        public static readonly Uri BLOGS_LINK = new("https://nitroxblog.rux.gg/");

        private readonly ObservableCollection<NitroxBlog> nitroxBlogs = new();

        public BlogPage()
        {
            InitializeComponent();

            Blogs.ItemsSource = nitroxBlogs;

            Dispatcher?.BeginInvoke(new Action(async () =>
            {
                try
                {
                    IList<NitroxBlog> blogs = await Downloader.GetBlogs();

                    foreach (NitroxBlog blog in blogs)
                    {
                        nitroxBlogs.Add(blog);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while trying to display nitrox blogs");
                }
            }
            ));
        }
    }
}

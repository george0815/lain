using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace lain.frameviews
{
    internal class SearchView : FrameView
    {

        public SearchView()
            : base(Resources.Search)
        {

            X = 20;
            Y = SettingsData.HeaderHeight;
            Width = Dim.Fill();
            Height = Dim.Fill();



        var searchBar = new TextField("")
        {
            X = 1,
            Y = 1,
            Width = 40
        };

            var searchBtn = new Button("Search")
            {
                X = Pos.Right(searchBar) + 2,
                Y = 1
            };

            var results = new ListView(new List<string>())
            {
                X = 1,
                Y = 3,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            searchBtn.Clicked += () =>
                {
                    // Example placeholder
                    results.SetSource(new List<string>()
                    {
                $"Result: {searchBar.Text}",
                "Result B",
                "Result C"
                    });
                };

             Add(searchBar, searchBtn, results);
        
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;
using lain.helpers;

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

                    //initialize ghidorah search args
                    SearchArgs args = new SearchArgs
                    {
                        Query = searchBar.Text.ToString() ?? "",
                        Limit = 5,
                        TotalLimit = 10,
                        Sources = new string[] { "thepiratebay", "limetorrents", "kickasstorrents" }, // Example sources
                        Categories = new string[] { "Movies", "Games", "Other" }, // Example categories
                        SortBy = "Sources"
                    };

                    string res = Ghidorah.Search(args);

                    Log.Write($"Ghidorah Search Result: {res}");

                    // Example placeholder
                    results.SetSource(new List<string>()
                    {
                res,
                "Result B",
                "Result C"
                    });
                };

             Add(searchBar, searchBtn, results);
        
        }
    }
}

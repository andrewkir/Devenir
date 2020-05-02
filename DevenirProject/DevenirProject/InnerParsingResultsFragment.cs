using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;

namespace DevenirProject
{
    class InnerParsingResultsFragment : Android.Support.V4.App.Fragment
    {
        string title, text, latex;
        public InnerParsingResultsFragment(string title, string text, string latex)
        {
            this.title = title;
            this.text = text == null ? "" : text;
            this.latex = latex == null ? "" : latex;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.pager_parsingresults, container, false);
            view.FindViewById<TextView>(Resource.Id.titleTextView).Text = title;
            view.FindViewById<TextInputLayout>(Resource.Id.textTextField).EditText.Text = text;
            view.FindViewById<TextInputLayout>(Resource.Id.latexTextField).EditText.Text = latex;
            return view;
        }
    }
}
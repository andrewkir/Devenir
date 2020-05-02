using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using System;

namespace DevenirProject
{
    public class ParsingResultFragment : Android.Support.V4.App.Fragment
    {
        ImageView imageView;
        ViewPager pager;
        TabLayout tabLayout;

        Bitmap sourceImage;
        string[] textBitmaps;
        string[] latexBitmaps;

        public ParsingResultFragment(Bitmap sourceImage, string[] textProcessed, string[] latexProcessed)
        {
            this.sourceImage = sourceImage;
            this.textBitmaps = textProcessed;
            this.latexBitmaps = latexProcessed;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_parsingResult, container, false);
            imageView = view.FindViewById<ImageView>(Resource.Id.imageView);
            imageView.SetImageBitmap(sourceImage);
            imageView.ClipToOutline = true;

            pager = view.FindViewById<ViewPager>(Resource.Id.pager);
            tabLayout = view.FindViewById<TabLayout>(Resource.Id.tabDots);
            tabLayout.SetupWithViewPager(pager, true);

            var adapter = new PagesAdapter(FragmentManager, Math.Max(textBitmaps.Length, latexBitmaps.Length), textBitmaps, latexBitmaps);
            pager.Adapter = adapter;
            pager.CurrentItem = 0;
            return view;
        }


        class PagesAdapter : Android.Support.V4.App.FragmentPagerAdapter
        {
            int maxFragments;
            string[] textProcessed, latexProcessed;

            public override int Count => maxFragments;

            public PagesAdapter(Android.Support.V4.App.FragmentManager fragmentManager, int maxFragments, string[] textBitmaps, string[] latexBitmaps) : base(fragmentManager)
            {
                this.maxFragments = maxFragments;
                this.textProcessed = textBitmaps;
                this.latexProcessed = latexBitmaps;
            }

            public override Android.Support.V4.App.Fragment GetItem(int position)
            {
                switch (position)
                {
                    case 0:
                        return new InnerParsingResultsFragment("Результат работы", textProcessed.Length >= 1 ? textProcessed[0] : null, latexProcessed.Length >= 1 ? latexProcessed[0] : null);
                    case 1:
                        return new InnerParsingResultsFragment("Результат анализа 1 фрагмента", textProcessed.Length >= 2 ? textProcessed[1] : null, latexProcessed.Length >= 2 ? latexProcessed[1] : null);
                    case 2:
                        return new InnerParsingResultsFragment("Результат анализа 2 фрагмента", textProcessed.Length >= 3 ? textProcessed[2] : null, latexProcessed.Length >= 3 ? latexProcessed[2] : null);
                    case 3:
                        return new InnerParsingResultsFragment("Результат анализа 3 фрагмента", textProcessed.Length >= 4 ? textProcessed[3] : null, latexProcessed.Length >= 4 ? latexProcessed[3] : null);
                    default:
                        return new InnerParsingResultsFragment("Результат работы", textProcessed.Length >= 1 ? textProcessed[0] : null, latexProcessed.Length >= 1 ? latexProcessed[0] : null);
                }
            }
        }
    }
}
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Threading;
using System.Reactive.Concurrency;

namespace ReactiveUISample
{
    public class MyViewModel : ReactiveObject
    {
        string name = string.Empty;
        public string Name
        {   //read-write property
            get { return name; }
            set
            {
                this.RaiseAndSetIfChanged
                      (ref name, value);
            }
        }

        ObservableAsPropertyHelper<string> firstName;
        public string FirstName // output property
        {
            get { return firstName.Value; }
        }

        ObservableAsPropertyHelper<string> lastName;
        public string LastName // output property
        {
            get { return lastName.Value; }
        }

        ObservableAsPropertyHelper<List<string>> nameDays;
        public List<string> NameDays // output property
        {
            get { return nameDays.Value; }
        }


        //read-only property
        public ReactiveCommand<object> ClearCommand
        { get; private set; }

        public MyViewModel()
        {
            this.ThrownExceptions.Subscribe(x =>
                {
                    x.ToString();
                });
            firstName = this.WhenAny(x => x.Name, x => x.Value)
                .Select(x => x.Split(' ')[0])
                .ToProperty(this, x => x.FirstName);

            lastName = this.WhenAny(x => x.Name, x => x.Value).Select(x =>
            {
                var s = x.Split(' ');
                if (s.Count() > 1) return s[1]; else return string.Empty;
            }).ToProperty(this, x => x.LastName);

            //BAD:
            //var input = this.WhenAnyValue(x => x.FirstName);
            //var results = from searchTerm in input
            //              where !string.IsNullOrWhiteSpace(searchTerm)
            //              select DoSearch(searchTerm).Results.Select(x => x.ToString()).ToList();
            //GOOD:
            // Transform the event stream into a stream of strings (the input values)
            var input = this.WhenAnyValue(x => x.FirstName)
                .Where(x => x == null || x.Length < 4)
                .Throttle(TimeSpan.FromSeconds(3))
                .Merge(this.WhenAnyValue(x => x.FirstName)
                .Where(x => x != null && x.Length >= 4)
                .Throttle(TimeSpan.FromMilliseconds(400)))
                .DistinctUntilChanged();

            // Setup an Observer for the search operation
            var search = Observable.ToAsync<string, SearchResult>(DoSearch);

            // Chain the input event stream and the search stream, cancelling searches when input is received
            var results = from searchTerm in input
                          from result in search(searchTerm).TakeUntil(input)
                          select result.Results.Select(x => x.ToString()).ToList();

            // Log the search result and add the results to the results collection
            nameDays = results.ObserveOn(CoreDispatcherScheduler.Current)
                .ToProperty(this, x => x.NameDays);

            var firstNameOK = this.WhenAnyValue(x => x.FirstName)
                .Select(x => !string.IsNullOrWhiteSpace(x) && x.Length >= 3);
            var lastNameOK = this.WhenAnyValue(x => x.LastName)
                .Select(x => !string.IsNullOrWhiteSpace(x) && x.Length >= 3);

            //ClearCommand = ReactiveCommand.Create
            //    (
            //    //Observable condition to enable command:
            //    firstNameOK.Zip(lastNameOK, (x, y) => x && y)
            //    );
            //ClearCommand.Subscribe
            //    (
            //    //action:
            //    x => Name = string.Empty
            //    );

            ClearCommand = ReactiveCommand.Create
            (
                //Observable condition to enable command:
            this.WhenAnyValue(x => x.LastName)
            .Select(name => !string.IsNullOrEmpty(name))
            );
            ClearCommand.Subscribe
            (
                //action:
            x => Name = string.Empty
            );
        }
        #region search
        private readonly Random random = new Random(DateTime.Now.Millisecond);
        private SearchResult DoSearch(string searchTerm)
        {
            ThreadSleep(random.Next(200, 1000)); // Simulate latency
            return new SearchResult
            {
                SearchTerm = searchTerm,
                Results =
                    NameDay.NameDays.Where(item => item.Names.ToUpperInvariant().Contains(searchTerm.ToUpperInvariant())).ToArray()
            };
        }
        public struct SearchResult
        {
            public string SearchTerm { get; set; }
            public IEnumerable<NameDay> Results { get; set; }

        }
        private static void ThreadSleep(int milliseconds)
        {
            using (EventWaitHandle tmpEvent = new ManualResetEvent(false))
            {
                tmpEvent.WaitOne(TimeSpan.FromMilliseconds(milliseconds));
            }
        }
        #endregion search
    }
}

#region Header

// ReSharper disable once CommentTypo
/*
 // Editors Note:  This is an ancient piece of code, from C and perl in late 80s, C++ in early 90's, VB in late 90s, and then C# 2003.
 // This is its 6th life into C# 6.0 Sept 2021.
 // And it's still a pile :)

< title ></ title >
< revision > $Revision: 1 $</ revision >
< author >$Author: jill $</ author >
< date >$Date: 10 / 11 / 04 11:32a $</ date >
< copyright > Copyright Jill England(c) 2004, All Rights Reserved.</copyright>
 
<summary>
 This is a timer class for capturing elapsed time of queries and web page displays.
</ summary >

< usage >
 C#
 StopWatch sw = new StopWatch();  // Causes timer to be created and started.
 ...
 sw.AddComment("Im in method abc and had an error: " + e.Message);   // Add a comment to a stopwatch.
 ...
 sw.Stop("Timer stopped while destroying xyz");  // Stop a stopwatch.
 ...
 Console.Writeline(sw.ToString() + Environment.NewLin);		// Return a string that reports the start/stop times and all the comments in order of occurrence.
 ...
 string html_text = sw.ToHtml(); // Formats a series of <br> separated lines suitable for html display.
 string xmlText = Simple serialization of this timer.
</usage>

<history>
</history>
*/

#endregion

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
// ReSharper disable All

namespace ErrorTrace

{
    /// <summary>
    ///  StopWatch keeps track of it's start / stop times and a list of comments that may be added to it.
    ///  This results is an array of Comments and a time span.
    /// </summary>
    [Serializable]
    public class StopWatch
    {
        #region Properties

        private protected  List<string> _comments = new List<string>();

        private readonly DateTime _timeStart;

        private DateTime _timeStop;

        /// <summary>
        /// A simple way to reference all the comments without using the Collection.
        /// </summary>
        public string[] AllComments
        {
            get
            {
                return _comments.ToArray();
            }
        }
        /// <summary>
        /// Total number of comments so far.
        /// </summary>
        public int CountComments => _comments.Count;

        /// <summary>
        /// The very last comment saved to this sw.
        /// </summary>
        public string LastComment => (string)_comments[^1];

        /// <summary>
        /// The TimeSpan since this object was instantiated 
        /// OR the timespan between instantiation and the first call to the Stop Method.
        /// </summary>
   
        public TimeSpan Value
        {
            get
            {
                var endTime = DateTime.Now;

                if (_timeStop > DateTime.MinValue)
                    endTime = _timeStop;

                return new TimeSpan(endTime.Ticks - _timeStart.Ticks);
            }
        }
        /// <summary>
        /// This regular expression finds empty line. Used internally to work with empty comments.
        /// </summary>
        private static readonly Regex IsEmpty = new Regex(@"^\s*$");
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public StopWatch()
        {
            _timeStart = DateTime.Now;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comment"></param>
        public StopWatch(string comment) : this()
        {
            AddComment(comment);
        }

        /// <summary>
        /// Stops the timer and returns a TimeSpan object. 
        /// You can only stop a timer once further calls to Stop will always return the same TimeSpan.  See the .Value method
        /// </summary>
        /// <param name="comment">Optional comment</param>
        /// <returns></returns>
        public TimeSpan Stop(string comment)
        {
            if (_timeStop.Ticks == 0)
                _timeStop = DateTime.Now;

            AddComment(comment);

            return Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TimeSpan Stop()
        {
            return this.Stop(string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comment"></param>
        public void AddComment(string comment)
        {
            if (!string.IsNullOrEmpty(comment))
                _comments.Add(comment);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(Environment.NewLine);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="separator"></param>
        /// <returns></returns>
        public string ToString(string separator)
        {
            return string.Join(separator, _comments) + $"{separator}{Value.TotalSeconds}";
        }
    }
}

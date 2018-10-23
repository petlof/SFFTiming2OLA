using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmiTagMerger
{
    internal class TagReadOut
    {
        public TagReadOut()
        {
            PostPasses = new List<PostPass>();
            TagNo = -1;
        }

        public int TagNo;
        public DateTime TimeOfReadout;
        public string TagText;
        public List<PostPass> PostPasses;

        public DateTime FinishTime
        {
            get
            {
                try
                {
                    //Finish is last 248 or 90 in the card..
                    var finishPass = PostPasses.Last(x => x.PostCode == 248 || x.PostCode == 90);
                    var readOutTime = PostPasses.Last(x => x.PostCode >= 250 && x.PostCode <= 254);
                    var timeBeforeReadout = finishPass.Time - readOutTime.Time;
                    return TimeOfReadout.AddMilliseconds(timeBeforeReadout.TotalMilliseconds);

                }
                catch (Exception)
                {

                    return DateTime.MinValue;

                }
            }
        }

        public TimeSpan? GetTimeToFinish(DateTime startTime)
        {
            return FinishTime - startTime;
        }

        public TimeSpan? GetTimeToSplit(DateTime startTime, int splitCode, int passing = -1)
        {
            PostPass postPass = null;

            if (passing == -1)
            {
                postPass = PostPasses.LastOrDefault(x => x.PostCode == splitCode);
            }
            else
            {
                //Else we want a specific passing-no
                var passes = PostPasses.Where(x => x.PostCode == splitCode).ToArray();
                if (passes.Length >= passing)
                    postPass = passes[passing - 1];
            }

            if (postPass == null)
                return null;
            else
            {
                var readOutTime = PostPasses.Last(x => x.PostCode >= 250 && x.PostCode <= 254);
                DateTime timeOfPostPass = TimeOfReadout.AddMilliseconds((postPass.Time - readOutTime.Time).TotalMilliseconds);
                return timeOfPostPass - startTime;
            }
        }

        public DateTime StartTime
        {
            get
            {
                try
                {


                    return new DateTime(2015, 8, 29, 10, 0, 0);
                    //Finish is last 249 or 90 in the card..
                    var startPass = PostPasses.LastOrDefault(x => x.PostCode == 0);
                    var readOutTime = PostPasses.Last(x => x.PostCode >= 251 && x.PostCode <= 254);
                    if (startPass == null)
                        startPass = new PostPass()
                        {
                            PostCode = 0,
                            PostNo = 0,
                            Time = new TimeSpan(0)
                        };
                    var timeBeforeReadout = startPass.Time - readOutTime.Time;
                    return TimeOfReadout.AddMilliseconds(timeBeforeReadout.TotalMilliseconds);
                }
                catch (Exception)
                {

                    return DateTime.MinValue;
                }
            }
        }
    }

    internal class PostPass
    {
        public int PostNo { get; set; }
        public int PostCode { get; set; }
        public TimeSpan Time { get; set; }
    }
}

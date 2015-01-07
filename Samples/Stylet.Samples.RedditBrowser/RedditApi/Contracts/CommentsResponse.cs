using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Stylet.Samples.RedditBrowser.RedditApi.Contracts
{
    [DataContract]
    public class CommentsResponse
    {
        [DataMember(Name = "kind")]
        public string Kind { get; set; }

        [DataMember(Name = "data")]
        public CommentsResponseData Data { get; set; }
    }

    [DataContract]
    public class CommentsResponseData
    {
        [DataMember(Name = "before")]
        public string Before { get; set; }

        [DataMember(Name = "after")]
        public string After { get; set; }

        [DataMember(Name = "children")]
        public List<CommentListing> Children { get; set; }
    }

    [DataContract]
    public class CommentListing
    {
        [DataMember(Name = "kind")]
        public string Kind { get; set; }

        [DataMember(Name = "data")]
        public CommentData Data { get; set; }
    }

    [DataContract]
    public class CommentData
    {
        [DataMember(Name = "body")]
        public string Body { get; set; }

        [DataMember(Name = "replies")]
        public CommentsResponse Replies { get; set; }

        [DataMember(Name = "author")]
        public string Author { get; set; }
    }
}

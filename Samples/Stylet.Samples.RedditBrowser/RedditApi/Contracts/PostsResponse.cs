using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Stylet.Samples.RedditBrowser.RedditApi.Contracts
{
    [DataContract]
    public class PostsResponse
    {
        [DataMember(Name = "data")]
        public PostsResponseData Data { get; set; }
    }

    [DataContract]
    public class PostsResponseData
    {
        [DataMember(Name = "before")]
        public string Before { get; set; }

        [DataMember(Name = "after")]
        public string After { get; set; }

        [DataMember(Name = "children")]
        public List<Post> Children { get; set; }
    }

    [DataContract]
    public class Post
    {
        [DataMember(Name = "data")]
        public PostData Data { get; set; }
    }

    [DataContract]
    public class PostData
    {
        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "num_comments")]
        public int NumComments { get; set; }
    }
}

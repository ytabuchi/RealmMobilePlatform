using System;
using System.Collections.Generic;
using Realms;

namespace RealmMobilePlatformSample
{
    public class TaskList : RealmObject
    {
		[PrimaryKey]
		[Required]
		[MapTo("id")]
		public string Id { get; set; }

		[MapTo("text")]
		[Required]
        public string Title { get; set; } = string.Empty;

        [MapTo("items")]
        public IList<Task> Items { get; }
    }

    public class Task : RealmObject
    {
        [MapTo("text")]
        [Required]
        public string Title { get; set; } = string.Empty;

        [MapTo("completed")]
        public bool Completed { get; set; } = false;
    }
}

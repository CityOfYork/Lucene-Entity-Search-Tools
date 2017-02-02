
## Lucene Entity Search Tools ##
Lucene Entity Search Tools is a library for easily indexing and searching entities. When combined with the **SearchableContextProvider**, indexing and searching can be easily applied to any entity framework database.
### Quick Start###
We are going to go through the necessary steps to create and index a code-first entity framework data context. We’ll use a single entity called **User** for this quick start guide.

####The Model####
 	public class User : LuceneIndexableEntityBase
	{
		[LuceneIndexable]
        public string FirstName { get; set; }

        [LuceneIndexable]
        public string Surname { get; set; }

        [LuceneIndexable]
        public string Email { get; set; }

        [LuceneIndexable]
        public string JobTitle { get; set; }

        public override Type Type { get { return typeof(User); } }
	}

**LuceneIndexableEntityBase** is the base class that enables converting an entity to a Lucene Document which then enables adding it to an index. Which properties are indexed is determined by using the **LuceneIndexable** attribute on the classes properties.

####The Context####
	public class UserContext : DbContext, IDbContext
	{
		public DbSet<User> Users { get; set; }
	}

That’s it for the context.  The only addition is the implementation of **IDbContext**, which just declares some key methods within a standard entity framework context so we don't need to do anything more to implement this interface.

###Creating an index###
Once we have our model and context we can start by indexing the data in our context.

	// create a test context
	context = new UserContext();          
	
	// create a searchProvider
	searchProvider = new SearchableContextProvider<UserContext>();
	
	// initialize the searchProvider using a Ram directory and existing context
	searchProvider.Initialize(new LuceneIndexerOptions(null, null, true), context);
	
	// index the db
	searchProvider.CreateIndex();

And that’s it, our context is now searchable!

###Updating the index###
To update the index after performing any **CUD** operations simply call the **SaveChanges()** method from the **SearchableContextProvider** instead of calling **SaveChanges()** from the **DbContext**. This will update the index and then call the Contexts SaveChanges() method.

###Searching an index###
To search an index, we need to create a query using a **SearchOptions** class and calling one of the search providers Search methods. There are a number of different options for searching, for now we’ll just use a typed ScoredSearch, which will return the results as actual User objects.

	// create a query
	var query = new SearchOptions();

	query.SearchText = "John";
	query.Fields.Add("FirstName");
	query.OrderBy.Add("Surname");

	// Do the search
	var results = searchProvider.ScoredSearch<User>(query);

####The Results####
The search returns an **`IScoredSearchResultCollection<User>`** which includes the time it took to perform the search, the total number of hits and a collection of the results each containing the User object and how much it scored in the search.

**NOTE:** We are using an in memory RAM Directory for indexing and searching in this demo, but this is only for testing purposes currently and an on disc directory should be used instead. This is easy to do and just means passing a path into the constructor:

	// initialize the searchProvider with the index path and the context to use
	searchProvider.Initialize(new LuceneIndexerOptions("c:\\Temp\\MyIndexDirectory\", null, false), context);




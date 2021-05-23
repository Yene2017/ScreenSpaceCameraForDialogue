namespace vcf
{
	namespace tools
	{
		public class Pair<T, U> 
        {
		    public Pair() 
            {
		    }

		    public Pair(T first, U second) 
            {
		        this.First = first;
		        this.Second = second;
		    }

		    public T First { get; set; }
		    public U Second { get; set; }
		}

        public class Tuple3<T, U, V>
        {
            public T First { get; set; }
            public U Second { get; set; }
            public V Third { get; set; }

            public Tuple3() 
            {
		    }

            public Tuple3(T first, U second, V third) 
            {
		        this.First = first;
		        this.Second = second;
                this.Third = third;
		    }
        }
	}
}

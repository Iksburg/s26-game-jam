using System;

namespace Cats.Genome
{
    public class GenomeException : ArgumentException
    {
        public GenomeException(string message) : base(message)
        {
            
        }
    }
    
    public class InvalidSexException : ArgumentException
    {
        public InvalidSexException(string message) : base(message)
        {
            
        }
    }
    
    public class GenomeNotFoundException : GenomeException
    {
        public GenomeNotFoundException(string message) : base(message)
        {
            
        }
    }
}
namespace FamilyTree
{
    public abstract class Data
    {
        public DateTime LastModifiedDate { get; protected set; }

        protected Data()
        {
            LastModifiedDate = DateTime.Now;
        }

        protected void UpdateModifiedDate()
        {
            LastModifiedDate = DateTime.Now;
        }
    }
}

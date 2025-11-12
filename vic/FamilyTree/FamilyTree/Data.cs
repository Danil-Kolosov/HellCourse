namespace FamilyTree
{
    public abstract class Data
    {
        public DateTime LastModifiedDate { get; protected set; }

        protected void UpdateModifiedDate()
        {
            LastModifiedDate = DateTime.Now;
        }
    }
}

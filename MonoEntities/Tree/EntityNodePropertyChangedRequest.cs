namespace MonoEntities.Tree
{
    internal class EntityNodePropertyChangedRequest
    {
        internal string PropertyName { get; set; }

        internal object OldValue { get; set; }

        internal object NewValue { get; set; }
    }
}

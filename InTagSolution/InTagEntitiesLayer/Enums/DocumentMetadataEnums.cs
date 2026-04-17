namespace InTagEntitiesLayer.Enums
{
    public enum MetadataFieldType
    {
        Text = 0,
        TextArea = 1,
        Number = 2,
        Decimal = 3,
        Date = 4,
        DateTime = 5,
        Boolean = 6,
        Dropdown = 7,
        MultiSelect = 8,
        Url = 9,
        Email = 10,
        User = 11
    }

    public enum DocumentLinkType
    {
        RelatedTo = 0,
        Supersedes = 1,
        SupersededBy = 2,
        References = 3,
        ReferencedBy = 4,
        ParentOf = 5,
        ChildOf = 6,
        AmendmentTo = 7
    }
}

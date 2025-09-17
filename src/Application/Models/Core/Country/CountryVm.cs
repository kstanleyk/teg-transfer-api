﻿namespace Agrovet.Application.Models.Core.Country;

public class CountryVm
{
    public string Id { get; set; } = null!;
    public Guid PublicId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}
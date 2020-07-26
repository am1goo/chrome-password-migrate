using System;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ColumnAttribute : Attribute
{
  public string Name;

  public override bool Equals(object obj)
  {
    var attribute = obj as ColumnAttribute;
    return attribute != null &&
           base.Equals(obj) &&
           Name == attribute.Name;
  }

  public override int GetHashCode()
  {
    var hashCode = 890389916;
    hashCode = hashCode * -1521134295 + base.GetHashCode();
    hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
    return hashCode;
  }
}
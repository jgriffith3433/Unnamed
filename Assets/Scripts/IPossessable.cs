using UnityEngine;

/// <summary>
/// Base controller interface that all possessable controllers implement
/// </summary>
public interface IPossessable
{
    void OnPossessed(Player player);
    void OnUnpossessed();
    bool IsPossessed();
}

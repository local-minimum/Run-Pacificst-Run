using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LevelFeatureValue = System.UInt32;

public static class LevelFeature
{
    static readonly LevelFeatureValue One = 1;
    static readonly LevelFeatureValue Zero = 0;
    static readonly ushort GroundIdBits = 3;


    public static LevelFeatureValue Ground(bool movable, bool obstruction, ushort identifier)
    {
        if (identifier > (One << GroundIdBits))
        {
            throw new System.NotSupportedException($"The identifier must be in the range 0 - 5 (not {identifier})");
        }
        return (movable ? One : Zero) | (obstruction ? One << 1 : Zero) | ((LevelFeatureValue)identifier << 2);
    }

    static readonly ulong SemanticGroundMatch = 0b11;

    public static bool FulfillsSemanticGroundMask(bool movable, bool obstruction, LevelFeatureValue value)
    {
        LevelFeatureValue mask = (movable ? One : Zero) | (obstruction ? One << 1 : Zero);
        return ((~(mask ^ value)) & SemanticGroundMatch) == SemanticGroundMatch;
    }

    static readonly ulong GroundIdMatch = 0b00111;

    public static bool FullfillsGroundMask(ushort identifier, LevelFeatureValue value)
    {
        ulong mask = (((ulong)identifier) << 2);
        return ((~(mask ^ value)) & GroundIdMatch) == GroundIdMatch;
    }

    static readonly ushort FirstObjectBit = 12;
    public static bool HasObject(LevelFeatureValue value)
    {
        return ((One << FirstObjectBit) & value) > 0;
    }

    static readonly ushort FirstAgentBit = 24;
    public static bool HasAgent(LevelFeatureValue value)
    {
        return ((One << FirstAgentBit) & value) > 0;
    }

    static readonly ushort AgentIdBits = 5;
    public static LevelFeatureValue Agent(bool isPlayer, bool conversable, ushort identifier)
    {
        if (identifier > One << AgentIdBits)
        {
            throw new System.NotSupportedException($"The identifier must be in the range 0 - 5 (not {identifier})");
        }
        return (One << FirstAgentBit) | (isPlayer ? One << (FirstAgentBit + 1) : Zero) | (conversable ? One << (FirstAgentBit + 2) : Zero) | ((LevelFeatureValue)identifier << (FirstAgentBit + 3));
    }

}


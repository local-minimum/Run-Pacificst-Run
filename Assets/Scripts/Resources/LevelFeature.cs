using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LevelFeatureValue = System.UInt32;

public enum GroundType { BASIC, BLOCKING_MOVABLE, BLOCKING_IMOVABLE, NONBLOCKING_MOVABLE };


public static class LevelFeature
{
    static readonly LevelFeatureValue One = 1;
    static readonly LevelFeatureValue Zero = 0;

    #region GROUND
    static readonly ushort GroundIdBits = 3;

    public static LevelFeatureValue Ground(bool movable, bool obstruction, ushort identifier)
    {
        if (identifier > (One << GroundIdBits))
        {
            throw new System.NotSupportedException($"The identifier must be in the range 0 - 5 (not {identifier})");
        }
        return (movable ? One : Zero) | (obstruction ? One << 1 : Zero) | ((LevelFeatureValue)identifier << 2);
    }

    static readonly LevelFeatureValue SemanticGroundMatch = 0b11;

    public static bool FulfillsSemanticGroundMask(bool movable, bool obstruction, LevelFeatureValue value)
    {
        LevelFeatureValue mask = (movable ? One : Zero) | (obstruction ? One << 1 : Zero);
        return ((~(mask ^ value)) & SemanticGroundMatch) == SemanticGroundMatch;
    }

    static readonly LevelFeatureValue ObstructionMask = 0b01;
    public static bool IsBlocked(LevelFeatureValue value) 
    {
        //TODO: Could be blocked by object
        return (value & ObstructionMask) == ObstructionMask;
    }

    public static GroundType GetGroundType(LevelFeatureValue value)
    {
        bool movable = (One & value) == One;
        bool obstruction = ((One << 1) & value) == (One << 1);
        if (movable)
        {
            return obstruction ? GroundType.BLOCKING_MOVABLE : GroundType.NONBLOCKING_MOVABLE;
        } else
        {
            return obstruction ? GroundType.BLOCKING_IMOVABLE : GroundType.BASIC;
        }
    }

    static readonly LevelFeatureValue GroundIdMask = 0b00111;

    public static bool FullfillsGroundMask(ushort identifier, LevelFeatureValue value)
    {
        LevelFeatureValue mask = (((LevelFeatureValue)identifier) << 2);
        return ((~(mask ^ value)) & GroundIdMask) == GroundIdMask;
    }

    static readonly LevelFeatureValue NotGroundMask = 0b00000000000011111111111111111111;
    //                                                  0   4   8   12  16  20  24  28  32
    public static LevelFeatureValue EvolveGround(bool movable, bool obstruction, LevelFeatureValue value)
    {
        LevelFeatureValue semanticGround = (movable ? One : Zero) | (obstruction ? One << 1 : Zero);
        LevelFeatureValue groundId = GroundIdMask & value;
        LevelFeatureValue nonground = value & NotGroundMask;
        return semanticGround | groundId | nonground;
    }
    #endregion

    #region OBJECTS
    static readonly ushort FirstObjectBit = 12;
    public static bool HasObject(LevelFeatureValue value)
    {
        return ((One << FirstObjectBit) & value) > 0;
    }
    #endregion

    #region AGENTS
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
    #endregion

    #region MIXED
    public static bool IsVacant(LevelFeatureValue value)
    {
        return !IsBlocked(value) && !HasObject(value) && !HasAgent(value);
    }
    #endregion
}

